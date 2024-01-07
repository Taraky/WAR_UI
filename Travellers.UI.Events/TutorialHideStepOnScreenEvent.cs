using Travellers.UI.Tutorial;

namespace Travellers.UI.Events;

public class TutorialHideStepOnScreenEvent : UIEvent
{
	public TutorialContent StepToHide;

	public TutorialHideStepOnScreenEvent(TutorialContent stepToHide)
	{
		StepToHide = stepToHide;
	}
}
