using System;
using System.Collections.Generic;
using Travellers.UI.Framework;
using Travellers.UI.Sound;
using UnityEngine;

namespace Travellers.UI.PlayerInventory;

public abstract class BaseSheetModule
{
	protected CharacterSheetTab tab;

	private HashSet<CharacterSheetTabType> _associatedTabTypes = new HashSet<CharacterSheetTabType>();

	private readonly UIToggleGroup _toggleGroup;

	public UIScreenComponent ScreenModule { get; private set; }

	protected abstract string tabName { get; }

	public abstract CharacterSheetTabType DefaultTabType { get; }

	public CharacterSheetTabType CurrentTabType => DefaultTabType;

	protected virtual HashSet<CharacterSheetTabType> AssociatedTabTypes => _associatedTabTypes;

	protected abstract HashSet<CharacterSheetTabType> showTabForTheseContexts { get; }

	protected BaseSheetModule(Transform screenParent, Transform tabParent, Action<CharacterSheetTabType> onTabPressed, UIToggleGroup toggleGroup)
	{
		BaseSheetModule baseSheetModule = this;
		tab = UIObjectFactory.Create<CharacterSheetTab>(UIElementType.ScreenComponent, UIFillType.KeepOriginalAnchoring, tabParent, isObjectActive: true);
		tab.SetTabName(tabName);
		tab.SetEvent(delegate
		{
			onTabPressed(baseSheetModule.DefaultTabType);
		});
		_toggleGroup = toggleGroup;
		tab.AddToToggleGroup(_toggleGroup);
		ScreenModule = CreateMainComponent(screenParent);
	}

	protected abstract UIScreenComponent CreateMainComponent(Transform screenParent);

	private void SetModuleSelected(bool isSelected)
	{
		ShowScreenObject(isSelected);
		SetTabSelected(isSelected);
		if (isSelected)
		{
			OnSelected();
			PlayOnSelectedSound();
		}
		else
		{
			OnDeselected();
		}
	}

	private void SetTabSelected(bool isSelected)
	{
		if (isSelected)
		{
			tab.SetActiveInToggleGroup(_toggleGroup);
		}
	}

	protected virtual void OnSelected()
	{
	}

	protected virtual void OnDeselected()
	{
	}

	protected void ShowScreenObject(bool show)
	{
		ScreenModule.SetObjectActive(show);
	}

	protected void ShowTab(CharacterSheetTabType tabToOpen)
	{
		tab.ShowTab(showTabForTheseContexts.Contains(tabToOpen));
	}

	protected void SetTabName(string name)
	{
		tab.SetTabName(name);
	}

	public virtual bool PrepareForContext(CharacterSheetTabType tabToOpen)
	{
		ShowTab(tabToOpen);
		bool flag = DefaultTabType == tabToOpen || AssociatedTabTypes.Contains(tabToOpen);
		SetModuleSelected(flag);
		return flag;
	}

	protected virtual void PlayOnSelectedSound()
	{
		SoundScreen.PlayASound("Play_MainMenu_SwitchTab");
	}
}
