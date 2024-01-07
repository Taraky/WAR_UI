using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using WAUtilities.Logging;

namespace Travellers.UI.Options;

public class KeyBindingsLibrarian
{
	private readonly Dictionary<InputButtons, KeyCode[]> _defaultBindings = new Dictionary<InputButtons, KeyCode[]>
	{
		{
			InputButtons.Jump,
			new KeyCode[2]
			{
				KeyCode.Space,
				KeyCode.None
			}
		},
		{
			InputButtons.Sit,
			new KeyCode[2]
			{
				KeyCode.X,
				KeyCode.None
			}
		},
		{
			InputButtons.Ragdoll,
			new KeyCode[2]
			{
				KeyCode.G,
				KeyCode.None
			}
		},
		{
			InputButtons.Grab,
			new KeyCode[2]
			{
				KeyCode.Q,
				KeyCode.None
			}
		},
		{
			InputButtons.UseLeftHand,
			new KeyCode[2]
			{
				KeyCode.Mouse0,
				KeyCode.None
			}
		},
		{
			InputButtons.UseRightHand,
			new KeyCode[2]
			{
				KeyCode.Mouse1,
				KeyCode.None
			}
		},
		{
			InputButtons.Interact,
			new KeyCode[2]
			{
				KeyCode.E,
				KeyCode.None
			}
		},
		{
			InputButtons.Walk,
			new KeyCode[2]
			{
				KeyCode.LeftAlt,
				KeyCode.None
			}
		},
		{
			InputButtons.Sprint,
			new KeyCode[2]
			{
				KeyCode.LeftShift,
				KeyCode.None
			}
		},
		{
			InputButtons.SelectItem1,
			new KeyCode[2]
			{
				KeyCode.Alpha1,
				KeyCode.None
			}
		},
		{
			InputButtons.SelectItem2,
			new KeyCode[2]
			{
				KeyCode.Alpha2,
				KeyCode.None
			}
		},
		{
			InputButtons.SelectItem3,
			new KeyCode[2]
			{
				KeyCode.Alpha3,
				KeyCode.None
			}
		},
		{
			InputButtons.SelectItem4,
			new KeyCode[2]
			{
				KeyCode.Alpha4,
				KeyCode.None
			}
		},
		{
			InputButtons.SelectItem5,
			new KeyCode[2]
			{
				KeyCode.Alpha5,
				KeyCode.None
			}
		},
		{
			InputButtons.SelectItem6,
			new KeyCode[2]
			{
				KeyCode.Alpha6,
				KeyCode.None
			}
		},
		{
			InputButtons.SelectItem7,
			new KeyCode[2]
			{
				KeyCode.Alpha7,
				KeyCode.None
			}
		},
		{
			InputButtons.SelectItem8,
			new KeyCode[2]
			{
				KeyCode.Alpha8,
				KeyCode.None
			}
		},
		{
			InputButtons.Melee,
			new KeyCode[2]
			{
				KeyCode.F,
				KeyCode.None
			}
		},
		{
			InputButtons.Holster,
			new KeyCode[2]
			{
				KeyCode.Q,
				KeyCode.None
			}
		},
		{
			InputButtons.RotatePart,
			new KeyCode[2]
			{
				KeyCode.Z,
				KeyCode.None
			}
		},
		{
			InputButtons.RopeReelIn,
			new KeyCode[2]
			{
				KeyCode.LeftShift,
				KeyCode.None
			}
		},
		{
			InputButtons.RopeReelOut,
			new KeyCode[2]
			{
				KeyCode.LeftControl,
				KeyCode.None
			}
		},
		{
			InputButtons.OpenInventory,
			new KeyCode[2]
			{
				KeyCode.Tab,
				KeyCode.None
			}
		},
		{
			InputButtons.OpenFeedback,
			new KeyCode[2]
			{
				KeyCode.F2,
				KeyCode.None
			}
		},
		{
			InputButtons.OpenChat,
			new KeyCode[2]
			{
				KeyCode.Return,
				KeyCode.None
			}
		},
		{
			InputButtons.OpenSocial,
			new KeyCode[2]
			{
				KeyCode.O,
				KeyCode.None
			}
		},
		{
			InputButtons.Cancel,
			new KeyCode[2]
			{
				KeyCode.Escape,
				KeyCode.None
			}
		},
		{
			InputButtons.ToggleUI,
			new KeyCode[2]
			{
				KeyCode.F4,
				KeyCode.None
			}
		},
		{
			InputButtons.UseUtilityHead,
			new KeyCode[2]
			{
				KeyCode.T,
				KeyCode.None
			}
		},
		{
			InputButtons.UseUtility,
			new KeyCode[2]
			{
				KeyCode.Space,
				KeyCode.None
			}
		},
		{
			InputButtons.UseUtilityFeet,
			new KeyCode[2]
			{
				KeyCode.Y,
				KeyCode.None
			}
		},
		{
			InputButtons.MoveForwards,
			new KeyCode[2]
			{
				KeyCode.W,
				KeyCode.None
			}
		},
		{
			InputButtons.MoveBackwards,
			new KeyCode[2]
			{
				KeyCode.S,
				KeyCode.None
			}
		},
		{
			InputButtons.MoveLeft,
			new KeyCode[2]
			{
				KeyCode.A,
				KeyCode.None
			}
		},
		{
			InputButtons.MoveRight,
			new KeyCode[2]
			{
				KeyCode.D,
				KeyCode.None
			}
		},
		{
			InputButtons.ShipThrottleUp,
			new KeyCode[2]
			{
				KeyCode.W,
				KeyCode.None
			}
		},
		{
			InputButtons.ShipThrottleDown,
			new KeyCode[2]
			{
				KeyCode.S,
				KeyCode.None
			}
		},
		{
			InputButtons.ShipYawLeft,
			new KeyCode[2]
			{
				KeyCode.A,
				KeyCode.None
			}
		},
		{
			InputButtons.ShipYawRight,
			new KeyCode[2]
			{
				KeyCode.D,
				KeyCode.None
			}
		},
		{
			InputButtons.ShipMoveUp,
			new KeyCode[2]
			{
				KeyCode.LeftShift,
				KeyCode.None
			}
		},
		{
			InputButtons.ShipMoveDown,
			new KeyCode[2]
			{
				KeyCode.LeftControl,
				KeyCode.None
			}
		},
		{
			InputButtons.ToggleHUDDebugDisplay,
			new KeyCode[2]
			{
				KeyCode.Equals,
				KeyCode.None
			}
		},
		{
			InputButtons.OpenDebugConsole,
			new KeyCode[2]
			{
				KeyCode.BackQuote,
				KeyCode.None
			}
		},
		{
			InputButtons.Crouch,
			new KeyCode[2]
			{
				KeyCode.LeftControl,
				KeyCode.None
			}
		},
		{
			InputButtons.Emotes,
			new KeyCode[2]
			{
				KeyCode.C,
				KeyCode.None
			}
		},
		{
			InputButtons.SwitchShoulder,
			new KeyCode[2]
			{
				KeyCode.Mouse2,
				KeyCode.None
			}
		},
		{
			InputButtons.FirstPerson,
			new KeyCode[2]
			{
				KeyCode.V,
				KeyCode.None
			}
		},
		{
			InputButtons.NoteE,
			new KeyCode[2]
			{
				KeyCode.Q,
				KeyCode.None
			}
		},
		{
			InputButtons.NoteF,
			new KeyCode[2]
			{
				KeyCode.W,
				KeyCode.None
			}
		},
		{
			InputButtons.NoteG,
			new KeyCode[2]
			{
				KeyCode.E,
				KeyCode.None
			}
		},
		{
			InputButtons.NoteA,
			new KeyCode[2]
			{
				KeyCode.R,
				KeyCode.None
			}
		},
		{
			InputButtons.NoteB,
			new KeyCode[2]
			{
				KeyCode.A,
				KeyCode.None
			}
		},
		{
			InputButtons.NoteC,
			new KeyCode[2]
			{
				KeyCode.S,
				KeyCode.None
			}
		},
		{
			InputButtons.NoteD,
			new KeyCode[2]
			{
				KeyCode.D,
				KeyCode.None
			}
		},
		{
			InputButtons.NoteEOctave,
			new KeyCode[2]
			{
				KeyCode.F,
				KeyCode.None
			}
		},
		{
			InputButtons.Modifier1,
			new KeyCode[2]
			{
				KeyCode.LeftShift,
				KeyCode.None
			}
		},
		{
			InputButtons.Modifier2,
			new KeyCode[2]
			{
				KeyCode.LeftControl,
				KeyCode.None
			}
		},
		{
			InputButtons.InstrumentControls,
			new KeyCode[2]
			{
				KeyCode.F1,
				KeyCode.None
			}
		},
		{
			InputButtons.PushToTalk,
			new KeyCode[2]
			{
				KeyCode.U,
				KeyCode.None
			}
		}
	};

	public HashSet<InputButtons> DevOnlyButtons = new HashSet<InputButtons>
	{
		InputButtons.ToggleHUDDebugDisplay,
		InputButtons.OpenDebugConsole
	};

	private readonly List<KeyBindingCategory> _keybindingCategories = new List<KeyBindingCategory>
	{
		new KeyBindingCategory("MOVEMENT", new InputButtons[11]
		{
			InputButtons.MoveForwards,
			InputButtons.MoveBackwards,
			InputButtons.MoveLeft,
			InputButtons.MoveRight,
			InputButtons.Jump,
			InputButtons.Walk,
			InputButtons.Sprint,
			InputButtons.RopeReelIn,
			InputButtons.RopeReelOut,
			InputButtons.Grab,
			InputButtons.Crouch
		}),
		new KeyBindingCategory("INTERACTIONS", new InputButtons[7]
		{
			InputButtons.UseLeftHand,
			InputButtons.UseRightHand,
			InputButtons.Interact,
			InputButtons.UseUtilityHead,
			InputButtons.UseUtility,
			InputButtons.UseUtilityFeet,
			InputButtons.Emotes
		}),
		new KeyBindingCategory("SHIP", new InputButtons[7]
		{
			InputButtons.ShipThrottleUp,
			InputButtons.ShipThrottleDown,
			InputButtons.ShipYawLeft,
			InputButtons.ShipYawRight,
			InputButtons.ShipMoveUp,
			InputButtons.ShipMoveDown,
			InputButtons.RotatePart
		}),
		new KeyBindingCategory("INVENTORY", new InputButtons[9]
		{
			InputButtons.OpenInventory,
			InputButtons.SelectItem1,
			InputButtons.SelectItem2,
			InputButtons.SelectItem3,
			InputButtons.SelectItem4,
			InputButtons.SelectItem5,
			InputButtons.SelectItem6,
			InputButtons.SelectItem7,
			InputButtons.SelectItem8
		}),
		new KeyBindingCategory("MUSICAL INSTRUMENTS", new InputButtons[11]
		{
			InputButtons.NoteE,
			InputButtons.NoteF,
			InputButtons.NoteG,
			InputButtons.NoteA,
			InputButtons.NoteB,
			InputButtons.NoteC,
			InputButtons.NoteD,
			InputButtons.NoteEOctave,
			InputButtons.Modifier1,
			InputButtons.Modifier2,
			InputButtons.InstrumentControls
		}),
		new KeyBindingCategory("MISCELLANEOUS", new InputButtons[6]
		{
			InputButtons.ToggleUI,
			InputButtons.OpenChat,
			InputButtons.OpenSocial,
			InputButtons.OpenFeedback,
			InputButtons.SwitchShoulder,
			InputButtons.FirstPerson
		}),
		new KeyBindingCategory("VOICE", new InputButtons[1] { InputButtons.PushToTalk })
	};

	private const string SavedKeyBindingsPrefsKey = "SavedKeyBindings";

	private Dictionary<InputButtons, KeyCode[]> _cachedBindings = new Dictionary<InputButtons, KeyCode[]>();

	public List<KeyBindingCategory> KeybindingCategories => _keybindingCategories;

	public Dictionary<InputButtons, KeyCode[]> CachedBindings
	{
		get
		{
			if (_cachedBindings.Count == 0)
			{
				WALogger.Error<KeyBindingsLibrarian>("Attempting to get keybindings before initialising BindingsLibrarian");
				return _defaultBindings;
			}
			return _cachedBindings;
		}
	}

	public KeyBindingsLibrarian()
	{
		CheckForMissingBindings();
		UpdateBindings();
	}

	private void CheckForMissingBindings()
	{
		InputButtons[] array = (InputButtons[])Enum.GetValues(typeof(InputButtons));
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array.Length; i++)
		{
			if (!_defaultBindings.ContainsKey(array[i]) && !DevOnlyButtons.Contains(array[i]))
			{
				stringBuilder.AppendLine($"Key: [{array[i]}]");
			}
		}
		if (stringBuilder.Length > 0)
		{
			Debug.LogErrorFormat("The following input keys are unbound: \n" + stringBuilder);
		}
	}

	public void RebindKey(InputButtons keyToRebind, int indexToRebind, KeyCode newKeyCode)
	{
		_cachedBindings[keyToRebind][indexToRebind] = newKeyCode;
		SaveCurrentKeyBindings();
	}

	public void SetKeyBindsToDefaults()
	{
		_cachedBindings = _defaultBindings;
		SaveCurrentKeyBindings();
	}

	public KeyCode[] GetKeyCodesForButton(InputButtons button)
	{
		if (!CachedBindings.TryGetValue(button, out var value))
		{
			WALogger.Error<KeyBindingsLibrarian>("Attempting to get keycodes for button [{0}]. Not in dictionary", new object[1] { button });
		}
		return value;
	}

	public string GetInputButtonLabel(InputButtons inputButton)
	{
		if (CachedBindings.Count == 0)
		{
			WALogger.Warn<KeyBindingsLibrarian>("No keybindings found");
			return string.Empty;
		}
		if (!CachedBindings.TryGetValue(inputButton, out var value))
		{
			WALogger.Error<KeyBindingsLibrarian>("Attempting to get keycodes for button [{0}]. Not in dictionary", new object[1] { inputButton });
			return "Not found";
		}
		return value[0].ToString();
	}

	private void UpdateBindings()
	{
		if (_cachedBindings.Count != 0)
		{
			return;
		}
		if (PlayerPrefs.HasKey("SavedKeyBindings"))
		{
			bool saveBindings = false;
			_cachedBindings = ParseSavedKeyBindings(PlayerPrefs.GetString("SavedKeyBindings"), out saveBindings);
			if (saveBindings)
			{
				SaveCurrentKeyBindings();
			}
		}
		else
		{
			_cachedBindings = _defaultBindings;
			SaveCurrentKeyBindings();
		}
	}

	private Dictionary<InputButtons, KeyCode[]> ParseSavedKeyBindings(string bindingsJson, out bool saveBindings)
	{
		saveBindings = false;
		try
		{
			Dictionary<string, KeyCode[]> dictionary = JsonConvert.DeserializeObject<Dictionary<string, KeyCode[]>>(bindingsJson);
			Dictionary<InputButtons, KeyCode[]> dictionary2 = new Dictionary<InputButtons, KeyCode[]>();
			foreach (KeyValuePair<string, KeyCode[]> item in dictionary)
			{
				try
				{
					InputButtons key = (InputButtons)Enum.Parse(typeof(InputButtons), item.Key);
					dictionary2[key] = item.Value;
				}
				catch
				{
					saveBindings = true;
				}
			}
			foreach (InputButtons key2 in _defaultBindings.Keys)
			{
				if (!dictionary2.ContainsKey(key2))
				{
					dictionary2[key2] = _defaultBindings[key2];
					saveBindings = true;
				}
			}
			return dictionary2;
		}
		catch
		{
			return _defaultBindings;
		}
	}

	private void SaveCurrentKeyBindings()
	{
		PlayerPrefs.SetString("SavedKeyBindings", JsonConvert.SerializeObject(_cachedBindings));
		PlayerPrefs.Save();
	}
}
