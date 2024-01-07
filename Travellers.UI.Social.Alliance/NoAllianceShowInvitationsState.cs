namespace Travellers.UI.Social.Alliance;

public class NoAllianceShowInvitationsState : IAllianceScreenState, ISocialScreenState
{
	private readonly YourAllianceTitleSegment _titleSegment;

	private readonly YourAllianceCreateButtons _createAllianceButtons;

	private readonly YourAllianceApplicationsPanel _yourAllianceInvitationsPanel;

	public NoAllianceShowInvitationsState(YourAllianceTitleSegment titleSegment, YourAllianceCreateButtons createAllianceButtons, YourAllianceApplicationsPanel yourAllianceApplicationsPanel)
	{
		_titleSegment = titleSegment;
		_createAllianceButtons = createAllianceButtons;
		_yourAllianceInvitationsPanel = yourAllianceApplicationsPanel;
	}

	public void EnterScreen()
	{
		_titleSegment.SetObjectActive(isActive: true);
		_createAllianceButtons.SetObjectActive(isActive: true);
		_createAllianceButtons.SetForInvitation();
		_yourAllianceInvitationsPanel.SetObjectActive(isActive: true);
		_yourAllianceInvitationsPanel.SetFilterAndRefreshList(new OutOfAllianceInvitationDirector());
		_titleSegment.RefreshPlayerData();
	}

	public void LeaveScreen()
	{
		_titleSegment.SetObjectActive(isActive: false);
		_createAllianceButtons.SetObjectActive(isActive: false);
		_yourAllianceInvitationsPanel.SetObjectActive(isActive: false);
	}
}
