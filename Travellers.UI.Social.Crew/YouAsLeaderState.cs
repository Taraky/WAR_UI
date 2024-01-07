namespace Travellers.UI.Social.Crew;

public class YouAsLeaderState : CrewState
{
	public override bool ShouldRefreshBeacon => false;

	public override bool ShouldRefreshSlots => true;

	public YouAsLeaderState(ScreenFields fields)
		: base(fields)
	{
	}

	public override void EnterState()
	{
		fields.HasCrewLeftPane.SetActive(value: true);
		fields.HasCrewRightPane.SetActive(value: true);
		fields.InvitePanel.SetActive(value: true);
		fields.BeaconPanel.SetActive(value: false);
	}

	public override void LeaveState()
	{
		fields.HasCrewLeftPane.SetActive(value: false);
		fields.HasCrewRightPane.SetActive(value: false);
		fields.InvitePanel.SetActive(value: false);
	}

	public override void UpdateBeaconDisplay(int cooldown, int maxCooldown)
	{
	}
}
