using System.Collections.Generic;
using UnityEngine;

namespace Travellers.UI.Components;

public class Toolbar : MonoBehaviour
{
	public enum Tab
	{
		None,
		Changelog,
		Chat,
		Help,
		PlayerControls,
		ShipControls,
		BuildControls
	}

	[SerializeField]
	private ToolbarButton[] buttons;

	[SerializeField]
	private GameObject[] correspondingObjects;

	[SerializeField]
	private Tab[] correspondingTabs;

	private Dictionary<Tab, int> tabIndexDict;

	private Tab currentTab;

	public Tab CurrentTab => currentTab;

	private void Start()
	{
		SelectNone();
		tabIndexDict = new Dictionary<Tab, int>();
		for (int i = 0; i < buttons.Length; i++)
		{
			if (buttons[i].startSelected)
			{
				ToolbarButtonPressed(buttons[i]);
			}
			tabIndexDict[correspondingTabs[i]] = i;
		}
	}

	public void ToolbarButtonPressed(ToolbarButton button)
	{
		for (int i = 0; i < buttons.Length; i++)
		{
			correspondingObjects[i].SetActive(buttons[i] == button);
			buttons[i].selected = buttons[i] == button;
			if (correspondingTabs.Length > 0 && buttons[i] == button)
			{
				currentTab = correspondingTabs[i];
			}
		}
	}

	public void SelectTab(Tab tab)
	{
		if (tab == Tab.None)
		{
			SelectNone();
		}
		else
		{
			ToolbarButtonPressed(buttons[tabIndexDict[tab]]);
		}
	}

	public void SelectNone()
	{
		for (int i = 0; i < buttons.Length; i++)
		{
			correspondingObjects[i].SetActive(value: false);
			buttons[i].selected = false;
		}
		currentTab = Tab.None;
	}
}
