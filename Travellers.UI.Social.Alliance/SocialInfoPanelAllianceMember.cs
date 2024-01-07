using System;
using System.Collections.Generic;
using System.Linq;
using Bossa.Travellers.Alliances;
using Bossa.Travellers.Social;
using Bossa.Travellers.Social.DataModel;
using TMPro;
using Travellers.UI.Framework;
using Travellers.UI.InfoPopups;
using UnityEngine;
using WAUtilities.Logging;

namespace Travellers.UI.Social.Alliance;

[InjectableClass]
public class SocialInfoPanelAllianceMember : UIScreenComponent
{
	private IAllianceClient _allianceClient;

	private AllianceMember _allianceMember;

	[SerializeField]
	private TextStylerTextMeshPro _allianceRank;

	[SerializeField]
	private GameObject _allianceRankActualContainer;

	[SerializeField]
	private GameObject _allianceRankTitleContainer;

	[SerializeField]
	private UIButtonController _bootMemberButton;

	[SerializeField]
	private GameObject _dropdownContainer;

	[SerializeField]
	private TMP_Dropdown _dropdownRankList;

	[SerializeField]
	private UIButtonController _makeFounderButton;

	[SerializeField]
	private GameObject _officerNoteContainer;

	[SerializeField]
	private GameObject _publicNoteContainer;

	[SerializeField]
	private EditableTextHandler _officerNote;

	[SerializeField]
	private EditableTextHandler _publicNote;

	[SerializeField]
	private TextStylerTextMeshPro _playerName;

	[SerializeField]
	private UIButtonController _rankChangeButton;

	private AllianceRankInformation _allianceRankInformation;

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
		_rankChangeButton.SetButtonEvent(OnRankChangePressed);
		_bootMemberButton.SetButtonEvent(OnBootPlayerPressed);
		_makeFounderButton.SetButtonEvent(OnMakeFounderPressed);
	}

	protected override void ProtectedDispose()
	{
	}

	public void SetData(AllianceMember allianceMemberToView, AllianceMember yourAllianceMemberInfo)
	{
		_allianceMember = allianceMemberToView;
		_playerName.SetText(allianceMemberToView.DisplayName);
		_allianceRank.SetText(allianceMemberToView.RankData.RankName);
		_allianceClient.GetYourAllianceRankInformation().Then(delegate(AllianceRankInformation rankData)
		{
			OnRankDataRetrieved(yourAllianceMemberInfo, allianceMemberToView, rankData);
			SetOfficerNote(allianceMemberToView, yourAllianceMemberInfo);
			SetPublicNote(allianceMemberToView, yourAllianceMemberInfo);
			SetBootAndfounderButtons(allianceMemberToView, yourAllianceMemberInfo, rankData);
		}).Catch(OnFailure);
	}

	private void OnFailure(Exception obj)
	{
		SocialCharacterSheet.TriggerAllianceExceptionHandler(obj, SocialCharacterSheet.TriggerCloseSocialScreen, SocialCharacterSheet.TriggerAllianceDataRefresh);
	}

	private void OnRankChangePressed()
	{
		SetRankBarState(RankBarState.RankBeingSelected);
	}

	private void OnRankDataRetrieved(AllianceMember yourMemberData, AllianceMember allianceMemberToView, AllianceRankInformation rankInformation)
	{
		rankInformation.ValidateData();
		_allianceRankInformation = rankInformation;
		_dropdownRankList.ClearOptions();
		List<string> options = (from x in _allianceRankInformation.RankList
			orderby x.RankName
			select x.RankName).ToList();
		_dropdownRankList.AddOptions(options);
		_dropdownRankList.onValueChanged.AddListener(OnDropdownListChanged);
		SetRankBarState(RankBarState.CantChangeRank);
		SetRank();
	}

	private void OnDropdownListChanged(int index)
	{
		TMP_Dropdown.OptionData rankName = _dropdownRankList.options[index];
		List<AllianceRank> list = _allianceRankInformation.RankList.Where((AllianceRank x) => x.RankName == rankName.text).ToList();
		if (list.Count > 1)
		{
			WALogger.Error<YourAllianceEditRankPanel>("More than one rank with the same name {0}", new object[1] { list[0].RankName });
		}
		else if (list.Count == 0)
		{
			WALogger.Error<YourAllianceEditRankPanel>("No rank found that matches dropdown selection {0}", new object[1] { rankName.text });
		}
		else
		{
			_allianceMember.RankId = list[0].RankID;
			_allianceClient.SetAllianceMemberDataForPlayer(_allianceMember, SocialGroupParsers.CreateChangePlayerRank(_allianceMember.RankId));
			SetRank();
		}
	}

	private void SetRank()
	{
		_allianceRank.SetText(_allianceMember.RankData.RankName);
	}

	private void SetRankBarState(RankBarState newState)
	{
		switch (newState)
		{
		case RankBarState.CanChangeRank:
			_rankChangeButton.SetObjectActive(isActive: true);
			_dropdownContainer.SetActive(value: false);
			_allianceRankTitleContainer.SetActive(value: true);
			_allianceRankActualContainer.SetActive(value: true);
			break;
		case RankBarState.CantChangeRank:
			_rankChangeButton.SetObjectActive(isActive: false);
			_dropdownContainer.SetActive(value: false);
			_allianceRankTitleContainer.SetActive(value: true);
			_allianceRankActualContainer.SetActive(value: true);
			break;
		case RankBarState.RankBeingSelected:
			_rankChangeButton.SetObjectActive(isActive: false);
			_dropdownContainer.SetActive(value: true);
			_allianceRankTitleContainer.SetActive(value: false);
			_allianceRankActualContainer.SetActive(value: false);
			break;
		}
	}

	private void SetOfficerNote(AllianceMember allianceMemberToView, AllianceMember yourAllianceMemberInfo)
	{
		bool readOfficerNote = yourAllianceMemberInfo.RankData.ReadOfficerNote;
		_officerNoteContainer.SetActive(readOfficerNote);
		if (readOfficerNote)
		{
			bool editOfficerNote = yourAllianceMemberInfo.RankData.EditOfficerNote;
			string text = ((!string.IsNullOrEmpty(allianceMemberToView.OfficerNote)) ? allianceMemberToView.OfficerNote : string.Empty);
			_officerNote.Setup(editOfficerNote, text, OnOfficerNoteChanged);
		}
	}

	private void OnOfficerNoteChanged(string newNote)
	{
		_allianceMember.OfficerNote = newNote;
		_allianceClient.SetAllianceMemberDataForPlayer(_allianceMember, SocialGroupParsers.CreateChangePlayerPrivateNote(_allianceMember.OfficerNote));
	}

	private void SetPublicNote(AllianceMember allianceMemberToView, AllianceMember yourAllianceMemberInfo)
	{
		bool readOfficerNote = yourAllianceMemberInfo.RankData.ReadOfficerNote;
		string text = ((!string.IsNullOrEmpty(allianceMemberToView.PublicNote)) ? allianceMemberToView.PublicNote : string.Empty);
		_publicNote.Setup(readOfficerNote, text, OnPublicNoteChanged);
	}

	private void OnPublicNoteChanged(string newNote)
	{
		_allianceMember.PublicNote = newNote;
		_allianceClient.SetAllianceMemberDataForPlayer(_allianceMember, SocialGroupParsers.CreateChangePlayerPublicNote(_allianceMember.PublicNote));
	}

	private void SetBootAndfounderButtons(AllianceMember allianceMemberToView, AllianceMember yourAllianceMemberInfo, AllianceRankInformation rankInformation)
	{
		bool objectActive = yourAllianceMemberInfo.RankData.RankID == rankInformation.Leader.RankID && allianceMemberToView.CharacterUiD != yourAllianceMemberInfo.CharacterUiD;
		bool objectActive2 = yourAllianceMemberInfo.RankData.EditMembers && allianceMemberToView.CharacterUiD != yourAllianceMemberInfo.CharacterUiD;
		_makeFounderButton.SetObjectActive(objectActive);
		_bootMemberButton.SetObjectActive(objectActive2);
	}

	private void OnBootPlayerPressed()
	{
		DialogPopupFacade.ShowConfirmationDialog("Remove member from alliance", $"Are you sure you want to remove {_allianceMember.DisplayName} from the alliance? Are they really that bad?", delegate
		{
			_allianceClient.RequestBootPlayerFromAlliance(_allianceMember).Then(delegate
			{
				SocialCharacterSheet.TriggerAllianceDataRefresh();
			}).Catch(OnFailure);
		}, "CONFIRM");
	}

	private void OnMakeFounderPressed()
	{
		DialogPopupFacade.ShowConfirmationDialog("Make this member the leader", $"By making this member the alliance leader you are giving up your role as leader, and will be demoted to lowly peon (default member rank). Are you sure you want to do that?", delegate
		{
			_allianceMember.RankData = _allianceRankInformation.Leader;
			_allianceMember.RankId = _allianceRankInformation.Leader.RankID;
			_allianceClient.SetAllianceMemberDataForPlayer(_allianceMember, SocialGroupParsers.CreateChangePlayerRank(_allianceMember.RankId)).Then(delegate
			{
				SocialCharacterSheet.TriggerAllianceStateCheck();
			}).Catch(OnFailure);
		}, "CONFIRM");
	}
}
