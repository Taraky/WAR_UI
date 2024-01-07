using System;
using Bossa.Travellers.Alliances;
using Bossa.Travellers.Social.DataModel;
using TMPro;
using Travellers.UI.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.Social.Alliance;

[InjectableClass]
public class SocialInfoPanelAllianceInfo : UIScreenComponent
{
	private AllianceBasicInformation _allianceBasicInformation;

	private IAllianceClient _allianceClient;

	[SerializeField]
	private TextStylerTextMeshPro _allianceDescription;

	[SerializeField]
	private Image _allianceIcon;

	[SerializeField]
	private TextStylerTextMeshPro _allianceMembersAndAge;

	[SerializeField]
	private TextStylerTextMeshPro _allianceName;

	[SerializeField]
	private GameObject _applicationForm;

	[SerializeField]
	private TMP_InputField _applicationInputField;

	[SerializeField]
	private TextStylerTextMeshPro _applicationMessage;

	[SerializeField]
	private GameObject _applicationMessageBox;

	[SerializeField]
	private UIButtonController _applyButton;

	[SerializeField]
	private UIButtonController _cancelButton;

	private ISocialInfoPanelAllianceState _currentState;

	[SerializeField]
	private Sprite _emblemPlaceholder;

	[InjectableMethod]
	public void InjectDependencies(IAllianceClient allianceClient)
	{
		_allianceClient = allianceClient;
	}

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUIAllianceEvents.BackPressed, OnCancelButton);
	}

	protected override void ProtectedInit()
	{
	}

	protected override void ProtectedDispose()
	{
	}

	private void OnFailure(Exception exception)
	{
		SocialCharacterSheet.TriggerAllianceExceptionHandler(exception, SocialCharacterSheet.TriggerCloseSocialScreen, SocialCharacterSheet.TriggerAllianceDataRefresh);
	}

	private void OnCancelButton(object[] obj)
	{
		_applicationInputField.text = string.Empty;
	}

	public void SetForSearchAllianceView(AllianceBasicInformation evtAllianceBasicToView)
	{
		SetAllianceInfo(evtAllianceBasicToView);
		_allianceClient.PlayerCanApplyToAlliance(_allianceBasicInformation.AllianceId).Then((Action<bool>)OnPlayerAllianceApplicationCheck).Catch(OnFailure);
	}

	private void SetAllianceInfo(AllianceBasicInformation evtAllianceBasicToView)
	{
		_allianceBasicInformation = evtAllianceBasicToView;
		_allianceName.SetText(_allianceBasicInformation.AllianceDisplayName);
		_applicationInputField.text = string.Empty;
		Epoch epoch = new Epoch(_allianceBasicInformation.FoundingDate);
		_allianceMembersAndAge.SetText($"Founded: {epoch.ToDateTime():dd/MM/yyyy}\nMembers: {_allianceBasicInformation.MemberCount}");
		_allianceDescription.SetText(evtAllianceBasicToView.Description);
		_allianceIcon.sprite = _emblemPlaceholder;
		if (string.IsNullOrEmpty(evtAllianceBasicToView.EmblemWebLink))
		{
			return;
		}
		_allianceClient.GetEmblem(evtAllianceBasicToView).Then(delegate(Sprite sprite)
		{
			if (sprite != null)
			{
				_allianceIcon.sprite = sprite;
			}
		});
	}

	public void SetForAllianceToPlayerInvitationView(AllianceBasicInformation evtAllianceBasicToView, MembershipChangeRequest application)
	{
		SetAllianceInfo(evtAllianceBasicToView);
		_applicationForm.SetActive(value: true);
		_applicationMessageBox.SetActive(value: true);
		_applicationMessage.SetText(application.Message);
		_applicationInputField.gameObject.SetActive(value: false);
		_applyButton.SetObjectActive(isActive: true);
		_cancelButton.SetObjectActive(isActive: true);
		_applyButton.SetButtonEvent(WAUIAllianceEvents.AcceptInvitationPressed, new ApplicationPressedEvent(application));
		_applyButton.SetText("ACCEPT");
		_cancelButton.SetButtonEvent(WAUIAllianceEvents.RejectInvitationPressed, new ApplicationPressedEvent(application));
		_cancelButton.SetText("REJECT");
	}

	public void SetForPlayerToAllianceApplicationView(AllianceBasicInformation evtAllianceBasicToView, MembershipChangeRequest application)
	{
		SetAllianceInfo(evtAllianceBasicToView);
		_applicationForm.SetActive(value: true);
		_applicationMessageBox.SetActive(value: true);
		_applicationMessage.SetText(application.Message);
		_applicationInputField.gameObject.SetActive(value: false);
		_applyButton.SetObjectActive(isActive: true);
		_cancelButton.SetObjectActive(isActive: true);
		_applyButton.SetButtonEvent(WAUIAllianceEvents.CancelPlayerToAllianceApplication, new ApplicationPressedEvent(application));
		_applyButton.SetText("RETRACT APPLICATION");
		_cancelButton.SetButtonEvent(WAUIAllianceEvents.BackPressed, new ApplicationPressedEvent(application));
		_cancelButton.SetText("CANCEL");
	}

	private void OnPlayerAllianceApplicationCheck(bool canApply)
	{
		if (!canApply)
		{
			_applicationForm.SetActive(value: false);
			_applyButton.SetObjectActive(isActive: false);
			_cancelButton.SetObjectActive(isActive: false);
			return;
		}
		_applicationForm.SetActive(value: true);
		_applicationMessageBox.SetActive(value: false);
		_applicationInputField.gameObject.SetActive(value: true);
		_applyButton.SetObjectActive(isActive: true);
		_cancelButton.SetObjectActive(isActive: true);
		_applyButton.SetButtonEvent(OnApplyPressed);
		_applyButton.SetText("APPLY");
		_cancelButton.SetButtonEvent(WAUIAllianceEvents.BackPressed);
		_cancelButton.SetText("CANCEL");
	}

	private void OnApplyPressed()
	{
		_allianceClient.PlayerSendApplicationToAlliance(_allianceBasicInformation, _applicationInputField.text).Then(delegate
		{
			ApplicationSuccessful();
		}).Catch(delegate(Exception exception)
		{
			OnFailure(exception);
		});
	}

	private void ApplicationSuccessful()
	{
		SetForSearchAllianceView(_allianceBasicInformation);
	}
}
