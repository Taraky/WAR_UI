using Assets.Scripts.Visualisers.Physical;
using Travellers.UI.Chat;

namespace Travellers.UI.HUDMessaging;

public class ColliderCullingCommand : IUserCommandRoute
{
	public void Execute(UserInputCommand command)
	{
		ClientColliderCulling.IsColliderCullingEnabled = !ClientColliderCulling.IsColliderCullingEnabled;
		if (ClientColliderCulling.IsColliderCullingEnabled)
		{
			OSDMessage.SendMessage("Collider culling turned on.");
		}
		else
		{
			OSDMessage.SendMessage("Collider culling turned off.");
		}
	}
}
