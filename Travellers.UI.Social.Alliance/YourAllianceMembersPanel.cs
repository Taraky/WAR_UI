using TMPro;
using Travellers.UI.Framework;
using UnityEngine;

namespace Travellers.UI.Social.Alliance;

[InjectableClass]
public class YourAllianceMembersPanel : UIScreenComponent
{
	[SerializeField]
	private UIToggleController _offlineToggle;

	[SerializeField]
	private UIButtonController _orderByNameButtonController;

	[SerializeField]
	private UIButtonController _orderByRankButtonController;

	[SerializeField]
	private AllianceMembersList _membersList;

	[SerializeField]
	private TMP_InputField _searchInputField;

	private bool _showOffline;

	private bool _rankOrderAscending;

	private bool _alphabeticalAscending;

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUIAllianceEvents.AllAllianceDataChanged, OnDataUpdated);
	}

	private void OnDataUpdated(object[] obj)
	{
		RefreshAllianceMembersList();
	}

	protected override void ProtectedInit()
	{
		_searchInputField.onValueChanged.AddListener(OnSearchChanged);
		_offlineToggle.SetButtonEvent(OnToggleOfflinePressed);
		_orderByNameButtonController.SetButtonEvent(OnToggleSearchByNamePressed);
		_orderByRankButtonController.SetButtonEvent(OnToggleSearchByRankPressed);
		_rankOrderAscending = true;
		_alphabeticalAscending = true;
		_showOffline = true;
		_offlineToggle.SetObjectActive(isActive: false);
		_membersList.SetupFilter(_alphabeticalAscending, _showOffline);
	}

	protected override void ProtectedDispose()
	{
	}

	private void OnSearchChanged(string newSearch)
	{
		_membersList.FilterByString(newSearch, _showOffline);
	}

	private void OnToggleSearchByRankPressed()
	{
		_rankOrderAscending = !_rankOrderAscending;
		OrderListbyRank();
	}

	private void OnToggleSearchByNamePressed()
	{
		_alphabeticalAscending = !_alphabeticalAscending;
		OrderListbyName();
	}

	public void RefreshAllianceMembersList()
	{
		_membersList.SetupForFirstLook();
	}

	private void OnToggleOfflinePressed()
	{
		_showOffline = !_showOffline;
		_offlineToggle.SetToggleSelected(_showOffline);
		ShowOfflineMembers();
	}

	private void OrderListbyRank()
	{
		_membersList.OrderByRank(_rankOrderAscending, _showOffline);
	}

	private void OrderListbyName()
	{
		_membersList.OrderByName(_alphabeticalAscending, _showOffline);
	}

	private void ShowOfflineMembers()
	{
		_membersList.ShowOfflineMembers(_showOffline);
	}

	public void Setup()
	{
		SetObjectActive(isActive: false);
	}
}
