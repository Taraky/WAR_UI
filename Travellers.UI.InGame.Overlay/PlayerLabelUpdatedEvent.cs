using Travellers.UI.Events;

namespace Travellers.UI.InGame.Overlay;

public class PlayerLabelUpdatedEvent : UIEvent
{
	public PlayerNameInfo PlayerNameInfo;

	public PlayerLabelUpdatedEvent(PlayerNameInfo playerNameInfo)
	{
		PlayerNameInfo = playerNameInfo;
	}
}
