using System.Collections.Generic;
using Travellers.UI.Framework;
using UnityEngine;

namespace Travellers.UI.DebugExtensions;

[InjectedInterface]
public interface IDebugExtensionsSystem
{
	List<KeyCode> KeyCodes { get; }

	void LoadJson();

	List<string> Process(string text);

	void CreateJsonFile();

	bool TryAndGetBoundMessage(KeyCode code, out string debugString);
}
