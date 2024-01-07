using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Travellers.UI.Framework;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.UI;
using WAUtilities.Logging;

namespace Travellers.UI.Options;

[InjectedSystem]
public class OptionsSystem : UISystem
{
	[Serializable]
	public class GraphicsSettings
	{
		public int preset;

		public int textureQuality;

		public int shadowQuality;

		public int shadowDistance;

		public int cloudQuality;

		public int aaQuality;

		public float dadLevel;

		public bool ssao;

		public bool bloom;

		public bool dof;

		public bool motionBlur;

		public bool screenShake;

		public bool dynamicFov;
	}

	private string graphicsPresetLow = "\n        {\n            \"preset\":0,\n            \"textureQuality\":1,\n            \"shadowQuality\":0,\n            \"shadowDistance\":0,\n            \"cloudQuality\":0,\n            \"aaQuality\":0,\n            \"dadLevel\":0.75,\n            \"ssao\":false,\n            \"bloom\":false,\n            \"dof\":false,\n            \"screenShake\":false,\n            \"motionBlur\":false,\n            \"dynamicFov\":false\n        }";

	private string graphicsPresetMed = "\n        {\n            \"preset\":1,\n            \"textureQuality\":2,\n            \"shadowQuality\":1,\n            \"shadowDistance\":1,\n            \"cloudQuality\":0,\n            \"aaQuality\":1,\n            \"dadLevel\":1.0,\n            \"ssao\":false,\n            \"bloom\":true,\n            \"dof\":false,\n            \"motionBlur\":false,\n            \"screenShake\":false,\n            \"dynamicFov\":true\n        }";

	private string graphicsPresetHigh = "\n         {\n            \"preset\":2,\n            \"textureQuality\":2,\n            \"shadowQuality\":1,\n            \"shadowDistance\":1,\n            \"cloudQuality\":1,\n            \"aaQuality\":2,\n            \"dadLevel\":1.0,\n            \"ssao\":true,\n            \"bloom\":true,\n            \"dof\":true,\n            \"motionBlur\":true,\n            \"screenShake\":true,\n            \"dynamicFov\":true\n        }";

	private string graphicsPresetUltra = "\n        {\n            \"preset\":3,\n            \"textureQuality\":2,\n            \"shadowQuality\":2,\n            \"shadowDistance\":2,\n            \"cloudQuality\":2,\n            \"aaQuality\":3,\n            \"dadLevel\":2.0,\n            \"ssao\":true,\n            \"bloom\":true,\n            \"dof\":true,\n            \"motionBlur\":true,\n            \"screenShake\":true,\n            \"dynamicFov\":true\n        }";

	public GraphicsSettings CurrentGraphics;

	private const string SavedGraphicsSettingsKey = "SavedGraphicsSettiongs";

	private KeyboardInputProvider _keyboardInputProvider;

	private KeyBindingsLibrarian _keyBindingsLibrarian;

	public bool VFXActive = true;

	public HashSet<InputButtons> DevOnlyKeyBindings => _keyBindingsLibrarian.DevOnlyButtons;

	public override void Init()
	{
		_keyBindingsLibrarian = new KeyBindingsLibrarian();
		CurrentGraphics = GetGraphicsSettings();
		UpdateGraphicsSettings();
	}

	public override void ControlledUpdate()
	{
	}

	protected override void AddListeners()
	{
	}

	protected override void Dispose()
	{
	}

	public void UpdateGraphicsSettings()
	{
		QualitySettings.SetQualityLevel(CurrentGraphics.textureQuality);
		QualitySettings.shadowResolution = (ShadowResolution)CurrentGraphics.shadowQuality;
		float t = Mathf.InverseLerp(0f, 3f, CurrentGraphics.shadowDistance);
		QualitySettings.shadowDistance = Mathf.Lerp(300f, 675f, t);
		CmdBufClouds cmdBufClouds = UnityEngine.Object.FindObjectOfType<CmdBufClouds>();
		if (cmdBufClouds != null)
		{
			if (CurrentGraphics.cloudQuality == 2)
			{
				cmdBufClouds.CurrentCloudQuality = CmdBufClouds.CloudQualityLevel.Ultra;
			}
			else if (CurrentGraphics.cloudQuality == 1)
			{
				cmdBufClouds.CurrentCloudQuality = CmdBufClouds.CloudQualityLevel.High;
			}
			else if (CurrentGraphics.cloudQuality == 0)
			{
				cmdBufClouds.CurrentCloudQuality = CmdBufClouds.CloudQualityLevel.Standard;
			}
		}
		QualitySettings.lodBias = CurrentGraphics.dadLevel;
		if (!(CameraVisualSettings.Instance == null))
		{
			CameraVisualSettings.Instance.postFx.profile.bloom.enabled = CurrentGraphics.bloom;
			CameraVisualSettings.Instance.postFx.profile.depthOfField.enabled = CurrentGraphics.dof;
			HBAO_Integrated hBAO_Integrated = UnityEngine.Object.FindObjectOfType<HBAO_Integrated>();
			if (hBAO_Integrated != null)
			{
				hBAO_Integrated.enabled = CurrentGraphics.ssao;
			}
			CameraVisualSettings.Instance.postFx.profile.antialiasing.enabled = CurrentGraphics.aaQuality > 0;
			AntialiasingModel.Settings settings = CameraVisualSettings.Instance.postFx.profile.antialiasing.settings;
			AntialiasingModel.FxaaSettings defaultSettings = AntialiasingModel.FxaaSettings.defaultSettings;
			switch (CurrentGraphics.aaQuality)
			{
			case 1:
				settings.method = AntialiasingModel.Method.Fxaa;
				defaultSettings.preset = AntialiasingModel.FxaaPreset.Performance;
				break;
			case 2:
				settings.method = AntialiasingModel.Method.Fxaa;
				defaultSettings.preset = AntialiasingModel.FxaaPreset.Default;
				break;
			case 3:
				settings.method = AntialiasingModel.Method.Fxaa;
				defaultSettings.preset = AntialiasingModel.FxaaPreset.ExtremeQuality;
				break;
			default:
				settings.method = AntialiasingModel.Method.Fxaa;
				defaultSettings.preset = AntialiasingModel.FxaaPreset.Default;
				WALogger.Warn<OptionsSystem>("Unknown Anti Aliasing option number {0}. Falling back to option {1}.", new object[2] { CurrentGraphics.aaQuality, 2 });
				CurrentGraphics.aaQuality = 2;
				break;
			case 0:
				break;
			}
			settings.fxaaSettings = defaultSettings;
			CameraVisualSettings.Instance.postFx.profile.antialiasing.settings = settings;
			CameraVisualSettings.Instance.MotionBlurEffect.enabled = CurrentGraphics.motionBlur;
			FOVChange.SetActive(CurrentGraphics.dynamicFov);
			ResolutionSelectionDropdown.SetVsync(PlayerPrefs.GetInt("Vsync"));
			SetUiScale();
			SaveGraphicsSettings();
		}
	}

	private void SetUiScale()
	{
		int width = Screen.width;
		int height = Screen.height;
		float a = ((width <= height) ? ((float)width / 1920f) : ((float)height / 1080f));
		CanvasScaler[] array = UnityEngine.Object.FindObjectsOfType<CanvasScaler>();
		foreach (CanvasScaler canvasScaler in array)
		{
			canvasScaler.scaleFactor = Mathf.Min(a, PlayerPrefs.GetFloat("UiScaleFactor", 1f));
		}
	}

	private GraphicsSettings GetGraphicsSettings()
	{
		if (PlayerPrefs.HasKey("SavedGraphicsSettiongs"))
		{
			try
			{
				return ParseGraphicSettings(PlayerPrefs.GetString("SavedGraphicsSettiongs"));
			}
			catch
			{
				PlayerPrefs.DeleteKey("SavedGraphicsSettiongs");
				return JsonConvert.DeserializeObject<GraphicsSettings>(graphicsPresetHigh);
			}
		}
		return JsonConvert.DeserializeObject<GraphicsSettings>(graphicsPresetHigh);
	}

	private GraphicsSettings ParseGraphicSettings(string json)
	{
		Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
		Dictionary<string, string> dictionary2 = JsonConvert.DeserializeObject<Dictionary<string, string>>(graphicsPresetHigh);
		foreach (KeyValuePair<string, string> item in dictionary2)
		{
			if (!dictionary.ContainsKey(item.Key))
			{
				dictionary.Add(item.Key, item.Value);
			}
		}
		return JsonConvert.DeserializeObject<GraphicsSettings>(JsonConvert.SerializeObject(dictionary));
	}

	public void SetGraphicsQualityPreset(int presetLevel)
	{
		string[] array = new string[4] { graphicsPresetLow, graphicsPresetMed, graphicsPresetHigh, graphicsPresetUltra };
		if (presetLevel < array.Length)
		{
			CurrentGraphics = JsonConvert.DeserializeObject<GraphicsSettings>(array[presetLevel]);
			UpdateGraphicsSettings();
		}
		else
		{
			CurrentGraphics.preset = presetLevel;
		}
	}

	private void SaveGraphicsSettings()
	{
		PlayerPrefs.SetString("SavedGraphicsSettiongs", JsonConvert.SerializeObject(CurrentGraphics));
	}

	public void SetKeyboardProvider(KeyboardInputProvider keyboardInputProvider)
	{
		_keyboardInputProvider = keyboardInputProvider;
		SetKeyboardInputBindings();
	}

	public List<KeyBindingCategory> GetBindingCategories()
	{
		return _keyBindingsLibrarian.KeybindingCategories;
	}

	public void SetKeyBindsToDefaults()
	{
		_keyBindingsLibrarian.SetKeyBindsToDefaults();
		SetKeyboardInputBindings();
	}

	public void RebindKey(InputButtons button, int index, KeyCode code)
	{
		_keyBindingsLibrarian.RebindKey(button, index, code);
		SetKeyboardInputBindings();
	}

	public string GetInputButtonLabel(InputButtons tutorialDataInputButton)
	{
		return _keyBindingsLibrarian.GetInputButtonLabel(tutorialDataInputButton);
	}

	public KeyCode[] GetKeyCodesForButton(InputButtons button)
	{
		return _keyBindingsLibrarian.GetKeyCodesForButton(button);
	}

	private void SetKeyboardInputBindings()
	{
		_keyboardInputProvider.SetBindings(_keyBindingsLibrarian.CachedBindings);
	}
}
