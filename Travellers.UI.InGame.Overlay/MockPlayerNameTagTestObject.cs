using System;
using Travellers.UI.Utility;
using UnityEngine;

namespace Travellers.UI.InGame.Overlay;

public class MockPlayerNameTagTestObject : MonoBehaviour
{
	private static string[] _names = new string[11]
	{
		"Brian", "Legsy", "Old Smooty", "Negative Dave", "Hardnuts", "Liam Jumpers", "Small Brian", "Tiny Brian", "Absolutely miniscule Brian", "Holy Baby Jesus",
		"Ten tons of Craig"
	};

	private static string[] _allianceNames = new string[11]
	{
		"The Clams", "Eggsy's Bacon Gang", "Punchy and the Blooty boys", "Herb and the Producers", "Luke and his Box", "Smelly and the Dirty Fingers", "Old man Murillo and the Brazilian strippers", "Maurizio and the Pizza Haters", "Oi!", "All our brave, brave boys",
		"Gals! Gals! Gals!"
	};

	private string _playerName;

	private string _allianceName;

	private string _allianceUId;

	private string _crewUId;

	private string _playerId;

	private void Awake()
	{
		_playerName = RandomHelper.Random(_names);
		_allianceName = ((!((double)UnityEngine.Random.value > 0.3)) ? string.Empty : RandomHelper.Random(_allianceNames));
		_allianceUId = ((!string.IsNullOrEmpty(_allianceName)) ? UnityEngine.Random.Range(0, 10).ToString() : string.Empty);
		_crewUId = ((!((double)UnityEngine.Random.value > 0.3)) ? string.Empty : UnityEngine.Random.Range(0, 10).ToString());
		_playerId = Guid.NewGuid().ToString();
		UpdatePlayerInNameSystem();
	}

	private void OnEnable()
	{
		UpdatePlayerInNameSystem();
	}

	private void OnDisable()
	{
		RemovePlayerInNameSystem();
	}

	private void RemovePlayerInNameSystem()
	{
		PlayerNameInfo playerNameInfo = new PlayerNameInfo(_playerName, _playerId, _playerId, base.transform, _allianceName, _allianceUId, _crewUId, isLookingAt: true, 2f);
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIPlayerProfileEvents.RemovePlayerLabel, new PlayerLabelUpdatedEvent(playerNameInfo));
	}

	private void UpdatePlayerInNameSystem()
	{
		PlayerNameInfo playerNameInfo = new PlayerNameInfo(_playerName, _playerId, _playerId, base.transform, _allianceName, _allianceUId, _crewUId, isLookingAt: true, 2f);
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIPlayerProfileEvents.AddPlayerLabel, new PlayerLabelUpdatedEvent(playerNameInfo));
	}
}
