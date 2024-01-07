using System;
using Travellers.UI.Social.Alliance;
using UnityEngine;

namespace Travellers.UI.PlayerInventory;

public abstract class SocialSheetModule : BaseSheetModule
{
	protected readonly SocialInfoPanel _infoPanel;

	protected SocialSheetModule(Transform screenParent, Transform tabParent, Action<CharacterSheetTabType> onTabPressed, SocialInfoPanel infoPanel, UIToggleGroup toggleGroup)
		: base(screenParent, tabParent, onTabPressed, toggleGroup)
	{
		_infoPanel = infoPanel;
	}

	public void UpdateTabName(string newName)
	{
		tab.SetTabName(newName);
	}
}
