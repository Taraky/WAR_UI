using System;
using System.Collections.Generic;
using Travellers.UI.Framework;
using Travellers.UI.PlayerInventory;
using Travellers.UI.Social.Alliance;
using UnityEngine;

namespace Travellers.UI.Social.Crew;

public class CrewSocialModule : SocialSheetModule
{
	private readonly HashSet<CharacterSheetTabType> _showTabForTheseContexts = new HashSet<CharacterSheetTabType>
	{
		CharacterSheetTabType.Crew,
		CharacterSheetTabType.YourAlliance,
		CharacterSheetTabType.SearchAlliance
	};

	protected override string tabName => "CREW";

	protected override HashSet<CharacterSheetTabType> showTabForTheseContexts => _showTabForTheseContexts;

	public override CharacterSheetTabType DefaultTabType => CharacterSheetTabType.Crew;

	public CrewSocialModule(Transform screenParent, Transform tabParent, Action<CharacterSheetTabType> onTabPressed, UIToggleGroup toggleGroup, SocialInfoPanel infoPanel)
		: base(screenParent, tabParent, onTabPressed, infoPanel, toggleGroup)
	{
	}

	protected override UIScreenComponent CreateMainComponent(Transform screenParent)
	{
		return UIObjectFactory.Create<CrewScreen>(UIElementType.ScreenComponent, UIFillType.KeepOriginalAnchoring, screenParent, isObjectActive: false);
	}

	protected override void OnSelected()
	{
		CrewScreen crewScreen = (CrewScreen)base.ScreenModule;
		crewScreen.CheckCachedCrewData();
		_infoPanel.SetObjectActive(isActive: false);
	}
}
