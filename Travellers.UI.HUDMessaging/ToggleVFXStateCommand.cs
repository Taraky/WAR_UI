using Travellers.UI.Framework;
using Travellers.UI.Chat;
using Travellers.UI.Options;

namespace Travellers.UI.HUDMessaging;

public class ToggleVFXStateCommand : IUserCommandRoute
{
	private readonly LazyUISystem<OptionsSystem> _optionSystem = new LazyUISystem<OptionsSystem>();

	public void Execute(UserInputCommand command)
	{
		_optionSystem.Value.VFXActive = !_optionSystem.Value.VFXActive;
		OSDMessage.SendMessage(string.Format("Changing VFX state to {0}", (!_optionSystem.Value.VFXActive) ? "off" : "on"));
	}
}
