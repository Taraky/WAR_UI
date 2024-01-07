using Travellers.UI.Chat;
using UnityEngine;

namespace Travellers.UI.HUDMessaging;

public class ToggleCloudsCommand : IUserCommandRoute
{
	public void Execute(UserInputCommand command)
	{
		CmdBufClouds cmdBufClouds = Resources.FindObjectsOfTypeAll<CmdBufClouds>()[0];
		if (cmdBufClouds != null)
		{
			cmdBufClouds.gameObject.SetActive(!cmdBufClouds.gameObject.activeSelf);
			OSDMessage.SendMessage((!cmdBufClouds.gameObject.activeSelf) ? "Clouds turned off." : "Clouds turned on.");
		}
	}
}
