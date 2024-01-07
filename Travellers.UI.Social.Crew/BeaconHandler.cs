using Travellers.UI.Framework;
using Travellers.UI.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.Social.Crew;

public class BeaconHandler
{
	public static readonly int MaxBeaconTimeInSeconds = 86400;

	private readonly TextStylerTextMeshPro _countdownText;

	private readonly UIButtonController _useBeaconButton;

	private readonly Image _beaconProgressBar;

	private bool _isActive;

	private int _lastServerTime;

	private float _timeSinceLastServerTimeSet;

	public BeaconHandler(TextStylerTextMeshPro countdownText, UIButtonController useBeaconButton, Image beaconProgressBar)
	{
		_countdownText = countdownText;
		_useBeaconButton = useBeaconButton;
		_beaconProgressBar = beaconProgressBar;
	}

	public void SetActive(bool isActive)
	{
		_isActive = isActive;
	}

	public void SetCountdown(long timeRemaining)
	{
		_lastServerTime = (int)timeRemaining;
		_timeSinceLastServerTimeSet = 0f;
		UpdateCountdown();
	}

	private void UpdateCountdown()
	{
		int num = _lastServerTime - (int)_timeSinceLastServerTimeSet;
		_isActive = num <= 0;
		_countdownText.SetText(num.FormatTimeForBeaconCountdown());
		_beaconProgressBar.fillAmount = (float)num / (float)MaxBeaconTimeInSeconds;
		_useBeaconButton.SetButtonEnabled(_isActive);
	}

	public void UpdateDisplay()
	{
		if (_isActive)
		{
			_timeSinceLastServerTimeSet -= Time.deltaTime;
			UpdateCountdown();
		}
	}
}
