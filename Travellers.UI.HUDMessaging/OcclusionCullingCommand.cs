using Travellers.UI.Chat;

namespace Travellers.UI.HUDMessaging;

public class OcclusionCullingCommand : IUserCommandRoute
{
	public void Execute(UserInputCommand command)
	{
		IslandOcclusionData.OcclusionCullingEnabled = !IslandOcclusionData.OcclusionCullingEnabled;
		OSDMessage.SendMessage((!IslandOcclusionData.OcclusionCullingEnabled) ? "Occlusion cullring turned off." : "Occlusion cullring turned on.");
	}
}
