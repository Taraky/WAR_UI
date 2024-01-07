using System.Collections.Generic;
using Travellers.UI.Framework;
using WAUtilities.Logging;

namespace Travellers.UI.InGame.Overlay;

[InjectedSystem(InjectionType.Mock)]
public class MockPlayerNameSystem : UISystem, IPlayerNameSystem
{
	private List<PlayerNameInfo> _playerInfoList;

	public PlayerNameInfo YourPlayerNameInfo { get; private set; }

	public List<PlayerNameInfo> PlayerInfoList => _playerInfoList;

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUIPlayerProfileEvents.AddPlayerLabel, OnPlayerLabelUpdated);
		_eventList.AddEvent(WAUIPlayerProfileEvents.RemovePlayerLabel, OnPlayerLabelRemoved);
		_eventList.AddEvent(WAUIPlayerProfileEvents.ClearPlayerLabels, OnPlayerLabelsCleared);
	}

	public override void Init()
	{
		_playerInfoList = new List<PlayerNameInfo>();
		YourPlayerNameInfo = new PlayerNameInfo("John", "0", "0", null, "John's alliance", "0", "0", isLookingAt: false, 0f);
	}

	public override void ControlledUpdate()
	{
	}

	protected override void Dispose()
	{
	}

	private void OnPlayerLabelUpdated(object[] obj)
	{
		PlayerLabelUpdatedEvent playerLabelUpdatedEvent = (PlayerLabelUpdatedEvent)obj[0];
		for (int num = _playerInfoList.Count - 1; num >= 0; num--)
		{
			if (_playerInfoList[num].CharacterUid == playerLabelUpdatedEvent.PlayerNameInfo.CharacterUid)
			{
				_playerInfoList[num] = playerLabelUpdatedEvent.PlayerNameInfo;
				ListUpdated();
				return;
			}
		}
		_playerInfoList.Add(playerLabelUpdatedEvent.PlayerNameInfo);
		ListUpdated();
	}

	private void OnPlayerLabelRemoved(object[] obj)
	{
		PlayerLabelUpdatedEvent playerLabelUpdatedEvent = (PlayerLabelUpdatedEvent)obj[0];
		for (int num = _playerInfoList.Count - 1; num >= 0; num--)
		{
			if (_playerInfoList[num].CharacterUid == playerLabelUpdatedEvent.PlayerNameInfo.CharacterUid)
			{
				_playerInfoList.RemoveAt(num);
				ListUpdated();
				return;
			}
		}
		WALogger.Error<PlayerNameSystem>("Attempting to remove a player label that doesn't exist");
	}

	private void OnPlayerLabelsCleared(object[] obj)
	{
		ClearLabels();
	}

	private void ClearLabels()
	{
		_playerInfoList.Clear();
	}

	private void ListUpdated()
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIPlayerProfileEvents.PlayerLabelsListUpdated, null);
	}
}
