using Travellers.UI.Chat;
using WAUtilities.Logging;

namespace Travellers.UI.HUDMessaging;

public class ChangeMuteStatusCommand : IUserCommandRoute
{
	private bool _mute;

	public ChangeMuteStatusCommand(bool mute)
	{
		_mute = mute;
	}

	public void Execute(UserInputCommand command)
	{
		if (MutedPlayerWarehouse.CurrentWarehouse == null)
		{
			WALogger.Error<HUDMessagingSystem>(LogChannel.UI, "Can't perform mute functions without a valid MutePlayerWarehouse", new object[0]);
		}
		else
		{
			MutedPlayerWarehouse.CurrentWarehouse.ChangeMuteState(command.CommandlessMessage, _mute);
		}
	}
}
