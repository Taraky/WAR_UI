using System.Collections.Generic;
using UnityEngine;

namespace Travellers.UI.DebugExtensions;

public class KeyBindingAliasJson
{
	public Dictionary<KeyCode, string> KeyBindings = new Dictionary<KeyCode, string>();

	public Dictionary<string, List<string>> Aliases = new Dictionary<string, List<string>>();

	public static KeyBindingAliasJson DefaultValues()
	{
		KeyBindingAliasJson keyBindingAliasJson = new KeyBindingAliasJson();
		keyBindingAliasJson.KeyBindings = new Dictionary<KeyCode, string>
		{
			{
				KeyCode.F1,
				"/pizza"
			},
			{
				KeyCode.F2,
				"/spawnship"
			},
			{
				KeyCode.F6,
				"/g iron 500"
			},
			{
				KeyCode.F7,
				"/g aluminium 500"
			},
			{
				KeyCode.F8,
				"/g oak 500"
			}
		};
		keyBindingAliasJson.Aliases = new Dictionary<string, List<string>>
		{
			{
				"/idkfa",
				new List<string> { "/g craftingstation", "/g shipyard", "/g iron 500", "/g oak 500", "/superuser", "/god", "/unlockAllFixedSchematics", "/hideTutorial", "/info pos", "/useKnowledgeNode Shipbuilding free" }
			},
			{
				"/tss",
				new List<string> { "/t -345 -181 22", "/ss" }
			}
		};
		return keyBindingAliasJson;
	}
}
