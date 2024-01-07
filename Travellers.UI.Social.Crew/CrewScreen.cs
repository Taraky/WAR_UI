using System;
using System.Collections.Generic;
using Bossa.Travellers.Social;
using Bossa.Travellers.Social.DataModel;
using Bossa.Travellers.World;
using TMPro;
using Travellers.Crew;
using Travellers.UI.Framework;
using Travellers.UI.HUDMessaging;
using Travellers.UI.InfoPopups;
using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.Social.Crew;

[InjectableClass]
public class CrewScreen : UIScreenComponent
{
	public static readonly int MaxBeaconTimeInSeconds = 86400;

	public static readonly int MaxCrewSlots = 5;

	[SerializeField]
	private Image _beaconProgress;

	[SerializeField]
	private TextMeshProUGUI _beaconProgressTimer;

	[SerializeField]
	private UIButtonController _createCrewButton;

	private ICrewClient _crewClient;

	private CrewMemberBar _crewLeader;

	[SerializeField]
	private RectTransform _crewLeaderParent;

	private List<CrewMemberBar> _crewUIObjects;

	private float _currentCooldownTime;

	private CrewState _currentCrewState;

	[SerializeField]
	private UIButtonController _disbandCrewButton;

	[SerializeField]
	private GameObject _hasCrewLeftPane;

	[SerializeField]
	private GameObject _hasCrewRightPane;

	[SerializeField]
	private GameObject _invitePanel;

	[SerializeField]
	private GameObject _beaconPanel;

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

	private ScreenFields _screenFields;

	[InjectableMethod]
	public void InjectDependencies(ICrewClient crewClient)
	{
		_crewClient = crewClient;
	}

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUICrewEvents.CrewStateRefresh, OnCrewMembersUpdated);
		_eventList.AddEvent(WAUICrewEvents.RequestBootCrewMember, OnRequestBootCrewMember);
		_eventList.AddEvent(WAUICrewEvents.RequestRescindInvite, OnRequestRescindInvite);
	}

	protected override void ProtectedInit()
	{
		_screenFields = new ScreenFields(_useBeaconButton, _beaconProgress, _beaconProgressTimer, _hasCrewLeftPane, _hasCrewRightPane, _invitePanel, _beaconPanel, _noCrewObject);
		SwitchState(new NoCrewState(_screenFields));
		_leaveCrewButton.SetButtonEvent(OnLeaveCrewPressed);
		_disbandCrewButton.SetButtonEvent(OnDisbandCrewPressed);
		_createCrewButton.SetButtonEvent(OnCreateCrewPressed);
		_useBeaconButton.SetButtonEvent(OnUseCrewBeaconPressed);
		_sendInviteButton.SetButtonEvent(OnSendInvitePressed);
		_crewClient.InvalidateCache();
		_useBeaconButton.SetObjectActive(!LocalPlayer.Instance.NewPlayerVisualiser.IsNew);
	}

	protected override void ProtectedDispose()
	{
	}

	private void Update()
	{
		if (_currentCooldownTime >= 0f)
		{
			_currentCooldownTime -= Time.deltaTime;
			_currentCrewState.UpdateBeaconDisplay((int)_currentCooldownTime, MaxBeaconTimeInSeconds);
		}
	}

	private void OnCrewMembersUpdated(object[] obj)
	{
		CheckState();
	}

	protected override void Activate()
	{
		_currentCrewState.EnterState();
	}

	protected override void Deactivate()
	{
	}

	public void CheckCachedCrewData()
	{
		CheckInvites();
		CheckState();
	}

	private void CheckInvites()
	{
		_crewClient.GetInvite().Then((Action<MembershipChangeRequest>)ShowInvite).Catch(delegate(Exception exc)
		{
			OnFailure(exc);
		});
	}

	private void ShowInvite(MembershipChangeRequest invitedBy)
	{
		if (invitedBy != null)
		{
			DialogPopupFacade.ShowConfirmationDialogAndOverrideSounds("Crew invite", $"{invitedBy.InviterName} has invited you to join their crew.", delegate
			{
				OnAcceptInvitePressed(invitedBy);
			}, delegate
			{
				OnRejectInvitePressed(invitedBy);
			}, "ACCEPT", "REJECT");
		}
	}

	private void CheckState()
	{
		_crewClient.GetCrewMembers().Then(delegate(List<CrewMember> slots)
		{
			RefreshCrewView(slots, SocialHelper.MyCharacterUid);
		}).Catch(delegate(Exception exc)
		{
			OnFailure(exc);
		});
	}

	private void RefreshCrewView(List<CrewMember> crewSlots, string myCharacterUid)
	{
		RefreshCrewMemberUIObjects();
		bool areYouLeader = UpdateState(crewSlots, myCharacterUid);
		if (_currentCrewState.ShouldRefreshSlots)
		{
			RefreshCrewSlots(crewSlots, myCharacterUid, areYouLeader);
		}
		if (_currentCrewState.ShouldRefreshBeacon)
		{
			RefreshBeaconCooldown();
		}
	}

	private bool UpdateState(List<CrewMember> crewSlots, string myCharacterUid)
	{
		bool flag = true;
		bool flag2 = false;
		for (int i = 0; i < crewSlots.Count; i++)
		{
			if (crewSlots[i].HasActivePlayer || crewSlots[i].HasPendingInvite)
			{
				flag = false;
			}
			if (crewSlots[i].IsLeader && crewSlots[i].CharacterId == myCharacterUid)
			{
				flag2 = true;
			}
		}
		if (flag2)
		{
			SwitchState(new YouAsLeaderState(_screenFields));
		}
		else if (flag)
		{
			SwitchState(new NoCrewState(_screenFields));
		}
		else
		{
			SwitchState(new YouAsMemberState(_screenFields));
		}
		return flag2;
	}

	private void RefreshCrewSlots(List<CrewMember> crewSlots, string myCharacterUid, bool areYouLeader)
	{
		if (crewSlots == null)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < crewSlots.Count; i++)
		{
			if (crewSlots[i].IsLeader)
			{
				_crewLeader.Setup(crewSlots[i], myCharacterUid, areYouLeader);
				continue;
			}
			_crewUIObjects[num].Setup(crewSlots[i], myCharacterUid, areYouLeader);
			num++;
		}
		for (int j = crewSlots.Count - 1; j < MaxCrewSlots; j++)
		{
			_crewUIObjects[j].Setup(null, string.Empty, areYouLeader);
		}
		_sendInviteButton.SetButtonEnabled(crewSlots.Count <= MaxCrewSlots);
	}

	private void RefreshBeaconCooldown()
	{
		_crewClient.GetBeaconCooldown().Then(delegate(long timeLeft)
		{
			SwitchState(new YouAsMemberState(_screenFields));
			SetServerBeaconCoolDown(timeLeft);
		}).Catch(delegate(Exception exc)
		{
			OnFailure(exc);
		});
	}

	private void RefreshCrewMemberUIObjects()
	{
		if (_crewLeader == null)
		{
			_crewLeader = CreateCrewMemberUIObject(_crewLeaderParent);
		}
		if (_crewUIObjects == null || _crewUIObjects.Count == 0)
		{
			_crewUIObjects = new List<CrewMemberBar>();
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

	private void SwitchState(CrewState newState)
	{
		if (_currentCrewState != null)
		{
			_currentCrewState.LeaveState();
		}
		newState.EnterState();
		_currentCrewState = newState;
	}

	private void SetServerBeaconCoolDown(long time)
	{
		_currentCooldownTime = time;
		_currentCrewState.UpdateBeaconDisplay((int)_currentCooldownTime, MaxBeaconTimeInSeconds);
	}

	private void OnCreateCrewPressed()
	{
		_crewClient.CreateCrew().Then(delegate
		{
			OnCrewCreated();
		}).Catch(delegate(Exception exc)
		{
			OnFailure(exc);
		});
	}

	private void OnCrewCreated()
	{
		SocialCharacterSheet.TriggerCrewDataRefresh();
	}

	private void OnSendInvitePressed()
	{
		if (!string.IsNullOrEmpty(_searchInputField.text))
		{
			string playerName = _searchInputField.text.Trim();
			_crewClient.InvitePlayerByName(playerName).Then(delegate(MembershipChangeRequest changeRequest)
			{
				OnSearchResult(changeRequest);
			}).Catch(OnFailure);
		}
	}

	private void OnSearchResult(MembershipChangeRequest membershipChangeRequest)
	{
		_searchInputField.text = string.Empty;
		OSDMessage.SendMessage($"Invite sent to {membershipChangeRequest.CharacterName}", MessageType.Crew);
		SocialCharacterSheet.TriggerCrewDataRefresh();
	}

	private void OnAcceptInvitePressed(MembershipChangeRequest membershipChangeRequest)
	{
		_crewClient.AcceptInvite(membershipChangeRequest).Then(delegate(bool wasSuccess)
		{
			OnInviteAccepted(wasSuccess);
		}).Catch(delegate(Exception exc)
		{
			OnFailure(exc);
		});
	}

	private void OnInviteAccepted(bool wasAccepted)
	{
		SocialCharacterSheet.TriggerCrewDataRefresh();
	}

	private void OnRejectInvitePressed(MembershipChangeRequest membershipChangeRequest)
	{
		_crewClient.RejectInvite(membershipChangeRequest).Then(delegate
		{
			SocialCharacterSheet.TriggerCrewDataRefresh();
		});
	}

	private void OnRequestBootCrewMember(object[] obj)
	{
		RequestCrewMemberBootEvent requestCrewMemberBootEvent = (RequestCrewMemberBootEvent)obj[0];
		BootPlayerRequest(requestCrewMemberBootEvent.CrewSlot);
	}

	private void BootPlayerRequest(CrewMember member)
	{
		DialogPopupFacade.ShowConfirmationDialog("Remove player", $"Do you want to boot {member.DisplayName} from your crew?", delegate
		{
			BootPlayer(member);
		}, "YES", "NO");
	}

	private void BootPlayer(CrewMember member)
	{
		_crewClient.BootPlayer(member.CharacterId).Then(delegate(bool wasSuccess)
		{
			OnPlayerBooted(wasSuccess);
		}).Catch(delegate(Exception exc)
		{
			OnFailure(exc);
		});
	}

	private void OnPlayerBooted(bool wasSuccess)
	{
		SocialCharacterSheet.TriggerCrewDataRefresh();
	}

	private void OnRequestRescindInvite(object[] obj)
	{
		RescindCrewInviteEvent rescindCrewInviteEvent = (RescindCrewInviteEvent)obj[0];
		RescindInviteRequested(rescindCrewInviteEvent.CrewSlot);
	}

	private void RescindInviteRequested(CrewMember member)
	{
		DialogPopupFacade.ShowConfirmationDialog("Cancel invite", $"Do you want cancel the invite sent to {member.DisplayName} to join your crew?", delegate
		{
			RescindInvite(member);
		}, "YES", "NO");
	}

	private void RescindInvite(CrewMember member)
	{
		_crewClient.CancelInvite(member.InviteId, member.CharacterId).Then(delegate(bool wasSuccess)
		{
			OnInviteRescinded(wasSuccess);
		}).Catch(delegate(Exception exc)
		{
			OnFailure(exc);
		});
	}

	private void OnInviteRescinded(bool wasSuccess)
	{
		SocialCharacterSheet.TriggerCrewDataRefresh();
	}

	private void OnUseCrewBeaconPressed()
	{
		DialogPopupFacade.ShowConfirmationDialog("Activate crew beacon", "Warning: Activating your crew beacon will unregister you from your reviver and drop your entire inventory, including items in your belt!\nIt will not affect your Stash.\n\nDo you want to activate your crew beacon ?", UseCrewBeacon, "YES", "NO");
	}

	private void UseCrewBeacon()
	{
		_crewClient.UseCrewBeacon();
	}

	private void OnLeaveCrewPressed()
	{
		DialogPopupFacade.ShowConfirmationDialog("Leave crew", "Do you want to leave the crew?", DoLeaveCrew, "YES", "NO");
	}

	private void DoLeaveCrew()
	{
		_crewClient.LeaveCrew().Then(delegate
		{
			SocialCharacterSheet.TriggerCrewDataRefresh();
		}).Catch(delegate(Exception exc)
		{
			OnFailure(exc);
		});
	}

	public void OnDisbandCrewPressed()
	{
		DialogPopupFacade.ShowConfirmationDialog("Disband crew", "Do you want to disband the crew?", DoDisbandCrew, "YES", "NO");
	}

	private void DoDisbandCrew()
	{
		_crewClient.DisbandCrew().Then(delegate
		{
			SocialCharacterSheet.TriggerCrewDataRefresh();
		}).Catch(delegate(Exception exc)
		{
			OnFailure(exc);
		});
	}

	private void OnFailure(Exception obj)
	{
		SocialCharacterSheet.TriggerAllianceExceptionHandler(obj, SocialCharacterSheet.TriggerCloseSocialScreen, CheckState);
	}
}
