using Bossa.Travellers.Social.DataModel;
using Travellers.UI.Events;

namespace Travellers.UI.Social.Crew;

public class RequestCrewMemberBootEvent : UIEvent
{
	public CrewMember CrewSlot;

	public RequestCrewMemberBootEvent(CrewMember crewSlot)
	{
		CrewSlot = crewSlot;
	}
}
