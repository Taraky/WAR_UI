using System;
using System.Collections.Generic;
using Bossa.Travellers.Alliances;
using Bossa.Travellers.Social.DataModel;
using Travellers.UI.Framework;
using Travellers.UI.InfoPopups;
using UnityEngine;

namespace Travellers.UI.Social.Alliance;

[InjectableClass]
public class AllianceList : UIScreenComponent
{
	[SerializeField]
	private ScrollPaginator _scrollPaginator;

	[SerializeField]
	private Transform _scrollItemContainer;

	private AllianceBasicInformationSlice _allianceInfoSlice;

	private string _searchParameter;

	private List<AllianceInfoBar> _allInfoBars = new List<AllianceInfoBar>();

	private IAllianceClient _allianceClient;

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
		_searchParameter = string.Empty;
		_scrollPaginator.Subscribe(OnPageButtonPressed);
	}

	protected override void ProtectedDispose()
	{
	}

	public void RefreshResultsPage(int pageToRefresh)
	{
		if (string.IsNullOrEmpty(_searchParameter))
		{
			_allianceClient.GetBasicInfoForAllAlliances(pageToRefresh, 100).Then(delegate(AllianceBasicInformationSlice list)
			{
				AddList(list);
			}).Catch(delegate(Exception exception)
			{
				OnGetAllianceDataFailed(exception);
			});
		}
		else
		{
			_allianceClient.GetBasicInfoForAllAlliancesWithParams(_searchParameter, pageToRefresh, 100).Then(delegate(AllianceBasicInformationSlice list)
			{
				AddList(list);
			}).Catch(delegate(Exception exception)
			{
				OnGetAllianceDataFailed(exception);
			});
		}
	}

	public void SetSearchParameter(string searchString)
	{
		_searchParameter = searchString;
		RefreshResultsPage(0);
	}

	private void AddList(AllianceBasicInformationSlice allianceInfoSlice)
	{
		if (allianceInfoSlice.TotalAlliances == 0)
		{
			DialogPopupFacade.ShowOkDialog("Alliance search", "No results found for search");
		}
		_allianceInfoSlice = allianceInfoSlice;
		_scrollPaginator.SplitIntoPages(_allianceInfoSlice.TotalAlliances, _allianceInfoSlice.FirstAllianceIndex, _allianceInfoSlice.Alliances.Count);
		CreateListObjects();
	}

	private void OnGetAllianceDataFailed(Exception ex)
	{
		SocialCharacterSheet.TriggerAllianceExceptionHandler(ex);
	}

	private void OnPageButtonPressed(int newPage)
	{
		RefreshResultsPage(newPage);
	}

	private void CreateListObjects()
	{
		foreach (AllianceInfoBar allInfoBar in _allInfoBars)
		{
			allInfoBar.Dispose();
		}
		_allInfoBars.Clear();
		foreach (AllianceBasicInformation alliance in _allianceInfoSlice.Alliances)
		{
			AllianceInfoBar allianceInfoBar = UIObjectFactory.Create<AllianceInfoBar>(UIElementType.ScreenComponent, UIFillType.KeepOriginalAnchoring, _scrollItemContainer, isObjectActive: true);
			allianceInfoBar.SetData(_searchParameter, alliance);
			_allInfoBars.Add(allianceInfoBar);
		}
	}
}
