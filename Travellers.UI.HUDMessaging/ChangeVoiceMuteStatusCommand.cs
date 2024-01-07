using Travellers.UI.Chat;
using WAUtilities.Logging;

namespace Travellers.UI.HUDMessaging;

public class ChangeVoiceMuteStatusCommand : IUserCommandRoute
{
	private bool _mute;

	public ChangeVoiceMuteStatusCommand(bool mute)
	{
		_mute = mute;
	}

	public void Execute(UserInputCommand command)
	{
		if (MutedPlayerWarehouse.CurrentWarehouse == null)
		{
			WALogger.Error<HUDMessagingSystem>(LogChannel.UI, "Can't perform mute functions without a valid MutePlayerWarehouse", new object[0]);
			return;
		}
		string message = command.CommandlessMessage + " -voice";
		MutedPlayerWarehouse.CurrentWarehouse.ChangeMuteState(message, _mute);
	}
}
