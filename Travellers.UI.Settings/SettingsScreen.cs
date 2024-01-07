using System;
using Travellers.UI.Framework;
using Travellers.UI.Options;
using Travellers.UI.Sound;
using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.Settings;

[InjectableClass]
public class SettingsScreen : UIScreen
{
	public enum SettingsTab
	{
		Graphics,
		Audio,
		Controls
	}

	[Serializable]
	public class TabContent
	{
		public UIInventoryTabToggleController tab;

		public GameObject container;
	}

	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private UIButtonController _closeButtonController;

	[SerializeField]
	private GameObject _blur;

	[SerializeField]
	private UIKeyBindings _keyBindingsTabScreen;

	[SerializeField]
	private TabContent[] tabs;

	public bool IsKeyRebindActive => _keyBindingsTabScreen.IsKeyRebindActive;

	protected override void ProtectedInit()
	{
		for (int i = 0; i < tabs.Length; i++)
		{
			int scopedIndex = i;
			tabs[i].tab.SetButtonEvent(delegate
			{
				ShowSettingsTab((SettingsTab)scopedIndex);
			});
		}
		_closeButtonController.SetButtonEvent(OnCloseButtonPressed);
		ShowSettingsTab(SettingsTab.Graphics);
	}

	protected override void ProtectedDispose()
	{
	}

	protected override void AddListeners()
	{
	}

	public void SetBlur(bool show)
	{
		_blur.SetActive(show);
	}

	private void OnCloseButtonPressed()
	{
		UIWindowController.PopState<SettingsScreenUIState>();
		SoundScreen.PlayASound("Play_MainMenu_Select");
	}

	private void ShowSettingsTab(SettingsTab tab)
	{
		SoundScreen.PlayASound("Play_MainMenu_SwitchTab");
		for (int i = 0; i < tabs.Length; i++)
		{
			tabs[i].tab.SetButtonState((i == (int)tab) ? UIToggleState.Selected : UIToggleState.Deselected);
			tabs[i].container.SetActive(i == (int)tab);
		}
		ResetScrollRect();
	}

	private void HideAllTabs()
	{
		for (int i = 0; i < tabs.Length; i++)
		{
			tabs[i].tab.SetButtonState(UIToggleState.Deselected);
			tabs[i].container.SetActive(value: false);
		}
		ResetScrollRect();
	}

	private void ResetScrollRect()
	{
		scrollRect.StopMovement();
		scrollRect.normalizedPosition = Vector2.up;
	}

	public static void ToggleSfx(bool selected)
	{
		if (Input.GetMouseButtonUp(0))
		{
			if (selected)
			{
				SoundScreen.PlayASound("Play_MainMenu_WindowOpen");
			}
			else
			{
				SoundScreen.PlayASound("Play_MainMenu_WindowClose");
			}
		}
	}
}
