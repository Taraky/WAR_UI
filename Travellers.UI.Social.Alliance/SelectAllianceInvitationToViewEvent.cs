using Bossa.Travellers.Social.DataModel;
using Travellers.UI.Events;

namespace Travellers.UI.Social.Alliance;

public class SelectAllianceInvitationToViewEvent : UIEvent
{
	public AllianceBasicInformation AllianceBasicToView;

	public MembershipChangeRequest AllianceApplication;

	public SelectAllianceInvitationToViewEvent(AllianceBasicInformation allianceBasicToView, MembershipChangeRequest application)
	{
		AllianceApplication = application;
		AllianceBasicToView = allianceBasicToView;
	}
}
