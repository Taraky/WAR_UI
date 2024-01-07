using System.Collections.Generic;

namespace Travellers.UI.Chat;

public class StandardChatFilter : ChatFilterModule
{
	private HashSet<UserCommandPrefix> _permittedCommandsLocal = new HashSet<UserCommandPrefix>
	{
		UserCommandPrefix.Say,
		UserCommandPrefix.Crew
	};

	public override UserCommandPrefix EntryPrefix => UserCommandPrefix.Say;

	protected override HashSet<UserCommandPrefix> permittedRoomSwitchCommands => _permittedCommandsLocal;
}
