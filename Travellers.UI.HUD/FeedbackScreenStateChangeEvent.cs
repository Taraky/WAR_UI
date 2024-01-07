using Travellers.UI.Events;

namespace Travellers.UI.HUD;

public class FeedbackScreenStateChangeEvent : UIEvent
{
	public readonly bool Show;

	public FeedbackScreenStateChangeEvent(bool show)
	{
		Show = show;
	}
}
