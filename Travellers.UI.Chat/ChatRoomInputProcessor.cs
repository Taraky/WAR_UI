using System.Collections.Generic;

namespace Travellers.UI.Chat;

public class ChatRoomInputProcessor
{
	private readonly Dictionary<UserCommandPrefix, RoomValues> _roomLookup = new Dictionary<UserCommandPrefix, RoomValues>
	{
		{
			UserCommandPrefix.Say,
			new RoomValues(string.Empty, ColourReference.ColourType.ChatOSDMessage)
		},
		{
			UserCommandPrefix.Shout,
			new RoomValues(string.Empty, ColourReference.ColourType.ChatOSDMessage)
		},
		{
			UserCommandPrefix.Crew,
			new RoomValues("[CREW]", ColourReference.ColourType.CrewOSDMessage)
		},
		{
			UserCommandPrefix.Alliance,
			new RoomValues("[ALLIANCE]", ColourReference.ColourType.AllianceOSDMessage)
		}
	};

	private RoomValues _currentRoom;

	private readonly ChatFilterModule _currentFilter;

	public ColourReference.ColourType RoomColour => _currentRoom.RoomDisplayColourType;

	public string RoomName => _currentRoom.RoomDisplayName;

	public ChatRoomInputProcessor(ChatFilterModule currentFilter)
	{
		_currentFilter = currentFilter;
		TrySetRoomType(currentFilter.EntryPrefix);
	}

	public UserInputCommand ProcessInput(string text)
	{
		UserInputCommand result = new UserInputCommand(text);
		if (result.Command.IsRoomSwitchCommand)
		{
			UserCommand command = result.Command;
			TrySetRoomType(command.CommandPrefix);
		}
		else if (result.Command.IsGameCommand)
		{
			return result;
		}
		return new UserInputCommand(_currentRoom.Command.CommandString + " " + result.CommandlessMessage);
	}

	private void TrySetRoomType(UserCommandPrefix commandPrefix)
	{
		if (TryGetRoomFromCommand(commandPrefix, out var roomValue))
		{
			_currentRoom = roomValue;
			_currentRoom.SetRoomCommand(commandPrefix);
		}
	}

	private bool TryGetRoomFromCommand(UserCommandPrefix commandPrefix, out RoomValues roomValue)
	{
		if (_currentFilter.IsAllowedRoomCommand(commandPrefix))
		{
			if (_roomLookup.TryGetValue(commandPrefix, out roomValue))
			{
				return true;
			}
			return false;
		}
		roomValue = null;
		return false;
	}
}
