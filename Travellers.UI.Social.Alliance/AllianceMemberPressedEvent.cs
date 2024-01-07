using Bossa.Travellers.Social.DataModel;
using Travellers.UI.Events;

namespace Travellers.UI.Social.Alliance;

public class AllianceMemberPressedEvent : UIEvent
{
	public AllianceMember AllianceMemberToView;

	public AllianceMemberPressedEvent(AllianceMember allianceMemberToView)
	{
		AllianceMemberToView = allianceMemberToView;
	}
}
