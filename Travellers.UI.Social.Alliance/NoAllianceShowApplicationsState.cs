namespace Travellers.UI.Social.Alliance;

public class NoAllianceShowApplicationsState : IAllianceScreenState, ISocialScreenState
{
	private readonly YourAllianceTitleSegment _titleSegment;

	private readonly YourAllianceCreateButtons _createAllianceButtons;

	private readonly YourAllianceApplicationsPanel _yourAllianceApplicationsPanel;

	public NoAllianceShowApplicationsState(YourAllianceTitleSegment titleSegment, YourAllianceCreateButtons createAllianceButtons, YourAllianceApplicationsPanel yourAllianceApplicationsPanel)
	{
		_titleSegment = titleSegment;
		_createAllianceButtons = createAllianceButtons;
		_yourAllianceApplicationsPanel = yourAllianceApplicationsPanel;
	}

	public void EnterScreen()
	{
		_titleSegment.SetObjectActive(isActive: true);
		_createAllianceButtons.SetObjectActive(isActive: true);
		_createAllianceButtons.SetForApplication();
		_yourAllianceApplicationsPanel.SetObjectActive(isActive: true);
		_yourAllianceApplicationsPanel.SetFilterAndRefreshList(new OutOfAllianceApplicationsDirector());
		_titleSegment.RefreshPlayerData();
	}

	public void LeaveScreen()
	{
		_titleSegment.SetObjectActive(isActive: false);
		_createAllianceButtons.SetObjectActive(isActive: false);
		_yourAllianceApplicationsPanel.SetObjectActive(isActive: false);
	}
}
