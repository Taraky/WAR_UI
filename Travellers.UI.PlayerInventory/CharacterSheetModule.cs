using System;
using Travellers.UI.Tutorial;
using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.PlayerInventory;

public abstract class CharacterSheetModule : BaseSheetModule
{
	protected virtual float rightSideSpacing => 40f;

	protected abstract TutorialHookType tutorialHookType { get; }

	protected CharacterSheetModule(Transform screenParent, Transform tabParent, HorizontalOrVerticalLayoutGroup layoutParent, Action<CharacterSheetTabType> onTabPressed, UIToggleGroup toggleGroup)
		: base(screenParent, tabParent, onTabPressed, toggleGroup)
	{
		layoutParent.spacing = rightSideSpacing;
		tab.SetTutorialType(tutorialHookType);
	}
}
