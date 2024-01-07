using Bossa.Travellers.Social.DataModel;
using Travellers.UI.Framework;
using UnityEngine;

namespace Travellers.UI.Social.Alliance;

public class YourAllianceManagementButtons : UIScreenComponent
{
	[SerializeField]
	private UIToggleController _addMembersButtonController;

	[SerializeField]
	private UIToggleController _applicationsButtonController;

	[SerializeField]
	private UIToggleController _editRankButtonController;

	private UIButtonController _lastSelectedButton;

	[SerializeField]
	private UIToggleController _membersButtonController;

	[SerializeField]
	private UIToggleGroup _toggleGroup;

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
		_membersButtonController.SetButtonEvent(WAUIAllianceEvents.MembersPressed);
		_applicationsButtonController.SetButtonEvent(WAUIAllianceEvents.ApplicationsButtonPressed);
		_addMembersButtonController.SetButtonEvent(WAUIAllianceEvents.InvitationsButtonPressed);
		_editRankButtonController.SetButtonEvent(WAUIAllianceEvents.EditRankPressed);
		_toggleGroup.AddToggleToGroup(_membersButtonController);
		_toggleGroup.AddToggleToGroup(_applicationsButtonController);
		_toggleGroup.AddToggleToGroup(_addMembersButtonController);
		_toggleGroup.AddToggleToGroup(_editRankButtonController);
	}

	protected override void ProtectedDispose()
	{
	}

	public void Setup()
	{
		SetObjectActive(isActive: false);
	}

	public void SetForPermissions(AllianceRank rank)
	{
		_applicationsButtonController.SetObjectActive(rank.EditMembers);
		_addMembersButtonController.SetObjectActive(isActive: true);
		_editRankButtonController.SetObjectActive(isActive: false);
	}

	public void SetForMembers()
	{
		_toggleGroup.SetActiveButton(_membersButtonController);
	}

	public void SetForApplications()
	{
		_toggleGroup.SetActiveButton(_applicationsButtonController);
	}

	public void SetForInvitations()
	{
		_toggleGroup.SetActiveButton(_addMembersButtonController);
	}

	public void SetForEditRank()
	{
		_toggleGroup.SetActiveButton(_editRankButtonController);
	}
}
