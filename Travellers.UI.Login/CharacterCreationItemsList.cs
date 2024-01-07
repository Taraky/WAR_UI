using System.Collections.Generic;
using System.Linq;
using Travellers.UI.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.Login;

public class CharacterCreationItemsList : MonoBehaviour
{
	[SerializeField]
	private CharacterCreationScreen _creationScreen;

	[SerializeField]
	private Text _title;

	[SerializeField]
	private List<CharacterCreationItem> _allItems;

	private ScrollRect _scrollRect;

	private bool _isPrimary = true;

	[SerializeField]
	private RectTransform _selectedOverlay;

	private bool _isInit;

	public bool IsPrimary
	{
		get
		{
			return _isPrimary;
		}
		set
		{
			_isPrimary = value;
		}
	}

	public void Init()
	{
		if (!_isInit)
		{
			if (_allItems == null || _allItems.Count == 0)
			{
				_allItems = GetComponentsInChildren<CharacterCreationItem>(includeInactive: true).ToList();
			}
			for (int i = 0; i < _allItems.Count; i++)
			{
				_allItems[i].List = this;
			}
			Select(_allItems[0]);
			_scrollRect = GetComponentInChildren<ScrollRect>();
			_isInit = true;
		}
	}

	private void AddItemToList()
	{
		CharacterCreationItem characterCreationItem = UIObjectFactory.Instantiate(_creationScreen.ListItemPrefab);
		characterCreationItem.List = this;
		_allItems.Add(characterCreationItem);
	}

	public void SetData(string title, Dictionary<string, CustomisationSettings.GenderItem> data)
	{
		_title.text = title;
		if (data.Count > _allItems.Count)
		{
			int num = data.Count - _allItems.Count;
			for (int i = 0; i < num; i++)
			{
				AddItemToList();
			}
		}
		int num2 = 0;
		foreach (KeyValuePair<string, CustomisationSettings.GenderItem> datum in data)
		{
			_allItems[num2].SetData(num2, (!_creationScreen.IsMale) ? datum.Value.FemaleItem : datum.Value.MaleItem);
			_allItems[num2].gameObject.SetActive(value: true);
			num2++;
		}
		for (int j = num2; j < _allItems.Count; j++)
		{
			_allItems[j].gameObject.SetActive(value: false);
		}
		if ((bool)_scrollRect)
		{
			_scrollRect.horizontalScrollbar.value = 0f;
		}
		Canvas.ForceUpdateCanvases();
	}

	public void Select(int iItem)
	{
		Select(_allItems[iItem]);
	}

	public void Select(CharacterCreationItem item)
	{
		_selectedOverlay.SetParent(item.transform);
		_selectedOverlay.gameObject.SetActive(value: true);
		_selectedOverlay.sizeDelta = (item.transform as RectTransform).sizeDelta + Vector2.one * 2f;
		_selectedOverlay.anchorMin = Vector2.one * 0.5f;
		_selectedOverlay.anchorMax = Vector2.one * 0.5f;
		_selectedOverlay.anchoredPosition = Vector2.zero;
		_creationScreen.SetOption(item.Id, _isPrimary);
	}
}
