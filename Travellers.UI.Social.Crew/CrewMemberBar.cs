using Bossa.Travellers.Crew;
using Bossa.Travellers.Social.DataModel;
using Travellers.UI.Framework;
using UnityEngine;

namespace Travellers.UI.Social.Crew;

public class CrewMemberBar : UIScreenComponent
{
	[SerializeField]
	private GameObject _bootMemberButtonContainer;

	[SerializeField]
	private UIButtonController _dynamicActionButton;

	[SerializeField]
	private GameObject _emptySlot;

	[SerializeField]
	private GameObject _occupiedSlot;

	[SerializeField]
	private TextStylerTextMeshPro _playerName;

	[SerializeField]
	private GameObject _playerSlot;

	[SerializeField]
	private GameObject _yourSlot;

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
	}

	protected override void ProtectedDispose()
	{
	}

	public void SetupForCrew(CrewSlot slot, bool isLeader, string yourPlayerId, bool areYouLeader)
	{
		CrewMember slot2 = new CrewMember(slot.playerId, slot.displayName, "Old crew system not the sam format", slot.active, !slot.active, isLeader);
		if (slot.playerId == yourPlayerId)
		{
			SetupForYou();
		}
		else if (isLeader)
		{
			SetupForMemberLeader(slot2);
		}
		else if (!slot.active)
		{
			SetupForPendingMemberInvite(slot2, areYouLeader);
		}
		else
		{
			SetupForMember(slot2, areYouLeader);
		}
	}

	public void Setup(CrewMember slot, string myCharacterUid, bool areYouLeader)
	{
		if (slot == null || (!slot.HasActivePlayer && !slot.HasPendingInvite))
		{
			SetupForEmpty();
		}
		else if (slot.HasPendingInvite)
		{
			SetupForPendingMemberInvite(slot, areYouLeader);
		}
		else if (slot.CharacterId == myCharacterUid)
		{
			SetupForYou();
		}
		else if (slot.IsLeader)
		{
			SetupForMemberLeader(slot);
		}
		else
		{
			SetupForMember(slot, areYouLeader);
		}
	}

	public void SetupForEmpty()
	{
		_occupiedSlot.SetActive(value: false);
		_emptySlot.SetActive(value: true);
	}

	private void SetupForYou()
	{
		_emptySlot.SetActive(value: false);
		_occupiedSlot.SetActive(value: true);
		_playerSlot.SetActive(value: false);
		_yourSlot.SetActive(value: true);
	}

	private void SetupForPendingMemberInvite(CrewMember slot, bool areYouLeader)
	{
		_emptySlot.SetActive(value: false);
		_occupiedSlot.SetActive(value: true);
		_playerSlot.SetActive(value: true);
		_yourSlot.SetActive(value: false);
		_playerName.SetText("PENDING...  " + slot.DisplayName);
		_bootMemberButtonContainer.SetActive(areYouLeader);
		_dynamicActionButton.SetText("CANCEL");
		_dynamicActionButton.SetButtonEvent(WAUICrewEvents.RequestRescindInvite, new RescindCrewInviteEvent(slot));
	}

	private void SetupForMember(CrewMember slot, bool areYouLeader)
	{
		_emptySlot.SetActive(value: false);
		_occupiedSlot.SetActive(value: true);
		_playerSlot.SetActive(value: true);
		_yourSlot.SetActive(value: false);
		_playerName.SetText(slot.DisplayName);
		_bootMemberButtonContainer.SetActive(areYouLeader);
		_dynamicActionButton.SetText("BOOT");
		_dynamicActionButton.SetButtonEvent(WAUICrewEvents.RequestBootCrewMember, new RequestCrewMemberBootEvent(slot));
	}

	private void SetupForMemberLeader(CrewMember slot)
	{
		_emptySlot.SetActive(value: false);
		_occupiedSlot.SetActive(value: true);
		_playerSlot.SetActive(value: true);
		_yourSlot.SetActive(value: false);
		_playerName.SetText(slot.DisplayName);
		_bootMemberButtonContainer.SetActive(value: false);
	}
}
