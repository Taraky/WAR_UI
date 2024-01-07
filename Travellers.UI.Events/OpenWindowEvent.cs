using Bossa.Travellers.Controls.Observer;

namespace Travellers.UI.Events;

public class OpenWindowEvent : UIEvent
{
	public ModalUIWindow WindowToOpen;

	public OpenWindowEvent(ModalUIWindow windowToOpen)
	{
		WindowToOpen = windowToOpen;
	}
}
