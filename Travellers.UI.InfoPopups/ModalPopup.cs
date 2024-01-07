using System;
using System.Collections.Generic;
using Bossa.Travellers.Game;
using GameStateMachine;
using Travellers.UI.Framework;
using Travellers.UI.Login;
using UnityEngine;
using WAUtilities.Logging;

namespace Travellers.UI.InfoPopups;

[InjectableClass]
public class ModalPopup : UIPopup
{
	[SerializeField]
	private TextStylerTextMeshPro _titleText;

	[SerializeField]
	private TextStylerTextMeshPro _messageText;

	[SerializeField]
	private GameObject _titleHolder;

	[SerializeField]
	private GameObject _messageHolder;

	[SerializeField]
	private GameObject _buttonHolder;

	[SerializeField]
	private ModalPopupButtonInstance _buttonPrefab;

	private List<ModalPopupButtonInstance> _activeButtons = new List<ModalPopupButtonInstance>();

	private Action<ModalPopupButtonAction> _onButtonPressedDelegate;

	private UnrecoverableErrorState _errorState;

	private LobbySystem _lobbySys;

	public ModalPopupState ThisPopupsState { get; private set; }

	[InjectableMethod]
	public void InjectDependencies(LobbySystem lobbySys)
	{
		_lobbySys = lobbySys;
	}

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
		_onButtonPressedDelegate = OnPopupButtonPressed;
	}

	protected override void Deactivate()
	{
	}

	protected override void ProtectedDispose()
	{
		_onButtonPressedDelegate = null;
	}

	public void SetData(UnrecoverableErrorState errorState, ModalPopupState thisPopupsState)
	{
		ThisPopupsState = thisPopupsState;
		_errorState = errorState;
		ModalPopupMessageWrapper messageWrapper = _errorState.GetMessageWrapper();
		if (messageWrapper.error.HasValue)
		{
			SetupPopupFromError(messageWrapper.error.Value);
			return;
		}
		if (messageWrapper.message.HasValue)
		{
			SetupPopupFromMessage(messageWrapper.message.Value);
			return;
		}
		WALogger.Error<ModalPopup>("Trying to set up a modal popup but the ModalPopupMessageWrapper used does not have a message or error! Please verify that this was a valid request. Closing popup.");
		ClosePopup();
		_errorState.SetNextState(new EndState());
	}

	private void OnPopupButtonPressed(ModalPopupButtonAction buttonAction)
	{
		switch (buttonAction)
		{
		case ModalPopupButtonAction.Dismiss:
			ClosePopup();
			break;
		case ModalPopupButtonAction.Quit:
			ModalPopupButtonActions.Quit();
			break;
		case ModalPopupButtonAction.BackToMenu:
			ModalPopupButtonActions.DisconnectFromSpatial(null);
			_errorState.SetNextState(new LobbyState(_errorState.GetImprobableBootstrapReference()));
			break;
		case ModalPopupButtonAction.Retry:
			ModalPopupButtonActions.DisconnectFromSpatial(delegate
			{
				_lobbySys.Login();
				_errorState.SetNextState(new ConnectToWorldState(_errorState.GetImprobableBootstrapReference()));
			});
			break;
		case ModalPopupButtonAction.CleanData:
			ModalPopupButtonActions.ForceCleanBookkeepingAppOnNextLogin();
			ModalPopupButtonActions.DisconnectFromSpatial(delegate
			{
				_lobbySys.Login();
				_errorState.SetNextState(new ConnectToWorldState(_errorState.GetImprobableBootstrapReference()));
			});
			break;
		}
	}

	private void ClosePopup()
	{
		DialogPopupFacade.ClosePopup(this);
		_errorState = null;
	}

	private void SetupPopupFromError(ModalErrorPopupMessage error)
	{
		ModalPopupButtonActions.DisconnectFromSpatial(null);
		if (LoadingScreen.CurrentVisibility == LoadingScreenVisibility.Enabled)
		{
			LoadingScreen.SetScreenTypeAndVisibility(LoadingScreenVisibility.FadingOut);
		}
		Bossa.Travellers.Game.ModalPopupMessage popupMessageFromError = ModalPopupTemplates.GetPopupMessageFromError(error);
		SetupPopupFromMessage(popupMessageFromError);
	}

	private void SetupPopupFromMessage(Bossa.Travellers.Game.ModalPopupMessage message)
	{
		if (!string.IsNullOrEmpty(message.header))
		{
			_titleHolder.gameObject.SetActive(value: true);
			_titleText.SetText(message.header);
		}
		else
		{
			_titleHolder.gameObject.SetActive(value: false);
		}
		if (!string.IsNullOrEmpty(message.message))
		{
			_messageHolder.gameObject.SetActive(value: true);
			_messageText.SetText(message.message);
		}
		else
		{
			_messageHolder.gameObject.SetActive(value: false);
		}
		for (int i = 0; i < _activeButtons.Count; i++)
		{
			UnityEngine.Object.Destroy(_activeButtons[i].gameObject);
		}
		_activeButtons.Clear();
		for (int j = 0; j < message.buttons.Count; j++)
		{
			ModalPopupButton modalPopupButton = message.buttons[j];
			ModalPopupButtonInstance modalPopupButtonInstance = UnityEngine.Object.Instantiate(_buttonPrefab, _buttonHolder.transform);
			modalPopupButtonInstance.Setup(modalPopupButton.label, modalPopupButton.action, _onButtonPressedDelegate);
		}
	}
}
