using Travellers.UI.Chat;

namespace Travellers.UI.HUDMessaging;

public class ParticleLODCommand : IUserCommandRoute
{
	public void Execute(UserInputCommand command)
	{
		ParticleEffectsLODController.IsParticleLODEnabled = !ParticleEffectsLODController.IsParticleLODEnabled;
		if (ParticleEffectsLODController.IsParticleLODEnabled)
		{
			ParticleEffectsLODController.ForceUpdateParticle();
			OSDMessage.SendMessage("Particle LOD turned on.");
		}
		else
		{
			ParticleEffectsLODController.ResetOptimizedParticle();
			OSDMessage.SendMessage("Particle LOD turned off.");
		}
	}
}
