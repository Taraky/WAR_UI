using Bossa.Travellers.Utils;
using Travellers.UI.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.HUD;

public class InGameInteractionScreen : UIScreen
{
	[SerializeField]
	private UICircleTimer _circleTimer;

	[SerializeField]
	private AimCrosshairUI _crosshairUI;

	[SerializeField]
	private Image _teleportSpinner;

	[SerializeField]
	private GameObject _feedbackButton;

	[SerializeField]
	private AnimationCurve _spinnerAlphaCurve;

	private SpinnerFlags _spinnerFlags;

	private float _spinnerActiveTime;

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUIInGameEvents.ObjectPlacementTimerState, OnObjectPlacementTimerStateChanged);
		_eventList.AddEvent(WAUIInGameEvents.TriggerCrosshairHit, OnCrosshairHitTriggered);
		_eventList.AddEvent(WAUIInGameEvents.ChangeTeleportSpinnerState, OnChangeTeleportSpinnerStateEvent);
		_eventList.AddEvent(WAUIInGameEvents.ChangeAimingCrosshairHandleDistance, OnChangeAimingCrosshairHandleDistance);
	}

	protected override void ProtectedInit()
	{
		_circleTimer.gameObject.SetActive(value: true);
		_circleTimer.Hide();
		_crosshairUI.gameObject.SetActive(value: true);
		if (SteamChecker.IsSteamBranchPTS())
		{
			_feedbackButton.SetActive(value: true);
		}
	}

	protected override void ProtectedDispose()
	{
	}

	public static void ChangeObjectPlacementTimerState(bool show, float normalisedTime)
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInGameEvents.ObjectPlacementTimerState, new ObjectPlacementTimerStateChangeEvent(show, normalisedTime));
	}

	public static void TriggerCrosshairHit()
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInGameEvents.TriggerCrosshairHit, null);
	}

	public static void ChangeStateOfTeleportSpinner(SpinnerFlags flags, bool show)
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInGameEvents.ChangeTeleportSpinnerState, new ChangeTeleportSpinnerStateEvent(flags, show));
	}

	public static void ChangeAimingCrosshairHandleDistance(float handleDistance)
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInGameEvents.ChangeAimingCrosshairHandleDistance, AimingCrosshairUIUpdatedEvent.AimingCrosshairHandleDistanceChange(handleDistance));
	}

	private void OnObjectPlacementTimerStateChanged(object[] obj)
	{
		ObjectPlacementTimerStateChangeEvent objectPlacementTimerStateChangeEvent = (ObjectPlacementTimerStateChangeEvent)obj[0];
		UpdateObjectPlacementTimerState(objectPlacementTimerStateChangeEvent.Show, objectPlacementTimerStateChangeEvent.NormalisedValue);
	}

	private void OnCrosshairHitTriggered(object[] obj)
	{
		_crosshairUI.Hit();
	}

	private void OnChangeTeleportSpinnerStateEvent(object[] obj)
	{
		ChangeTeleportSpinnerStateEvent changeTeleportSpinnerStateEvent = (ChangeTeleportSpinnerStateEvent)obj[0];
		UpdateTeleportSpinnerState(changeTeleportSpinnerStateEvent.Flags, changeTeleportSpinnerStateEvent.Show);
	}

	private void OnChangeAimingCrosshairHandleDistance(object[] obj)
	{
		AimingCrosshairUIUpdatedEvent aimingCrosshairUIUpdatedEvent = (AimingCrosshairUIUpdatedEvent)obj[0];
		UpdateCrosshairHandleDistance(aimingCrosshairUIUpdatedEvent.HandleDistance);
	}

	private void UpdateObjectPlacementTimerState(bool evtShow, float normalisedValue)
	{
		_circleTimer.IsVisible = evtShow;
		_circleTimer.SetValue(normalisedValue);
	}

	private void UpdateTeleportSpinnerState(SpinnerFlags flags, bool showSpinner)
	{
		if (showSpinner)
		{
			_spinnerFlags |= flags;
		}
		else
		{
			_spinnerFlags &= ~flags;
		}
	}

	private void UpdateCrosshairHandleDistance(float evtHandleDistance)
	{
		_crosshairUI.ChangeCrosshairHandleDistance(evtHandleDistance);
	}

	public void ShowFeedBack()
	{
		UIWindowController.PushState(FeedbackPopupUIState.Default);
	}

	private void Update()
	{
		_spinnerActiveTime += ((_spinnerFlags == SpinnerFlags.None) ? (0f - Time.deltaTime) : Time.deltaTime);
		_spinnerActiveTime = Mathf.Clamp01(_spinnerActiveTime);
		float num = _spinnerAlphaCurve.Evaluate(_spinnerActiveTime);
		if (num > 0f)
		{
			_teleportSpinner.gameObject.SetActive(value: true);
			Color color = _teleportSpinner.color;
			color.a = num;
			_teleportSpinner.color = color;
		}
		else
		{
			_teleportSpinner.gameObject.SetActive(value: false);
		}
	}
}
