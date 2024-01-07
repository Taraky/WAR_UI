using WAUtilities.Logging;

namespace Travellers.UI.Chat;

public class RoomValues
{
	public readonly string RoomDisplayName;

	public readonly ColourReference.ColourType RoomDisplayColourType;

	private UserCommand _command;

	public UserCommand Command => _command;

	public RoomValues(string roomDisplayName, ColourReference.ColourType roomDisplayColourType)
	{
		RoomDisplayName = roomDisplayName;
		RoomDisplayColourType = roomDisplayColourType;
	}

	public void SetRoomCommand(UserCommandPrefix commandPrefix)
	{
		if (!UserCommandReference.TryGetCommandTypeFromPrefix(commandPrefix, out _command))
		{
			WALogger.Error<ChatRoomInputProcessor>(LogChannel.UI, "Attempting to set room {0} to use an invalid command prefix {1}", new object[2] { RoomDisplayName, commandPrefix });
		}
	}
}
