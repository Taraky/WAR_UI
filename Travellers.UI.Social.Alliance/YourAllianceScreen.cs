using System;
using Bossa.Travellers.Alliances;
using Bossa.Travellers.Social.DataModel;
using RSG;
using Travellers.UI.Framework;
using Travellers.UI.InfoPopups;
using UnityEngine;

namespace Travellers.UI.Social.Alliance;

[InjectableClass]
public class YourAllianceScreen : UIScreenComponent
{
	[SerializeField]
	private YourAllianceApplicationsPanel _allianceApplicationsPanel;

	private IAllianceClient _allianceClient;

	[SerializeField]
	private YourAllianceCreateAlliancePanel _allianceCreateAlliancePanel;

	[SerializeField]
	private YourAllianceEditRankPanel _allianceEditRankPanel;

	[SerializeField]
	private YourAllianceInvitationsPanel _allianceInvitationsPanel;

	[Header("Right panel")]
	[SerializeField]
	private YourAllianceMembersPanel _allianceMembersPanel;

	[SerializeField]
	private YourAllianceCreateButtons _createAllianceButtons;

	private IAllianceScreenState _currentScreenState;

	[SerializeField]
	private UIButtonController _disbandAllianceButton;

	[SerializeField]
	private YourAllianceManagementButtons _managementButtons;

	[Header("Left panel")]
	[SerializeField]
	private YourAllianceTitleSegment _titleSegment;

	[InjectableMethod]
	public void InjectDependencies(IAllianceClient allianceClient)
	{
		_allianceClient = allianceClient;
	}

	protected override void ProtectedInit()
	{
		_disbandAllianceButton.SetButtonEvent(OnDisbandPressed);
		_titleSegment.Setup();
		_managementButtons.Setup();
		_createAllianceButtons.Setup();
		_disbandAllianceButton.SetObjectActive(isActive: false);
		_allianceInvitationsPanel.Setup();
		_allianceApplicationsPanel.Setup();
		_allianceInvitationsPanel.Setup();
		_allianceEditRankPanel.Setup();
		_allianceCreateAlliancePanel.Setup();
		_allianceMembersPanel.Setup();
	}

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUIAllianceEvents.GotoCreateAlliancePressed, OnGotoCreateAlliancePressed);
		_eventList.AddEvent(WAUIAllianceEvents.PersonalApplicationsButtonPressed, OnPersonalApplicationButtonPressed);
		_eventList.AddEvent(WAUIAllianceEvents.PersonalInvitationsButtonPressed, OnPersonalInvitationButtonsPressed);
		_eventList.AddEvent(WAUIAllianceEvents.MembersPressed, OnMembersButtonPressed);
		_eventList.AddEvent(WAUIAllianceEvents.ApplicationsButtonPressed, OnApplicationButtonPressed);
		_eventList.AddEvent(WAUIAllianceEvents.InvitationsButtonPressed, OnInvitationButtonsPressed);
		_eventList.AddEvent(WAUIAllianceEvents.EditRankPressed, OnEditRankPressed);
		_eventList.AddEvent(WAUIAllianceEvents.AllianceStateRefresh, OnAllianceStateChanged);
	}

	private void OnGotoCreateAlliancePressed(object[] obj)
	{
		ClearInfoPane();
		SwitchState(new NoAllianceCreateAllianceState(_titleSegment, _createAllianceButtons, _allianceCreateAlliancePanel));
	}

	private void OnPersonalInvitationButtonsPressed(object[] obj)
	{
		ClearInfoPane();
		SwitchState(new NoAllianceShowInvitationsState(_titleSegment, _createAllianceButtons, _allianceApplicationsPanel));
	}

	private void OnPersonalApplicationButtonPressed(object[] obj)
	{
		ClearInfoPane();
		SwitchState(new NoAllianceShowApplicationsState(_titleSegment, _createAllianceButtons, _allianceApplicationsPanel));
	}

	private void OnMembersButtonPressed(object[] obj)
	{
		ClearInfoPane();
		SwitchState(new YourAllianceShowMembersState(_titleSegment, _managementButtons, _disbandAllianceButton, _allianceMembersPanel));
	}

	private void OnApplicationButtonPressed(object[] obj)
	{
		ClearInfoPane();
		SwitchState(new YourAllianceShowApplicationsState(_titleSegment, _managementButtons, _disbandAllianceButton, _allianceApplicationsPanel));
	}

	private void OnInvitationButtonsPressed(object[] obj)
	{
		ClearInfoPane();
		SwitchState(new YourAllianceShowInvitationsState(_titleSegment, _managementButtons, _disbandAllianceButton, _allianceInvitationsPanel));
	}

	private void OnEditRankPressed(object[] obj)
	{
		ClearInfoPane();
		SwitchState(new YourAllianceEditSettingsState(_titleSegment, _managementButtons, _disbandAllianceButton, _allianceEditRankPanel));
	}

	protected override void ProtectedDispose()
	{
	}

	private void OnPromiseFailed(Exception ex)
	{
		SocialCharacterSheet.TriggerAllianceExceptionHandler(ex, SocialCharacterSheet.TriggerCloseSocialScreen, SocialCharacterSheet.TriggerAllianceStateCheck);
	}

	private void OnDisbandPressed()
	{
		DialogPopupFacade.ShowConfirmationDialog("Disband Alliance", "Are you sure you want to disband the alliance? All that hard work, gone....", delegate
		{
			_allianceClient.RequestDisbandAlliance().Then(delegate(bool resultFlag)
			{
				OnResponseDisbandAlliance(resultFlag);
			}).Catch(delegate(Exception exception)
			{
				OnPromiseFailed(exception);
			});
		}, "CONFIRM");
	}

	private void OnResponseDisbandAlliance(bool resultFlag)
	{
		SocialCharacterSheet.TriggerAllianceStateCheck();
	}

	private void OnLeaveAlliancePressed()
	{
		DialogPopupFacade.ShowConfirmationDialog("Leave Alliance", "Are you sure you want to leave the alliance? Remember the good times?", delegate
		{
			_allianceClient.RequestLeaveAlliance().Then(delegate(bool resultFlag)
			{
				OnResponseLeaveAlliance(resultFlag);
			}).Catch(delegate(Exception exception)
			{
				OnPromiseFailed(exception);
			});
		}, "CONFIRM");
	}

	private void OnResponseLeaveAlliance(bool resultFlag)
	{
		SocialCharacterSheet.TriggerAllianceStateCheck();
	}

	private void OnAllianceStateChanged(object[] obj)
	{
		RefreshForPlayerState();
	}

	private void ClearInfoPane()
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIAllianceEvents.BackPressed);
	}

	public void RefreshForPlayerState()
	{
		_allianceClient.GetYourAllianceMemberData().Then((Action<AllianceMember>)CheckPlayerInAlliance).Then((AllianceMember memberData) => (memberData != null) ? _allianceClient.GetYourBasicAllianceInfo().Then(delegate(AllianceBasicInformation allianceInfo)
		{
			CheckIfAllianceLeader(memberData, allianceInfo);
		}) : Promise<AllianceBasicInformation>.Resolved(null))
			.Catch(OnPromiseFailed);
	}

	private void CheckPlayerInAlliance(AllianceMember member)
	{
		if (member != null)
		{
			SwitchState(new YourAllianceShowMembersState(_titleSegment, _managementButtons, _disbandAllianceButton, _allianceMembersPanel));
			SetManagementButtons(member);
		}
		else
		{
			SwitchState(new NoAllianceCreateAllianceState(_titleSegment, _createAllianceButtons, _allianceCreateAlliancePanel));
		}
	}

	private void SetManagementButtons(AllianceMember member)
	{
		_managementButtons.SetForPermissions(member.RankData);
	}

	private void CheckIfAllianceLeader(AllianceMember memberData, AllianceBasicInformation allianceInfo)
	{
		if (allianceInfo.LeaderCharacterUiD == memberData.CharacterUiD)
		{
			_disbandAllianceButton.SetText("DISBAND ALLIANCE");
			_disbandAllianceButton.SetButtonEvent(OnDisbandPressed);
		}
		else
		{
			_disbandAllianceButton.SetText("LEAVE ALLIANCE");
			_disbandAllianceButton.SetButtonEvent(OnLeaveAlliancePressed);
		}
	}

	private void SwitchState(IAllianceScreenState newState)
	{
		if (_currentScreenState != null)
		{
			_currentScreenState.LeaveScreen();
		}
		_currentScreenState = newState;
		_currentScreenState.EnterScreen();
	}
}
