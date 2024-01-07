using System.Collections.Generic;
using Bossa.Travellers.Crew;
using Bossa.Travellers.Social.DataModel;
using Improbable.Collections;
using TMPro;
using Travellers.Crew;
using Travellers.UI.Framework;
using Travellers.UI.InfoPopups;
using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.Social.Crew;

public class OldCrewScreen : UIScreenComponent
{
	public enum CrewUIState
	{
		NoCrew,
		YouAsLeader,
		YouAsMember
	}

	public struct CrewStateDefiningValues
	{
		public bool IsLeader;

		public bool IsMember;

		public CrewStateDefiningValues(bool isLeader, bool isMember)
		{
			IsLeader = isLeader;
			IsMember = isMember;
		}
	}

	public static readonly int MaxCrewSlots = 5;

	[SerializeField]
	private GameObject _beaconPanel;

	[SerializeField]
	private Image _beaconProgress;

	[SerializeField]
	private TextStylerTextMeshPro _beaconProgressTimer;

	[SerializeField]
	private UIButtonController _createCrewButton;

	[SerializeField]
	private UIButtonController _disbandCrewButton;

	[SerializeField]
	private GameObject _hasCrewLeftPane;

	[SerializeField]
	private GameObject _hasCrewRightPane;

	[SerializeField]
	private GameObject _invitePanel;

	[SerializeField]
	private UIButtonController _leaveCrewButton;

	[SerializeField]
	private RectTransform _membersParent;

	[SerializeField]
	private GameObject _noCrewObject;

	[SerializeField]
	private TMP_InputField _searchInputField;

	[SerializeField]
	private UIButtonController _sendInviteButton;

	[SerializeField]
	private UIButtonController _useBeaconButton;

	[SerializeField]
	private RectTransform _crewLeaderParent;

	private ICrewClient _crewClient;

	private CrewMemberBar _crewLeader;

	private System.Collections.Generic.List<CrewMemberBar> _crewUIObjects;

	private float _currentCooldownTime;

	private BeaconHandler _beaconHandler;

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUICrewEvents.RequestBootCrewMember, OnRequestBootCrewMember);
		_eventList.AddEvent(WAUICrewEvents.RequestRescindInvite, OnRequestRescindInvite);
	}

	protected override void ProtectedInit()
	{
		RefreshCrewMemberUIObjects();
		_disbandCrewButton.SetButtonEvent(DisbandCrew);
		_leaveCrewButton.SetButtonEvent(LeaveCrew);
		_sendInviteButton.SetButtonEvent(InvitePlayerByName);
		_useBeaconButton.SetButtonEvent(UseCrewBeacon);
		_createCrewButton.SetButtonEvent(CreateCrew);
		_beaconHandler = new BeaconHandler(_beaconProgressTimer, _useBeaconButton, _beaconProgress);
	}

	protected override void ProtectedDispose()
	{
	}

	private void OnRequestBootCrewMember(object[] obj)
	{
		RequestCrewMemberBootEvent requestCrewMemberBootEvent = (RequestCrewMemberBootEvent)obj[0];
		RequestBootPlayer(requestCrewMemberBootEvent.CrewSlot);
	}

	private void OnRequestRescindInvite(object[] obj)
	{
		RescindCrewInviteEvent rescindCrewInviteEvent = (RescindCrewInviteEvent)obj[0];
		RequestCancelInvite(rescindCrewInviteEvent.CrewSlot);
	}

	protected override void Activate()
	{
		LocalPlayer.Instance.crewManagementVisualiser.OnCrewMembersUpdated += OnCrewMembersUpdated;
		LocalPlayer.Instance.crewManagementVisualiser.OnFeedbackReceived += OnFeedbackReceived;
		LocalPlayer.Instance.crewManagementVisualiser.OnBeaconCoolDownUpdated += OnBeaconCoolDownUpdated;
		LocalPlayer.Instance.crewManagementVisualiser.OnSearchResult += OnSearchPlayerResult;
		OnBeaconCoolDownUpdated(LocalPlayer.Instance.crewManagementVisualiser.GetBeaconCoolDown());
		RefreshState();
	}

	protected override void Deactivate()
	{
		if (LocalPlayer.Instance != null)
		{
			LocalPlayer.Instance.crewManagementVisualiser.OnCrewMembersUpdated -= OnCrewMembersUpdated;
			LocalPlayer.Instance.crewManagementVisualiser.OnFeedbackReceived -= OnFeedbackReceived;
			LocalPlayer.Instance.crewManagementVisualiser.OnBeaconCoolDownUpdated -= OnBeaconCoolDownUpdated;
			LocalPlayer.Instance.crewManagementVisualiser.OnSearchResult -= OnSearchPlayerResult;
		}
	}

	private void RefreshCrewMemberUIObjects()
	{
		if (_crewLeader == null)
		{
			_crewLeader = CreateCrewMemberUIObject(_crewLeaderParent);
		}
		if (_crewUIObjects == null || _crewUIObjects.Count == 0)
		{
			_crewUIObjects = new System.Collections.Generic.List<CrewMemberBar>();
			for (int i = 0; i < MaxCrewSlots; i++)
			{
				CrewMemberBar item = CreateCrewMemberUIObject(_membersParent);
				_crewUIObjects.Add(item);
			}
		}
	}

	private CrewMemberBar CreateCrewMemberUIObject(RectTransform parent)
	{
		return UIObjectFactory.Create<CrewMemberBar>(UIElementType.ScreenComponent, UIFillType.FillParentTransform, parent, isObjectActive: true);
	}

	public void CreateCrew()
	{
		_noCrewObject.SetActive(value: false);
		_hasCrewLeftPane.SetActive(value: true);
		_hasCrewRightPane.SetActive(value: true);
	}

	public void InvitePlayerByName()
	{
		if (!string.IsNullOrEmpty(_searchInputField.text))
		{
			LocalPlayer.Instance.crewManagementVisualiser.InvitePlayerByName(_searchInputField.text);
		}
	}

	public void AcceptInvite()
	{
		LocalPlayer.Instance.crewManagementVisualiser.AcceptInvite();
	}

	public void RejectInvite()
	{
		LocalPlayer.Instance.crewManagementVisualiser.RejectInvite();
	}

	public void UseCrewBeacon()
	{
		DialogPopupFacade.ShowConfirmationDialog("Warning!", "Activating your crew beacon will unregister you from your reviver and drop your entire inventory, including items in your belt!\nIt will not affect your Stash.\n\nDo you want to activate your crew beacon ?", DoUseCrewBeacon, "YES", "NO");
	}

	public void DoUseCrewBeacon()
	{
		LocalPlayer.Instance.crewManagementVisualiser.UseCrewBeacon();
	}

	public void RequestBootPlayer(CrewMember member)
	{
		DialogPopupFacade.ShowConfirmationDialog("Boot player", $"Do you want to boot {member.DisplayName} from your crew?", delegate
		{
			BootPlayerByName(base.name);
		}, "YES", "NO");
	}

	public void BootPlayerByName(string name)
	{
		LocalPlayer.Instance.crewManagementVisualiser.BootPlayerByName(name);
	}

	public void RequestCancelInvite(CrewMember member)
	{
		DialogPopupFacade.ShowConfirmationDialog("Cancel invite", $"Do you want cancel the invite sent to {member.DisplayName} to join your crew?", delegate
		{
			BootPlayerByName(member.DisplayName);
		}, "YES", "NO");
	}

	public void LeaveCrew()
	{
		DialogPopupFacade.ShowConfirmationDialog("Leave crew", "Do you want to leave the crew?", DoLeaveCrew, "YES", "NO");
	}

	public void DoLeaveCrew()
	{
		LocalPlayer.Instance.crewManagementVisualiser.LeaveCrew();
	}

	public void DisbandCrew()
	{
		DialogPopupFacade.ShowConfirmationDialog("Disband crew", "Do you want to disband the crew?", DoDisbandCrew, "YES", "NO");
	}

	public void DoDisbandCrew()
	{
		Improbable.Collections.List<CrewSlot> crewSlots = LocalPlayer.Instance.crewManagementVisualiser.GetCrewSlots();
		for (int num = crewSlots.Count - 1; num >= 1; num--)
		{
			LocalPlayer.Instance.crewManagementVisualiser.BootPlayer(crewSlots[num].playerId);
		}
	}

	private void RefreshState()
	{
		CrewStateDefiningValues crewStateDefiningValues = RefreshCrewMembers();
		if (crewStateDefiningValues.IsLeader)
		{
			ChangeCrewState(CrewUIState.YouAsLeader);
		}
		else if (crewStateDefiningValues.IsMember)
		{
			ChangeCrewState(CrewUIState.YouAsMember);
		}
		else
		{
			ChangeCrewState(CrewUIState.NoCrew);
		}
	}

	private CrewStateDefiningValues RefreshCrewMembers()
	{
		Improbable.Collections.List<CrewSlot> crewSlots = LocalPlayer.Instance.crewManagementVisualiser.GetCrewSlots();
		bool flag = false;
		bool isMember = false;
		if (crewSlots.Count > 0)
		{
			string playerId = LocalPlayer.Instance.crewManagementVisualiser.GetPlayerId();
			flag = crewSlots.Count == 0 || crewSlots[0].playerId == playerId;
			_crewLeader.SetupForCrew(crewSlots[0], flag, playerId, flag);
			for (int i = 0; i < _crewUIObjects.Count; i++)
			{
				int num = i + 1;
				if (num < crewSlots.Count)
				{
					_crewUIObjects[i].SetupForCrew(crewSlots[num], isLeader: false, playerId, flag);
					if (crewSlots[num].playerId == playerId)
					{
						isMember = true;
					}
				}
				else
				{
					_crewUIObjects[i].SetupForEmpty();
				}
			}
		}
		return new CrewStateDefiningValues(flag, isMember);
	}

	private void ChangeCrewState(CrewUIState newCrewState)
	{
		switch (newCrewState)
		{
		case CrewUIState.NoCrew:
			_noCrewObject.SetActive(value: true);
			_hasCrewLeftPane.SetActive(value: false);
			_hasCrewRightPane.SetActive(value: false);
			_invitePanel.SetActive(value: false);
			_beaconPanel.SetActive(value: false);
			_beaconHandler.SetActive(isActive: false);
			break;
		case CrewUIState.YouAsLeader:
			_noCrewObject.SetActive(value: false);
			_hasCrewLeftPane.SetActive(value: true);
			_hasCrewRightPane.SetActive(value: true);
			_invitePanel.SetActive(value: true);
			_beaconPanel.SetActive(value: false);
			_beaconHandler.SetActive(isActive: false);
			break;
		case CrewUIState.YouAsMember:
			_noCrewObject.SetActive(value: false);
			_hasCrewLeftPane.SetActive(value: true);
			_hasCrewRightPane.SetActive(value: true);
			_invitePanel.SetActive(value: false);
			_beaconPanel.SetActive(value: true);
			_beaconHandler.SetActive(isActive: true);
			break;
		}
	}

	private void OnFeedbackReceived(CrewManagementFeedback feedback)
	{
		if (feedback.result)
		{
			RefreshState();
		}
	}

	private void OnCrewMembersUpdated(Improbable.Collections.List<CrewSlot> members)
	{
		RefreshState();
	}

	private void OnBeaconCoolDownUpdated(long time)
	{
		_beaconHandler.SetCountdown(time);
	}

	private void OnSearchPlayerResult(SearchPlayerResult result)
	{
		if (!result.playerFound)
		{
		}
	}

	private void Update()
	{
		_beaconHandler.UpdateDisplay();
	}
}
