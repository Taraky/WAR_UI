using System;
using Bossa.Travellers.Alliances;
using TMPro;
using Travellers.UI.Framework;
using Travellers.UI.HUDMessaging;
using UnityEngine;

namespace Travellers.UI.Social.Alliance;

[InjectableClass]
public class YourAllianceApplicationsPanel : UIScreenComponent
{
	[SerializeField]
	private TMP_InputField _searchInputField;

	[SerializeField]
	private MembershipMessageList _applicationList;

	private IMembershipMessageDirector _messageDirector;

	private IAllianceClient _allianceClient;

	[InjectableMethod]
	public void InjectDependencies(IAllianceClient allianceClient)
	{
		_allianceClient = allianceClient;
	}

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUIAllianceEvents.CancelPlayerToAllianceApplication, OnOutgoingApplicationRescinded);
		_eventList.AddEvent(WAUIAllianceEvents.RejectInvitationPressed, OnRejectInvitation);
		_eventList.AddEvent(WAUIAllianceEvents.AcceptInvitationPressed, OnAcceptInvitation);
		_eventList.AddEvent(WAUIAllianceEvents.AllianceRejectPlayerApplicationPressed, OnRejectApplication);
		_eventList.AddEvent(WAUIAllianceEvents.AllianceAcceptPlayerApplicationPressed, OnAcceptApplication);
		_eventList.AddEvent(WAUIAllianceEvents.AllAllianceDataChanged, OnDataUpdated);
	}

	protected override void Activate()
	{
		ResetFields();
	}

	private void OnAcceptApplication(object[] obj)
	{
		ApplicationPressedEvent applicationPressedEvent = (ApplicationPressedEvent)obj[0];
		if (applicationPressedEvent != null)
		{
			_messageDirector.Confirm(_allianceClient, applicationPressedEvent.ApplicationInfo, OnAcceptApplicationSuccess, OnFailed);
		}
	}

	private void OnRejectApplication(object[] obj)
	{
		ApplicationPressedEvent applicationPressedEvent = (ApplicationPressedEvent)obj[0];
		if (applicationPressedEvent != null)
		{
			_messageDirector.Reject(_allianceClient, applicationPressedEvent.ApplicationInfo, OnRejectApplicationSuccess, OnFailed);
		}
	}

	private void OnOutgoingApplicationRescinded(object[] obj)
	{
		ApplicationPressedEvent applicationPressedEvent = (ApplicationPressedEvent)obj[0];
		if (applicationPressedEvent != null)
		{
			_messageDirector.Reject(_allianceClient, applicationPressedEvent.ApplicationInfo, OnRejectMembershipChangeRequestSuccess, OnFailed);
		}
	}

	private void OnAcceptInvitation(object[] obj)
	{
		ApplicationPressedEvent applicationPressedEvent = (ApplicationPressedEvent)obj[0];
		if (applicationPressedEvent != null)
		{
			_messageDirector.Confirm(_allianceClient, applicationPressedEvent.ApplicationInfo, OnAcceptInvitationSuccess, OnFailed);
		}
	}

	private void OnRejectInvitation(object[] obj)
	{
		ApplicationPressedEvent applicationPressedEvent = (ApplicationPressedEvent)obj[0];
		if (applicationPressedEvent != null)
		{
			_messageDirector.Reject(_allianceClient, applicationPressedEvent.ApplicationInfo, OnRejectMembershipChangeRequestSuccess, OnFailed);
		}
	}

	private void OnAcceptApplicationSuccess()
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIAllianceEvents.BackPressed);
		SocialCharacterSheet.TriggerAllianceDataRefresh();
		OSDMessage.SendMessage("A new member has joined your alliance");
	}

	private void OnRejectApplicationSuccess()
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIAllianceEvents.BackPressed);
		SocialCharacterSheet.TriggerAllianceDataRefresh();
	}

	private void OnAcceptInvitationSuccess()
	{
		SocialCharacterSheet.TriggerAllianceStateCheck();
		OSDMessage.SendMessage("Congratulations, you've joined an alliance");
	}

	private void OnRejectMembershipChangeRequestSuccess()
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIAllianceEvents.BackPressed);
	}

	private void OnFailed(Exception ex)
	{
		SocialCharacterSheet.TriggerAllianceExceptionHandler(ex, SocialCharacterSheet.TriggerCloseSocialScreen, SocialCharacterSheet.TriggerAllianceDataRefresh);
	}

	private void OnDataUpdated(object[] obj)
	{
		RefreshAllianceApplicationsList();
	}

	protected override void ProtectedInit()
	{
		_searchInputField.onValueChanged.AddListener(OnSearchChanged);
		_messageDirector = new InAllianceApplicationsDirector();
	}

	private void OnSearchChanged(string newSearch)
	{
		_applicationList.FilterByString(newSearch);
	}

	protected override void ProtectedDispose()
	{
	}

	public void Setup()
	{
		SetObjectActive(isActive: false);
	}

	public void SetFilterAndRefreshList(IMembershipMessageDirector applicationsDirector)
	{
		_messageDirector = applicationsDirector;
		RefreshAllianceApplicationsList();
	}

	public void RefreshAllianceApplicationsList()
	{
		_messageDirector.RefreshMembershipMessageList(_applicationList);
	}

	private void ResetFields()
	{
		_searchInputField.text = string.Empty;
	}
}
