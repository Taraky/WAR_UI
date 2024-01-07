using System;
using Bossa.Travellers.Alliances;
using Bossa.Travellers.Social.DataModel;
using GameDBLocalization;
using Travellers.UI.Framework;
using UnityEngine;
using UnityEngine.UI;
using WAUtilities.Logging;

namespace Travellers.UI.Social.Alliance;

[InjectableClass]
public class YourAllianceTitleSegment : UIScreenComponent
{
	private AllianceBasicInformation _allianceBasicInfo;

	private IAllianceClient _allianceClient;

	[SerializeField]
	private EditableTextHandler _allianceDescription;

	[SerializeField]
	private Image _allianceEmblem;

	[SerializeField]
	private Sprite _allianceEmblemPlaceholder;

	[SerializeField]
	private TextStylerTextMeshPro _allianceMembers;

	[SerializeField]
	private TextStylerTextMeshPro _allianceName;

	[SerializeField]
	private TextStylerTextMeshPro _founderName;

	[SerializeField]
	private EditableTextHandler _messageOfTheDay;

	[InjectableMethod]
	public void InjectDependencies(IAllianceClient allianceClient)
	{
		_allianceClient = allianceClient;
	}

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUIAllianceEvents.AllAllianceDataChanged, OnDataChanged);
	}

	private void OnDataChanged(object[] obj)
	{
		RefreshPlayerData();
	}

	protected override void ProtectedInit()
	{
	}

	protected override void ProtectedDispose()
	{
	}

	public void Setup()
	{
		SetObjectActive(isActive: false);
	}

	public void RefreshPlayerData()
	{
		_allianceClient.GetYourAllianceMemberData().Then((Action<AllianceMember>)ShowIfInAlliance).Catch(OnPromiseFailed);
	}

	private void ShowIfInAlliance(AllianceMember yourAllianceInfo)
	{
		if (yourAllianceInfo != null)
		{
			_allianceClient.GetYourBasicAllianceInfo().Then(delegate(AllianceBasicInformation allianceData)
			{
				PopulateFields(allianceData, yourAllianceInfo);
			}).Catch(delegate(Exception exception)
			{
				OnPromiseFailed(exception);
			});
		}
		else
		{
			SetForNoAlliance();
		}
	}

	private void OnPromiseFailed(Exception ex)
	{
		WALogger.Exception<YourAllianceTitleSegment>(ex);
	}

	private void PopulateFields(AllianceBasicInformation allianceBasicData, AllianceMember yourAllianceData)
	{
		_allianceBasicInfo = allianceBasicData;
		_allianceEmblem.sprite = _allianceEmblemPlaceholder;
		if (!string.IsNullOrEmpty(allianceBasicData.EmblemWebLink))
		{
			_allianceClient.GetEmblem(allianceBasicData).Then(delegate(Sprite sprite)
			{
				if (sprite != null)
				{
					_allianceEmblem.sprite = sprite;
				}
			});
		}
		_allianceName.SetText(allianceBasicData.AllianceDisplayName);
		AllianceRank allianceRank = yourAllianceData.RankData ?? AllianceRank.DefaultMemberRank;
		_founderName.SetText($"FOUNDER\n{allianceBasicData.LeaderName}");
		_allianceMembers.SetText($"{allianceBasicData.MemberCount}");
		string orElse = allianceBasicData.MessageOfTheDay.GetOrElse(string.Empty);
		_messageOfTheDay.Setup(allianceRank.EditMessageOfTheDay, orElse, OnChangeMotd);
		_messageOfTheDay.SetObjectActive(isActive: true);
		orElse = allianceBasicData.Description.GetOrElse(string.Empty);
		_allianceDescription.Setup(allianceRank.EditGroup, orElse, OnDescriptionChanged);
	}

	private void OnDescriptionChanged(string newDesc)
	{
		_allianceBasicInfo.Description = newDesc;
		_allianceClient.UpdateYourAllianceBasicInfo(_allianceBasicInfo).Then(delegate
		{
			SocialCharacterSheet.TriggerAllianceDataRefresh();
		});
	}

	private void OnChangeMotd(string newMotd)
	{
		_allianceBasicInfo.MessageOfTheDay = newMotd;
		_allianceClient.UpdateYourAllianceBasicInfo(_allianceBasicInfo).Then(delegate
		{
			SocialCharacterSheet.TriggerAllianceDataRefresh();
		});
	}

	private void SetForNoAlliance()
	{
		_allianceName.SetText("<i>Create an alliance...</i>");
		_founderName.SetText(string.Empty);
		_allianceMembers.SetText(string.Empty);
		string text = Localizer.LocalizeString(LocalizationSchema.KeyCREATE_ALLIANCE_BLURB);
		_allianceDescription.Setup(isEditable: false, text);
		_messageOfTheDay.SetObjectActive(isActive: false);
	}
}
