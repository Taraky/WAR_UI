using System.Collections.Generic;
using Travellers.UI.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.PlayerInventory;

public class LockboxCategory : UIScreenComponent
{
	[SerializeField]
	private Text _nameText;

	[SerializeField]
	private Transform _contentRoot;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private GridLayoutGroup _gridLayout;

	[SerializeField]
	private Button _downButton;

	[SerializeField]
	private Button _upButton;

	private Dictionary<int, LockboxItem> _allItems = new Dictionary<int, LockboxItem>(Comparers.IntComparer);

	private List<int> _deletedItems = new List<int>();

	public string Name
	{
		set
		{
			_nameText.text = value;
		}
	}

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
	}

	protected override void ProtectedDispose()
	{
	}

	public void SetScale(Vector2 itemScale)
	{
		_gridLayout.cellSize = itemScale;
	}

	public void ToggleShow()
	{
		Show(!_contentRoot.gameObject.activeInHierarchy);
	}

	public void Show(bool show)
	{
		if (_contentRoot != null)
		{
			_contentRoot.gameObject.SetActive(show);
		}
		if (_downButton != null)
		{
			_downButton.gameObject.SetActive(!show);
		}
		if (_upButton != null)
		{
			_upButton.gameObject.SetActive(show);
		}
		if (show)
		{
			_canvasGroup.alpha = 0f;
			Invoke("DoShow", 0.05f);
		}
	}

	private void DoShow()
	{
		_canvasGroup.alpha = 1f;
	}

	public void AddItem(InventorySlotData data)
	{
		if (!_allItems.ContainsKey(data.serverItemId))
		{
			LockboxItem lockboxItem = UIObjectFactory.Create<LockboxItem>(UIElementType.ScreenComponent, UIFillType.KeepOriginalAnchoring, _contentRoot, isObjectActive: true);
			lockboxItem.SetData(data);
			_allItems.Add(data.serverItemId, lockboxItem);
		}
		else
		{
			_allItems[data.serverItemId].SetData(data);
		}
		_deletedItems.Remove(data.serverItemId);
	}

	public void StartRefresh()
	{
		_deletedItems.Clear();
		foreach (int key in _allItems.Keys)
		{
			_deletedItems.Add(key);
		}
	}

	public bool StopRefresh()
	{
		for (int i = 0; i < _deletedItems.Count; i++)
		{
			Object.Destroy(_allItems[_deletedItems[i]].gameObject);
			_allItems.Remove(_deletedItems[i]);
		}
		return _allItems.Count == 0;
	}
}
