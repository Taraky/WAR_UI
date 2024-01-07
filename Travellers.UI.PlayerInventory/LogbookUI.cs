using Travellers.UI.Framework;
using UnityEngine;

namespace Travellers.UI.PlayerInventory;

public class LogbookUI : UIScreenComponent
{
	public enum LogbookTab
	{
		Diary,
		Codex,
		Photos,
		TOTAL
	}

	private LoreUI _loreUI;

	public GameObject[] tabs = new GameObject[3];

	protected override void Activate()
	{
		bool flag = true;
		for (int i = 0; i < 3; i++)
		{
			if (i < tabs.Length && tabs[i] != null && tabs[i].activeSelf)
			{
				flag = false;
			}
		}
		if (flag)
		{
			ShowTab(LogbookTab.Codex);
		}
	}

	public void ShowTab(int tab)
	{
		if (tabs == null)
		{
			return;
		}
		for (int i = 0; i < 3; i++)
		{
			if (i < tabs.Length && tabs[i] != null)
			{
				tabs[i].SetActive(i == tab);
			}
		}
	}

	public void ShowTab(LogbookTab tab)
	{
		ShowTab((int)tab);
	}

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
		_loreUI = GetComponentInChildren<LoreUI>(includeInactive: true);
		_loreUI.Init();
	}

	protected override void ProtectedDispose()
	{
	}
}
