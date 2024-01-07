using System.Collections.Generic;
using Travellers.UI.Framework;
using WAUtilities.Logging;

namespace Travellers.UI.InGame.Overlay;

[InjectedSystem(InjectionType.Real)]
public class PlayerNameSystem : UISystem, IPlayerNameSystem
{
	private List<PlayerNameInfo> _playerInfoList;

	private List<PlayerNameInfo> _playersCurrentlyLookingAt;

	public PlayerNameInfo YourPlayerNameInfo { get; private set; }

	public List<PlayerNameInfo> PlayerInfoList => _playerInfoList;

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUIPlayerProfileEvents.AddPlayerLabel, OnPlayerLabelUpdated);
		_eventList.AddEvent(WAUIPlayerProfileEvents.RemovePlayerLabel, OnPlayerLabelRemoved);
		_eventList.AddEvent(WAUIPlayerProfileEvents.ClearPlayerLabels, OnPlayerLabelsCleared);
		_eventList.AddEvent(WAUIPlayerProfileEvents.AddYourPlayerNameInfo, OnAddYourPlayerNameInfo);
	}

	public override void Init()
	{
		_playerInfoList = new List<PlayerNameInfo>();
	}

	public override void ControlledUpdate()
	{
	}

	protected override void Dispose()
	{
		ClearLabels();
	}

	private void OnPlayerLabelUpdated(object[] obj)
	{
		PlayerLabelUpdatedEvent playerLabelUpdatedEvent = (PlayerLabelUpdatedEvent)obj[0];
		for (int num = _playerInfoList.Count - 1; num >= 0; num--)
		{
			if (_playerInfoList[num].CharacterUid == playerLabelUpdatedEvent.PlayerNameInfo.CharacterUid)
			{
				PlayerNameInfo to = _playerInfoList[num];
				PlayerNameInfo.CopyFromTo(playerLabelUpdatedEvent.PlayerNameInfo, ref to);
				_playerInfoList[num] = to;
				ListUpdated();
				return;
			}
			if (YourPlayerNameInfo != null && _playerInfoList[num].CharacterUid == YourPlayerNameInfo.CharacterUid)
			{
				_playerInfoList.RemoveAt(num);
				break;
			}
		}
		if (YourPlayerNameInfo != null && playerLabelUpdatedEvent.PlayerNameInfo.CharacterUid == YourPlayerNameInfo.CharacterUid)
		{
			PlayerNameInfo to2 = YourPlayerNameInfo;
			PlayerNameInfo.CopyFromTo(playerLabelUpdatedEvent.PlayerNameInfo, ref to2);
			YourPlayerNameInfo = to2;
			ListUpdated();
		}
		else
		{
			_playerInfoList.Add(playerLabelUpdatedEvent.PlayerNameInfo);
			ListUpdated();
		}
	}

	private void OnPlayerLabelRemoved(object[] obj)
	{
		PlayerLabelUpdatedEvent playerLabelUpdatedEvent = (PlayerLabelUpdatedEvent)obj[0];
		if (YourPlayerNameInfo.CharacterUid == playerLabelUpdatedEvent.PlayerNameInfo.CharacterUid)
		{
			return;
		}
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

	private void OnAddYourPlayerNameInfo(object[] obj)
	{
		PlayerLabelUpdatedEvent playerLabelUpdatedEvent = (PlayerLabelUpdatedEvent)obj[0];
		YourPlayerNameInfo = playerLabelUpdatedEvent.PlayerNameInfo;
		for (int num = _playerInfoList.Count - 1; num >= 0; num--)
		{
			if (_playerInfoList[num].CharacterUid == YourPlayerNameInfo.CharacterUid)
			{
				_playerInfoList.RemoveAt(num);
			}
		}
		ListUpdated();
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
