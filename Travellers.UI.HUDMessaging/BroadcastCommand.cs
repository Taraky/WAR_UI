using Bossa.Travellers.World.Observer;
using Travellers.UI.Chat;
using Travellers.UI.InfoPopups;
using UnityEngine;

namespace Travellers.UI.HUDMessaging;

public class BroadcastCommand : IUserCommandRoute
{
	public void Execute(UserInputCommand command)
	{
		ChatSpeak chatSpeak = Object.FindObjectOfType<ChatSpeak>();
		if (chatSpeak != null)
		{
			DialogPopupFacade.ShowConfirmationDialog("Send broadcast message", "Sending message to all player. Are you sure?", delegate
			{
				chatSpeak.SendSocialWorkerMessage(command.FullMessage, isCheatCommand: true);
			}, "CONFIRM");
		}
	}
}
