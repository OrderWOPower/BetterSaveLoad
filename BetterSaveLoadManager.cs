using HarmonyLib;
using SandBox;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.SaveSystem;
using TaleWorlds.SaveSystem.Load;

namespace BetterSaveLoad
{
    [HarmonyPatch(typeof(MBSaveLoad))]
    public static class BetterSaveLoadManager
    {
        private static readonly BetterSaveLoadSettings Settings = BetterSaveLoadSettings.Instance;
        private static readonly string QuickSaveNamePrefix = "save_quick_", BattleAutoSaveNamePrefix = "save_auto_battle_";

        private static int QuickSaveIndex = 0, BattleAutoSaveIndex = 0;
        private static string ActiveSaveSlotName = null;

        public static SaveGameFileInfo SaveFile { get; set; }

        public static bool CanLoad => SaveFile != null && !SaveFile.IsCorrupted;

        public static string PlayerClanAndMainHeroName => Clan.PlayerClan.Name.ToString().ToLower() + "_" + Hero.MainHero.Name.ToString().ToLower() + "_";

        // Get the name of the currently loaded save file.
        [HarmonyPostfix]
        [HarmonyPatch("LoadSaveGameData")]
        public static void Postfix1(string ___ActiveSaveSlotName) => ActiveSaveSlotName = ___ActiveSaveSlotName;

        [HarmonyPostfix]
        [HarmonyPatch("OnNewGame")]
        public static void Postfix2(string ___ActiveSaveSlotName) => ActiveSaveSlotName = ___ActiveSaveSlotName;

        [HarmonyPatch("QuickSaveCurrentGame")]
        public static void Prefix(ref string ___ActiveSaveSlotName)
        {
            // Increment the quick save index.
            QuickSaveIndex++;

            if (Settings.ShouldLimitSaves && QuickSaveIndex > Settings.QuickSaveLimit)
            {
                // Reset the quick save index if it is greater than the maximum number in the settings.
                QuickSaveIndex = 1;
            }

            // Replace the save file name with a custom one with the quick save index.
            ___ActiveSaveSlotName = QuickSaveNamePrefix + PlayerClanAndMainHeroName + QuickSaveIndex;

            ActiveSaveSlotName = ___ActiveSaveSlotName;

            // Display the file name of the saved game in a debug message.
            InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=BSLmsg001}Game saved: \"{SAVENAME}\".").SetTextVariable("SAVENAME", ActiveSaveSlotName).ToString()));
        }

        public static void InitializeSaveIndexes()
        {
            string quickSaveName = string.Empty, battleAutoSaveName = string.Empty, quickSaveNameWithoutIndex = QuickSaveNamePrefix + PlayerClanAndMainHeroName, battleAutoSaveNameWithoutIndex = BattleAutoSaveNamePrefix + PlayerClanAndMainHeroName;

            quickSaveName = MBSaveLoad.GetSaveFiles().FirstOrDefault(saveFile => saveFile.Name.StartsWith(quickSaveNameWithoutIndex))?.Name;
            battleAutoSaveName = MBSaveLoad.GetSaveFiles().FirstOrDefault(saveFile => saveFile.Name.StartsWith(battleAutoSaveNameWithoutIndex))?.Name;

            QuickSaveIndex = 0;
            BattleAutoSaveIndex = 0;

            if (!string.IsNullOrEmpty(quickSaveName) && int.TryParse(quickSaveName.Substring(quickSaveNameWithoutIndex.Length), out int quickSaveIndex) && quickSaveIndex > 0 && quickSaveIndex <= Settings.QuickSaveLimit)
            {
                // Set the quick save index to the number in the latest quick save file name.
                QuickSaveIndex = quickSaveIndex;
            }

            if (!string.IsNullOrEmpty(battleAutoSaveName) && int.TryParse(battleAutoSaveName.Substring(battleAutoSaveNameWithoutIndex.Length), out int battleAutoSaveIndex) && battleAutoSaveIndex > 0 && battleAutoSaveIndex <= Settings.BattleAutoSaveLimit)
            {
                // Set the battle auto save index to the number in the latest battle auto save file name.
                BattleAutoSaveIndex = battleAutoSaveIndex;
            }

            if (ActiveSaveSlotName != null)
            {
                // Display the file name of the loaded game in a debug message.
                InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=BSLmsg002}Game loaded: \"{SAVENAME}\".").SetTextVariable("SAVENAME", ActiveSaveSlotName).ToString()));
            }
        }

        public static void AutoSaveForBattle(MapEvent mapEvent)
        {
            bool shouldAutoSaveBeforeBattle = !mapEvent.IsFinalized && (Settings.BattleAutoSaveTrigger.SelectedIndex == 0 || Settings.BattleAutoSaveTrigger.SelectedIndex == 2) && mapEvent.AttackerSide.TroopCount >= Settings.MinAttackerTroopCount && mapEvent.DefenderSide.TroopCount >= Settings.MinDefenderTroopCount;
            bool shouldAutoSaveAfterBattle = mapEvent.IsFinalized && (Settings.BattleAutoSaveTrigger.SelectedIndex == 1 || Settings.BattleAutoSaveTrigger.SelectedIndex == 2) && mapEvent.AttackerSide.TroopCount + mapEvent.AttackerSide.TroopCasualties >= Settings.MinAttackerTroopCount && mapEvent.DefenderSide.TroopCount + mapEvent.DefenderSide.TroopCasualties >= Settings.MinDefenderTroopCount;

            // Execute only if the numbers of attackers and defenders are greater than or equal to the minimum numbers in the settings.
            if (Settings.ShouldAutoSaveForBattle && (shouldAutoSaveBeforeBattle || shouldAutoSaveAfterBattle))
            {
                // Increment the battle auto save index.
                BattleAutoSaveIndex++;

                if (Settings.ShouldLimitSaves && BattleAutoSaveIndex > Settings.BattleAutoSaveLimit)
                {
                    // Reset the battle auto save index if it is greater than the maximum number in the settings.
                    BattleAutoSaveIndex = 1;
                }

                ActiveSaveSlotName = BattleAutoSaveNamePrefix + PlayerClanAndMainHeroName + BattleAutoSaveIndex;

                Campaign.Current.SaveHandler.SaveAs(ActiveSaveSlotName);
                // Display the file name of the saved game in a debug message.
                InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=BSLmsg001}Game saved: \"{SAVENAME}\".").SetTextVariable("SAVENAME", ActiveSaveSlotName).ToString()));
            }
        }

        public static void QuickLoadPreviousGame()
        {
            // Get the latest quick save, manual save or auto save.
            SaveFile = MBSaveLoad.GetSaveFileWithName(BannerlordConfig.LatestSaveGameName);

            if (CanLoad)
            {
                MBGameManager.EndGame();

                return;
            }

            InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=BSLmsg003}No save files to load!").ToString()));
        }

        public static void StartGame(LoadResult loadResult)
        {
            SaveFile = null;

            MBGameManager.StartNewGame(new SandBoxGameManager(loadResult));
        }
    }
}
