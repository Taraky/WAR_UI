using System.Collections.Generic;
using Travellers.UI.Chat;
using WAUtilities.Logging;

namespace Travellers.UI.HUDMessaging;

public class ClientMessagingCommandRouter
{
	private Dictionary<UserCommandPrefix, IUserCommandRoute> _commandRoutes;

	public ClientMessagingCommandRouter()
	{
		_commandRoutes = new Dictionary<UserCommandPrefix, IUserCommandRoute>
		{
			{
				UserCommandPrefix.Mute,
				new ChangeMuteStatusCommand(mute: true)
			},
			{
				UserCommandPrefix.Unmute,
				new ChangeMuteStatusCommand(mute: false)
			},
			{
				UserCommandPrefix.MuteVoice,
				new ChangeVoiceMuteStatusCommand(mute: true)
			},
			{
				UserCommandPrefix.UnmuteVoice,
				new ChangeVoiceMuteStatusCommand(mute: false)
			},
			{
				UserCommandPrefix.MuteList,
				new ShowMuteListCommand()
			},
			{
				UserCommandPrefix.FPSGraph,
				new ChangeFPSGraphStateCommand()
			},
			{
				UserCommandPrefix.ToggleVFX,
				new ToggleVFXStateCommand()
			},
			{
				UserCommandPrefix.DevConsole,
				new ChangeDevConsoleStateCommand()
			},
			{
				UserCommandPrefix.Graph,
				new RetargetGraphCommand()
			},
			{
				UserCommandPrefix.FPSLog,
				new FPSLogCommand()
			},
			{
				UserCommandPrefix.ColliderCulling,
				new ColliderCullingCommand()
			},
			{
				UserCommandPrefix.ParticleLOD,
				new ParticleLODCommand()
			},
			{
				UserCommandPrefix.OcclusionCulling,
				new OcclusionCullingCommand()
			},
			{
				UserCommandPrefix.Imposters,
				new ImposterCommand()
			},
			{
				UserCommandPrefix.ToggleClouds,
				new ToggleCloudsCommand()
			},
			{
				UserCommandPrefix.HighlightEntity,
				new HighlightCommand()
			},
			{
				UserCommandPrefix.ToggleUIStateLogging,
				new UILoggingToggleCommand()
			},
			{
				UserCommandPrefix.BroadcastMessage,
				new BroadcastCommand()
			}
		};
	}

	public void CheckMessageForCommands(UserInputCommand userCommand)
	{
		Dictionary<UserCommandPrefix, IUserCommandRoute> commandRoutes = _commandRoutes;
		UserCommand command = userCommand.Command;
		if (commandRoutes.TryGetValue(command.CommandPrefix, out var value))
		{
			value.Execute(userCommand);
			WALogger.Info<ClientMessagingCommandRouter>("Player name[" + LocalPlayer.Instance.PlayerName + "] id [" + LocalPlayer.Instance.PlayerId + "] has sent cheat command: " + userCommand.FullMessage);
		}
	}

	public bool IsMessageSourceMuted(string fromWhom)
	{
		if (MutedPlayerWarehouse.CurrentWarehouse != null)
		{
			return MutedPlayerWarehouse.CurrentWarehouse.IsMuted(fromWhom);
		}
		return false;
	}
}
