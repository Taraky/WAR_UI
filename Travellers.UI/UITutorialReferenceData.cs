using System.Collections.Generic;
using Travellers.UI.Framework;
using Travellers.UI.Tutorial;
using UnityEngine;

namespace Travellers.UI;

[InjectedReferenceDataObject]
public class UITutorialReferenceData
{
	public List<TutorialContent> AllTutorialSteps;

	public UITutorialReferenceData()
	{
		AllTutorialSteps = new List<TutorialContent>(Resources.LoadAll<TutorialContent>("UI/ReferenceData/TutorialItems"));
		for (int i = 0; i < AllTutorialSteps.Count; i++)
		{
			AllTutorialSteps[i].Initialise();
		}
	}
}
