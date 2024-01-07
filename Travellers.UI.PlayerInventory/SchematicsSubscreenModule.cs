using System;
using System.Collections.Generic;
using Travellers.UI.Framework;
using Travellers.UI.Tutorial;
using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.PlayerInventory;

public class SchematicsSubscreenModule : CharacterSheetModule
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

	private HashSet<CharacterSheetTabType> _associatedTabTypes = new HashSet<CharacterSheetTabType>();

	protected override string tabName => "SCHEMATICS";

	protected override HashSet<CharacterSheetTabType> showTabForTheseContexts => _showTabForTheseContexts;

	public override CharacterSheetTabType DefaultTabType => CharacterSheetTabType.Schematics;

	protected override HashSet<CharacterSheetTabType> AssociatedTabTypes => _associatedTabTypes;

	protected override TutorialHookType tutorialHookType => TutorialHookType.Inventory_SchematicsTab;

	public SchematicsSubscreenModule(Transform screenParent, Transform tabParent, HorizontalOrVerticalLayoutGroup layoutParent, Action<CharacterSheetTabType> onTabPressed, UIToggleGroup toggleGroup)
		: base(screenParent, tabParent, layoutParent, onTabPressed, toggleGroup)
	{
	}

	protected override UIScreenComponent CreateMainComponent(Transform screenParent)
	{
		return UIObjectFactory.Create<SchematicsSubScreen>(UIElementType.ScreenComponent, UIFillType.KeepOriginalAnchoring, screenParent, isObjectActive: false);
	}
}
