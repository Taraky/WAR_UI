using Bossa.Travellers.Social.DataModel;
using Travellers.UI.Framework;
using UnityEngine;

namespace Travellers.UI.Social.Alliance;

public class AllianceApplicationViewState : ISocialScreenState
{
	private readonly UIButtonController _acceptButton;

	private readonly MembershipChangeRequest _applicationInfo;

	private readonly TextStylerTextMeshPro _applicationName;

	private readonly UIButtonController _cancelButton;

	private readonly TextStylerTextMeshPro _message;

	private readonly GameObject _messageBox;

	private readonly GameObject _nameAndImageBox;

	public AllianceApplicationViewState(MembershipChangeRequest applicationInfo, TextStylerTextMeshPro applicationName, GameObject nameAndImageBox, TextStylerTextMeshPro message, GameObject messageBox, UIButtonController acceptButton, UIButtonController cancelButton)
	{
		_applicationInfo = applicationInfo;
		_applicationName = applicationName;
		_nameAndImageBox = nameAndImageBox;
		_message = message;
		_messageBox = messageBox;
		_acceptButton = acceptButton;
		_cancelButton = cancelButton;
	}

	public void EnterScreen()
	{
		_applicationName.SetObjectActive(isActive: true);
		_nameAndImageBox.SetActive(value: true);
		_messageBox.SetActive(value: true);
		_acceptButton.SetObjectActive(isActive: true);
		_cancelButton.SetObjectActive(isActive: true);
		_message.SetText(_applicationInfo.Message);
		_applicationName.SetText(_applicationInfo.CharacterGroupTargetName);
		_cancelButton.SetText("REJECT");
		_cancelButton.SetButtonEvent(WAUIAllianceEvents.AllianceRejectPlayerApplicationPressed, new ApplicationPressedEvent(_applicationInfo));
		_acceptButton.SetText("ACCEPT");
		_acceptButton.SetButtonEvent(WAUIAllianceEvents.AllianceAcceptPlayerApplicationPressed, new ApplicationPressedEvent(_applicationInfo));
	}

	public void LeaveScreen()
	{
		_applicationName.SetObjectActive(isActive: false);
		_nameAndImageBox.SetActive(value: false);
		_messageBox.SetActive(value: false);
		_acceptButton.SetObjectActive(isActive: false);
		_cancelButton.SetObjectActive(isActive: false);
	}
}
