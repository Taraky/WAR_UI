namespace Travellers.UI.Social.Crew;

public class NoCrewState : CrewState
{
	public override bool ShouldRefreshBeacon => false;

	public override bool ShouldRefreshSlots => false;

	public NoCrewState(ScreenFields fields)
		: base(fields)
	{
	}

	public override void EnterState()
	{
		fields.NoCrewObject.SetActive(value: true);
		fields.HasCrewLeftPane.SetActive(value: false);
		fields.HasCrewRightPane.SetActive(value: false);
		fields.InvitePanel.SetActive(value: false);
		fields.BeaconPanel.SetActive(value: false);
	}

	public override void LeaveState()
	{
		fields.NoCrewObject.SetActive(value: false);
	}

	public override void UpdateBeaconDisplay(int cooldown, int maxCooldown)
	{
	}
}
