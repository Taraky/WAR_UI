using System;
using Bossa.Travellers.Alliances;
using Bossa.Travellers.Social.DataModel;

namespace Travellers.UI.Social.Alliance;

public interface IMembershipMessageDirector
{
	void Confirm(IAllianceClient allianceClient, MembershipChangeRequest application, Action onSuccess, Action<Exception> onFailure);

	void Reject(IAllianceClient allianceClient, MembershipChangeRequest application, Action onSuccess, Action<Exception> onFailure);

	void RefreshMembershipMessageList(MembershipMessageList applicationsList);
}
