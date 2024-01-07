using Travellers.UI.Events;

namespace Travellers.UI.HUD;

public class ChangeTeleportSpinnerStateEvent : UIEvent
{
	public SpinnerFlags Flags { get; private set; }

	public bool Show { get; private set; }

	public ChangeTeleportSpinnerStateEvent(SpinnerFlags flags, bool show)
	{
		Flags = flags;
		Show = show;
	}
}
