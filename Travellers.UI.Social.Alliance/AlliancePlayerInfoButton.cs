using System;
using Bossa.Travellers.Social.DataModel;
using Travellers.UI.Framework;
using UnityEngine;

namespace Travellers.UI.Social.Alliance;

public class AlliancePlayerInfoButton : UIScreenComponent
{
	[SerializeField]
	private UIAllianceMemberToggleController _playerButtonController;

	private Action _buttonEvent;

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
	}

	protected override void ProtectedDispose()
	{
	}

	public void AddButtonToGroupToggle(UIToggleGroup toggleGroupVisitor)
	{
		toggleGroupVisitor.AddToggleToGroup(_playerButtonController);
	}

	public void TriggerButtonEvent()
	{
		if (_buttonEvent != null)
		{
			_buttonEvent();
		}
	}

	public void SetDataForPlayer(UIToggleGroup toggleGroup)
	{
		_playerButtonController.SetText("YOU", string.Empty);
		_buttonEvent = delegate
		{
			if (toggleGroup != null)
			{
				toggleGroup.SetActiveButton(_playerButtonController);
			}
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIAllianceEvents.YourProfileButtonPressed, null);
		};
		_playerButtonController.SetButtonEvent(_buttonEvent);
		_playerButtonController.SetToggleSelected(isSelected: false);
	}

	public void SetDataForAllianceMember(UIToggleGroup toggleGroup, AllianceMember memberInfo)
	{
		_playerButtonController.SetText(memberInfo.DisplayName, (memberInfo.RankData == null) ? "Error getting rank" : memberInfo.RankData.RankName);
		if (toggleGroup != null)
		{
			toggleGroup.AddToggleToGroup(_playerButtonController);
		}
		_buttonEvent = delegate
		{
			if (toggleGroup != null)
			{
				toggleGroup.SetActiveButton(_playerButtonController);
			}
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIAllianceEvents.MemberBarPressed, new AllianceMemberPressedEvent(memberInfo));
		};
		_playerButtonController.SetButtonEvent(_buttonEvent);
		_playerButtonController.SetInfoButtonAsOnline(memberInfo.IsOnline);
		_playerButtonController.SetButtonState((!memberInfo.IsOnline) ? UIToggleState.EnabledButGreyed : UIToggleState.Deselected);
	}

	public void SetDataForRecievedApplication(UIToggleGroup toggleGroup, MembershipChangeRequest applicationInfo)
	{
		_playerButtonController.SetText(applicationInfo.CharacterName, string.Empty);
		if (toggleGroup != null)
		{
			toggleGroup.AddToggleToGroup(_playerButtonController);
		}
		_buttonEvent = delegate
		{
			if (toggleGroup != null)
			{
				toggleGroup.SetActiveButton(_playerButtonController);
			}
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIAllianceEvents.ViewApplicationPressed, new ApplicationPressedEvent(applicationInfo));
		};
		_playerButtonController.SetButtonEvent(_buttonEvent);
		_playerButtonController.SetToggleSelected(isSelected: false);
	}

	public void SetDataForSentInvitation(UIToggleGroup toggleGroup, MembershipChangeRequest applicationInfo)
	{
		_playerButtonController.SetText(applicationInfo.CharacterName, string.Empty);
		if (toggleGroup != null)
		{
			toggleGroup.AddToggleToGroup(_playerButtonController);
		}
		_buttonEvent = delegate
		{
			if (toggleGroup != null)
			{
				toggleGroup.SetActiveButton(_playerButtonController);
			}
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIAllianceEvents.ViewInvitationPressed, new ApplicationPressedEvent(applicationInfo));
		};
		_playerButtonController.SetButtonEvent(_buttonEvent);
		_playerButtonController.SetToggleSelected(isSelected: false);
	}

	public void SetDataForPersonalInvitation(UIToggleGroup toggleGroup, AllianceBasicInformation allianceBasicInfo, MembershipChangeRequest application)
	{
		_playerButtonController.SetText(allianceBasicInfo.AllianceDisplayName, string.Empty);
		if (toggleGroup != null)
		{
			toggleGroup.AddToggleToGroup(_playerButtonController);
		}
		_buttonEvent = delegate
		{
			if (toggleGroup != null)
			{
				toggleGroup.SetActiveButton(_playerButtonController);
			}
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIAllianceEvents.OutOfAllianceInvitationPressed, new SelectAllianceInvitationToViewEvent(allianceBasicInfo, application));
		};
		_playerButtonController.SetButtonEvent(_buttonEvent);
		_playerButtonController.SetToggleSelected(isSelected: false);
	}

	public void SetDataForPersonalApplication(UIToggleGroup toggleGroup, AllianceBasicInformation allianceBasicInfo, MembershipChangeRequest application)
	{
		_playerButtonController.SetText(allianceBasicInfo.AllianceDisplayName, string.Empty);
		if (toggleGroup != null)
		{
			toggleGroup.AddToggleToGroup(_playerButtonController);
		}
		_buttonEvent = delegate
		{
			if (toggleGroup != null)
			{
				toggleGroup.SetActiveButton(_playerButtonController);
			}
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIAllianceEvents.OutOfAllianceApplicationPressed, new SelectAllianceInvitationToViewEvent(allianceBasicInfo, application));
		};
		_playerButtonController.SetButtonEvent(_buttonEvent);
		_playerButtonController.SetToggleSelected(isSelected: false);
	}
}
