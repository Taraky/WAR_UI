using System;
using Travellers.UI.Framework;
using Travellers.UI.Tutorial;
using UnityEngine;

namespace Travellers.UI.PlayerInventory;

public class CharacterSheetTab : UIScreenComponent
{
	[SerializeField]
	private UIInventoryTabToggleController _tabToggleController;

	[SerializeField]
	private TutorialTransformAnchor _tutorialHook;

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
	}

	protected override void ProtectedDispose()
	{
	}

	public void SetTutorialType(TutorialHookType hookType)
	{
		if (_tutorialHook != null)
		{
			_tutorialHook.ReRegisterAnchor(hookType);
		}
	}

	public void SetTabName(string nameToDisplay)
	{
		_tabToggleController.SetText(nameToDisplay);
	}

	public void SetEvent(Action onTabPressed)
	{
		_tabToggleController.SetButtonEvent(onTabPressed);
	}

	public void ShowTab(bool show)
	{
		SetObjectActive(show);
	}

	public void SetSelected(bool isSelected)
	{
		_tabToggleController.SetToggleSelected(isSelected);
	}

	public void AddToToggleGroup(UIToggleGroup toggleGroup)
	{
		toggleGroup.AddToggleToGroup(_tabToggleController);
	}

	public void SetActiveInToggleGroup(UIToggleGroup toggleGroup)
	{
		toggleGroup.SetActiveButton(_tabToggleController);
	}
}
