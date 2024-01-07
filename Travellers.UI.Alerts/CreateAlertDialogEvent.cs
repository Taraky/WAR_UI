using System;
using Travellers.UI.Events;

namespace Travellers.UI.Alerts;

public class CreateAlertDialogEvent : UIEvent
{
	public AlertDialogType DialogType;

	public string Title;

	public string Msg;

	public string InputPlaceholder;

	public string FirstButtonText;

	public string SecondButtonText;

	public string ThirdButtonText;

	public Action FirstButtonCallback;

	public Action<string> InputBoxCallback;

	public Action SecondButtonCallback;

	public Action ThirdButtonCallback;

	public Action ClosedCallback;

	public bool UseSolidBackground;

	public string OverrideConfirmSound;

	public string OverrideCancelSound;

	public float? ConfirmationEnableDelay;

	public CreateAlertDialogEvent(AlertDialogType dialogType, string title = "", string msg = "", string inputPlaceholder = "", string firstButtonText = "", string secondButtonText = "", string thirdButtonText = "", Action firstButtonCallback = null, Action secondButtonCallback = null, Action thirdButtonCallback = null, Action closedCallback = null, Action<string> inputBoxCallback = null, bool useSolidBg = true, string overrideConfirmSound = null, string overrideCancelSound = null, float? confirmationEnableDelay = null)
	{
		DialogType = dialogType;
		Title = title;
		Msg = msg;
		InputPlaceholder = inputPlaceholder;
		FirstButtonText = firstButtonText;
		SecondButtonText = secondButtonText;
		ThirdButtonText = thirdButtonText;
		FirstButtonCallback = firstButtonCallback;
		SecondButtonCallback = secondButtonCallback;
		ThirdButtonCallback = thirdButtonCallback;
		ClosedCallback = closedCallback;
		InputBoxCallback = inputBoxCallback;
		UseSolidBackground = useSolidBg;
		OverrideConfirmSound = overrideConfirmSound;
		OverrideCancelSound = overrideCancelSound;
		ConfirmationEnableDelay = confirmationEnableDelay;
	}
}
