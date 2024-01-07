using System;
using System.Collections.Generic;
using Travellers.UI.Framework;
using Travellers.UI.PlayerInventory;
using Travellers.UI.Tutorial;
using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.Knowledge;

public class KnowledgeModule : CharacterSheetModule
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

	protected override string tabName => "KNOWLEDGE";

	protected override HashSet<CharacterSheetTabType> showTabForTheseContexts => _showTabForTheseContexts;

	public override CharacterSheetTabType DefaultTabType => CharacterSheetTabType.Knowledge;

	protected override TutorialHookType tutorialHookType => TutorialHookType.Inventory_KnowledgeTab;

	public KnowledgeModule(Transform screenParent, Transform tabParent, HorizontalOrVerticalLayoutGroup layoutParent, Action<CharacterSheetTabType> onTabPressed, UIToggleGroup toggleGroup)
		: base(screenParent, tabParent, layoutParent, onTabPressed, toggleGroup)
	{
	}

	protected override UIScreenComponent CreateMainComponent(Transform screenParent)
	{
		return UIObjectFactory.Create<KnowledgeManagerScreen>(UIElementType.ScreenComponent, UIFillType.KeepOriginalAnchoring, screenParent, isObjectActive: false);
	}
}
