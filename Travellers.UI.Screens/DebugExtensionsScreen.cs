using Travellers.UI.DebugExtensions;
using Travellers.UI.Framework;
using Travellers.UI.HUDMessaging;
using Travellers.UI.Chat;
using UnityEngine;

namespace Travellers.UI.Screens;

[InjectableClass]
public class DebugExtensionsScreen : UIScreen
{
	private IHUDMessagingSystem _hudMessagingSystem;

	private IDebugExtensionsSystem _debugExtensionsSystem;

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
		if (_hudMessagingSystem.IsPlayerAdmin())
		{
			_debugExtensionsSystem.LoadJson();
		}
	}

	protected override void ProtectedDispose()
	{
	}

	[InjectableMethod]
	public void InjectDependencies(IHUDMessagingSystem hudMessagingSystem, IDebugExtensionsSystem debugExtensionsSystem)
	{
		_hudMessagingSystem = hudMessagingSystem;
		_debugExtensionsSystem = debugExtensionsSystem;
	}

	private void Update()
	{
		foreach (KeyCode keyCode in _debugExtensionsSystem.KeyCodes)
		{
			if (Input.GetKeyDown(keyCode))
			{
				string debugString = string.Empty;
				if (_debugExtensionsSystem.TryAndGetBoundMessage(keyCode, out debugString))
				{
					UserInputCommand debugCommand = new UserInputCommand(debugString);
					_hudMessagingSystem.SendDebugCommand(debugCommand);
				}
			}
		}
	}
}
