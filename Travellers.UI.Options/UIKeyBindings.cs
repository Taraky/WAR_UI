using System;
using System.Collections.Generic;
using TMPro;
using Travellers.UI.Framework;
using Travellers.UI.InfoPopups;
using Travellers.UI.Settings;
using Travellers.UI.Sound;
using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.Options;

[InjectableClass]
public class UIKeyBindings : UIScreenComponent
{
	[SerializeField]
	private SliderScaledValue mouseSensitivity;

	[SerializeField]
	private Toggle invertMouse;

	[SerializeField]
	private GameObject extraButtonsForKeyBindings;

	[SerializeField]
	private TextMeshProUGUI categoryTitlePrefab;

	[SerializeField]
	private Transform categorySectionPrefab;

	[SerializeField]
	private UIKeyBinding keyBindingPrefab;

	[SerializeField]
	private GameObject rebindingWindow;

	[SerializeField]
	private Text rebindingTextField;

	private KeyCode[] allKeyCodes;

	private OptionsSystem _optionsSystem;

	private bool rebindingKey;

	private InputButtons currentlyRebinding;

	private bool _warningDisplaying;

	private int currentlyRebindingIndex;

	private Dictionary<InputButtons, UIKeyBinding> allKeyBindings;

	public bool IsKeyRebindActive => rebindingWindow.activeSelf;

	private bool VoiceChatFeatureEnabled => WAConfig.Get<bool>(ConfigKeys.VOIPEnabled);

	[InjectableMethod]
	public void InjectDependencies(OptionsSystem optionsSystem)
	{
		_optionsSystem = optionsSystem;
	}

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
	}

	protected override void ProtectedDispose()
	{
	}

	protected override void Activate()
	{
		extraButtonsForKeyBindings.SetActive(value: true);
	}

	protected override void Deactivate()
	{
		extraButtonsForKeyBindings.SetActive(value: false);
	}

	public void ToggleInvertY(bool isInverted)
	{
		MouseInputProvider.InvertY = isInverted;
	}

	public void ChangeSensitivity(float value)
	{
		MouseInputProvider.MouseSensitivity = value;
	}

	private void Start()
	{
		invertMouse.onValueChanged.AddListener(SettingsScreen.ToggleSfx);
		invertMouse.isOn = MouseInputProvider.InvertY;
		mouseSensitivity.SetValue(MouseInputProvider.MouseSensitivity);
		List<KeyBindingCategory> bindingCategories = _optionsSystem.GetBindingCategories();
		allKeyBindings = new Dictionary<InputButtons, UIKeyBinding>();
		allKeyCodes = (KeyCode[])Enum.GetValues(typeof(KeyCode));
		foreach (KeyBindingCategory item in bindingCategories)
		{
			if (item.name == "VOICE" && !VoiceChatFeatureEnabled)
			{
				continue;
			}
			TextMeshProUGUI textMeshProUGUI = UnityEngine.Object.Instantiate(categoryTitlePrefab, base.transform);
			textMeshProUGUI.text = item.name.ToUpper();
			Transform parent = UnityEngine.Object.Instantiate(categorySectionPrefab, base.transform);
			InputButtons[] buttons = item.buttons;
			foreach (InputButtons inputButtons in buttons)
			{
				if (!_optionsSystem.DevOnlyKeyBindings.Contains(inputButtons))
				{
					UIKeyBinding uIKeyBinding = UnityEngine.Object.Instantiate(keyBindingPrefab, parent);
					uIKeyBinding.Init(this, inputButtons, _optionsSystem.GetKeyCodesForButton(inputButtons));
					allKeyBindings.Add(inputButtons, uIKeyBinding);
				}
			}
		}
	}

	public void SetToDefaults()
	{
		_optionsSystem.SetKeyBindsToDefaults();
		foreach (KeyValuePair<InputButtons, UIKeyBinding> allKeyBinding in allKeyBindings)
		{
			allKeyBinding.Value.Init(this, allKeyBinding.Key, _optionsSystem.GetKeyCodesForButton(allKeyBinding.Key));
		}
		mouseSensitivity.SetValue(1f);
		invertMouse.isOn = false;
		SoundScreen.PlayASound("Play_MainMenu_Error");
	}

	public void Clear(InputButtons button, int index)
	{
		_optionsSystem.RebindKey(button, index, KeyCode.None);
		allKeyBindings[button].Init(this, button, _optionsSystem.GetKeyCodesForButton(button));
	}

	public void StartRebind(InputButtons button, int index)
	{
		rebindingWindow.SetActive(value: true);
		rebindingTextField.text = "Rebinding : \"" + button.ToString().CamelCaseToSpaces() + "\"";
		rebindingKey = true;
		currentlyRebinding = button;
		currentlyRebindingIndex = index;
	}

	private void Update()
	{
		if (!rebindingKey || _warningDisplaying)
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			rebindingKey = false;
			rebindingWindow.SetActive(value: false);
			return;
		}
		KeyCode[] array = allKeyCodes;
		foreach (KeyCode keyCode in array)
		{
			if (!Input.GetKeyDown(keyCode))
			{
				continue;
			}
			if ((keyCode == KeyCode.Mouse0 || keyCode == KeyCode.Mouse1) && (currentlyRebinding != InputButtons.UseLeftHand || currentlyRebinding != InputButtons.UseRightHand))
			{
				_warningDisplaying = true;
				DialogPopupFacade.ShowOkDialog("Can't bind", "Mouse button can only be assigned to left or right hand", delegate
				{
					_warningDisplaying = false;
				});
				break;
			}
			_optionsSystem.RebindKey(currentlyRebinding, currentlyRebindingIndex, keyCode);
			rebindingKey = false;
			rebindingWindow.SetActive(value: false);
			allKeyBindings[currentlyRebinding].Init(this, currentlyRebinding, _optionsSystem.GetKeyCodesForButton(currentlyRebinding));
		}
	}
}
