using Travellers.UI.Events;

namespace Travellers.UI.Social.Crew;

public class CrewBeaconCooldownUpdateEvent : UIEvent
{
	public long Timestamp;

	public CrewBeaconCooldownUpdateEvent(long timestamp)
	{
		Timestamp = timestamp;
	}
}
