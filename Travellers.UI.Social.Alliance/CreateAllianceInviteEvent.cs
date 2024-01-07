using Bossa.Travellers.Social.DataModel;
using Travellers.UI.Events;

namespace Travellers.UI.Social.Alliance;

public class CreateAllianceInviteEvent : UIEvent
{
	public MembershipChangeRequest AllianceApplication;

	public CreateAllianceInviteEvent(MembershipChangeRequest application)
	{
		AllianceApplication = application;
	}
}
