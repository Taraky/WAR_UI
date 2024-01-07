using System;
using System.Collections.Generic;
using Bossa.Travellers.World;
using WAUtilities.Logging;

namespace Travellers.UI.Chat;

public class UserCommandReference
{
	private static readonly HashSet<MessageType> _chatSpecificTypes;

	private static readonly List<UserCommand> _userCommands;

	private static readonly Dictionary<string, UserCommand> _commandByCommandString;

	private static readonly Dictionary<UserCommandPrefix, UserCommand> _commandByCommandPrefix;

	static UserCommandReference()
	{
		_chatSpecificTypes = new HashSet<MessageType>
		{
			MessageType.Alliance,
			MessageType.Crew,
			MessageType.WhitelistedChat,
			MessageType.Chat
		};
		_userCommands = new List<UserCommand>
		{
			new UserCommand(UserCommandPrefix.Say, UserCommandType.ChatRoomSwitch, "/say"),
			new UserCommand(UserCommandPrefix.Crew, UserCommandType.ChatRoomSwitch, "/crew"),
			new UserCommand(UserCommandPrefix.Alliance, UserCommandType.ChatRoomSwitch, "/alliance"),
			new UserCommand(UserCommandPrefix.Shout, UserCommandType.UniversalDebug, "/shout"),
			new UserCommand(UserCommandPrefix.Mute, UserCommandType.UniversalDebug, "/mute"),
			new UserCommand(UserCommandPrefix.Unmute, UserCommandType.UniversalDebug, "/unmute"),
			new UserCommand(UserCommandPrefix.MuteVoice, UserCommandType.UniversalDebug, "/muteVoice"),
			new UserCommand(UserCommandPrefix.UnmuteVoice, UserCommandType.UniversalDebug, "/unmuteVoice"),
			new UserCommand(UserCommandPrefix.MuteList, UserCommandType.UniversalDebug, "/muteList"),
			new UserCommand(UserCommandPrefix.DevConsole, UserCommandType.DevDebugOnly, "/devConsole"),
			new UserCommand(UserCommandPrefix.Graph, UserCommandType.DevDebugOnly, "/graph"),
			new UserCommand(UserCommandPrefix.FPSGraph, UserCommandType.UniversalDebug, "/showFPS"),
			new UserCommand(UserCommandPrefix.ToggleVFX, UserCommandType.DevDebugOnly, "/vfx"),
			new UserCommand(UserCommandPrefix.FPSLog, UserCommandType.DevDebugOnly, "/fpslog"),
			new UserCommand(UserCommandPrefix.ColliderCulling, UserCommandType.DevDebugOnly, "/colliderCulling"),
			new UserCommand(UserCommandPrefix.ParticleLOD, UserCommandType.DevDebugOnly, "/particleLOD"),
			new UserCommand(UserCommandPrefix.OcclusionCulling, UserCommandType.DevDebugOnly, "/occlusionCulling"),
			new UserCommand(UserCommandPrefix.Imposters, UserCommandType.DevDebugOnly, "/imposters"),
			new UserCommand(UserCommandPrefix.ToggleClouds, UserCommandType.DevDebugOnly, "/toggleClouds"),
			new UserCommand(UserCommandPrefix.HighlightEntity, UserCommandType.DevDebugOnly, "/highlight"),
			new UserCommand(UserCommandPrefix.ToggleUIStateLogging, UserCommandType.DevDebugOnly, "/uilogs"),
			new UserCommand(UserCommandPrefix.BroadcastMessage, UserCommandType.DevDebugOnly, "/broadcast")
		};
		_commandByCommandString = new Dictionary<string, UserCommand>();
		_commandByCommandPrefix = new Dictionary<UserCommandPrefix, UserCommand>();
		HashSet<UserCommandPrefix> hashSet = new HashSet<UserCommandPrefix>((UserCommandPrefix[])Enum.GetValues(typeof(UserCommandPrefix)));
		foreach (UserCommand userCommand in _userCommands)
		{
			_commandByCommandPrefix[userCommand.CommandPrefix] = userCommand;
			_commandByCommandString[userCommand.CommandString.ToLower()] = userCommand;
			hashSet.Remove(userCommand.CommandPrefix);
		}
		if (hashSet.Count <= 0)
		{
			return;
		}
		foreach (UserCommandPrefix item in hashSet)
		{
			if (item != 0)
			{
				WALogger.Error<UserCommandReference>(LogChannel.UI, "Prefix {0} has not been assigned to valid command list", new object[1] { item });
			}
		}
	}

	public static bool TryGetCommandTypeFromString(string prefix, out UserCommand command)
	{
		if (_commandByCommandString.TryGetValue(prefix.ToLower(), out command))
		{
			return true;
		}
		command = new UserCommand(UserCommandPrefix.Unknown, UserCommandType.Unknown, prefix);
		return false;
	}

	public static bool TryGetCommandTypeFromPrefix(UserCommandPrefix prefix, out UserCommand command)
	{
		if (_commandByCommandPrefix.TryGetValue(prefix, out command))
		{
			return true;
		}
		return false;
	}

	public static bool ServerMessageTypeIsChat(MessageType messageTypeToCheck)
	{
		return _chatSpecificTypes.Contains(messageTypeToCheck);
	}
}
