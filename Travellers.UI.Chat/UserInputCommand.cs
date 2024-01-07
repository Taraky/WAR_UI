using Travellers.UI.Utility;

namespace Travellers.UI.Chat;

public struct UserInputCommand
{
	public readonly UserCommand Command;

	public readonly string CommandlessMessage;

	public readonly string FullMessage;

	public readonly bool EmptyMessage;

	public readonly bool OnlyRoomSwitch;

	private static readonly char[] StringSplitter = new char[1] { ' ' };

	public UserInputCommand(string unParsedMessage)
	{
		unParsedMessage = unParsedMessage.Trim();
		if (unParsedMessage.StartsWith("/"))
		{
			string[] array = unParsedMessage.Split(StringSplitter, 2);
			string prefix = array[0];
			string commandlessMessage = ((array.Length <= 1) ? string.Empty : array[1]);
			UserCommandReference.TryGetCommandTypeFromString(prefix, out Command);
			CommandlessMessage = commandlessMessage;
		}
		else
		{
			Command = new UserCommand(UserCommandPrefix.Say, UserCommandType.ChatMessage, null);
			CommandlessMessage = unParsedMessage;
		}
		FullMessage = unParsedMessage;
		EmptyMessage = FullMessage.IsNullOrEmptyOrOnlySpaces();
		OnlyRoomSwitch = Command.IsRoomSwitchCommand && CommandlessMessage.IsNullOrEmptyOrOnlySpaces();
	}
}
