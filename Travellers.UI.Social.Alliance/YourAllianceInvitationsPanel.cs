using System;
using Bossa.Travellers.Alliances;
using Bossa.Travellers.Social.DataModel;
using TMPro;
using Travellers.UI.Framework;
using UnityEngine;

namespace Travellers.UI.Social.Alliance;

[InjectableClass]
public class YourAllianceInvitationsPanel : UIScreenComponent
{
	private IAllianceClient _allianceClient;

	[SerializeField]
	private MembershipMessageList _invitationList;

	private IMembershipMessageDirector _messageDirector;

	[SerializeField]
	private GameObject _searchBoxObject;

	[SerializeField]
	private UIButtonController _searchButtonController;

	[SerializeField]
	private TMP_InputField _searchInputField;

	[InjectableMethod]
	public void InjectDependencies(IAllianceClient allianceClient)
	{
		_allianceClient = allianceClient;
	}

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUIAllianceEvents.AllianceCancelInvitationToPlayer, OnOutgoingInvitationRescinded);
		_eventList.AddEvent(WAUIAllianceEvents.SendInvitationPressed, OnSendInvitation);
		_eventList.AddEvent(WAUIAllianceEvents.AllAllianceDataChanged, OnDataUpdated);
	}

	private void OnOutgoingInvitationRescinded(object[] obj)
	{
		ApplicationPressedEvent applicationPressedEvent = (ApplicationPressedEvent)obj[0];
		if (applicationPressedEvent != null)
		{
			_messageDirector.Reject(_allianceClient, applicationPressedEvent.ApplicationInfo, OnSuccess, OnFailed);
		}
	}

	private void OnSendInvitation(object[] obj)
	{
		ApplicationPressedEvent applicationPressedEvent = (ApplicationPressedEvent)obj[0];
		if (applicationPressedEvent != null)
		{
			_messageDirector.Confirm(_allianceClient, applicationPressedEvent.ApplicationInfo, OnSuccess, OnFailed);
		}
	}

	private void OnDataUpdated(object[] obj)
	{
		RefreshAllianceApplicationsList();
	}

	private void OnSuccess()
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIAllianceEvents.BackPressed);
	}

	protected override void ProtectedInit()
	{
		_searchButtonController.SetButtonEvent(OnSearchPressed);
		_messageDirector = new InAllianceInvitationDirector();
	}

	private void OnSearchPressed()
	{
		if (!string.IsNullOrEmpty(_searchInputField.text))
		{
			_allianceClient.CheckIfPlayerExistsAndIsNotInAlliance(_searchInputField.text).Then(delegate(CharacterSearchResponseModel application)
			{
				CheckSearchResponse(application);
			}).Catch(OnFailed);
		}
	}

	private void CheckSearchResponse(CharacterSearchResponseModel responseWrapper)
	{
		_searchInputField.text = string.Empty;
		MembershipChangeRequest application = MembershipChangeRequest.CreateEmpty(responseWrapper.screenname.characterUid, responseWrapper.screenname.displayName, SocialGroupType.Alliance, MembershipStatusChangeRequestType.Invite);
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIAllianceEvents.CreateInvitationPressed, new CreateAllianceInviteEvent(application));
	}

	protected override void ProtectedDispose()
	{
	}

	public void SetFilterAndRefreshList(IMembershipMessageDirector invitationPanelStates)
	{
		_messageDirector = invitationPanelStates;
		CheckIfPlayerCanInvite();
		RefreshAllianceApplicationsList();
	}

	private void CheckIfPlayerCanInvite()
	{
		_allianceClient.GetYourAllianceMemberData().Then(delegate(AllianceMember memberData)
		{
			_searchBoxObject.SetActive(memberData.RankData.EditMembers);
		}).Catch(delegate(Exception ex)
		{
			OnFailed(ex);
		});
	}

	private void RefreshAllianceApplicationsList()
	{
		_messageDirector.RefreshMembershipMessageList(_invitationList);
	}

	public void Setup()
	{
		SetObjectActive(isActive: false);
	}

	private void OnFailed(Exception ex)
	{
		SocialCharacterSheet.TriggerAllianceExceptionHandler(ex, SocialCharacterSheet.TriggerCloseSocialScreen, SocialCharacterSheet.TriggerAllianceDataRefresh);
	}
}
