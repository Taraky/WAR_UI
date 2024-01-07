using Bossa.Travellers.Utils;
using Bossa.Travellers.Utils.ErrorHandling;
using Travellers.UI.Events;
using Travellers.UI.Framework;
using Travellers.UI.HUDMessaging;
using UnityEngine;

namespace Travellers.UI.InGame;

public class InGameMenuScreen : UIScreen
{
	[SerializeField]
	private GameObject _menuGameObject;

	[SerializeField]
	private UICanvasGroupButton _resumeButton;

	[SerializeField]
	private UICanvasGroupButton _respawnButton;

	[SerializeField]
	private UICanvasGroupButton _feedbackButton;

	[SerializeField]
	private GameObject _extraFeedbackButton;

	[SerializeField]
	private UIButtonController _settingsButton;

	[SerializeField]
	private UICanvasGroupButton _helpButton;

	[SerializeField]
	private UICanvasGroupButton _exitButton;

	private LogoutCountdownPanel _logoutPanel;

	private InGameMenuState _currentState;

	protected override void ProtectedInit()
	{
		_logoutPanel = base.gameObject.GetComponentInChildren<LogoutCountdownPanel>(includeInactive: true);
		_helpButton.SetButtonState(UIButtonState.Disabled);
		_settingsButton.SetButtonEvent(OnSettingsButtonPressed);
		if (SteamChecker.IsSteamBranchPTS())
		{
			_extraFeedbackButton.SetActive(value: true);
		}
	}

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUIInGameEvents.InGameMenuStateChange, OnChangeInGameMenuState);
	}

	protected override void ProtectedDispose()
	{
	}

	public static void ChangeInGameMenuState(InGameMenuState newState)
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInGameEvents.InGameMenuStateChange, new InGameMenuStateEvent(newState));
	}

	private void OnSettingsButtonPressed()
	{
		UIWindowController.PushState(SettingsScreenUIState.WithInGameMenu);
	}

	private void OnChangeInGameMenuState(object[] obj)
	{
		InGameMenuStateEvent inGameMenuStateEvent = (InGameMenuStateEvent)obj[0];
		InGameMenuState currentState = inGameMenuStateEvent.NewState;
		if (inGameMenuStateEvent.ToggleState)
		{
			switch (_currentState)
			{
			case InGameMenuState.Off:
				currentState = InGameMenuState.Menu;
				break;
			case InGameMenuState.Menu:
				currentState = InGameMenuState.Off;
				break;
			case InGameMenuState.ExitModal:
				return;
			}
		}
		_currentState = currentState;
		SetState(_currentState);
	}

	private void Update()
	{
		UpdateLogoutAndExitButtons();
	}

	public void SetState(InGameMenuState newState)
	{
		_menuGameObject.SetActive(newState == InGameMenuState.Menu);
		_logoutPanel.SetActive(newState == InGameMenuState.ExitModal);
	}

	public void ResumeGame()
	{
		UIWindowController.PopState<InGameMenuUIState>();
	}

	public void CancelLogout()
	{
		SetState(InGameMenuState.Menu);
	}

	public void ShowFeedback()
	{
		LocalPlayer.Instance.logoutBehaviour.CancelLogout();
		UIWindowController.PopState<InGameMenuUIState>();
		UIWindowController.PushState(FeedbackPopupUIState.Default);
	}

	public void Respawn()
	{
		LocalPlayer.Instance.respawnVisualizer.RequestRespawn();
		OSDMessage.SendMessage("Reviving...");
		UIWindowController.PopState<InGameMenuUIState>();
	}

	private void UpdateLogoutAndExitButtons()
	{
		if (!(_exitButton != null))
		{
			return;
		}
		UIButtonState buttonState = UIButtonState.Disabled;
		ReleaseAssert.IsTrue<InGameMenuScreen>(LocalPlayer.Exists, () => "WA-3975: Local player doesn't exist!?");
		if (LocalPlayer.Exists)
		{
			ReleaseAssert.IsNotNull<InGameMenuScreen>(LocalPlayer.Instance.logoutBehaviour, () => "WA-3975: Local player logout behaviour is null!");
			buttonState = ((LocalPlayer.Instance.logoutBehaviour != null && LocalPlayer.Instance.logoutBehaviour.CanLogout()) ? UIButtonState.Default : UIButtonState.Disabled);
		}
		_exitButton.SetButtonState(buttonState);
	}

	public void ShowLogoutCountdownPanel()
	{
		ShowCountdownPanel(LogoutCountdownPanel.Mode.Logout);
	}

	public void ShowExitCountdownPanel()
	{
		ShowCountdownPanel(LogoutCountdownPanel.Mode.Exit);
	}

	private void ShowCountdownPanel(LogoutCountdownPanel.Mode mode)
	{
		_logoutPanel.SetLogoutMode(mode);
		SetState(InGameMenuState.ExitModal);
	}
}
