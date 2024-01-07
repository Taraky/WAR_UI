using System;
using Bossa.Travellers.Alliances;
using Bossa.Travellers.Social.DataModel;

namespace Travellers.UI.Social.Alliance;

public abstract class AllianceApplicationsFilter
{
	public abstract void AcceptInvitation(IAllianceClient allianceClient, MembershipChangeRequest application, Action onSuccess, Action<Exception> onFailure);

	public abstract void RejectInvitation(IAllianceClient allianceClient, MembershipChangeRequest application, Action onSuccess, Action<Exception> onFailure);

	public abstract void RefreshList(MembershipMessageList applicationsList);
}
