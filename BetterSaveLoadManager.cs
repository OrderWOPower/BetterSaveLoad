using HarmonyLib;
using SandBox;
using System.IO;
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

        public static string ActiveSaveSlotName { get; private set; }
        public static SaveGameFileInfo SaveFile { get; private set; }

        public static bool CanLoad => SaveFile != null && !SaveFile.IsCorrupted;

        public static string PlayerSaveNamePrefix => Clan.PlayerClan?.Name.ToString().ToLower() + "_" + Hero.MainHero.Name.ToString().ToLower() + "_";

        // Get the name of the currently loaded save file.
        [HarmonyPostfix]
        [HarmonyPatch("LoadSaveGameData")]
        public static void Postfix1() => ActiveSaveSlotName = MBSaveLoad.ActiveSaveSlotName;

        [HarmonyPostfix]
        [HarmonyPatch("OnNewGame")]
        public static void Postfix2() => ActiveSaveSlotName = MBSaveLoad.ActiveSaveSlotName;

        [HarmonyPatch("QuickSaveCurrentGame")]
        public static void Prefix()
        {
            string saveName = QuickSaveNamePrefix + PlayerSaveNamePrefix + QuickSaveIndex;

            // Increment the quick save index.
            QuickSaveIndex++;

            if (Settings.ShouldLimitSaves && QuickSaveIndex > Settings.QuickSaveLimit)
            {
                // Reset the quick save index if it is greater than the maximum number in the settings.
                QuickSaveIndex = 1;
            }

            if (saveName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                // Remove illegal characters from the save name.
                saveName = new string(saveName.Where(c => !Path.GetInvalidFileNameChars().Contains(c)).ToArray());

                // Display a warning message if the save name contains illegal characters.
                InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=BSLmsg004}Warning: Save name contains illegal characters! Removing illegal characters.").ToString()));
            }

            // Replace the save file name with a custom one with the quick save index.
            AccessTools.Property(typeof(MBSaveLoad), "ActiveSaveSlotName").SetValue(null, saveName);

            ActiveSaveSlotName = MBSaveLoad.ActiveSaveSlotName;

            // Display the file name of the saved game in a debug message.
            InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=BSLmsg001}Game saved: \"{SAVENAME}\".").SetTextVariable("SAVENAME", ActiveSaveSlotName).ToString()));
        }

        public static void InitializeSaveIndexes()
        {
            string quickSaveName = string.Empty, battleAutoSaveName = string.Empty, quickSaveNameWithoutIndex = QuickSaveNamePrefix + PlayerSaveNamePrefix, battleAutoSaveNameWithoutIndex = BattleAutoSaveNamePrefix + PlayerSaveNamePrefix;

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
                string saveName = BattleAutoSaveNamePrefix + PlayerSaveNamePrefix + BattleAutoSaveIndex;

                // Increment the battle auto save index.
                BattleAutoSaveIndex++;

                if (Settings.ShouldLimitSaves && BattleAutoSaveIndex > Settings.BattleAutoSaveLimit)
                {
                    // Reset the battle auto save index if it is greater than the maximum number in the settings.
                    BattleAutoSaveIndex = 1;
                }

                if (saveName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                {
                    // Remove illegal characters from the save name.
                    saveName = new string(saveName.Where(c => !Path.GetInvalidFileNameChars().Contains(c)).ToArray());

                    // Display a warning message if the save name contains illegal characters.
                    InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=BSLmsg004}Warning: Save name contains illegal characters! Removing illegal characters.").ToString()));
                }

                ActiveSaveSlotName = saveName;

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

            // Display an error message if there are no save files to load.
            InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=BSLmsg003}Error: No save files to load!").ToString()));
        }

        public static void StartGame(LoadResult loadResult)
        {
            SaveFile = null;

            MBGameManager.StartNewGame(new SandBoxGameManager(loadResult));
        }
    }
}
