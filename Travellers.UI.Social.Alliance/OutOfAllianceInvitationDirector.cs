using System;
using Bossa.Travellers.Alliances;
using Bossa.Travellers.Social.DataModel;

namespace Travellers.UI.Social.Alliance;

public class OutOfAllianceInvitationDirector : IMembershipMessageDirector
{
	public void Confirm(IAllianceClient allianceClient, MembershipChangeRequest application, Action onSuccess, Action<Exception> onFailure)
	{
		allianceClient.PlayerAcceptAllianceInvitation(application).Then(delegate
		{
			onSuccess();
		}).Catch(onFailure);
	}

	public void Reject(IAllianceClient allianceClient, MembershipChangeRequest application, Action onSuccess, Action<Exception> onFailure)
	{
		allianceClient.PlayerRejectAllianceInvitation(application).Then(delegate
		{
			onSuccess();
		}).Catch(onFailure);
	}

	public void RefreshMembershipMessageList(MembershipMessageList applicationsList)
	{
		applicationsList.RefreshResultsPage(new OutOfAllianceInvitationsList(), 0);
	}
}
