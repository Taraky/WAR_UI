using Travellers.UI.Tutorial;

namespace Travellers.UI.Events;

public class TutorialShowStepOnScreenEvent : UIEvent
{
	public TutorialContent StepToShow;

	public TutorialShowStepOnScreenEvent(TutorialContent stepToAdd)
	{
		StepToShow = stepToAdd;
	}
}
