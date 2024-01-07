using System.Collections.Generic;
using Travellers.UI.Framework;
using UnityEngine;

namespace Travellers.UI.PlayerInventory;

public class InventoryActionList : UIScreenComponent
{
	[SerializeField]
	private UIButtonController _buttonPrefabControl;

	private List<UIButtonController> _activeButtons = new List<UIButtonController>();

	private Queue<UIButtonController> _unusedButtonPool = new Queue<UIButtonController>();

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
		_unusedButtonPool.Enqueue(_buttonPrefabControl);
	}

	protected override void ProtectedDispose()
	{
	}

	public void SetListData(List<InventoryActionButtonData> actionButtonList)
	{
		ReturnAllButtons();
		if (actionButtonList == null)
		{
			return;
		}
		foreach (InventoryActionButtonData actionButton in actionButtonList)
		{
			UIButtonController button = GetButton();
			button.SetText(actionButton.Title);
			button.SetButtonEvent(actionButton.Action);
		}
	}

	private UIButtonController GetButton()
	{
		UIButtonController uIButtonController;
		if (_unusedButtonPool.Count > 0)
		{
			uIButtonController = _unusedButtonPool.Dequeue();
		}
		else
		{
			UIButtonController uIButtonController2 = UIObjectFactory.Instantiate(_buttonPrefabControl, base.transform, UIFillType.FillParentTransform);
			uIButtonController = uIButtonController2.GetComponent<UIButtonController>();
		}
		uIButtonController.SetObjectActive(isActive: true);
		return uIButtonController;
	}

	private void ReturnAllButtons()
	{
		foreach (UIButtonController activeButton in _activeButtons)
		{
			activeButton.SetObjectActive(isActive: false);
			_unusedButtonPool.Enqueue(activeButton);
		}
		_activeButtons.Clear();
	}
}
