using System;
using System.Collections.Generic;
using Travellers.UI.Framework;
using Travellers.UI.PlayerInventory;
using UnityEngine;

namespace Travellers.UI.Social.Alliance;

public class YourAllianceSocialModule : SocialSheetModule
{
	private readonly HashSet<CharacterSheetTabType> _showTabForTheseContexts = new HashSet<CharacterSheetTabType>
	{
		CharacterSheetTabType.Crew,
		CharacterSheetTabType.YourAlliance,
		CharacterSheetTabType.SearchAlliance
	};

	protected override string tabName => "CREATE ALLIANCE";

	protected override HashSet<CharacterSheetTabType> showTabForTheseContexts => _showTabForTheseContexts;

	public override CharacterSheetTabType DefaultTabType => CharacterSheetTabType.YourAlliance;

	public YourAllianceSocialModule(Transform screenParent, Transform tabParent, Action<CharacterSheetTabType> onTabPressed, UIToggleGroup toggleGroup, SocialInfoPanel infoPanel)
		: base(screenParent, tabParent, onTabPressed, infoPanel, toggleGroup)
	{
	}

	protected override UIScreenComponent CreateMainComponent(Transform screenParent)
	{
		return UIObjectFactory.Create<YourAllianceScreen>(UIElementType.ScreenComponent, UIFillType.KeepOriginalAnchoring, screenParent, isObjectActive: false);
	}

	protected override void OnSelected()
	{
		YourAllianceScreen yourAllianceScreen = (YourAllianceScreen)base.ScreenModule;
		yourAllianceScreen.RefreshForPlayerState();
		_infoPanel.SetObjectActive(isActive: true);
		_infoPanel.SetupForNeutralState();
	}
}
