using System;
using System.Collections.Generic;
using Bossa.Travellers.Social.DataModel;
using Travellers.UI.Utility;

namespace Travellers.UI.Social.Crew;

public class MockCrewData
{
	private static readonly string[] _crewNames = new string[11]
	{
		"Brian", "Legsy", "Old Smooty", "Negative Dave", "Hardnuts", "Liam Jumpers", "Small Brian", "Tiny Brian", "Absolutely miniscule Brian", "Holy Baby Jesus",
		"Ten tons of Craig"
	};

	public static List<CrewMember> GenerateNewCrew()
	{
		return new List<CrewMember>();
	}

	public static MembershipChangeRequest GenerateNewInvite(string playerId)
	{
		return MembershipChangeRequest.CreateEmpty("d0712fcc-c8b8-4dc9-84ba-c8f3b4759d96", RandomHelper.Random(_crewNames), SocialGroupType.Crew, MembershipStatusChangeRequestType.Invite);
	}

	public static CrewMember GenerateLeaderCrewSlotData(string displayName, string uuid)
	{
		CrewMember crewMember = new CrewMember();
		crewMember.CharacterId = uuid;
		crewMember.PlayerName = displayName;
		crewMember.HasActivePlayer = true;
		crewMember.HasPendingInvite = false;
		crewMember.InviteId = Guid.NewGuid().ToString();
		crewMember.IsLeader = true;
		return crewMember;
	}

	public static CrewMember GenerateCrewSlotData(string playerId, string playerName)
	{
		CrewMember crewMember = new CrewMember();
		crewMember.CharacterId = playerId;
		crewMember.PlayerName = playerName;
		crewMember.HasActivePlayer = true;
		crewMember.HasPendingInvite = false;
		crewMember.InviteId = Guid.NewGuid().ToString();
		crewMember.IsLeader = false;
		return crewMember;
	}

	public static CrewMember GenerateInvitedCrewSlotData()
	{
		CrewMember crewMember = new CrewMember();
		crewMember.CharacterId = Guid.NewGuid().ToString();
		crewMember.PlayerName = RandomHelper.Random(_crewNames);
		crewMember.HasActivePlayer = false;
		crewMember.HasPendingInvite = true;
		crewMember.InviteId = Guid.NewGuid().ToString();
		crewMember.IsLeader = false;
		return crewMember;
	}
}
