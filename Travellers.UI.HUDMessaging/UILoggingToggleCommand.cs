using Travellers.UI.Framework;
using Travellers.UI.Chat;

namespace Travellers.UI.HUDMessaging;

public class UILoggingToggleCommand : IUserCommandRoute
{
	public void Execute(UserInputCommand command)
	{
		UIWindowController.ActivityLogging = !UIWindowController.ActivityLogging;
		OSDMessage.SendMessage(string.Format("UI logging is now {0}", (!UIWindowController.ActivityLogging) ? "disabled" : "enabled"));
	}
}
