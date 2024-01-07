using Travellers.UI.Utility;

namespace Travellers.UI.Social.Crew;

public class YouAsMemberState : CrewState
{
	public override bool ShouldRefreshBeacon => true;

	public override bool ShouldRefreshSlots => true;

	public YouAsMemberState(ScreenFields fields)
		: base(fields)
	{
	}

	public override void EnterState()
	{
		fields.HasCrewLeftPane.SetActive(value: true);
		fields.HasCrewRightPane.SetActive(value: true);
		fields.BeaconPanel.SetActive(value: true);
		fields.InvitePanel.SetActive(value: false);
	}

	public override void LeaveState()
	{
		fields.HasCrewLeftPane.SetActive(value: false);
		fields.HasCrewRightPane.SetActive(value: false);
		fields.BeaconPanel.SetActive(value: false);
	}

	public override void UpdateBeaconDisplay(int cooldown, int maxCooldown)
	{
		fields.BeaconTimer.SetText(cooldown.FormatTimeForBeaconCountdown());
		fields.BeaconProgress.fillAmount = (float)cooldown / (float)maxCooldown;
		fields.BeaconButton.SetButtonEnabled(cooldown <= 0);
	}
}
