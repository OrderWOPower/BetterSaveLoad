using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;
using MCM.Common;
using TaleWorlds.Localization;

namespace BetterSaveLoad
{
	public class BetterSaveLoadSettings : AttributeGlobalSettings<BetterSaveLoadSettings>
	{
		public override string Id => "BetterSaveLoad";

		public override string DisplayName => "Better Save/Load";

		public override string FolderName => "BetterSaveLoad";

		public override string FormatType => "json2";

		[SettingPropertyBool("{=BSLopt001}Toggle Save Limits", Order = 0, RequireRestart = false, HintText = "{=BSLopt001Hint}Limit number of quick saves and battle auto saves. Enabled by default.", IsToggle = true)]
		[SettingPropertyGroup("{=BSLoptg001}Save Limits", GroupOrder = 0)]
		public bool ShouldLimitSaves { get; set; } = true;

		[SettingPropertyInteger("{=BSLopt002}Quick Save Limit", 1, 100, "0", Order = 1, RequireRestart = false, HintText = "{=BSLopt002Hint}Maximum number of quick saves. Default is 3.")]
		[SettingPropertyGroup("{=BSLoptg001}Save Limits", GroupOrder = 0)]
		public int QuickSaveLimit { get; set; } = 3;

		[SettingPropertyInteger("{=BSLopt003}Battle Auto Save Limit", 1, 100, "0", Order = 2, RequireRestart = false, HintText = "{=BSLopt003Hint}Maximum number of battle auto saves. Default is 3.")]
		[SettingPropertyGroup("{=BSLoptg001}Save Limits", GroupOrder = 0)]
		public int BattleAutoSaveLimit { get; set; } = 3;

		[SettingPropertyBool("{=BSLopt004}Toggle Battle Auto Save", Order = 0, RequireRestart = false, HintText = "{=BSLopt004Hint}Auto save for battles. Enabled by default.", IsToggle = true)]
		[SettingPropertyGroup("{=BSLoptg002}Battle Auto Save", GroupOrder = 1)]
		public bool ShouldAutoSaveForBattle { get; set; } = true;

		[SettingPropertyDropdown("{=BSLopt005}Trigger", Order = 1, RequireRestart = false, HintText = "{=BSLopt005Hint}When to trigger battle auto saves. Default is Pre-battle.")]
		[SettingPropertyGroup("{=BSLoptg002}Battle Auto Save", GroupOrder = 1)]
		public Dropdown<TextObject> BattleAutoSaveTrigger { get; set; } = new Dropdown<TextObject>(new TextObject[] { new TextObject("{=BSLoptv001}Pre-battle"), new TextObject("{=BSLoptv002}Post-battle"), new TextObject("{=BSLoptv003}Both") }, 0);

		[SettingPropertyInteger("{=BSLopt006}Minimum Attacker Troop Count", 1, 1000, "0", Order = 2, RequireRestart = false, HintText = "{=BSLopt006Hint}Minimum number of attacking troops to trigger auto saving before and after battles. Default is 50.")]
		[SettingPropertyGroup("{=BSLoptg002}Battle Auto Save", GroupOrder = 1)]
		public int MinAttackerTroopCount { get; set; } = 50;

		[SettingPropertyInteger("{=BSLopt007}Minimum Defender Troop Count", 1, 1000, "0", Order = 3, RequireRestart = false, HintText = "{=BSLopt007Hint}Minimum number of defending troops to trigger auto saving before and after battles. Default is 50.")]
		[SettingPropertyGroup("{=BSLoptg002}Battle Auto Save", GroupOrder = 1)]
		public int MinDefenderTroopCount { get; set; } = 50;
	}
}
