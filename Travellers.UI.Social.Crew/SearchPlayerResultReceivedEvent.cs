using Bossa.Travellers.Crew;
using Travellers.UI.Events;

namespace Travellers.UI.Social.Crew;

public class SearchPlayerResultReceivedEvent : UIEvent
{
	public SearchPlayerResult SearchPlayerResult;

	public SearchPlayerResultReceivedEvent(SearchPlayerResult searchPlayerResult)
	{
		SearchPlayerResult = searchPlayerResult;
	}
}
