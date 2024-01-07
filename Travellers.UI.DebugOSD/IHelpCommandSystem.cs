using System;
using System.Collections.Generic;
using Assets.Scripts.Utils;
using Travellers.UI.Framework;

namespace Travellers.UI.DebugOSD;

[InjectedInterface]
public interface IHelpCommandSystem
{
	bool IsPopulated { get; }

	void RetrieveHelpCommandsAsync(Action<Dictionary<string, List<Utils.Tuple<string, string>>>> onRetrieveHelpCommandsList = null);

	bool GetPlayerPermissionState();
}
