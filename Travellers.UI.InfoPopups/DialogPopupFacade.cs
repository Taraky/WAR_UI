using System;
using System.Collections.Generic;
using GameStateMachine;
using Travellers.UI.Alerts;
using Travellers.UI.Framework;
using Travellers.UI.Login;
using UnityEngine;
using WAUtilities.Logging;

namespace Travellers.UI.InfoPopups;

public class DialogPopupFacade
{
	private static Queue<DialogWrapper> _dialogQueue = new Queue<DialogWrapper>();

	public static void ClearQueue()
	{
		_dialogQueue.Clear();
	}

	public static void ShowConfirmationDialog(string title, string message, Action onConfirm, string confirmText = "OK", string cancelText = "CANCEL", Action onCancel = null, bool useSolidBackground = true, float? confirmationEnableDelay = null)
	{
		AlertDialogType dialogType = AlertDialogType.Confirmation;
		CreateAlertDialogEvent dataToAdd = new CreateAlertDialogEvent(dialogType, title, message, string.Empty, confirmText, cancelText, string.Empty, onConfirm, onCancel, null, null, null, useSolidBackground, null, null, confirmationEnableDelay);
		OpenGeneralDialog(dataToAdd);
	}

	public static void ShowConfirmationDialogAndOverrideSounds(string title, string message, Action onConfirm, Action onCancel, string confirmText = "OK", string cancelText = "CANCEL", bool useSolidBackground = true, string overrideConfirmSound = null, string overrideCancelSound = null, float? confirmationEnableDelay = null)
	{
		AlertDialogType dialogType = AlertDialogType.Confirmation;
		CreateAlertDialogEvent dataToAdd = new CreateAlertDialogEvent(dialogType, title, message, string.Empty, confirmText, cancelText, string.Empty, onConfirm, onCancel, null, null, null, useSolidBackground, overrideConfirmSound, overrideCancelSound, confirmationEnableDelay);
		OpenGeneralDialog(dataToAdd);
	}

	public static void ShowOkDialog(string title, string message, Action onConfirm = null, string confirmText = "OK", bool useSolidBackground = true, float? confirmationEnableDelay = null)
	{
		AlertDialogType dialogType = AlertDialogType.Confirmation;
		bool useSolidBg = useSolidBackground;
		CreateAlertDialogEvent dataToAdd = new CreateAlertDialogEvent(dialogType, title, message, string.Empty, confirmText, string.Empty, string.Empty, onConfirm, null, null, null, null, useSolidBg, null, null, confirmationEnableDelay);
		OpenGeneralDialog(dataToAdd);
	}

	public static void ShowTwoButtonDialog(string title, string message, string firstButtonText = "OK", string secondButtonText = "CLOSE", Action onCloseFirst = null, Action onCloseSecond = null, bool useSolidBackground = true, float? confirmationEnableDelay = null)
	{
		AlertDialogType dialogType = AlertDialogType.WithoutInputField;
		CreateAlertDialogEvent dataToAdd = new CreateAlertDialogEvent(dialogType, title, message, string.Empty, firstButtonText, secondButtonText, string.Empty, onCloseFirst, onCloseSecond, null, null, null, useSolidBackground, null, null, confirmationEnableDelay);
		OpenGeneralDialog(dataToAdd);
	}

	public static void ShowThreeButtonDialog(string title, string message, string firstButtonText = "OK", string secondButtonText = "CLOSE", string thirdButtonText = "BYE", Action onCloseFirst = null, Action onCloseSecond = null, Action onCloseThird = null, bool useSolidBackground = true, float? confirmationEnableDelay = null)
	{
		AlertDialogType dialogType = AlertDialogType.WithoutInputField;
		CreateAlertDialogEvent dataToAdd = new CreateAlertDialogEvent(dialogType, title, message, string.Empty, firstButtonText, secondButtonText, thirdButtonText, onCloseFirst, onCloseSecond, onCloseThird, null, null, useSolidBackground, null, null, confirmationEnableDelay);
		OpenGeneralDialog(dataToAdd);
	}

	public static void ShowInputWithSingleButton(string title, string placeholder, string firstButtonText, Action<string> onPress, float? confirmationEnableDelay = null)
	{
		AlertDialogType dialogType = AlertDialogType.WithInputField;
		CreateAlertDialogEvent dataToAdd = new CreateAlertDialogEvent(dialogType, title, string.Empty, placeholder, firstButtonText, string.Empty, string.Empty, null, null, null, null, onPress, useSolidBg: true, null, null, confirmationEnableDelay);
		OpenGeneralDialog(dataToAdd);
	}

	public static void ShowInputWithTwoButtons(string title, string message, string placeholder, string firstButtonText, string secondButtonText, Action<string> inputBoxCallback, Action secondButtonpress = null, float? confirmationEnableDelay = null)
	{
		AlertDialogType dialogType = AlertDialogType.WithInputField;
		CreateAlertDialogEvent dataToAdd = new CreateAlertDialogEvent(dialogType, title, message, placeholder, firstButtonText, secondButtonText, string.Empty, null, secondButtonpress, null, null, inputBoxCallback, useSolidBg: true, null, null, confirmationEnableDelay);
		OpenGeneralDialog(dataToAdd);
	}

	public static void ShowSpatialErrorDialog(string message, string header, Action returnToLoginScreen, bool quitOnAcknowledge = false)
	{
		returnToLoginScreen?.Invoke();
		if (LoadingScreen.CurrentVisibility == LoadingScreenVisibility.Enabled)
		{
			LoadingScreen.SetScreenTypeAndVisibility(LoadingScreenVisibility.FadingOut);
		}
		if (quitOnAcknowledge)
		{
			ShowOkDialog(header, message, Application.Quit, "QUIT");
		}
		else
		{
			ShowOkDialog(header, message);
		}
	}

	private static void OpenGeneralDialog(CreateAlertDialogEvent dataToAdd)
	{
		_dialogQueue.Enqueue(new DialogWrapper
		{
			PopupType = DialogPopupType.GeneralDialogPopup,
			PopupData = dataToAdd
		});
		if (_dialogQueue.Count == 1)
		{
			OpenNextPopup();
		}
	}

	public static void OpenModalPopup(UnrecoverableErrorState errorState)
	{
		_dialogQueue.Enqueue(new DialogWrapper
		{
			PopupType = DialogPopupType.ModalPopup,
			PopupData = errorState
		});
		if (_dialogQueue.Count == 1)
		{
			OpenNextPopup();
		}
	}

	public static void ClosePopup(GeneralDialogPopup popup)
	{
		ClosePopup(popup, DialogPopupType.GeneralDialogPopup);
	}

	public static void ClosePopup(ModalPopup popup)
	{
		ClosePopup(popup, DialogPopupType.ModalPopup);
	}

	private static void ClosePopup<T>(T popup, DialogPopupType popupType)
	{
		DialogWrapper dialogWrapper = _dialogQueue.Peek();
		if (dialogWrapper.PopupType != popupType)
		{
			WALogger.Error<DialogPopupFacade>("Trying to close a popup with DialogPopupType [{0}] but the first of the _dialogQueue is of type [{1}]! Ignoring request, check your code!", new object[2] { popupType, dialogWrapper.PopupType });
			return;
		}
		_dialogQueue.Dequeue();
		if (_dialogQueue.Count > 0)
		{
			DialogWrapper dialogWrapper2 = _dialogQueue.Peek();
			if (dialogWrapper2.PopupType == popupType)
			{
				switch (popupType)
				{
				case DialogPopupType.GeneralDialogPopup:
				{
					GeneralDialogPopup generalDialogPopup = popup as GeneralDialogPopup;
					generalDialogPopup.SetData((CreateAlertDialogEvent)dialogWrapper2.PopupData, generalDialogPopup.ThisPopupsState);
					break;
				}
				case DialogPopupType.ModalPopup:
				{
					ModalPopup modalPopup = popup as ModalPopup;
					modalPopup.SetData((UnrecoverableErrorState)dialogWrapper2.PopupData, modalPopup.ThisPopupsState);
					break;
				}
				}
				return;
			}
		}
		switch (popupType)
		{
		case DialogPopupType.GeneralDialogPopup:
		{
			GeneralDialogPopup generalDialogPopup2 = popup as GeneralDialogPopup;
			UIWindowController.PopState(generalDialogPopup2.ThisPopupsState);
			break;
		}
		case DialogPopupType.ModalPopup:
		{
			ModalPopup modalPopup2 = popup as ModalPopup;
			UIWindowController.PopState(modalPopup2.ThisPopupsState);
			break;
		}
		}
		OpenNextPopup();
	}

	public static DialogPopupType GetNextPopupType()
	{
		return _dialogQueue.Peek().PopupType;
	}

	public static void OpenNextPopup()
	{
		if (_dialogQueue.Count > 0)
		{
			DialogWrapper dialogWrapper = _dialogQueue.Peek();
			if (dialogWrapper.PopupType == DialogPopupType.GeneralDialogPopup)
			{
				UIWindowController.PushState(new GeneralDialogPopupState((CreateAlertDialogEvent)dialogWrapper.PopupData));
			}
			else
			{
				UIWindowController.PushState(new ModalPopupState((UnrecoverableErrorState)dialogWrapper.PopupData));
			}
		}
	}
}
