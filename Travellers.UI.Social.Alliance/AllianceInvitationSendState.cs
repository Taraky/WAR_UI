using Bossa.Travellers.Social.DataModel;
using TMPro;
using Travellers.UI.Framework;
using UnityEngine;

namespace Travellers.UI.Social.Alliance;

public class AllianceInvitationSendState : ISocialScreenState
{
	private readonly UIButtonController _acceptButton;

	private readonly MembershipChangeRequest _applicationInfo;

	private readonly UIButtonController _cancelButton;

	private readonly GameObject _inputBox;

	private readonly TMP_InputField _inputField;

	private readonly TextStylerTextMeshPro _invitationName;

	private readonly GameObject _nameAndImageBox;

	public AllianceInvitationSendState(MembershipChangeRequest applicationInfo, TextStylerTextMeshPro invitationName, GameObject nameAndImageBox, GameObject inputBox, TMP_InputField inputField, UIButtonController cancelButton, UIButtonController acceptButton)
	{
		_applicationInfo = applicationInfo;
		_invitationName = invitationName;
		_nameAndImageBox = nameAndImageBox;
		_inputBox = inputBox;
		_inputField = inputField;
		_cancelButton = cancelButton;
		_acceptButton = acceptButton;
	}

	public void EnterScreen()
	{
		_invitationName.SetObjectActive(isActive: true);
		_nameAndImageBox.SetActive(value: true);
		_inputBox.SetActive(value: true);
		_cancelButton.SetObjectActive(isActive: true);
		_acceptButton.SetObjectActive(isActive: true);
		_inputField.text = "Join our alliance";
		_invitationName.SetText(_applicationInfo.CharacterName);
		_cancelButton.SetText("CANCEL");
		_cancelButton.SetButtonEvent(WAUIAllianceEvents.BackPressed);
		_acceptButton.SetText("SEND INVITE");
		_acceptButton.SetButtonEvent(delegate
		{
			_applicationInfo.Message = _inputField.text;
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIAllianceEvents.SendInvitationPressed, new ApplicationPressedEvent(_applicationInfo));
		});
	}

	public void LeaveScreen()
	{
		_invitationName.SetObjectActive(isActive: false);
		_nameAndImageBox.SetActive(value: false);
		_inputBox.SetActive(value: false);
		_cancelButton.SetObjectActive(isActive: false);
		_acceptButton.SetObjectActive(isActive: false);
	}
}
