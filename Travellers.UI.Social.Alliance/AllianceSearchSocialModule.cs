using System;
using System.Collections.Generic;
using Travellers.UI.Framework;
using Travellers.UI.PlayerInventory;
using UnityEngine;

namespace Travellers.UI.Social.Alliance;

public class AllianceSearchSocialModule : SocialSheetModule
{
	private readonly HashSet<CharacterSheetTabType> _showTabForTheseContexts = new HashSet<CharacterSheetTabType>
	{
		CharacterSheetTabType.Crew,
		CharacterSheetTabType.YourAlliance,
		CharacterSheetTabType.SearchAlliance
	};

	protected override string tabName => "ALLIANCE";

	protected override HashSet<CharacterSheetTabType> showTabForTheseContexts => _showTabForTheseContexts;

	public override CharacterSheetTabType DefaultTabType => CharacterSheetTabType.SearchAlliance;

	public AllianceSearchSocialModule(Transform screenParent, Transform tabParent, Action<CharacterSheetTabType> onTabPressed, UIToggleGroup toggleGroup, SocialInfoPanel infoPanel)
		: base(screenParent, tabParent, onTabPressed, infoPanel, toggleGroup)
	{
	}

	protected override UIScreenComponent CreateMainComponent(Transform screenParent)
	{
		return UIObjectFactory.Create<AllianceSearchScreen>(UIElementType.ScreenComponent, UIFillType.KeepOriginalAnchoring, screenParent, isObjectActive: false);
	}

	protected override void OnSelected()
	{
		AllianceSearchScreen allianceSearchScreen = (AllianceSearchScreen)base.ScreenModule;
		allianceSearchScreen.CheckAllianceList();
		_infoPanel.SetObjectActive(isActive: true);
		_infoPanel.SetupForNeutralState();
	}
}
