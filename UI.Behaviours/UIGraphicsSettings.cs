using Improbable.Collections;
using Travellers.UI.Framework;
using Travellers.UI.Options;
using Travellers.UI.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Behaviours;

[InjectableClass]
public class UIGraphicsSettings : UIScreenComponent
{
	[SerializeField]
	private UIDropdown preset;

	[SerializeField]
	private UIDropdown textureQuality;

	[SerializeField]
	private UIDropdown shadowQuality;

	[SerializeField]
	private UIDropdown shadowDistance;

	[SerializeField]
	private UIDropdown cloudQuality;

	[SerializeField]
	private UIDropdown antiAliasing;

	[SerializeField]
	private Slider dadLevel;

	[SerializeField]
	private Toggle ao;

	[SerializeField]
	private Toggle bloom;

	[SerializeField]
	private Toggle depthOfField;

	[SerializeField]
	private Toggle motionBlur;

	[SerializeField]
	private Toggle screenShake;

	[SerializeField]
	private Toggle dynamicFov;

	[SerializeField]
	private CanvasGroup optionsCanvasGroup;

	private OptionsSystem optionsSystem;

	private bool allowChanges;

	[InjectableMethod]
	public void InjectDependencies(OptionsSystem optionsSystem)
	{
		this.optionsSystem = optionsSystem;
	}

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
		SetupDropdowns();
		UpdateUIElements();
		ao.onValueChanged.AddListener(SettingsScreen.ToggleSfx);
		bloom.onValueChanged.AddListener(SettingsScreen.ToggleSfx);
		depthOfField.onValueChanged.AddListener(SettingsScreen.ToggleSfx);
		motionBlur.onValueChanged.AddListener(SettingsScreen.ToggleSfx);
		dynamicFov.onValueChanged.AddListener(SettingsScreen.ToggleSfx);
	}

	protected override void ProtectedDispose()
	{
	}

	public void SetPreset(int value)
	{
		optionsSystem.SetGraphicsQualityPreset(value);
		UpdateUIElements();
	}

	private void SetupDropdowns()
	{
		List<Dropdown.OptionData> list = new List<Dropdown.OptionData>();
		list.Add(new Dropdown.OptionData("Low"));
		list.Add(new Dropdown.OptionData("Medium"));
		list.Add(new Dropdown.OptionData("High"));
		list.Add(new Dropdown.OptionData("Ultra"));
		list.Add(new Dropdown.OptionData("Custom"));
		List<Dropdown.OptionData> list2 = new List<Dropdown.OptionData>();
		list2.Add(new Dropdown.OptionData("Low"));
		list2.Add(new Dropdown.OptionData("Medium"));
		list2.Add(new Dropdown.OptionData("High"));
		List<Dropdown.OptionData> list3 = new List<Dropdown.OptionData>();
		list3.Add(new Dropdown.OptionData("Low"));
		list3.Add(new Dropdown.OptionData("Medium"));
		list3.Add(new Dropdown.OptionData("High"));
		list3.Add(new Dropdown.OptionData("Very High"));
		List<Dropdown.OptionData> list4 = new List<Dropdown.OptionData>();
		list4.Add(new Dropdown.OptionData("Standard"));
		list4.Add(new Dropdown.OptionData("High"));
		list4.Add(new Dropdown.OptionData("Ultra"));
		List<Dropdown.OptionData> list5 = new List<Dropdown.OptionData>();
		list5.Add(new Dropdown.OptionData("Off"));
		list5.Add(new Dropdown.OptionData("FXAA Performance"));
		list5.Add(new Dropdown.OptionData("FXAA Default"));
		list5.Add(new Dropdown.OptionData("FXAA Quality"));
		preset.options = list;
		antiAliasing.options = list5;
		textureQuality.options = list2;
		cloudQuality.options = list4;
		shadowQuality.options = list3;
		shadowDistance.options = list3;
	}

	private int AdjustAntiAliasing(int desiredLevel)
	{
		return Mathf.Clamp(desiredLevel, 0, antiAliasing.options.Count - 1);
	}

	private void UpdateUIElements()
	{
		allowChanges = false;
		preset.SetValue(optionsSystem.CurrentGraphics.preset);
		int value = AdjustAntiAliasing(optionsSystem.CurrentGraphics.aaQuality);
		antiAliasing.SetValue(value);
		textureQuality.SetValue(optionsSystem.CurrentGraphics.textureQuality);
		cloudQuality.SetValue(optionsSystem.CurrentGraphics.cloudQuality);
		shadowQuality.SetValue(optionsSystem.CurrentGraphics.shadowQuality);
		shadowDistance.SetValue(optionsSystem.CurrentGraphics.shadowDistance);
		dadLevel.value = optionsSystem.CurrentGraphics.dadLevel;
		ao.isOn = optionsSystem.CurrentGraphics.ssao;
		bloom.isOn = optionsSystem.CurrentGraphics.bloom;
		depthOfField.isOn = optionsSystem.CurrentGraphics.dof;
		motionBlur.isOn = optionsSystem.CurrentGraphics.motionBlur;
		screenShake.isOn = optionsSystem.CurrentGraphics.screenShake;
		dynamicFov.isOn = optionsSystem.CurrentGraphics.dynamicFov;
		antiAliasing.interactable = optionsSystem.CurrentGraphics.preset == 4;
		textureQuality.interactable = optionsSystem.CurrentGraphics.preset == 4;
		cloudQuality.interactable = optionsSystem.CurrentGraphics.preset == 4;
		shadowQuality.interactable = optionsSystem.CurrentGraphics.preset == 4;
		shadowDistance.interactable = optionsSystem.CurrentGraphics.preset == 4;
		dadLevel.interactable = optionsSystem.CurrentGraphics.preset == 4;
		ao.interactable = optionsSystem.CurrentGraphics.preset == 4;
		bloom.interactable = optionsSystem.CurrentGraphics.preset == 4;
		depthOfField.interactable = optionsSystem.CurrentGraphics.preset == 4;
		motionBlur.interactable = optionsSystem.CurrentGraphics.preset == 4;
		dynamicFov.interactable = optionsSystem.CurrentGraphics.preset == 4;
		optionsCanvasGroup.alpha = ((optionsSystem.CurrentGraphics.preset != 4) ? 0.5f : 1f);
		allowChanges = true;
	}

	public void ChangeSettings()
	{
		if (allowChanges)
		{
			optionsSystem.CurrentGraphics.aaQuality = antiAliasing.value;
			optionsSystem.CurrentGraphics.textureQuality = textureQuality.value;
			optionsSystem.CurrentGraphics.cloudQuality = cloudQuality.value;
			optionsSystem.CurrentGraphics.shadowQuality = shadowQuality.value;
			optionsSystem.CurrentGraphics.shadowDistance = shadowDistance.value;
			optionsSystem.CurrentGraphics.dadLevel = dadLevel.value;
			optionsSystem.CurrentGraphics.ssao = ao.isOn;
			optionsSystem.CurrentGraphics.bloom = bloom.isOn;
			optionsSystem.CurrentGraphics.dof = depthOfField.isOn;
			optionsSystem.CurrentGraphics.motionBlur = motionBlur.isOn;
			optionsSystem.CurrentGraphics.screenShake = screenShake.isOn;
			optionsSystem.CurrentGraphics.dynamicFov = dynamicFov.isOn;
			optionsSystem.UpdateGraphicsSettings();
		}
	}
}
