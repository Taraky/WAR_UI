namespace Travellers.UI.Events;

public class UIButtonPressedEvent : UIEvent
{
	public InputButtons ButtonPressed;

	public UIButtonPressedEvent(InputButtons buttonPressed)
	{
		ButtonPressed = buttonPressed;
	}
}
