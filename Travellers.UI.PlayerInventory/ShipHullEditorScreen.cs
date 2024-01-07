using Travellers.UI.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.PlayerInventory;

public class ShipHullEditorScreen : UIScreen
{
	[SerializeField]
	private UIButtonController _doneButton;

	[SerializeField]
	private Toggle _scaleAvatarToggle;

	protected override void ProtectedInit()
	{
		_doneButton.SetButtonEvent(CloseScreen);
	}

	private void CloseScreen()
	{
		UIWindowController.PopState<ShipyardEditorUIState>();
	}

	protected override void Deactivate()
	{
		base.Deactivate();
		_scaleAvatarToggle.isOn = false;
	}

	protected override void AddListeners()
	{
		_scaleAvatarToggle.onValueChanged.AddListener(HandleScaleAvatarToggleChanged);
	}

	protected override void ProtectedDispose()
	{
		_scaleAvatarToggle.onValueChanged.RemoveListener(HandleScaleAvatarToggleChanged);
	}

	private void HandleScaleAvatarToggleChanged(bool value)
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIShipEditorEvents.ScaleAvatarToggleChanged, value);
	}
}
