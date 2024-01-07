using System;
using System.Collections.Generic;
using Travellers.UI.Framework;
using Travellers.UI.Tutorial;
using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.PlayerInventory;

public class StorageDisplayModule : CharacterSheetModule
{
	private readonly HashSet<CharacterSheetTabType> _showTabForTheseContexts = new HashSet<CharacterSheetTabType> { CharacterSheetTabType.StorageObject };

	protected override string tabName => "STORAGE";

	protected override HashSet<CharacterSheetTabType> showTabForTheseContexts => _showTabForTheseContexts;

	public override CharacterSheetTabType DefaultTabType => CharacterSheetTabType.StorageObject;

	protected override TutorialHookType tutorialHookType => TutorialHookType.Inventory_StorageContainer;

	protected override float rightSideSpacing => 70f;

	public StorageDisplayModule(Transform screenParent, Transform tabParent, HorizontalOrVerticalLayoutGroup layoutParent, Action<CharacterSheetTabType> onTabPressed, UIToggleGroup toggleGroup)
		: base(screenParent, tabParent, layoutParent, onTabPressed, toggleGroup)
	{
	}

	protected override UIScreenComponent CreateMainComponent(Transform screenParent)
	{
		InventoryChestScreen inventoryChestScreen = UIObjectFactory.Create<InventoryChestScreen>(UIElementType.ScreenComponent, UIFillType.KeepOriginalAnchoring, screenParent, isObjectActive: false);
		inventoryChestScreen.SetNameUpdateFunc(delegate(string name)
		{
			tab.SetTabName(name);
		});
		return inventoryChestScreen;
	}

	protected override void PlayOnSelectedSound()
	{
	}
}
