using Travellers.UI.Framework;
using Travellers.UI.Chat;

namespace Travellers.UI.HUDMessaging;

public class RetargetGraphCommand : IUserCommandRoute
{
	public void Execute(UserInputCommand command)
	{
		UIWindowController.PushState(DebugGraphState.Default);
	}
}
