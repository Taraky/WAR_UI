using System;
using Bossa.Travellers.Game;
using UnityEngine;

namespace Travellers.UI.InfoPopups;

public class ModalPopupButtonInstance : MonoBehaviour
{
	[SerializeField]
	private TextStylerTextMeshPro _label;

	private ModalPopupButtonAction _buttonAction;

	private Action<ModalPopupButtonAction> _onPressedDelegate;

	public void Setup(string label, ModalPopupButtonAction buttonAction, Action<ModalPopupButtonAction> onPressedDelegate)
	{
		_label.SetText(label);
		_buttonAction = buttonAction;
		_onPressedDelegate = onPressedDelegate;
	}

	private void OnDisable()
	{
		_onPressedDelegate = null;
	}

	public void OnButtonPressed()
	{
		if (_onPressedDelegate != null)
		{
			_onPressedDelegate(_buttonAction);
		}
	}
}
