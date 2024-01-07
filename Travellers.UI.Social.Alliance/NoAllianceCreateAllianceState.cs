namespace Travellers.UI.Social.Alliance;

public class NoAllianceCreateAllianceState : IAllianceScreenState, ISocialScreenState
{
	private readonly YourAllianceTitleSegment _titleSegment;

	private readonly YourAllianceCreateButtons _createAllianceButtons;

	private readonly YourAllianceCreateAlliancePanel _allianceCreateAlliancePanel;

	public NoAllianceCreateAllianceState(YourAllianceTitleSegment titleSegment, YourAllianceCreateButtons createAllianceButtons, YourAllianceCreateAlliancePanel allianceCreateAlliancePanel)
	{
		_titleSegment = titleSegment;
		_createAllianceButtons = createAllianceButtons;
		_allianceCreateAlliancePanel = allianceCreateAlliancePanel;
	}

	public void EnterScreen()
	{
		_titleSegment.SetObjectActive(isActive: true);
		_createAllianceButtons.SetObjectActive(isActive: true);
		_createAllianceButtons.SetForCreateAlliance();
		_allianceCreateAlliancePanel.SetObjectActive(isActive: true);
		_titleSegment.RefreshPlayerData();
	}

	public void LeaveScreen()
	{
		_titleSegment.SetObjectActive(isActive: false);
		_createAllianceButtons.SetObjectActive(isActive: false);
		_allianceCreateAlliancePanel.SetObjectActive(isActive: false);
	}
}
