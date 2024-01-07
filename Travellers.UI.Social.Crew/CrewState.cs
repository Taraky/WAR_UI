namespace Travellers.UI.Social.Crew;

public abstract class CrewState
{
	protected ScreenFields fields;

	public abstract bool ShouldRefreshBeacon { get; }

	public abstract bool ShouldRefreshSlots { get; }

	protected CrewState(ScreenFields fields)
	{
		this.fields = fields;
	}

	public abstract void EnterState();

	public abstract void LeaveState();

	public abstract void UpdateBeaconDisplay(int currentCooldown, int maxCooldown);
}
