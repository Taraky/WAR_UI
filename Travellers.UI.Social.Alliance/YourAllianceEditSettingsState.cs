using Travellers.UI.Framework;

namespace Travellers.UI.Social.Alliance;

public class YourAllianceEditSettingsState : IAllianceScreenState, ISocialScreenState
{
	private readonly YourAllianceTitleSegment _yourAllianceTitleSegment;

	private readonly YourAllianceManagementButtons _yourAllianceManagementButtons;

	private readonly UIButtonController _disbandAllianceButton;

	private readonly YourAllianceEditRankPanel _yourAllianceEditRankPanelSection;

	public YourAllianceEditSettingsState(YourAllianceTitleSegment titleSegment, YourAllianceManagementButtons managementButtons, UIButtonController disbandAllianceButton, YourAllianceEditRankPanel allianceEditRankPanelSection)
	{
		_yourAllianceEditRankPanelSection = allianceEditRankPanelSection;
		_disbandAllianceButton = disbandAllianceButton;
		_yourAllianceManagementButtons = managementButtons;
		_yourAllianceTitleSegment = titleSegment;
	}

	public void EnterScreen()
	{
		_yourAllianceTitleSegment.SetObjectActive(isActive: true);
		_yourAllianceManagementButtons.SetObjectActive(isActive: true);
		_yourAllianceManagementButtons.SetForEditRank();
		_disbandAllianceButton.SetObjectActive(isActive: true);
		_disbandAllianceButton.SetButtonEnabled(isShowing: true);
		_yourAllianceEditRankPanelSection.SetObjectActive(isActive: true);
		_yourAllianceEditRankPanelSection.RefreshData();
		_yourAllianceTitleSegment.RefreshPlayerData();
	}

	public void LeaveScreen()
	{
		_yourAllianceTitleSegment.SetObjectActive(isActive: false);
		_yourAllianceManagementButtons.SetObjectActive(isActive: false);
		_disbandAllianceButton.SetObjectActive(isActive: false);
		_yourAllianceEditRankPanelSection.SetObjectActive(isActive: false);
	}
}
