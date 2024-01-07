using System;
using Bossa.Travellers.Alliances;
using Bossa.Travellers.Social.DataModel;

namespace Travellers.UI.Social.Alliance;

public class InAllianceInvitationDirector : IMembershipMessageDirector
{
	public void Confirm(IAllianceClient allianceClient, MembershipChangeRequest application, Action onSuccess, Action<Exception> onFailure)
	{
		allianceClient.AllianceSendInvitationToPlayer(application).Then(delegate
		{
			onSuccess();
		}).Catch(onFailure);
	}

	public void Reject(IAllianceClient allianceClient, MembershipChangeRequest application, Action onSuccess, Action<Exception> onFailure)
	{
		allianceClient.AllianceCancelInvitationToPlayer(application).Then(delegate
		{
			onSuccess();
		}).Catch(onFailure);
	}

	public void RefreshMembershipMessageList(MembershipMessageList applicationsList)
	{
		applicationsList.RefreshResultsPage(new InAllianceInvitationsList(), 0);
	}
}
