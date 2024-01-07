using System;
using System.Collections.Generic;
using Travellers.UI.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Travellers.UI.PlayerInventory;

public class ShipSchematicSlot : UIScreenComponent, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
{
	[SerializeField]
	private Text _nameText;

	[SerializeField]
	private GameObject _selectedCarat;

	[SerializeField]
	private Button _deleteButtonGameObject;

	[SerializeField]
	private Button _renameButtonGameObject;

	[SerializeField]
	private Color _unselectedTextColor = Color.white;

	[SerializeField]
	private Color _selectedTextColor = Color.white;

	private Action _mainButtonClicked;

	private List<GameObject> _miniButtons = new List<GameObject>();

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
		SetMiniButtonsEnabled(enabled: false);
		SetSelected(selected: false);
	}

	protected override void ProtectedDispose()
	{
	}

	public void Setup(string name, Action mainButtonClicked, Action renameButtonClicked, Action deleteButtonClicked)
	{
		_nameText.text = name;
		_mainButtonClicked = mainButtonClicked;
		_miniButtons.Clear();
		RegisterMiniButton(_renameButtonGameObject, renameButtonClicked);
		RegisterMiniButton(_deleteButtonGameObject, deleteButtonClicked);
	}

	private void RegisterMiniButton(Button miniButton, Action handler)
	{
		if (handler != null)
		{
			_miniButtons.Add(miniButton.gameObject);
			miniButton.onClick.AddListener(delegate
			{
				handler();
			});
			miniButton.gameObject.SetActive(value: false);
		}
		else
		{
			miniButton.onClick.RemoveAllListeners();
			miniButton.gameObject.SetActive(value: false);
		}
	}

	public void SetSelected(bool selected)
	{
		_selectedCarat.SetActive(selected);
		_nameText.color = ((!selected) ? _unselectedTextColor : _selectedTextColor);
	}

	public void HandleMainButtonClicked()
	{
		_mainButtonClicked.SafeInvoke();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		SetMiniButtonsEnabled(enabled: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		SetMiniButtonsEnabled(enabled: false);
	}

	private void SetMiniButtonsEnabled(bool enabled)
	{
		foreach (GameObject miniButton in _miniButtons)
		{
			miniButton.SetActive(enabled);
		}
	}
}
