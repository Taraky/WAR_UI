using Bossa.Travellers.Ship;

namespace Travellers.UI.Events;

public class ShipPartCountDataUpdatedEvent : UIEvent
{
	public readonly ShipPartCountData ShipPartCountData;

	public readonly ShipVisualizer ShipVisualizer;

	public ShipPartCountDataUpdatedEvent(ShipVisualizer shipVisualizer, ShipPartCountData shipPartCountData)
	{
		ShipPartCountData = shipPartCountData;
		ShipVisualizer = shipVisualizer;
	}
}
