using System;
using Bossa.Travellers.Alliances;
using Bossa.Travellers.Social.DataModel;
using Bossa.Travellers.World;
using GameDBClient;
using TMPro;
using Travellers.UI.Framework;
using Travellers.UI.HUDMessaging;
using Travellers.UI.Utility;
using UnityEngine;

namespace Travellers.UI.Social.Alliance;

[InjectableClass]
public class YourAllianceCreateAlliancePanel : UIScreenComponent
{
	private IAllianceClient _allianceClient;

	[SerializeField]
	private UIButtonController _cancelCreateAllianceButtonController;

	[SerializeField]
	private UIButtonController _createAllianceButtonController;

	[SerializeField]
	private TMP_InputField _descriptionInputField;

	[SerializeField]
	private TMP_InputField _messageOfTheDayInputField;

	[SerializeField]
	private TMP_InputField _nameInputField;

	[InjectableMethod]
	public void InjectDependencies(IAllianceClient allianceClient)
	{
		_allianceClient = allianceClient;
	}

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
		_createAllianceButtonController.SetButtonEvent(OnCreateAllianceButtonPressed);
		_cancelCreateAllianceButtonController.SetButtonEvent(OnCancelButtonPressed);
		SocialInputCharacterLimits byKey = Singleton<GameDBAccessor>.Instance.ClientGameDB.SocialInputCharacterLimitsTable.GetByKey(SocialInputCharacterLimitsSchema.KeyALLIANCE_NAME);
		_nameInputField.characterLimit = byKey.MAXVal;
	}

	private void OnCancelButtonPressed()
	{
		_nameInputField.text = string.Empty;
		_descriptionInputField.text = string.Empty;
		_messageOfTheDayInputField.text = string.Empty;
	}

	private void OnCreateAllianceButtonPressed()
	{
		try
		{
			StringFormatHelper.TryAllianceName(_nameInputField.text);
		}
		catch (Exception e)
		{
			SocialCharacterSheet.TriggerAllianceExceptionHandler(e);
			return;
		}
		_allianceClient.CreateAlliance(_nameInputField.text, _descriptionInputField.text, _messageOfTheDayInputField.text).Then((Action<AllianceBasicInformation>)OnAllianceCreated).Catch(OnAllianceCreateFailed);
	}

	private void OnAllianceCreated(AllianceBasicInformation allianceInfo)
	{
		_nameInputField.text = string.Empty;
		_descriptionInputField.text = string.Empty;
		_messageOfTheDayInputField.text = string.Empty;
		OSDMessage.SendMessage($"Alliance {allianceInfo.AllianceDisplayName} has been successfully created", MessageType.Server);
		SocialCharacterSheet.TriggerAllianceStateCheck();
	}

	private void OnAllianceCreateFailed(Exception ex)
	{
		SocialCharacterSheet.TriggerAllianceExceptionHandler(ex, SocialCharacterSheet.TriggerCloseSocialScreen, SocialCharacterSheet.TriggerAllianceDataRefresh);
	}

	protected override void ProtectedDispose()
	{
	}

	public void Setup()
	{
		SetObjectActive(isActive: false);
	}
}
