using GameDBLocalization;
using TMPro;
using Travellers.UI.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.Crafting;

public class EnterCodePopup : UIPopup
{
	[SerializeField]
	private TMP_InputField _input;

	[SerializeField]
	private Text _status;

	[SerializeField]
	private Button _submitButton;

	private ILockAgentVisualizer _lockAgentVisualiser;

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
		_input.onEndEdit.AddListener(OnEnterCode);
	}

	protected override void ProtectedDispose()
	{
	}

	public void SetLockAgent(ILockAgentVisualizer lockAgentVisualiser)
	{
		if (_lockAgentVisualiser != null)
		{
			_lockAgentVisualiser.Reset();
		}
		_lockAgentVisualiser = lockAgentVisualiser;
		_lockAgentVisualiser.SetCallbacks(OnCodeError, OnUnlockRequestsEnabled);
		EvaluateTutorialMessage();
	}

	private void OnUnlockRequestsEnabled(bool requestsEnabled)
	{
		_input.enabled = requestsEnabled;
		_submitButton.interactable = requestsEnabled;
		EvaluateTutorialMessage();
	}

	private void EvaluateTutorialMessage()
	{
		if (_input.enabled)
		{
			_status.text = Localizer.LocalizeString(LocalizationSchema.KeySHIPYARD_LOCK_TUTORIAL_MESSAGE);
		}
	}

	public void OnEnterCode(string codeEntered)
	{
		_lockAgentVisualiser.EnterCode(codeEntered);
	}

	private void OnCodeError(string errorMsg)
	{
		_input.text = string.Empty;
		_status.text = errorMsg;
	}
}
