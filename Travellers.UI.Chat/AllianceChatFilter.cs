using System.Collections.Generic;

namespace Travellers.UI.Chat;

public class AllianceChatFilter : ChatFilterModule
{
	private HashSet<UserCommandPrefix> _permittedCommandsLocal = new HashSet<UserCommandPrefix> { UserCommandPrefix.Alliance };

	public override UserCommandPrefix EntryPrefix => UserCommandPrefix.Alliance;

	protected override HashSet<UserCommandPrefix> permittedRoomSwitchCommands => _permittedCommandsLocal;
}
