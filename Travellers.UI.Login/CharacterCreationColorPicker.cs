using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.Login;

public class CharacterCreationColorPicker : MonoBehaviour
{
	[SerializeField]
	private CharacterCreationScreen _creationScreen;

	[SerializeField]
	private Text _title;

	[SerializeField]
	private CharacterCreationColorItem[] _allColours;

	[SerializeField]
	private GridLayoutGroup _grid;

	[SerializeField]
	private ContentSizeFitter _contentSizeFitter;

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
			if (_grid == null)
			{
				_grid = GetComponentInChildren<GridLayoutGroup>();
			}
			if (_contentSizeFitter == null)
			{
				_contentSizeFitter = GetComponentInChildren<ContentSizeFitter>();
			}
			if (_allColours == null || _allColours.Length == 0)
			{
				_allColours = GetComponentsInChildren<CharacterCreationColorItem>(includeInactive: true);
			}
			for (int i = 0; i < _allColours.Length; i++)
			{
				_allColours[i].ColorPicker = this;
			}
			Select(_allColours[0]);
			_isInit = true;
		}
	}

	public void SetData(string title, Color[] colors)
	{
		if (colors.Length % 5 == 0)
		{
			_grid.constraintCount = 5;
			_grid.cellSize = Vector2.one * 49f;
		}
		else
		{
			_grid.constraintCount = 7;
			_grid.cellSize = Vector2.one * 34.5f;
		}
		_title.text = title;
		int num = 0;
		for (num = 0; num < colors.Length; num++)
		{
			_allColours[num].SetData(this, num, colors[num]);
			_allColours[num].gameObject.SetActive(value: true);
		}
		for (int i = num; i < _allColours.Length; i++)
		{
			_allColours[i].gameObject.SetActive(value: false);
		}
		_selectedOverlay.sizeDelta = _grid.cellSize + Vector2.one * 2f;
		if (_contentSizeFitter != null)
		{
			_contentSizeFitter.verticalFit = ((_contentSizeFitter.verticalFit == ContentSizeFitter.FitMode.PreferredSize) ? ContentSizeFitter.FitMode.MinSize : ContentSizeFitter.FitMode.PreferredSize);
		}
		Canvas.ForceUpdateCanvases();
	}

	public void Select(int iColor)
	{
		Select(_allColours[iColor]);
	}

	public void Select(CharacterCreationColorItem item)
	{
		_selectedOverlay.SetParent(item.transform);
		_selectedOverlay.anchorMin = Vector2.zero;
		_selectedOverlay.anchorMax = Vector2.one;
		_selectedOverlay.anchoredPosition = Vector2.zero;
		_selectedOverlay.sizeDelta = Vector2.zero;
		_creationScreen.SetColor(item.Id, _isPrimary);
	}
}
