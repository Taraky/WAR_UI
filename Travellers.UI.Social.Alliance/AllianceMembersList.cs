using System;
using System.Collections.Generic;
using Bossa.Travellers.Alliances;
using Bossa.Travellers.Social.DataModel;
using Travellers.UI.Framework;
using UnityEngine;

namespace Travellers.UI.Social.Alliance;

[InjectableClass]
public class AllianceMembersList : UIScreenComponent
{
	[SerializeField]
	private ScrollPaginator _scrollPaginator;

	[SerializeField]
	private UIToggleGroup _toggleGroup;

	[SerializeField]
	private AlliancePlayerInfoButton _playerInfoButton;

	[SerializeField]
	private int _numberOfItemsPerPage;

	private AllianceMembersSlice _allianceMembersInfoSlice;

	private List<AlliancePlayerInfoButton> _allMemberBars = new List<AlliancePlayerInfoButton>();

	private IAllianceClient _allianceClient;

	private List<AllianceMember> _filteredAllianceInformations;

	private AllianceMemberFilterType _mainFilter;

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
		_playerInfoButton.SetDataForPlayer(_toggleGroup);
		_scrollPaginator.Subscribe(OnPageButtonPressed);
	}

	protected override void ProtectedDispose()
	{
	}

	public void SetupFilter(bool alphabeticalAscending, bool showOffline)
	{
		_mainFilter = new FilterMemberAlphabetical(alphabeticalAscending, showOffline);
	}

	public void SetupForFirstLook()
	{
		_playerInfoButton.TriggerButtonEvent();
		RefreshMembersPage(0);
	}

	public void RefreshMembersPage(int pageToRefresh)
	{
		_mainFilter.Filter(_allianceClient, pageToRefresh, _numberOfItemsPerPage, AddList, OnFailure);
	}

	private void AddList(AllianceMembersSlice allianceMembers)
	{
		_allianceMembersInfoSlice = allianceMembers;
		_scrollPaginator.SplitIntoPages(allianceMembers.MemberCount, allianceMembers.FirstMemberIndex, _numberOfItemsPerPage);
		CreateListObjects();
	}

	private void OnPageButtonPressed(int newPage)
	{
		RefreshMembersPage(newPage);
	}

	private void CreateListObjects()
	{
		_toggleGroup.ClearGroup();
		foreach (AlliancePlayerInfoButton allMemberBar in _allMemberBars)
		{
			allMemberBar.Dispose();
		}
		_allMemberBars.Clear();
		foreach (AllianceMember member in _allianceMembersInfoSlice.Members)
		{
			AlliancePlayerInfoButton alliancePlayerInfoButton = UIObjectFactory.Create<AlliancePlayerInfoButton>(UIElementType.ScreenComponent, UIFillType.KeepOriginalAnchoring, base.transform, isObjectActive: true);
			alliancePlayerInfoButton.SetDataForAllianceMember(_toggleGroup, member);
			_allMemberBars.Add(alliancePlayerInfoButton);
		}
		_playerInfoButton.AddButtonToGroupToggle(_toggleGroup);
	}

	public void FilterByString(string newSearch, bool showOffline)
	{
		_mainFilter = new FilterMemberBySearchKey(newSearch, showOffline);
		_mainFilter.Filter(_allianceClient, 0, _numberOfItemsPerPage, AddList, OnFailure);
	}

	public void OrderByRank(bool rankOrderAscending, bool showOffline)
	{
		_mainFilter = new FilterMemberByRank(rankOrderAscending, showOffline);
		_mainFilter.Filter(_allianceClient, 0, _numberOfItemsPerPage, AddList, OnFailure);
	}

	public void OrderByName(bool nameOrderAlphabetical, bool showOffline)
	{
		_mainFilter = new FilterMemberAlphabetical(nameOrderAlphabetical, showOffline);
		_mainFilter.Filter(_allianceClient, 0, _numberOfItemsPerPage, AddList, OnFailure);
	}

	public void ShowOfflineMembers(bool showOffline)
	{
		_mainFilter.SetOffline(showOffline);
		_mainFilter.Filter(_allianceClient, 0, _numberOfItemsPerPage, AddList, OnFailure);
	}

	private void OnFailure(Exception ex)
	{
		SocialCharacterSheet.TriggerAllianceExceptionHandler(ex, SocialCharacterSheet.TriggerCloseSocialScreen, SocialCharacterSheet.TriggerAllianceDataRefresh);
	}
}
