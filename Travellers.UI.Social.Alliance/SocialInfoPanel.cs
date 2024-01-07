using System;
using Bossa.Travellers.Alliances;
using Bossa.Travellers.Social.DataModel;
using Travellers.UI.Framework;
using UnityEngine;
using WAUtilities.Logging;

namespace Travellers.UI.Social.Alliance;

[InjectableClass]
public class SocialInfoPanel : UIScreenComponent
{
	private IAllianceClient _allianceClient;

	private ISocialInfoPanelState _currentScreenState;

	[SerializeField]
	private GameObject _allianceLogoBox;

	[SerializeField]
	private SocialInfoPanelAllianceInfo _socialInfoPanelAllianceInfo;

	[SerializeField]
	private SocialInfoPanelAllianceMember _socialInfoPanelAllianceMember;

	[SerializeField]
	private SocialInfoPanelApplicationAndInvitation _socialInfoPanelApplicationAndInvitation;

	[SerializeField]
	private TextStylerTextMeshPro _textWarning;

	[InjectableMethod]
	public void InjectDependencies(IAllianceClient allianceClient)
	{
		_allianceClient = allianceClient;
	}

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUIAllianceEvents.AllianceInfoButtonPressed, OnSelectAlliancePressed);
		_eventList.AddEvent(WAUIAllianceEvents.OutOfAllianceInvitationPressed, OnSelectOutOfAllianceInvitationPressed);
		_eventList.AddEvent(WAUIAllianceEvents.OutOfAllianceApplicationPressed, OnSelectOutOfAllianceApplicationPressed);
		_eventList.AddEvent(WAUIAllianceEvents.YourProfileButtonPressed, OnYourProfilePressed);
		_eventList.AddEvent(WAUIAllianceEvents.MemberBarPressed, OnAllianceMemberPressed);
		_eventList.AddEvent(WAUIAllianceEvents.ViewApplicationPressed, OnViewApplicationPressed);
		_eventList.AddEvent(WAUIAllianceEvents.ViewInvitationPressed, OnViewInvitationPressed);
		_eventList.AddEvent(WAUIAllianceEvents.CreateInvitationPressed, OnCreateInvitationPressed);
		_eventList.AddEvent(WAUIAllianceEvents.BackPressed, OnBackPressed);
		_eventList.AddEvent(WAUIAllianceEvents.AllianceCancelInvitationToPlayer, OnCancelledInvitation);
		_eventList.AddEvent(WAUIAllianceEvents.CancelPlayerToAllianceApplication, OnOutgoingApplicationRescinded);
		_eventList.AddEvent(WAUIAllianceEvents.RejectInvitationPressed, OnIncomingInvitationRejected);
	}

	protected override void ProtectedInit()
	{
		SwitchToNeutralState();
	}

	protected override void ProtectedDispose()
	{
	}

	public void SetupForNeutralState()
	{
		SwitchToNeutralState();
	}

	private void OnSelectAlliancePressed(object[] obj)
	{
		SelectAllianceToViewEvent selectAllianceToViewEvent = (SelectAllianceToViewEvent)obj[0];
		if (selectAllianceToViewEvent != null)
		{
			SwitchState(new SocialInfoPanelAllianceState(_socialInfoPanelAllianceInfo, selectAllianceToViewEvent.AllianceBasicToView));
		}
	}

	private void OnSelectOutOfAllianceInvitationPressed(object[] obj)
	{
		SelectAllianceInvitationToViewEvent selectAllianceInvitationToViewEvent = (SelectAllianceInvitationToViewEvent)obj[0];
		if (selectAllianceInvitationToViewEvent != null)
		{
			SwitchState(new SocialInfoPanelOutOfAllianceInvitationState(_socialInfoPanelAllianceInfo, selectAllianceInvitationToViewEvent.AllianceBasicToView, selectAllianceInvitationToViewEvent.AllianceApplication));
		}
	}

	private void OnSelectOutOfAllianceApplicationPressed(object[] obj)
	{
		SelectAllianceInvitationToViewEvent selectAllianceInvitationToViewEvent = (SelectAllianceInvitationToViewEvent)obj[0];
		if (selectAllianceInvitationToViewEvent != null)
		{
			SwitchState(new SocialInfoPanelOutOfAllianceApplicationState(_socialInfoPanelAllianceInfo, selectAllianceInvitationToViewEvent.AllianceBasicToView, selectAllianceInvitationToViewEvent.AllianceApplication));
		}
	}

	private void OnYourProfilePressed(object[] obj)
	{
		_allianceClient.GetYourAllianceMemberData().Then(delegate(AllianceMember allianceMemberData)
		{
			_allianceClient.GetYourAllianceRankInformation().Then(delegate
			{
				TryShowPlayerProfile(allianceMemberData);
			});
		}).Catch(OnGetPlayerDataFailed);
	}

	private void TryShowPlayerProfile(AllianceMember playerMemberInfo)
	{
		if (playerMemberInfo != null)
		{
			SwitchState(new SocialInfoPanelAllianceMemberState(_socialInfoPanelAllianceMember, playerMemberInfo, playerMemberInfo));
		}
	}

	private void OnGetPlayerDataFailed(Exception exception)
	{
		WALogger.Exception<SocialInfoPanel>(exception);
	}

	private void OnAllianceMemberPressed(object[] obj)
	{
		AllianceMemberPressedEvent evt = (AllianceMemberPressedEvent)obj[0];
		if (evt != null)
		{
			_allianceClient.GetYourAllianceMemberData().Then(delegate(AllianceMember yourData)
			{
				SwitchState(new SocialInfoPanelAllianceMemberState(_socialInfoPanelAllianceMember, evt.AllianceMemberToView, yourData));
			});
		}
	}

	private void OnViewApplicationPressed(object[] obj)
	{
		ApplicationPressedEvent applicationPressedEvent = (ApplicationPressedEvent)obj[0];
		if (applicationPressedEvent != null)
		{
			SwitchState(new SocialInfoPanelInAllianceViewApplicationState(applicationPressedEvent.ApplicationInfo, _socialInfoPanelApplicationAndInvitation));
		}
	}

	private void OnViewInvitationPressed(object[] obj)
	{
		ApplicationPressedEvent applicationPressedEvent = (ApplicationPressedEvent)obj[0];
		if (applicationPressedEvent != null)
		{
			SwitchState(new SocialInfoPanelInAllianceViewInvitationState(applicationPressedEvent.ApplicationInfo, _socialInfoPanelApplicationAndInvitation));
		}
	}

	private void OnCreateInvitationPressed(object[] obj)
	{
		CreateAllianceInviteEvent createAllianceInviteEvent = (CreateAllianceInviteEvent)obj[0];
		if (createAllianceInviteEvent != null)
		{
			SwitchState(new SocialInfoPanelInAllianceSendInvitationState(createAllianceInviteEvent.AllianceApplication, _socialInfoPanelApplicationAndInvitation));
		}
	}

	private void OnBackPressed(object[] obj)
	{
		SwitchToNeutralState();
	}

	private void OnCancelledInvitation(object[] obj)
	{
		SwitchToNeutralState();
	}

	private void OnOutgoingApplicationRescinded(object[] obj)
	{
		SwitchToNeutralState();
	}

	private void OnIncomingInvitationRejected(object[] obj)
	{
		SwitchToNeutralState();
	}

	private void SwitchToNeutralState()
	{
		SwitchState(new SocialInfoPanelTextWarningState(_allianceLogoBox, _textWarning, _socialInfoPanelAllianceInfo, _socialInfoPanelAllianceMember, _socialInfoPanelApplicationAndInvitation));
	}

	private void SwitchState(ISocialInfoPanelState newState)
	{
		if (_currentScreenState != null)
		{
			_currentScreenState.LeaveScreen();
		}
		_currentScreenState = newState;
		_currentScreenState.EnterScreen();
	}
}
