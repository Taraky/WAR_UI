using System;
using System.Collections.Generic;
using Travellers.UI.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Behaviours;

public class UIVolumeSliders : MonoBehaviour
{
	[SerializeField]
	private SliderScaledValue masterVolumeSlider;

	[SerializeField]
	private SliderScaledValue musicVolumeSlider;

	[SerializeField]
	private SliderScaledValue sfxVolumeSlider;

	[SerializeField]
	private UIDropdown _enableVoiceChat;

	[SerializeField]
	private SliderScaledValue _voiceTransmitVolumeSlider;

	[SerializeField]
	private SliderScaledValue _voiceReceiveVolumeSlider;

	private bool _initialised;

	private bool VoiceChatFeatureEnabled => WAConfig.Get<bool>(ConfigKeys.VOIPEnabled);

	private void OnEnable()
	{
		masterVolumeSlider.SetValue(AudioOptions.MasterVolume);
		musicVolumeSlider.SetValue(AudioOptions.MusicVolume);
		sfxVolumeSlider.SetValue(AudioOptions.SFXVolume);
		_voiceReceiveVolumeSlider.SetValue(AudioOptions.VoiceReceiveVolume);
		_voiceTransmitVolumeSlider.SetValue(AudioOptions.VoiceTransmitVolume);
		List<Dropdown.OptionData> list = new List<Dropdown.OptionData>();
		list.Add(new Dropdown.OptionData("Push To Talk"));
		list.Add(new Dropdown.OptionData("Always On"));
		list.Add(new Dropdown.OptionData("Off"));
		_enableVoiceChat.options = list;
		_enableVoiceChat.SetValue((int)AudioOptions.VoiceChatMode);
		if (!VoiceChatFeatureEnabled)
		{
			Transform transform = base.gameObject.transform.FindChild("VoiceChatScreenTitle");
			if ((bool)transform)
			{
				transform.gameObject.SetActive(value: false);
			}
			Transform transform2 = base.gameObject.transform.FindChild("VoiceSection");
			if ((bool)transform2)
			{
				transform2.gameObject.SetActive(value: false);
			}
		}
		_initialised = true;
	}

	public void OnDisable()
	{
		_initialised = false;
	}

	public void ValueChanged()
	{
		if (_initialised)
		{
			VOIPProvider.VOIPSettings settings = GetSettings();
			AudioOptions.MasterVolume = masterVolumeSlider.GetValue();
			AudioOptions.SFXVolume = sfxVolumeSlider.GetValue();
			AudioOptions.MusicVolume = musicVolumeSlider.GetValue();
			AudioOptions.VoiceReceiveVolume = _voiceReceiveVolumeSlider.GetValue();
			AudioOptions.VoiceTransmitVolume = _voiceTransmitVolumeSlider.GetValue();
			AudioOptions.VoiceChatMode = (AudioOptions.VoiceChatModeOption)_enableVoiceChat.value;
			VOIPManager.UpdateSettings(settings, GetSettings());
		}
	}

	private VOIPProvider.VOIPSettings GetSettings()
	{
		bool flag = Math.Abs(AudioOptions.MasterVolume) > 0.001f;
		VOIPProvider.VOIPSettings result = default(VOIPProvider.VOIPSettings);
		result.enabled = flag && AudioOptions.VoiceChatMode != AudioOptions.VoiceChatModeOption.Off;
		result.pushToTalk = flag && AudioOptions.VoiceChatMode == AudioOptions.VoiceChatModeOption.OnPushToTalk;
		result.receiveVolume = AudioOptions.VoiceReceiveVolume * AudioOptions.MasterVolume / 100f;
		result.transmitVolume = AudioOptions.VoiceTransmitVolume * AudioOptions.MasterVolume / 100f;
		return result;
	}
}
