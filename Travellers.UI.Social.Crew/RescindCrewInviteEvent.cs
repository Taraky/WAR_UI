using Bossa.Travellers.Social.DataModel;
using Travellers.UI.Events;

namespace Travellers.UI.Social.Crew;

public class RescindCrewInviteEvent : UIEvent
{
	public CrewMember CrewSlot;

	public RescindCrewInviteEvent(CrewMember crewSlot)
	{
		CrewSlot = crewSlot;
	}
}
