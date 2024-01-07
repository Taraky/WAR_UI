using Bossa.Travellers.Crew;
using Travellers.UI.Events;

namespace Travellers.UI.Social.Crew;

public class FeedbackReceivedEvent : UIEvent
{
	public CrewManagementFeedback CrewManagementFeedback;

	public FeedbackReceivedEvent(CrewManagementFeedback crewManagementFeedback)
	{
		CrewManagementFeedback = crewManagementFeedback;
	}
}
