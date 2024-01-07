using System;
using Bossa.Travellers.Alliances;
using Bossa.Travellers.Social.DataModel;

namespace Travellers.UI.Social.Alliance;

public class InAllianceInvitationsList : IMembershipListMessageFilter
{
	public void GetMembershipMessages(IAllianceClient allianceClient, int pageIndex, int resultsPerPage, Action<AllianceMembershipMessageSlice> onSuccess, Action<Exception> onFailure)
	{
		allianceClient.GetAllianceInvitations(pageIndex, resultsPerPage).Then(onSuccess).Catch(onFailure);
	}

	public void GetMembershipMessagesWithParams(IAllianceClient allianceClient, string searchTerm, int pageIndex, int resultsPerPage, Action<AllianceMembershipMessageSlice> onSuccess, Action<Exception> onFailure)
	{
		allianceClient.GetAllianceInvitationsWithParams(searchTerm, pageIndex, resultsPerPage).Then(onSuccess).Catch(onFailure);
	}

	public void SetDataForMessageChoiceButton(UIToggleGroup buttonToggleGroup, AlliancePlayerInfoButton infoButton, MembershipChangeRequest applicationInfo, IAllianceClient allianceClient, Action<Exception> onFailure)
	{
		infoButton.SetDataForSentInvitation(buttonToggleGroup, applicationInfo);
	}
}
