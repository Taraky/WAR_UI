using Bossa.Travellers.Analytics;
using Travellers.UI.Chat;

namespace Travellers.UI.HUDMessaging;

public class FPSLogCommand : IUserCommandRoute
{
	public void Execute(UserInputCommand command)
	{
		if (command.CommandlessMessage.Contains("save"))
		{
			FPSLogging.SaveLogs(continueLogging: true);
			OSDMessage.SendMessage("Saving FPS to base directory");
		}
	}
}
