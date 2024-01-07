using System.Collections.Generic;
using Travellers.UI.Tutorial;

namespace Travellers.UI.Events;

public class TutorialRemoveStepFromSystemEvent : UIEvent
{
	public List<TutorialStep> HideIfInThisList;

	public bool HideCurrentStep;

	public TutorialStep StepToHide;

	public TutorialRemoveStepFromSystemEvent()
	{
		HideCurrentStep = true;
	}

	public TutorialRemoveStepFromSystemEvent(List<TutorialStep> hideIfInList)
	{
		HideIfInThisList = hideIfInList;
	}

	public TutorialRemoveStepFromSystemEvent(TutorialStep stepToHide)
	{
		StepToHide = stepToHide;
	}
}
