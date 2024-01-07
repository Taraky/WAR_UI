using Bossa.Travellers.Ship;
using Improbable.Collections;

namespace Travellers.UI.Events;

public class ShipRegisteredCharactersUpdatedEvent : UIEvent
{
	public readonly List<ReviverInfo> ReviverInfos;

	public readonly ShipVisualizer ShipVisualizer;

	public ShipRegisteredCharactersUpdatedEvent(ShipVisualizer shipVisualizer, List<ReviverInfo> reviverInfos)
	{
		ReviverInfos = reviverInfos;
		ShipVisualizer = shipVisualizer;
	}
}
