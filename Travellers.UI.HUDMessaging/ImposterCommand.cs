using Bossa.Travellers.Visualisers.Islands;
using ImposterSystem;
using Travellers.UI.Chat;

namespace Travellers.UI.HUDMessaging;

public class ImposterCommand : IUserCommandRoute
{
	public void Execute(UserInputCommand command)
	{
		if (command.EmptyMessage)
		{
			ImposterSystem.Singleton<ImpostersHandler>.Instance.enabled = !ImposterSystem.Singleton<ImpostersHandler>.Instance.enabled;
			OSDMessage.SendMessage((!ImposterSystem.Singleton<ImpostersHandler>.Instance.enabled) ? "Imposter system turned off." : "Imposter system turned on.");
		}
		else if (command.CommandlessMessage.Contains("ship"))
		{
			bool flag = !ShipImposter.ShipImpostersEnabled;
			ShipImposter.SetShipImpostersState(flag);
			OSDMessage.SendMessage((!flag) ? "Ship imposters turned off." : "Ship imposters turned on.");
		}
		else if (command.CommandlessMessage.Contains("island"))
		{
			bool flag2 = !IslandVisualiser.IslandImpostersEnabled;
			IslandVisualiser.SetIslandImpostersState(flag2);
			OSDMessage.SendMessage((!flag2) ? "Island imposters turned off." : "Island imposters turned on.");
		}
	}
}
