using Travellers.UI.Framework;
using Travellers.UI.Chat;

namespace Travellers.UI.HUDMessaging;

public class ChangeDevConsoleStateCommand : IUserCommandRoute
{
	private readonly LazyUIInterface<IHUDMessagingSystem> _hudMessaging = new LazyUIInterface<IHUDMessagingSystem>();

	public void Execute(UserInputCommand command)
	{
		_hudMessaging.Value.UnityDevConsoleVisibility = !_hudMessaging.Value.UnityDevConsoleVisibility;
		OSDMessage.SendMessage(string.Format("Turning dev console {0}", (!_hudMessaging.Value.UnityDevConsoleVisibility) ? "off" : "on"));
	}
}
