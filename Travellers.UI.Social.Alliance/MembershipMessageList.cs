using System;
using System.Collections.Generic;
using Bossa.Travellers.Alliances;
using Bossa.Travellers.Social.DataModel;
using Travellers.UI.Framework;
using UnityEngine;

namespace Travellers.UI.Social.Alliance;

[InjectableClass]
public class MembershipMessageList : UIScreenComponent
{
	[SerializeField]
	private ScrollPaginator _scrollPaginator;

	[SerializeField]
	private UIToggleGroup _toggleButtonGroup;

	[SerializeField]
	private int _numberOfItemsPerPage;

	private List<AlliancePlayerInfoButton> _allMemberBars = new List<AlliancePlayerInfoButton>();

	private IAllianceClient _allianceClient;

	private List<MembershipChangeRequest> _fullAllianceInformations;

	private List<MembershipChangeRequest> _filteredAllianceInformations;

	private IMembershipListMessageFilter _membershipListMessageFilter;

	private AllianceMembershipMessageSlice _messageSlice;

	private string _searchFilter;

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
		_scrollPaginator.Subscribe(OnPageButtonPressed);
		_searchFilter = string.Empty;
	}

	protected override void ProtectedDispose()
	{
	}

	public void RefreshResultsPage(IMembershipListMessageFilter applicationsFilter, int pageToRefresh)
	{
		_membershipListMessageFilter = applicationsFilter;
		if (string.IsNullOrEmpty(_searchFilter))
		{
			_membershipListMessageFilter.GetMembershipMessages(_allianceClient, pageToRefresh, _numberOfItemsPerPage, AddList, OnFailed);
		}
		else
		{
			_membershipListMessageFilter.GetMembershipMessagesWithParams(_allianceClient, _searchFilter, pageToRefresh, _numberOfItemsPerPage, AddList, OnFailed);
		}
	}

	private void AddList(AllianceMembershipMessageSlice messageSlice)
	{
		_messageSlice = messageSlice;
		_scrollPaginator.SplitIntoPages(_messageSlice.TotalMessages, _messageSlice.FirstMessageIndex, _numberOfItemsPerPage);
		CreateListObjects();
	}

	private void OnPageButtonPressed(int newPage)
	{
		RefreshResultsPage(_membershipListMessageFilter, newPage);
	}

	private void CreateListObjects()
	{
		_toggleButtonGroup.ClearGroup();
		foreach (AlliancePlayerInfoButton allMemberBar in _allMemberBars)
		{
			allMemberBar.Dispose();
		}
		_allMemberBars.Clear();
		foreach (MembershipChangeRequest message in _messageSlice.Messages)
		{
			AlliancePlayerInfoButton alliancePlayerInfoButton = UIObjectFactory.Create<AlliancePlayerInfoButton>(UIElementType.ScreenComponent, UIFillType.KeepOriginalAnchoring, base.transform, isObjectActive: true);
			_membershipListMessageFilter.SetDataForMessageChoiceButton(_toggleButtonGroup, alliancePlayerInfoButton, message, _allianceClient, OnFailed);
			_allMemberBars.Add(alliancePlayerInfoButton);
		}
	}

	public void FilterByString(string newSearch)
	{
		_searchFilter = newSearch;
		RefreshResultsPage(_membershipListMessageFilter, 0);
	}

	private void OnFailed(Exception ex)
	{
		SocialCharacterSheet.TriggerAllianceExceptionHandler(ex, SocialCharacterSheet.TriggerCloseSocialScreen, SocialCharacterSheet.TriggerAllianceDataRefresh);
	}
}
