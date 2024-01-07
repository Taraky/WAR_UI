using Travellers.UI.DebugDisplay;
using Travellers.UI.Framework;
using Travellers.UI.Chat;

namespace Travellers.UI.HUDMessaging;

public class ChangeFPSGraphStateCommand : IUserCommandRoute
{
	public void Execute(UserInputCommand command)
	{
		UIWindowController.PushState(new DebugFPSState(new DebugFPSScreenFlags(command.CommandlessMessage)));
	}
}
