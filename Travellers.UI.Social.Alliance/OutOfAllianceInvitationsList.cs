using System;
using Bossa.Travellers.Alliances;
using Bossa.Travellers.Social.DataModel;

namespace Travellers.UI.Social.Alliance;

public class OutOfAllianceInvitationsList : IMembershipListMessageFilter
{
	public void GetMembershipMessages(IAllianceClient allianceClient, int pageIndex, int resultsPerPage, Action<AllianceMembershipMessageSlice> onSuccess, Action<Exception> onFailure)
	{
		allianceClient.GetPlayerFromAllianceInvitations(pageIndex, resultsPerPage).Then(onSuccess).Catch(onFailure);
	}

	public void GetMembershipMessagesWithParams(IAllianceClient allianceClient, string searchTerm, int pageIndex, int resultsPerPage, Action<AllianceMembershipMessageSlice> onSuccess, Action<Exception> onFailure)
	{
		allianceClient.GetPlayerFromAllianceInvitationsWithParams(searchTerm, pageIndex, resultsPerPage).Then(onSuccess).Catch(onFailure);
	}

	public void SetDataForMessageChoiceButton(UIToggleGroup buttonToggleGroup, AlliancePlayerInfoButton infoButton, MembershipChangeRequest applicationInfo, IAllianceClient allianceClient, Action<Exception> onFailure)
	{
		allianceClient.GetBasicAllianceInfoForAllianceId(applicationInfo.CharacterGroupTargetId).Then(delegate(AllianceBasicInformation allianceInfo)
		{
			infoButton.SetDataForPersonalInvitation(buttonToggleGroup, allianceInfo, applicationInfo);
		}).Catch(onFailure);
	}
}
