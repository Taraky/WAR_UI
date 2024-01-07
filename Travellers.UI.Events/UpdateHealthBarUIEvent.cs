namespace Travellers.UI.Events;

public class UpdateHealthBarUIEvent : UIEvent
{
	public int CurrentHealth;

	public int MaxHealth;

	public UpdateHealthBarUIEvent(int currentHealth, int maxHealth)
	{
		CurrentHealth = currentHealth;
		MaxHealth = maxHealth;
	}
}
