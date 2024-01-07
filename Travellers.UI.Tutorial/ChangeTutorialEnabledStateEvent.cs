namespace Travellers.UI.Tutorial;

public class ChangeTutorialEnabledStateEvent
{
	public readonly bool IsActive;

	public ChangeTutorialEnabledStateEvent(bool isActive)
	{
		IsActive = isActive;
	}
}
