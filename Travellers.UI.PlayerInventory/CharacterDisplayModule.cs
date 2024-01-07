using System;
using System.Collections.Generic;
using Travellers.UI.Framework;
using Travellers.UI.Tutorial;
using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.PlayerInventory;

public class CharacterDisplayModule : CharacterSheetModule
{
	private readonly HashSet<CharacterSheetTabType> _showTabForTheseContexts = new HashSet<CharacterSheetTabType>
	{
		CharacterSheetTabType.MultitoolCraft,
		CharacterSheetTabType.Schematics,
		CharacterSheetTabType.Knowledge,
		CharacterSheetTabType.OldCrew,
		CharacterSheetTabType.Character,
		CharacterSheetTabType.Logbook
	};

	protected override string tabName => "CHARACTER";

	protected override HashSet<CharacterSheetTabType> showTabForTheseContexts => _showTabForTheseContexts;

	public override CharacterSheetTabType DefaultTabType => CharacterSheetTabType.Character;

	protected override TutorialHookType tutorialHookType => TutorialHookType.Inventory_CharacterTab;

	public CharacterDisplayModule(Transform screenParent, Transform tabParent, HorizontalOrVerticalLayoutGroup layoutParent, Action<CharacterSheetTabType> onTabPressed, UIToggleGroup toggleGroup)
		: base(screenParent, tabParent, layoutParent, onTabPressed, toggleGroup)
	{
	}

	protected override UIScreenComponent CreateMainComponent(Transform screenParent)
	{
		return UIObjectFactory.Create<InventoryCharacterSubscreen>(UIElementType.ScreenComponent, UIFillType.KeepOriginalAnchoring, screenParent, isObjectActive: false);
	}
}
