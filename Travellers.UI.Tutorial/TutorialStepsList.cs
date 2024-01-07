using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Travellers.UI.Tutorial;

public class TutorialStepsList : IAddOnlyContainer
{
	[SerializeField]
	private List<TutorialStep> _steps = new List<TutorialStep>();

	public void Add(TutorialStep step)
	{
		_steps.Add(step);
	}

	public bool Contains(TutorialStep step)
	{
		return _steps.Contains(step);
	}

	public int GetDisplayedTimes(TutorialStep step)
	{
		return _steps.Count((TutorialStep s) => s == step);
	}

	public void Merge(TutorialStepsList other)
	{
		_steps.AddRange(other._steps);
	}

	public void Clear()
	{
		_steps.Clear();
	}
}
