using System;
using System.Collections.Generic;
using Bossa.Travellers.Alliances;
using Bossa.Travellers.Social;
using Bossa.Travellers.Social.DataModel;
using Bossa.Travellers.Utils.Http;
using RSG;
using Travellers.Crew;
using Travellers.UI.Framework;
using Travellers.UI.InfoPopups;
using Travellers.UI.PlayerInventory;
using Travellers.UI.Social.Alliance;
using Travellers.UI.Social.Crew;
using UnityEngine;
using WAUtilities.Logging;

namespace Travellers.UI.Social;

[InjectableClass]
public class SocialCharacterSheet : UIScreen
{
	private IAllianceClient _allianceClient;

	[SerializeField]
	private Transform _screenModuleParent;

	[SerializeField]
	private Transform _tabParent;

	[SerializeField]
	private SocialInfoPanel _socialInfoPanel;

	[SerializeField]
	private GameObject _mainPane;

	[SerializeField]
	private GameObject _rightPane;

	[SerializeField]
	private UIToggleGroup _buttonToggleGroup;

	private readonly Dictionary<Type, SocialSheetModule> _loadedScreenModules = new Dictionary<Type, SocialSheetModule>();

	private LoadingInputBlocker _mainPaneBlocker;

	private LoadingInputBlocker _rightPaneBlocker;

	private ICrewClient _crewClient;

	private SocialSheetModule _currentModule;

	[InjectableMethod]
	public void InjectDependencies(IAllianceClient allianceClient, ICrewClient crewClient)
	{
		_crewClient = crewClient;
		_allianceClient = allianceClient;
	}

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUIAllianceEvents.AllianceStateRefresh, OnYourAllianceStateChanged);
	}

	protected override void ProtectedInit()
	{
		SocialRequestMonitor.AddEventForServerStatusChange(UpdateServerStatus);
		_loadedScreenModules[typeof(CrewSocialModule)] = new CrewSocialModule(_screenModuleParent, _tabParent, OnTabPressed, _buttonToggleGroup, _socialInfoPanel);
		_loadedScreenModules[typeof(AllianceSearchSocialModule)] = new AllianceSearchSocialModule(_screenModuleParent, _tabParent, OnTabPressed, _buttonToggleGroup, _socialInfoPanel);
		_loadedScreenModules[typeof(YourAllianceSocialModule)] = new YourAllianceSocialModule(_screenModuleParent, _tabParent, OnTabPressed, _buttonToggleGroup, _socialInfoPanel);
		_muteListenersWhenInactive = false;
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIAllianceEvents.RefreshServerCache);
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUICrewEvents.RefreshServerCache);
		CheckAllianceState();
	}

	protected override void ProtectedDispose()
	{
		SocialRequestMonitor.StopAllRequests();
		SocialRequestMonitor.RemoveEventForServerStatusChange(UpdateServerStatus);
		_crewClient.InvalidateCache();
		_allianceClient.InvalidateCache();
	}

	public static void TriggerAllianceDataRefresh()
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIAllianceEvents.RefreshServerCache);
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIAllianceEvents.AllAllianceDataChanged);
	}

	public static void TriggerAllianceStateCheck()
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIAllianceEvents.RefreshServerCache);
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIAllianceEvents.AllianceStateRefresh);
	}

	public static void TriggerCrewDataRefresh()
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUICrewEvents.RefreshServerCache);
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUICrewEvents.CrewStateRefresh);
	}

	public static void TriggerCloseSocialScreen()
	{
		UIWindowController.PopState<SocialScreenUIState>();
	}

	public static void TriggerAllianceExceptionHandler(Exception e, Action onClose, Action onReload, string title = null)
	{
		SocialRequestMonitor.StopAllRequests();
		if (e is SocialServerResponseErrorException)
		{
			DialogPopupFacade.ShowOkDialog("Error", e.Message);
		}
		else
		{
			if (e is PromiseEarlyExitException)
			{
				return;
			}
			if (e is InvalidAllianceNameException)
			{
				DialogPopupFacade.ShowOkDialog("Invalid alliance name", e.Message);
				return;
			}
			if (string.IsNullOrEmpty(title))
			{
				title = "Can't retrieve alliance or crew data";
			}
			UIErrorHandler.TriggerExceptionHandler<SocialCharacterSheet>(title, e, onClose, onReload);
		}
	}

	public static void TriggerAllianceExceptionHandler(Exception e)
	{
		TriggerAllianceExceptionHandler(e, TriggerCloseSocialScreen, TriggerAllianceDataRefresh);
	}

	public void RestoreState(CharacterSheetTabType socialState)
	{
		OnTabPressed(socialState);
	}

	public CharacterSheetTabType RetrieveState()
	{
		return _currentModule.CurrentTabType;
	}

	public void SetAsCrew()
	{
		OnTabPressed(CharacterSheetTabType.Crew);
	}

	private void OnTabPressed(CharacterSheetTabType tabType)
	{
		foreach (KeyValuePair<Type, SocialSheetModule> loadedScreenModule in _loadedScreenModules)
		{
			if (loadedScreenModule.Value.PrepareForContext(tabType))
			{
				_currentModule = loadedScreenModule.Value;
			}
		}
	}

	private void OnYourAllianceStateChanged(object[] obj)
	{
		CheckAllianceState();
	}

	private void UpdateServerStatus(bool isBusy)
	{
		CheckLoadingObjects();
		if (isBusy)
		{
			_buttonToggleGroup.LockButtonsAndDisableInteractivity();
		}
		else
		{
			_buttonToggleGroup.UnlockButtonsAndRestorePreviousStates();
		}
		_mainPaneBlocker.SetObjectActive(isBusy);
		_rightPaneBlocker.SetObjectActive(isBusy);
	}

	private void CheckLoadingObjects()
	{
		if (_mainPaneBlocker == null)
		{
			_mainPaneBlocker = UIObjectFactory.Create<LoadingInputBlocker>(UIElementType.ScreenComponent, UIFillType.FillParentTransform, _mainPane.transform, isObjectActive: false);
		}
		if (_rightPaneBlocker == null)
		{
			_rightPaneBlocker = UIObjectFactory.Create<LoadingInputBlocker>(UIElementType.ScreenComponent, UIFillType.FillParentTransform, _rightPane.transform, isObjectActive: false);
		}
	}

	private void CheckAllianceState()
	{
		_allianceClient.GetYourBasicAllianceInfo().Then(delegate(AllianceBasicInformation allianceBasic)
		{
			OnPlayerAllianceDataRecieved(allianceBasic);
			return Promise.Resolved();
		}).Catch(OnFailure);
	}

	private void OnPlayerAllianceDataRecieved(AllianceBasicInformation allianceBasic)
	{
		if (_loadedScreenModules.TryGetValue(typeof(YourAllianceSocialModule), out var value))
		{
			if (allianceBasic != null)
			{
				value.UpdateTabName(allianceBasic.AllianceDisplayName.ToUpper());
			}
			else
			{
				value.UpdateTabName("CREATE ALLIANCE");
			}
		}
		else
		{
			WALogger.Error<SocialCharacterSheet>(LogChannel.UI, "Can't find YourAlliance ui module", new object[0]);
		}
	}

	private void OnFailure(Exception ex)
	{
		TriggerAllianceExceptionHandler(ex, TriggerCloseSocialScreen, CheckAllianceState);
	}
}
