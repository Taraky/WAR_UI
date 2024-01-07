using Travellers.UI.Events;

namespace Travellers.UI.InfoPopups;

public class UpdatePlayingMusicalInstrumentEvent : UIEvent
{
	public readonly MusicalInstrumentScreenState State;

	public UpdatePlayingMusicalInstrumentEvent(MusicalInstrumentScreenState state)
	{
		State = state;
	}
}
