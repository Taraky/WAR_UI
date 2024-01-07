using Assets.Scripts.Player;
using Travellers.UI.Chat;
using UnityEngine;

namespace Travellers.UI.HUDMessaging;

public class HighlightCommand : IUserCommandRoute
{
	public void Execute(UserInputCommand command)
	{
		GameObject debugLookingAtEntity = PlayerLookingAt.Instance.DebugLookingAtEntity;
		if (!debugLookingAtEntity)
		{
			OSDMessage.SendMessage("Not looking at an entity. " + debugLookingAtEntity);
		}
		else if (EntityHighlightSystem.IsEntityHighlighted(debugLookingAtEntity))
		{
			EntityHighlightSystem.StopHighlightEntity(debugLookingAtEntity);
			OSDMessage.SendMessage("Removing highlight on " + debugLookingAtEntity);
		}
		else
		{
			EntityHighlightSystem.HighlightEntity(debugLookingAtEntity);
			OSDMessage.SendMessage("Applying highlight on " + debugLookingAtEntity);
		}
	}
}
