using Travellers.UI.Chat;
using WAUtilities.Logging;

namespace Travellers.UI.HUDMessaging;

public class ShowMuteListCommand : IUserCommandRoute
{
	public void Execute(UserInputCommand command)
	{
		if (MutedPlayerWarehouse.CurrentWarehouse == null)
		{
			WALogger.Error<HUDMessagingSystem>(LogChannel.UI, "Can't perform mute functions without a valid MutePlayerWarehouse", new object[0]);
		}
		else
		{
			MutedPlayerWarehouse.CurrentWarehouse.ShowMutedList();
		}
	}
}
