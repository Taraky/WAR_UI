using Bossa.Travellers.Controls.Observer;

namespace Travellers.UI.Events;

public class CloseWindowEvent : UIEvent
{
	public ModalUIWindow WindowToClose;

	public CloseWindowEvent(ModalUIWindow windowToClose)
	{
		WindowToClose = windowToClose;
	}
}
