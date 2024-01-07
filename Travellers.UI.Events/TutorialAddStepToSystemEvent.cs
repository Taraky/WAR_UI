using Travellers.UI.Tutorial;

namespace Travellers.UI.Events;

public class TutorialAddStepToSystemEvent : UIEvent
{
	public TutorialStep StepToShow;

	public TutorialAddStepToSystemEvent(TutorialStep stepToAdd)
	{
		StepToShow = stepToAdd;
	}
}
