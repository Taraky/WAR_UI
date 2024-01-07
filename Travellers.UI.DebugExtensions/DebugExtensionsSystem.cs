using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Travellers.UI.Framework;
using UnityEngine;
using WAUtilities.Logging;

namespace Travellers.UI.DebugExtensions;

[InjectedSystem(InjectionType.Real)]
public class DebugExtensionsSystem : UISystem, IDebugExtensionsSystem
{
	private const string JSON_FILE_NAME = "KeybindingsToCommands.json";

	private const string LOAD_PATH = "/DebugKeyBindings";

	private char[] specialCharacters = new char[23]
	{
		'!', '"', '£', '$', '%', '^', '&', '*', '@', '~',
		'#', '?', '_', '\\', ':', ';', '<', '>', '`', '{',
		'}', '¬', '|'
	};

	private KeyBindingAliasJson _keyBindingWithAliases = new KeyBindingAliasJson();

	public const string CommandPrefix = "Running alias command";

	private KeyBindingAliasJson _defaultJsonContent = KeyBindingAliasJson.DefaultValues();

	public List<KeyCode> KeyCodes { get; private set; }

	protected override void AddListeners()
	{
	}

	public override void Init()
	{
		KeyCodes = new List<KeyCode>();
	}

	public void LoadJson()
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(Application.persistentDataPath + "/DebugKeyBindings/KeybindingsToCommands.json");
		if (File.Exists(directoryInfo.FullName))
		{
			string value = File.ReadAllText(directoryInfo.FullName);
			try
			{
				_keyBindingWithAliases = JsonConvert.DeserializeObject<KeyBindingAliasJson>(value);
				foreach (KeyValuePair<KeyCode, string> item in _keyBindingWithAliases.KeyBindings.Where((KeyValuePair<KeyCode, string> kvp) => kvp.Value.IndexOfAny(specialCharacters) != -1).ToList())
				{
					_keyBindingWithAliases.KeyBindings.Remove(item.Key);
					WALogger.Warn<IDebugExtensionsSystem>("Removing entry with keycode {0} and command {1} due to containing special character(s)", new object[2] { item.Key, item.Value });
				}
			}
			catch
			{
				WALogger.Warn<IDebugExtensionsSystem>("Issue with {0} - the JSON contents has failed to deserialize correctly so a new JSON will be created", new object[1] { "KeybindingsToCommands.json" });
				CreateJsonFile();
			}
		}
		else
		{
			CreateJsonFile();
		}
		KeyCodes = _keyBindingWithAliases.KeyBindings.Keys.ToList();
	}

	public void CreateJsonFile()
	{
		Directory.CreateDirectory(Application.persistentDataPath + "/DebugKeyBindings");
		using (StreamWriter textWriter = new StreamWriter(Application.persistentDataPath + "/DebugKeyBindings/KeybindingsToCommands.json", append: false))
		{
			JsonSerializer jsonSerializer = new JsonSerializer();
			jsonSerializer.Formatting = Formatting.Indented;
			jsonSerializer.Serialize(textWriter, _defaultJsonContent);
		}
		_keyBindingWithAliases = _defaultJsonContent;
	}

	public List<string> Process(string text)
	{
		bool flag = _keyBindingWithAliases.Aliases.ContainsKey(text);
		List<string> list = new List<string>();
		if (flag)
		{
			list.Add(string.Format("{0} \"{1}\"", "Running alias command", text));
			List<string> list2 = _keyBindingWithAliases.Aliases[text];
			for (int i = 0; i < list2.Count; i++)
			{
				list.Add(list2[i]);
			}
		}
		return list;
	}

	public bool TryAndGetBoundMessage(KeyCode code, out string debugString)
	{
		return _keyBindingWithAliases.KeyBindings.TryGetValue(code, out debugString);
	}

	public override void ControlledUpdate()
	{
	}

	protected override void Dispose()
	{
	}
}
