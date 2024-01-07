using System.Collections.Generic;

namespace Travellers.UI.Chat;

public abstract class ChatFilterModule
{
	public abstract UserCommandPrefix EntryPrefix { get; }

	protected abstract HashSet<UserCommandPrefix> permittedRoomSwitchCommands { get; }

	public bool IsAllowedRoomCommand(UserCommandPrefix prefixToCheck)
	{
		return permittedRoomSwitchCommands.Contains(prefixToCheck);
	}
}
