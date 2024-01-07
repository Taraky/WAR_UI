using System;
using System.Collections;
using TMPro;
using Travellers.UI.Alerts;
using Travellers.UI.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.InfoPopups;

public class GeneralDialogPopup : UIPopup
{
	[SerializeField]
	private TextStylerTextMeshPro _titleText;

	[SerializeField]
	private TextStylerTextMeshPro _messageText;

	[SerializeField]
	private GameObject _titleBox;

	[SerializeField]
	private GameObject _messageTextObject;

	[SerializeField]
	private GameObject _inputFieldObject;

	[SerializeField]
	private TMP_InputField _inputField;

	[SerializeField]
	private TextStylerTextMeshPro _inputFieldPlaceholder;

	[SerializeField]
	private GameObject _buttons;

	[SerializeField]
	private GameObject _confirmButtons;

	[SerializeField]
	private Image _solidBackgroundImage;

	[SerializeField]
	private UIButtonController _firstButtonController;

	[SerializeField]
	private UIButtonController _secondButtonController;

	[SerializeField]
	private UIButtonController _thirdButtonController;

	[SerializeField]
	private UIButtonController _confirmButtonController;

	[SerializeField]
	private UIButtonController _cancelButtonController;

	[SerializeField]
	private Image _confirmEnableProgress;

	private UIButtonSound _confirmButtonSound;

	private UIButtonSound _cancelButtonSound;

	private Action _firstCallback;

	private Action _secondCallback;

	private Action _thirdCallback;

	private Action _closedCallback;

	public GeneralDialogPopupState ThisPopupsState { get; private set; }

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
		_firstButtonController.SetButtonEvent(OnFirstButtonPressed);
		_secondButtonController.SetButtonEvent(OnSecondButtonPressed);
		_thirdButtonController.SetButtonEvent(OnThirdButtonPressed);
		_confirmButtonController.SetButtonEvent(OnFirstButtonPressed);
		_cancelButtonController.SetButtonEvent(OnSecondButtonPressed);
		_confirmButtonSound = _confirmButtonController.GetComponent<UIButtonSound>();
		_cancelButtonSound = _cancelButtonController.GetComponent<UIButtonSound>();
	}

	public void SetData(CreateAlertDialogEvent evt, GeneralDialogPopupState thisPopupsState)
	{
		switch (evt.DialogType)
		{
		case AlertDialogType.WithInputField:
			ShowTwoButtonWithInputField(evt);
			break;
		case AlertDialogType.WithoutInputField:
			ShowTextAlertWithoutInput(evt);
			break;
		case AlertDialogType.Confirmation:
			ShowConfirmationDialog(evt);
			break;
		}
		ThisPopupsState = thisPopupsState;
	}

	private void ShowTextAlertWithoutInput(CreateAlertDialogEvent evt)
	{
		_titleText.SetText(evt.Title);
		_titleText.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(evt.Title));
		_messageText.SetText(evt.Msg);
		_messageTextObject.SetActive(!string.IsNullOrEmpty(evt.Msg));
		_firstButtonController.SetText(evt.FirstButtonText);
		_secondButtonController.SetText(evt.SecondButtonText);
		_secondButtonController.SetObjectActive(!string.IsNullOrEmpty(evt.SecondButtonText));
		_thirdButtonController.SetText(evt.ThirdButtonText);
		_thirdButtonController.SetObjectActive(!string.IsNullOrEmpty(evt.ThirdButtonText));
		_confirmButtons.SetActive(value: false);
		_buttons.SetActive(value: true);
		_firstCallback = evt.FirstButtonCallback;
		_secondCallback = evt.SecondButtonCallback;
		_thirdCallback = evt.ThirdButtonCallback;
		_closedCallback = evt.ClosedCallback;
		_inputFieldObject.SetActive(value: false);
		_solidBackgroundImage.gameObject.SetActive(evt.UseSolidBackground);
		base.gameObject.SetActive(value: true);
	}

	public void ShowTwoButtonWithInputField(CreateAlertDialogEvent evt)
	{
		_titleText.SetText(evt.Title);
		_titleBox.SetActive(!string.IsNullOrEmpty(evt.Title));
		_messageTextObject.SetActive(!string.IsNullOrEmpty(evt.Msg));
		_messageText.SetText(evt.Msg);
		_firstButtonController.SetObjectActive(isActive: true);
		_firstButtonController.SetText(evt.FirstButtonText);
		_secondButtonController.SetObjectActive(!string.IsNullOrEmpty(evt.SecondButtonText));
		_secondButtonController.SetText(evt.SecondButtonText);
		_thirdButtonController.SetObjectActive(!string.IsNullOrEmpty(evt.ThirdButtonText));
		_thirdButtonController.SetText(evt.ThirdButtonText);
		_confirmButtons.SetActive(value: false);
		_buttons.SetActive(value: true);
		_inputFieldObject.SetActive(value: true);
		_inputFieldPlaceholder.SetText(evt.InputPlaceholder);
		_firstCallback = delegate
		{
			evt.InputBoxCallback(_inputField.text);
		};
		_secondCallback = evt.SecondButtonCallback;
		_thirdCallback = evt.ThirdButtonCallback;
		_solidBackgroundImage.gameObject.SetActive(evt.UseSolidBackground);
		base.gameObject.SetActive(value: true);
	}

	private void ShowConfirmationDialog(CreateAlertDialogEvent evt)
	{
		_titleText.SetText(evt.Title);
		_titleText.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(evt.Title));
		_messageText.SetText(evt.Msg);
		_messageTextObject.SetActive(!string.IsNullOrEmpty(evt.Msg));
		_buttons.SetActive(value: false);
		_confirmButtons.SetActive(value: true);
		float? confirmationEnableDelay = evt.ConfirmationEnableDelay;
		StartCoroutine(ConfirmEnableCoroutine((!confirmationEnableDelay.HasValue) ? 0f : confirmationEnableDelay.Value));
		_confirmButtonController.SetText(evt.FirstButtonText);
		_cancelButtonController.SetText(evt.SecondButtonText);
		_cancelButtonController.SetObjectActive(!string.IsNullOrEmpty(evt.SecondButtonText));
		_firstCallback = evt.FirstButtonCallback;
		_secondCallback = evt.SecondButtonCallback;
		_thirdCallback = null;
		_closedCallback = evt.ClosedCallback;
		_inputFieldObject.SetActive(value: false);
		_solidBackgroundImage.gameObject.SetActive(evt.UseSolidBackground);
		_confirmButtonSound.OverrideSoundId = evt.OverrideConfirmSound;
		_cancelButtonSound.OverrideSoundId = evt.OverrideCancelSound;
		base.gameObject.SetActive(value: true);
	}

	private IEnumerator ConfirmEnableCoroutine(float duration)
	{
		_confirmButtonController.SetButtonEnabled(isShowing: false);
		for (float timer = 0f; timer < duration; timer += Time.deltaTime)
		{
			_confirmEnableProgress.fillAmount = timer / duration;
			yield return null;
		}
		_confirmEnableProgress.fillAmount = 1f;
		_confirmButtonController.SetButtonEnabled(isShowing: true);
	}

	public void OnFirstButtonPressed()
	{
		HandleCallback(_firstCallback);
	}

	public void OnSecondButtonPressed()
	{
		HandleCallback(_secondCallback);
	}

	public void OnThirdButtonPressed()
	{
		HandleCallback(_thirdCallback);
	}

	private void HandleCallback(Action callback)
	{
		callback?.Invoke();
		DialogPopupFacade.ClosePopup(this);
	}

	protected override void Deactivate()
	{
		if (_closedCallback != null)
		{
			_closedCallback();
		}
		_firstCallback = null;
		_secondCallback = null;
		_thirdCallback = null;
		_closedCallback = null;
	}

	protected override void ProtectedDispose()
	{
		_firstCallback = null;
		_secondCallback = null;
		_thirdCallback = null;
		_closedCallback = null;
	}
}
