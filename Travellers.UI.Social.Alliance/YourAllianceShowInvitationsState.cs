using Travellers.UI.Framework;

namespace Travellers.UI.Social.Alliance;

public class YourAllianceShowInvitationsState : IAllianceScreenState, ISocialScreenState
{
	private readonly YourAllianceTitleSegment _yourAllianceTitleSegment;

	private readonly YourAllianceManagementButtons _yourAllianceManagementButtons;

	private readonly UIButtonController _disbandAllianceButton;

	private readonly YourAllianceInvitationsPanel _yourAllianceInvitationsPanel;

	public YourAllianceShowInvitationsState(YourAllianceTitleSegment titleSegment, YourAllianceManagementButtons managementButtons, UIButtonController disbandAllianceButton, YourAllianceInvitationsPanel allianceInvitationsPanel)
	{
		_yourAllianceInvitationsPanel = allianceInvitationsPanel;
		_disbandAllianceButton = disbandAllianceButton;
		_yourAllianceManagementButtons = managementButtons;
		_yourAllianceTitleSegment = titleSegment;
	}

	public void EnterScreen()
	{
		_yourAllianceTitleSegment.SetObjectActive(isActive: true);
		_yourAllianceManagementButtons.SetObjectActive(isActive: true);
		_yourAllianceManagementButtons.SetForInvitations();
		_disbandAllianceButton.SetObjectActive(isActive: true);
		_disbandAllianceButton.SetButtonEnabled(isShowing: true);
		_yourAllianceInvitationsPanel.SetObjectActive(isActive: true);
		_yourAllianceInvitationsPanel.SetFilterAndRefreshList(new InAllianceInvitationDirector());
		_yourAllianceTitleSegment.RefreshPlayerData();
	}

	public void LeaveScreen()
	{
		_yourAllianceTitleSegment.SetObjectActive(isActive: false);
		_yourAllianceManagementButtons.SetObjectActive(isActive: false);
		_disbandAllianceButton.SetObjectActive(isActive: false);
		_yourAllianceInvitationsPanel.SetObjectActive(isActive: false);
	}
}
