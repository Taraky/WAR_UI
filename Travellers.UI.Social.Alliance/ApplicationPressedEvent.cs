using Bossa.Travellers.Social.DataModel;
using Travellers.UI.Events;

namespace Travellers.UI.Social.Alliance;

public class ApplicationPressedEvent : UIEvent
{
	public MembershipChangeRequest ApplicationInfo;

	public ApplicationPressedEvent(MembershipChangeRequest applicationInfo)
	{
		ApplicationInfo = applicationInfo;
	}
}
