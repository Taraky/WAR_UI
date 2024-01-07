using Travellers.UI.Framework;

namespace Travellers.UI.Social.Alliance;

public class YourAllianceShowMembersState : IAllianceScreenState, ISocialScreenState
{
	private readonly YourAllianceTitleSegment _yourAllianceTitleSegment;

	private readonly YourAllianceManagementButtons _yourAllianceManagementButtons;

	private readonly UIButtonController _disbandAllianceButton;

	private readonly YourAllianceMembersPanel _yourAllianceMembersListSection;

	public YourAllianceShowMembersState(YourAllianceTitleSegment titleSegment, YourAllianceManagementButtons managementButtons, UIButtonController disbandAllianceButton, YourAllianceMembersPanel allianceMembersListSection)
	{
		_yourAllianceMembersListSection = allianceMembersListSection;
		_disbandAllianceButton = disbandAllianceButton;
		_yourAllianceManagementButtons = managementButtons;
		_yourAllianceTitleSegment = titleSegment;
	}

	public void EnterScreen()
	{
		_yourAllianceTitleSegment.SetObjectActive(isActive: true);
		_yourAllianceManagementButtons.SetObjectActive(isActive: true);
		_yourAllianceManagementButtons.SetForMembers();
		_disbandAllianceButton.SetObjectActive(isActive: true);
		_yourAllianceMembersListSection.SetObjectActive(isActive: true);
		_yourAllianceMembersListSection.RefreshAllianceMembersList();
		_yourAllianceTitleSegment.RefreshPlayerData();
	}

	public void LeaveScreen()
	{
		_yourAllianceTitleSegment.SetObjectActive(isActive: false);
		_yourAllianceManagementButtons.SetObjectActive(isActive: false);
		_disbandAllianceButton.SetObjectActive(isActive: false);
		_yourAllianceMembersListSection.SetObjectActive(isActive: false);
	}
}
