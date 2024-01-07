using Travellers.UI.Chat;

namespace Travellers.UI.HUDMessaging;

public interface IUserCommandRoute
{
	void Execute(UserInputCommand command);
}
