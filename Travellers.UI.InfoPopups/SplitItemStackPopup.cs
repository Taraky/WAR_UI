using Travellers.UI.Framework;
using Travellers.UI.Login;
using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.InfoPopups;

public class SplitItemStackPopup : UIPopup
{
	[SerializeField]
	private TextStylerTextMeshPro _titleText;

	[SerializeField]
	private SplitAmountDisplay _originAmountDisplay;

	[SerializeField]
	private SplitAmountDisplay _destinationAmountDisplay;

	[SerializeField]
	private UIButtonController _okButtonController;

	[SerializeField]
	private UIButtonController _cancelButtonController;

	[SerializeField]
	private Slider _slider;

	private SplitItemData _splitItemPackage;

	private const string SplitTitleFormat = "Splitting stack of {0}";

	private int _newAmount;

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
		_slider.maxValue = 1f;
		_originAmountDisplay.SetValueToDisplay(0);
		_destinationAmountDisplay.SetValueToDisplay(0);
		_originAmountDisplay.SetUpdateCallback(UpdateOriginValue);
		_originAmountDisplay.RemoveInput();
		_destinationAmountDisplay.SetUpdateCallback(UpdateDestinationValue);
		_okButtonController.SetButtonEvent(OnConfirm);
		_cancelButtonController.SetButtonEvent(OnReject);
		_slider.onValueChanged.AddListener(OnSliderChange);
	}

	protected override void ProtectedDispose()
	{
	}

	public void SetDataForPopup(SplitItemData splitItemPackage)
	{
		_titleText.SetText($"Splitting stack of {splitItemPackage.InventorySlotData.InventoryItemData.name}");
		_splitItemPackage = splitItemPackage;
		_slider.maxValue = splitItemPackage.InventorySlotData.amount - 1;
		_slider.minValue = 1f;
		_originAmountDisplay.SetValueRange(1, splitItemPackage.InventorySlotData.amount - 1);
		_destinationAmountDisplay.SetValueRange(1, splitItemPackage.InventorySlotData.amount - 1);
		UpdateDestinationValue(1);
	}

	private void UpdateDestinationValue(int value)
	{
		UpdateDisplayValues(_splitItemPackage.InventorySlotData.amount - value, value);
		UpdateSlider(value);
	}

	private void UpdateOriginValue(int value)
	{
		UpdateDisplayValues(value, _splitItemPackage.InventorySlotData.amount - value);
		UpdateSlider(_splitItemPackage.InventorySlotData.amount - value);
	}

	private void UpdateDisplayValues(int origin, int destination)
	{
		_originAmountDisplay.SetValueToDisplay(origin);
		_destinationAmountDisplay.SetValueToDisplay(destination);
		_newAmount = destination;
	}

	private void UpdateSlider(int destination)
	{
		_slider.value = Mathf.Min(1, destination);
	}

	private void OnSliderChange(float value)
	{
		int num = (int)value;
		int origin = _splitItemPackage.InventorySlotData.amount - num;
		UpdateDisplayValues(origin, num);
	}

	private void OnConfirm()
	{
		if (_splitItemPackage.Callback != null)
		{
			InventorySlotData inventorySlotData = _splitItemPackage.InventorySlotData.Clone();
			inventorySlotData.amount = Mathf.Clamp(_newAmount, 1, _splitItemPackage.InventorySlotData.amount - 1);
			inventorySlotData.SetAsNewlySplit();
			_splitItemPackage.InventorySlotData.amount -= _newAmount;
			_splitItemPackage.Callback(inventorySlotData);
			UIWindowController.PopState<SplitItemStackPopupState>();
		}
	}

	private void OnReject()
	{
		UIWindowController.PopState<SplitItemStackPopupState>();
	}
}
