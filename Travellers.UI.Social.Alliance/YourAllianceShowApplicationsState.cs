using Travellers.UI.Framework;

namespace Travellers.UI.Social.Alliance;

public class YourAllianceShowApplicationsState : IAllianceScreenState, ISocialScreenState
{
	private readonly YourAllianceTitleSegment _yourAllianceTitleSegment;

	private readonly YourAllianceManagementButtons _yourAllianceManagementButtons;

	private readonly UIButtonController _disbandAllianceButton;

	private readonly YourAllianceApplicationsPanel _yourAllianceApplicationsPanel;

	public YourAllianceShowApplicationsState(YourAllianceTitleSegment titleSegment, YourAllianceManagementButtons managementButtons, UIButtonController disbandAllianceButton, YourAllianceApplicationsPanel allianceApplicationsPanel)
	{
		_yourAllianceApplicationsPanel = allianceApplicationsPanel;
		_disbandAllianceButton = disbandAllianceButton;
		_yourAllianceManagementButtons = managementButtons;
		_yourAllianceTitleSegment = titleSegment;
	}

	public void EnterScreen()
	{
		_yourAllianceTitleSegment.SetObjectActive(isActive: true);
		_yourAllianceManagementButtons.SetObjectActive(isActive: true);
		_yourAllianceManagementButtons.SetForApplications();
		_disbandAllianceButton.SetObjectActive(isActive: true);
		_disbandAllianceButton.SetButtonEnabled(isShowing: true);
		_yourAllianceApplicationsPanel.SetObjectActive(isActive: true);
		_yourAllianceApplicationsPanel.SetFilterAndRefreshList(new InAllianceApplicationsDirector());
		_yourAllianceTitleSegment.RefreshPlayerData();
	}

	public void LeaveScreen()
	{
		_yourAllianceTitleSegment.SetObjectActive(isActive: false);
		_yourAllianceManagementButtons.SetObjectActive(isActive: false);
		_disbandAllianceButton.SetObjectActive(isActive: false);
		_yourAllianceApplicationsPanel.SetObjectActive(isActive: false);
	}
}
