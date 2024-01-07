using System;
using Travellers.UI.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.PlayerInventory;

public class ItemActionDeleteConfirm : UIScreenComponent
{
	[SerializeField]
	private Button _yesButton;

	[SerializeField]
	private Button _noButton;

	[SerializeField]
	private TextStylerTextMeshPro _titleText;

	private IInventoryActionSource _actionSource;

	private Action<IInventoryActionSource> _confirmAction;

	private Action _rejectAction;

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
		_yesButton.onClick.AddListener(Confirm);
		_noButton.onClick.AddListener(Reject);
	}

	protected override void ProtectedDispose()
	{
	}

	public void SetActionSource(IInventoryActionSource actionSource, Action<IInventoryActionSource> confirmAction, Action rejectAction)
	{
		_rejectAction = rejectAction;
		_confirmAction = confirmAction;
		_actionSource = actionSource;
		if (actionSource == null || actionSource.InventorySlotSourceData == null || actionSource.InventorySlotSourceData.InventoryItemData == null)
		{
			SetObjectActive(isActive: false);
			return;
		}
		_titleText.SetText($"Delete {actionSource.InventorySlotSourceData.InventoryItemData.name}?");
		SetObjectActive(isActive: true);
	}

	private void Confirm()
	{
		if (_confirmAction != null)
		{
			_confirmAction(_actionSource);
		}
		SetObjectActive(isActive: false);
	}

	private void Reject()
	{
		if (_rejectAction != null)
		{
			_rejectAction();
		}
		SetObjectActive(isActive: false);
	}
}
