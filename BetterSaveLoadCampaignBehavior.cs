using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;

namespace BetterSaveLoad
{
	public class BetterSaveLoadCampaignBehavior : CampaignBehaviorBase
	{
		public override void RegisterEvents()
		{
			CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnNewGameCreated));
			CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnGameLoaded));
			CampaignEvents.MapEventStarted.AddNonSerializedListener(this, new Action<MapEvent, PartyBase, PartyBase>(OnMapEventStarted));
			CampaignEvents.MapEventEnded.AddNonSerializedListener(this, new Action<MapEvent>(OnMapEventEnded));
		}

		public override void SyncData(IDataStore dataStore) { }

		private void OnNewGameCreated(CampaignGameStarter campaignGameStarter) => BetterSaveLoadManager.InitializeSaveIndexes();

		private void OnGameLoaded(CampaignGameStarter campaignGameStarter) => BetterSaveLoadManager.InitializeSaveIndexes();

		private void OnMapEventStarted(MapEvent mapEvent, PartyBase attackerParty, PartyBase defenderParty)
		{
			if (mapEvent.IsPlayerMapEvent && ((attackerParty.MapFaction != null && attackerParty.MapFaction.IsAtWarWith(PartyBase.MainParty.MapFaction)) || (defenderParty.MapFaction != null && defenderParty.MapFaction.IsAtWarWith(PartyBase.MainParty.MapFaction))))
			{
				// Auto save when the player enters a battle.
				BetterSaveLoadManager.AutoSaveForBattle(mapEvent);
			}
		}

		private void OnMapEventEnded(MapEvent mapEvent)
		{
			if (mapEvent.IsPlayerMapEvent)
			{
				// Auto save when the player leaves a battle.
				BetterSaveLoadManager.AutoSaveForBattle(mapEvent);
			}
		}
	}
}
