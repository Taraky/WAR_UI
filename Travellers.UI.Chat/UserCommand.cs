namespace Travellers.UI.Chat;

public struct UserCommand
{
	public UserCommandPrefix CommandPrefix;

	public UserCommandType CommandType;

	public string CommandString;

	public bool IsRoomSwitchCommand => CommandType == UserCommandType.ChatRoomSwitch;

	public bool IsGameCommand => CommandType != UserCommandType.ChatMessage && !IsRoomSwitchCommand;

	public bool IsClientOnlyGameCommand => CommandType != 0 && IsGameCommand;

	public bool IsDebugCommand => IsDevOnlyDebugCommand || CommandType == UserCommandType.UniversalDebug;

	public bool IsDevOnlyDebugCommand => CommandType == UserCommandType.DevDebugOnly || CommandType == UserCommandType.Unknown;

	public UserCommand(UserCommandPrefix commandPrefix, UserCommandType commandType, string commandString)
	{
		CommandString = commandString;
		CommandPrefix = commandPrefix;
		CommandType = commandType;
	}
}
