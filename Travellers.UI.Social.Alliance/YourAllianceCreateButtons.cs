using Travellers.UI.Framework;
using UnityEngine;

namespace Travellers.UI.Social.Alliance;

public class YourAllianceCreateButtons : UIScreenComponent
{
	[SerializeField]
	private UIToggleController _createAllianceButtonController;

	[SerializeField]
	private UIToggleController _invitationsButtonController;

	[SerializeField]
	private UIToggleController _applicationsButtonController;

	[SerializeField]
	private UIToggleGroup _buttonToggleGroup;

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
		_createAllianceButtonController.SetButtonEvent(WAUIAllianceEvents.GotoCreateAlliancePressed);
		_invitationsButtonController.SetButtonEvent(WAUIAllianceEvents.PersonalInvitationsButtonPressed);
		_applicationsButtonController.SetButtonEvent(WAUIAllianceEvents.PersonalApplicationsButtonPressed);
		_buttonToggleGroup.AddToggleToGroup(_createAllianceButtonController);
		_buttonToggleGroup.AddToggleToGroup(_invitationsButtonController);
		_buttonToggleGroup.AddToggleToGroup(_applicationsButtonController);
	}

	protected override void ProtectedDispose()
	{
	}

	public void Setup()
	{
		SetObjectActive(isActive: false);
	}

	public void SetForCreateAlliance()
	{
		_buttonToggleGroup.SetActiveButton(_createAllianceButtonController);
	}

	public void SetForInvitation()
	{
		_buttonToggleGroup.SetActiveButton(_invitationsButtonController);
	}

	public void SetForApplication()
	{
		_buttonToggleGroup.SetActiveButton(_applicationsButtonController);
	}
}
