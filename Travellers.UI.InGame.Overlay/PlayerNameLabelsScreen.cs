using System.Collections.Generic;
using Travellers.UI.Framework;
using UnityEngine;

namespace Travellers.UI.InGame.Overlay;

[InjectableClass]
public class PlayerNameLabelsScreen : UIScreen
{
	[SerializeField]
	private float _verticalOffset = 1f;

	private IPlayerNameSystem _playerNameSystem;

	private Stack<PlayerLabel> _inactivePlayerLabels = new Stack<PlayerLabel>();

	private List<PlayerLabel> _activePlayerLabels = new List<PlayerLabel>();

	[InjectableMethod]
	public void InjectDependencies(IPlayerNameSystem playerNameSystem)
	{
		_playerNameSystem = playerNameSystem;
	}

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUIPlayerProfileEvents.PlayerLabelsListUpdated, OnPlayerLabelsListUpdated);
	}

	protected override void ProtectedInit()
	{
	}

	protected override void ProtectedDispose()
	{
	}

	private void OnPlayerLabelsListUpdated(object[] obj)
	{
		BuildPlayerLabels();
	}

	private void BuildPlayerLabels()
	{
		for (int i = 0; i < _playerNameSystem.PlayerInfoList.Count; i++)
		{
			if (i >= _activePlayerLabels.Count)
			{
				_activePlayerLabels.Add(GetNewOrUnusedLabel());
			}
			_activePlayerLabels[i].SetData(_playerNameSystem.PlayerInfoList[i], _playerNameSystem.YourPlayerNameInfo.CrewUId, _playerNameSystem.YourPlayerNameInfo.AllianceUid);
		}
		while (_activePlayerLabels.Count > _playerNameSystem.PlayerInfoList.Count)
		{
			int index = _activePlayerLabels.Count - 1;
			_activePlayerLabels[index].SetObjectActive(isActive: false);
			_inactivePlayerLabels.Push(_activePlayerLabels[index]);
			_activePlayerLabels.RemoveAt(index);
		}
	}

	private PlayerLabel GetNewOrUnusedLabel()
	{
		if (_inactivePlayerLabels.Count > 0)
		{
			PlayerLabel playerLabel = _inactivePlayerLabels.Pop();
			playerLabel.SetObjectActive(isActive: true);
			return playerLabel;
		}
		return UIObjectFactory.Create<PlayerLabel>(UIElementType.ScreenComponent, UIFillType.KeepOriginalAnchoring, base.transform, isObjectActive: true);
	}

	private void LateUpdate()
	{
		for (int i = 0; i < _activePlayerLabels.Count; i++)
		{
			bool flag = _activePlayerLabels[i].PlayerNameInfo.IsTalking.HasValue && _activePlayerLabels[i].PlayerNameInfo.IsTalking.Value;
			if (_activePlayerLabels[i].PlayerNameInfo.IsLookingAt || _activePlayerLabels[i].IsInPlayerAlliance || _activePlayerLabels[i].IsInPlayerCrew || flag)
			{
				_activePlayerLabels[i].SetObjectActive(isActive: true);
				_activePlayerLabels[i].UpdateLabel(_verticalOffset);
			}
			else if (_activePlayerLabels[i].PlayerNameInfo.LastLookedAt > 0f)
			{
				_activePlayerLabels[i].PlayerNameInfo.LastLookedAt -= Time.deltaTime;
				_activePlayerLabels[i].SetObjectActive(isActive: true);
				_activePlayerLabels[i].UpdateLabel(_verticalOffset);
			}
			else
			{
				_activePlayerLabels[i].SetObjectActive(isActive: false);
			}
		}
	}
}
