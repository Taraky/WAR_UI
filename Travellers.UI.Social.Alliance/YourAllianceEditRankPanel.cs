using System;
using System.Collections.Generic;
using System.Linq;
using Bossa.Travellers.Alliances;
using Bossa.Travellers.Social.DataModel;
using TMPro;
using Travellers.UI.Framework;
using Travellers.UI.InfoPopups;
using UnityEngine;
using WAUtilities.Logging;

namespace Travellers.UI.Social.Alliance;

[InjectableClass]
public class YourAllianceEditRankPanel : UIScreenComponent
{
	[SerializeField]
	private UIButtonController _acceptButton;

	[SerializeField]
	private UIButtonController _addRankButton;

	[SerializeField]
	private TMP_InputField _addRankName;

	private IAllianceClient _allianceClient;

	private AllianceRankInformation _allianceRankInformation;

	[SerializeField]
	private UIButtonController _cancelButton;

	private AllianceRank _currentRank;

	[SerializeField]
	private UIButtonController _deleteRank;

	[SerializeField]
	private TMP_Dropdown _dropdownRankList;

	[SerializeField]
	private UIRankToggleController _editAllianceDescriptionFlag;

	[SerializeField]
	private UIRankToggleController _editNotesFlag;

	[SerializeField]
	private UIRankToggleController _editRanksFlag;

	[SerializeField]
	private UIRankToggleController _editStatusMessageFlag;

	[SerializeField]
	private GameObject _inputShield;

	[SerializeField]
	private UIRankToggleController _leaderPermissionFlag;

	[SerializeField]
	private UIRankToggleController _manageApplicationsFlag;

	[SerializeField]
	private UIRankToggleController _promoteDemoteFlag;

	[SerializeField]
	private UIButtonController _showDropdownButton;

	[InjectableMethod]
	public void InjectDependencies(IAllianceClient allianceClient)
	{
		_allianceClient = allianceClient;
	}

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUIAllianceEvents.AllAllianceDataChanged, OnAllianceDataChanged);
	}

	private void OnAllianceDataChanged(object[] obj)
	{
		RefreshData();
	}

	protected override void ProtectedInit()
	{
		_cancelButton.SetButtonEvent(OnCancelPressed);
		_acceptButton.SetButtonEvent(OnAcceptPressed);
		_addRankButton.SetButtonEvent(OnAddRankPressed);
		_deleteRank.SetButtonEvent(OnDeleteRankPressed);
		_showDropdownButton.SetButtonEvent(OnDropdownPressed);
	}

	protected override void ProtectedDispose()
	{
	}

	public void Setup()
	{
		SetObjectActive(isActive: false);
		_dropdownRankList.Show();
	}

	public void RefreshData()
	{
		_allianceClient.GetYourAllianceRankInformation().Then((Action<AllianceRankInformation>)OnAllianceDataRetrieved).Catch(OnRequestFailure);
	}

	private void OnAllianceDataRetrieved(AllianceRankInformation rankInformation)
	{
		_allianceRankInformation = rankInformation;
		_dropdownRankList.ClearOptions();
		List<string> options = (from x in _allianceRankInformation.RankList
			orderby x.RankName
			select x.RankName).ToList();
		_dropdownRankList.AddOptions(options);
		_dropdownRankList.onValueChanged.AddListener(OnDropdownListChanged);
		OnDropdownListChanged(0);
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
			SelectRank(list[0]);
		}
	}

	private void SelectRank(AllianceRank randInfo)
	{
		_currentRank = randInfo;
		RefreshPermissions(randInfo);
	}

	private void OnAcceptPressed()
	{
		AllianceRank rankPermissions = HarvestPermissions(_currentRank);
		_allianceClient.SetRankPermissions(rankPermissions).Then(delegate(AllianceRank newRank)
		{
			SelectRank(newRank);
			SocialCharacterSheet.TriggerAllianceDataRefresh();
		}).Catch(OnRequestFailure);
	}

	private void OnAddRankPressed()
	{
		AllianceRank rankToUse = new AllianceRank(_addRankName.text, string.Empty, leaderChat: false, editMembers: false, editMessageOfTheDay: false, editGroup: false, editOfficerNote: false, readOfficerNote: false, editRankPermisions: false, isDefaultLeaderRank: false, isDefaultMemberRank: false, isRankEditable: true);
		_allianceClient.CreateNewRank(rankToUse).Then(delegate
		{
			_addRankName.text = string.Empty;
			SocialCharacterSheet.TriggerAllianceDataRefresh();
		}).Catch(OnRequestFailure);
	}

	private void OnDeleteRankPressed()
	{
		DialogPopupFacade.ShowConfirmationDialog("Delete Rank", "Removing this rank will set all players currently using it to the default member rank. Are you sure?", delegate
		{
			_allianceClient.DeleteRank(_currentRank).Then(delegate
			{
				SocialCharacterSheet.TriggerAllianceDataRefresh();
			}).Catch(OnRequestFailure);
		}, "CONFIRM");
	}

	private void OnDropdownPressed()
	{
		if (_dropdownRankList.IsExpanded)
		{
			_dropdownRankList.Hide();
		}
		else
		{
			_dropdownRankList.Show();
		}
	}

	private void OnCancelPressed()
	{
		RefreshPermissions(_currentRank);
	}

	private void OnRequestFailure(Exception exception)
	{
		WALogger.Exception<YourAllianceEditRankPanel>(exception);
	}

	private void RefreshPermissions(AllianceRank rankToUse)
	{
		_inputShield.SetActive(!rankToUse.IsRankEditable);
		_leaderPermissionFlag.SetToggleSelected(rankToUse.LeaderChat);
		_promoteDemoteFlag.SetToggleSelected(rankToUse.EditMembers);
		_manageApplicationsFlag.SetToggleSelected(rankToUse.EditMembers);
		_editStatusMessageFlag.SetToggleSelected(rankToUse.EditMessageOfTheDay);
		_editAllianceDescriptionFlag.SetToggleSelected(rankToUse.EditGroup);
		_editRanksFlag.SetToggleSelected(rankToUse.EditRankPermisions);
		_editNotesFlag.SetToggleSelected(rankToUse.EditMembers);
	}

	private AllianceRank HarvestPermissions(AllianceRank rankToUse)
	{
		rankToUse.LeaderChat = _leaderPermissionFlag.IsSelected;
		rankToUse.EditMembers = _manageApplicationsFlag.IsSelected;
		rankToUse.EditMessageOfTheDay = _editStatusMessageFlag.IsSelected;
		rankToUse.EditGroup = _editAllianceDescriptionFlag.IsSelected;
		rankToUse.EditRankPermisions = _editRanksFlag.IsSelected;
		return rankToUse;
	}
}
