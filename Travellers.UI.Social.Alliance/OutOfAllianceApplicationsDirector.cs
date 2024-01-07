using System;
using Bossa.Travellers.Alliances;
using Bossa.Travellers.Social.DataModel;
using WAUtilities.Logging;

namespace Travellers.UI.Social.Alliance;

public class OutOfAllianceApplicationsDirector : IMembershipMessageDirector
{
	public void Confirm(IAllianceClient allianceClient, MembershipChangeRequest application, Action onSuccess, Action<Exception> onFailure)
	{
		WALogger.Error<YourAllianceApplicationsPanel>("You can't accept your own application");
	}

	public void Reject(IAllianceClient allianceClient, MembershipChangeRequest application, Action onSuccess, Action<Exception> onFailure)
	{
		allianceClient.PlayerDeleteAllianceApplication(application).Then(delegate
		{
			onSuccess();
		}).Catch(onFailure);
	}

	public void RefreshMembershipMessageList(MembershipMessageList applicationsList)
	{
		applicationsList.RefreshResultsPage(new OutOfAllianceApplicationsList(), 0);
	}
}
