using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Travellers.UI.InGame.Overlay;

[Serializable]
public class PlayerNameInfo
{
	public readonly string Name;

	public readonly string PlayerId;

	public readonly string CharacterUid;

	[JsonIgnore]
	public readonly Transform PlayerTransform;

	public readonly string AllianceName;

	public readonly string AllianceUid;

	public readonly string CrewUId;

	public readonly bool IsLookingAt;

	public float LastLookedAt;

	public bool IsInPlayersAlliance;

	public bool IsInPlayercCrew;

	public bool? IsTalking { get; private set; }

	public PlayerNameInfo(string name, string playerId, string characterUid, Transform playerTransform, string allianceName, string allianceUid, string crewUid, bool isLookingAt, float lastLookedAt, bool? isTalking = null)
	{
		Name = name;
		PlayerId = playerId;
		CharacterUid = characterUid;
		AllianceUid = allianceUid;
		CrewUId = crewUid;
		PlayerTransform = playerTransform;
		AllianceName = allianceName;
		IsLookingAt = isLookingAt;
		LastLookedAt = lastLookedAt;
		IsTalking = isTalking;
	}

	public PlayerNameInfo SetIsTalking(bool isTalking)
	{
		IsTalking = isTalking;
		return this;
	}

	public static void CopyFromTo(PlayerNameInfo from, ref PlayerNameInfo to)
	{
		bool? isTalking = to.IsTalking;
		to = from;
		if (!to.IsTalking.HasValue)
		{
			to.IsTalking = isTalking;
		}
	}
}
