using System.Collections.Generic;
using Bossa.Travellers.Social.DataModel;
using Travellers.UI.Events;

namespace Travellers.UI.Social.Crew;

public class CrewMembersUpdatedEvent : UIEvent
{
	public List<CrewMember> CrewSlots;

	public CrewMembersUpdatedEvent(List<CrewMember> crewSlots)
	{
		CrewSlots = crewSlots;
	}
}
