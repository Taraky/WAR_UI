using Bossa.Travellers.Social.DataModel;
using TMPro;
using Travellers.UI.Framework;
using UnityEngine;

namespace Travellers.UI.Social.Alliance;

public class SocialInfoPanelApplicationAndInvitation : UIScreenComponent
{
	[SerializeField]
	private UIButtonController _acceptButton;

	[SerializeField]
	private TextStylerTextMeshPro _applicationName;

	[SerializeField]
	private UIButtonController _cancelButton;

	private ISocialScreenState _currentScreenState;

	[SerializeField]
	private GameObject _inputBox;

	[SerializeField]
	private TMP_InputField _inputField;

	[SerializeField]
	private TextStylerTextMeshPro _invitationName;

	[SerializeField]
	private TextStylerTextMeshPro _message;

	[SerializeField]
	private GameObject _messageBox;

	[SerializeField]
	private GameObject _nameAndImageBox;

	[SerializeField]
	private GameObject _rightNameContainer;

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
	}

	protected override void ProtectedDispose()
	{
	}

	public void Setup()
	{
		_messageBox.SetActive(value: false);
		_inputBox.SetActive(value: false);
		_nameAndImageBox.SetActive(value: false);
		_rightNameContainer.SetActive(value: false);
		_acceptButton.SetObjectActive(isActive: false);
		_cancelButton.SetObjectActive(isActive: false);
	}

	public void SetForOutGoingInvitation(MembershipChangeRequest evtApplicationInfo)
	{
		SwitchState(new AllianceInvitationViewState(evtApplicationInfo, _invitationName, _rightNameContainer, _message, _messageBox, _cancelButton));
	}

	public void SetForIncomingApplication(MembershipChangeRequest evtApplicationInfo)
	{
		SwitchState(new AllianceApplicationViewState(evtApplicationInfo, _applicationName, _nameAndImageBox, _message, _messageBox, _acceptButton, _cancelButton));
	}

	public void SetForCreateInvitation(MembershipChangeRequest evtApplicationInfo)
	{
		SwitchState(new AllianceInvitationSendState(evtApplicationInfo, _invitationName, _rightNameContainer, _inputBox, _inputField, _cancelButton, _acceptButton));
	}

	private void SwitchState(ISocialScreenState newState)
	{
		if (_currentScreenState != null)
		{
			_currentScreenState.LeaveScreen();
		}
		_currentScreenState = newState;
		_currentScreenState.EnterScreen();
	}
}
