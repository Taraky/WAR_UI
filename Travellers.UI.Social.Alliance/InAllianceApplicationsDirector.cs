using System;
using Bossa.Travellers.Alliances;
using Bossa.Travellers.Social.DataModel;

namespace Travellers.UI.Social.Alliance;

public class InAllianceApplicationsDirector : IMembershipMessageDirector
{
	public void Confirm(IAllianceClient allianceClient, MembershipChangeRequest application, Action onSuccess, Action<Exception> onFailure)
	{
		allianceClient.AllianceAcceptPlayerApplication(application).Then(delegate
		{
			onSuccess();
		}).Catch(onFailure);
	}

	public void Reject(IAllianceClient allianceClient, MembershipChangeRequest application, Action onSuccess, Action<Exception> onFailure)
	{
		allianceClient.AllianceRejectPlayerApplication(application).Then(delegate
		{
			onSuccess();
		}).Catch(onFailure);
	}

	public void RefreshMembershipMessageList(MembershipMessageList applicationsList)
	{
		applicationsList.RefreshResultsPage(new InAllianceApplicationsList(), 0);
	}
}
