using Bossa.Travellers.Social.DataModel;
using Travellers.UI.Framework;
using UnityEngine;

namespace Travellers.UI.Social.Alliance;

public class AllianceInvitationViewState : ISocialScreenState
{
	private readonly MembershipChangeRequest _applicationInfo;

	private readonly UIButtonController _cancelButton;

	private readonly TextStylerTextMeshPro _invitationName;

	private readonly TextStylerTextMeshPro _message;

	private readonly GameObject _messageBox;

	private readonly GameObject _nameAndImageBox;

	public AllianceInvitationViewState(MembershipChangeRequest applicationInfo, TextStylerTextMeshPro invitationName, GameObject nameAndImageBox, TextStylerTextMeshPro message, GameObject messageBox, UIButtonController cancelButton)
	{
		_applicationInfo = applicationInfo;
		_invitationName = invitationName;
		_nameAndImageBox = nameAndImageBox;
		_message = message;
		_messageBox = messageBox;
		_cancelButton = cancelButton;
	}

	public void EnterScreen()
	{
		_invitationName.SetObjectActive(isActive: true);
		_nameAndImageBox.SetActive(value: true);
		_messageBox.SetActive(value: true);
		_cancelButton.SetObjectActive(isActive: true);
		_message.SetText(_applicationInfo.Message);
		_invitationName.SetText(_applicationInfo.CharacterName);
		_cancelButton.SetText("CANCEL INVITE");
		_cancelButton.SetButtonEvent(delegate
		{
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIAllianceEvents.AllianceCancelInvitationToPlayer, new ApplicationPressedEvent(_applicationInfo));
		});
	}

	public void LeaveScreen()
	{
		_invitationName.SetObjectActive(isActive: false);
		_nameAndImageBox.SetActive(value: false);
		_messageBox.SetActive(value: false);
		_cancelButton.SetObjectActive(isActive: false);
	}
}
