using System.Collections.Generic;
using Bossa.Travellers.Utils.ErrorHandling;
using Improbable;
using Travellers.UI.Events;
using Travellers.UI.Exceptions;
using Travellers.UI.Framework;
using Travellers.UI.Knowledge;
using Travellers.UI.Social.Crew;
using Travellers.UI.Sound;
using Travellers.UI.Tutorial;
using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.PlayerInventory;

[InjectableClass]
public class CharacterSheetScreen : UIScreen
{
	public static CharacterSheetScreenStateFacade Facade;

	public static HashSet<CharacterSheetTabType> TabsToSaveOnExit = new HashSet<CharacterSheetTabType>
	{
		CharacterSheetTabType.MultitoolCraft,
		CharacterSheetTabType.Character,
		CharacterSheetTabType.Knowledge,
		CharacterSheetTabType.Schematics,
		CharacterSheetTabType.Logbook
	};

	[SerializeField]
	private RectTransform _subScreenParentRect;

	[SerializeField]
	private RectTransform _tabContainer;

	[SerializeField]
	private UIInventoryTabToggleController _playerInventoryToggle;

	[SerializeField]
	private UIInventoryTabToggleController _lockboxToggle;

	[SerializeField]
	private GameObject inventoryRoot;

	[SerializeField]
	private GameObject lockboxRoot;

	[SerializeField]
	public InventoryGridUI _playerGrid;

	[SerializeField]
	public LockboxGridUI _lockboxGrid;

	[SerializeField]
	private HorizontalLayoutGroup _layoutOverlord;

	[SerializeField]
	private UIToggleGroup _uiToggleGroup;

	public CharacterSheetTabType CurrentTabType;

	private List<CharacterSheetModule> _loadedInventoryModules;

	private CraftingSubScreen _craftingSubScreen;

	private InventoryCharacterSubscreen _characteSubScreen;

	private InventorySystem _inventorySystem;

	private bool _shipHullEditorOpen;

	public CraftingUI CurrentCrafting => _craftingSubScreen.CurrentCrafting;

	private CanvasGroup characterSlotsCanvasGroup => _characteSubScreen.characterSlotsCanvasGroup;

	[InjectableMethod]
	public void InjectDependencies(InventorySystem inventorySystem)
	{
		_inventorySystem = inventorySystem;
	}

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUIInventoryEvents.UpdateLockbox, OnLockboxUpdated);
		_eventList.AddEvent(WAUICraftingEvents.OpenShipHullEditor, OnOpenShipHullEditor);
		_eventList.AddEvent(WAUICraftingEvents.CloseShipHullEditor, OnCloseShipHullEditor);
		_eventList.AddEvent(WAUIInventoryEvents.PerformCraftingReset, OnCraftingResetRequested);
		_eventList.AddEvent(WAUIInventoryEvents.ChangeCharacterScreenState, OnChangeCharacterScreenState);
	}

	protected override void ProtectedInit()
	{
		_loadedInventoryModules = new List<CharacterSheetModule>();
		CharacterDisplayModule characterDisplayModule = new CharacterDisplayModule(_subScreenParentRect, _tabContainer, _layoutOverlord, ShowInventoryTab, _uiToggleGroup);
		_loadedInventoryModules.Add(characterDisplayModule);
		CraftingSubscreenModule craftingSubscreenModule = new CraftingSubscreenModule(_subScreenParentRect, _tabContainer, _layoutOverlord, ShowInventoryTab, _uiToggleGroup);
		_loadedInventoryModules.Add(craftingSubscreenModule);
		_loadedInventoryModules.Add(new SchematicsSubscreenModule(_subScreenParentRect, _tabContainer, _layoutOverlord, ShowInventoryTab, _uiToggleGroup));
		_loadedInventoryModules.Add(new KnowledgeModule(_subScreenParentRect, _tabContainer, _layoutOverlord, ShowInventoryTab, _uiToggleGroup));
		_loadedInventoryModules.Add(new LogbookModule(_subScreenParentRect, _tabContainer, _layoutOverlord, ShowInventoryTab, _uiToggleGroup));
		_loadedInventoryModules.Add(new StorageDisplayModule(_subScreenParentRect, _tabContainer, _layoutOverlord, ShowInventoryTab, _uiToggleGroup));
		if (!WAConfig.GetOrDefault<bool>(ConfigKeys.AlliancesEnabled))
		{
			_loadedInventoryModules.Add(new OldCrewScreenModule(_subScreenParentRect, _tabContainer, _layoutOverlord, ShowInventoryTab, _uiToggleGroup));
		}
		_muteListenersWhenInactive = false;
		_characteSubScreen = (InventoryCharacterSubscreen)characterDisplayModule.ScreenModule;
		_craftingSubScreen = (CraftingSubScreen)craftingSubscreenModule.ScreenModule;
		_playerInventoryToggle.SetButtonEvent(delegate
		{
			ShowPlayerInventoryOrLockbox(LockboxTabType.PlayerInventory);
		});
		_lockboxToggle.SetButtonEvent(delegate
		{
			ShowPlayerInventoryOrLockbox(LockboxTabType.Lockbox);
		});
		_playerGrid.SetInventory(_inventorySystem.PlayerInventory);
		_playerGrid.CheckifFirstTimeOpen();
	}

	protected override void Activate()
	{
		Facade = new CharacterSheetScreenStateFacade(this);
		AudioPlayer.SetState("Inventory_Status", "In_Inventory");
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUITutorialEvents.RemoveStepFromSystem, new TutorialRemoveStepFromSystemEvent(TutorialStep.LEAVE_REVIVAL_CHAMBER));
		FloorClickReceiverScreen.SetFloorClickReceiverScreenState(isActive: true);
	}

	protected override void Deactivate()
	{
		Facade = null;
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInventoryEvents.ChangeInWorldStorageEntityID, default(EntityId));
		if (UICharacterModel.Instance != null)
		{
			UICharacterModel.Instance.gameObject.SetActive(value: false);
		}
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInventoryEvents.OnCloseInventory);
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUITutorialEvents.RemoveStepFromSystem, new TutorialRemoveStepFromSystemEvent());
		AudioPlayer.SetState("Inventory_Status", "Out_of_Inventory");
		_inventorySystem.SetCurrentCraftingData(null);
		FloorClickReceiverScreen.SetFloorClickReceiverScreenState(isActive: false);
	}

	protected override void ProtectedDispose()
	{
		Facade = null;
	}

	public static void PerformCraftReset()
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInventoryEvents.PerformCraftingReset, new PerformCraftingResetEvent());
	}

	public static void ChangeScreenState(bool showState, CharacterSheetTabType typeToShow = CharacterSheetTabType.MultitoolCraft)
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInventoryEvents.ChangeCharacterScreenState, new ChangeCharacterScreenStateEvent(showState, typeToShow));
	}

	private void OnLockboxUpdated(object[] obj)
	{
		UpdateLockbox();
	}

	private void OnOpenShipHullEditor(object[] obj)
	{
		_shipHullEditorOpen = true;
	}

	private void OnCloseShipHullEditor(object[] obj)
	{
		_shipHullEditorOpen = false;
	}

	private void OnCraftingResetRequested(object[] obj)
	{
		if (CurrentCrafting != null)
		{
			CurrentCrafting.ResetCraftingState();
		}
	}

	private void OnChangeCharacterScreenState(object[] obj)
	{
		ChangeCharacterScreenStateEvent changeCharacterScreenStateEvent = obj[0] as ChangeCharacterScreenStateEvent;
		if (changeCharacterScreenStateEvent.ShowState)
		{
			SetObjectActive(isActive: true);
			ShowInventoryTab(changeCharacterScreenStateEvent.TypeToShow);
		}
		else
		{
			_playerGrid.PrepareToClose();
			_playerGrid.CurrentInventory.IsInventoryOpen = false;
			SetObjectActive(isActive: false);
		}
		CheckSFX(changeCharacterScreenStateEvent.ShowState, changeCharacterScreenStateEvent.TypeToShow);
	}

	private void CheckSFX(bool isOpening, CharacterSheetTabType typeToCheck)
	{
		if (typeToCheck != CharacterSheetTabType.StorageObject)
		{
			SoundScreen.PlayASound((!isOpening) ? "Play_MainMenu_WindowClose" : "Play_MainMenu_WindowOpen");
		}
	}

	private void Update()
	{
		if (_shipHullEditorOpen)
		{
			return;
		}
		ReleaseAssert.IsNotNull<CharacterSheetScreen>(_inventorySystem, () => "WA-3667: _inventorySystem is null!");
		Cursor.visible = _inventorySystem != null && !_inventorySystem.IsPlayerDraggingItem;
		ReleaseAssert.IsTrue<CharacterSheetScreen>(LocalPlayer.Exists, () => "WA-3667: LocalPlayer does not exist!");
		if (LocalPlayer.Exists)
		{
			ReleaseAssert.IsNotNull<CharacterSheetScreen>(LocalPlayer.Instance.playerMove, () => "WA-3667: LocalPlayer doesn't yet have PlayerMove visualiser!");
			if (LocalPlayer.Instance.playerMove != null)
			{
				characterSlotsCanvasGroup.alpha = ((!LocalPlayer.Instance.playerMove.CanEquipThings) ? 0.2f : 1f);
				characterSlotsCanvasGroup.interactable = LocalPlayer.Instance.playerMove.CanEquipThings;
				characterSlotsCanvasGroup.blocksRaycasts = LocalPlayer.Instance.playerMove.CanEquipThings;
			}
		}
	}

	private void ShowPlayerInventoryOrLockbox(LockboxTabType tabTypeToUse)
	{
		_playerGrid.SetInventory(_inventorySystem.PlayerInventory);
		_playerGrid.RefreshInventory();
		UpdateLockbox();
		switch (tabTypeToUse)
		{
		case LockboxTabType.PlayerInventory:
			lockboxRoot.SetActive(value: false);
			inventoryRoot.SetActive(value: true);
			_playerGrid.SetObjectActive(isActive: true);
			_playerGrid.CurrentInventory.IsInventoryOpen = true;
			_lockboxToggle.SetButtonState(UIToggleState.Deselected);
			_playerInventoryToggle.SetButtonState(UIToggleState.Selected);
			break;
		case LockboxTabType.Lockbox:
			lockboxRoot.SetActive(value: true);
			inventoryRoot.SetActive(value: false);
			_playerGrid.SetObjectActive(isActive: false);
			_playerGrid.CurrentInventory.IsInventoryOpen = false;
			_playerInventoryToggle.SetButtonState(UIToggleState.Deselected);
			_lockboxToggle.SetButtonState(UIToggleState.Selected);
			break;
		}
	}

	private void UpdateLockbox()
	{
		_lockboxGrid.StartRefresh();
		foreach (KeyValuePair<InventoryItemKey, InventorySlotData> item in _inventorySystem.PlayerInventory.LockboxSlotDataLookup)
		{
			_lockboxGrid.AddItem(item.Value);
		}
		_lockboxGrid.StopRefresh();
	}

	private void ShowInventoryTab(CharacterSheetTabType tabType)
	{
		if (_inventorySystem.PlayerCraftingStationData == null)
		{
			throw new CharacterSheetException("Player crafting station data is null");
		}
		bool active = false;
		foreach (CharacterSheetModule loadedInventoryModule in _loadedInventoryModules)
		{
			if (loadedInventoryModule.PrepareForContext(tabType))
			{
				active = true;
			}
		}
		_subScreenParentRect.gameObject.SetActive(active);
		CurrentTabType = tabType;
		switch (tabType)
		{
		case CharacterSheetTabType.MultitoolCraft:
			LocalPlayer.Instance.playerCraftingInteraction.SetCraftingData(_inventorySystem.PlayerCraftingStationData);
			_inventorySystem.SetCurrentCraftingData(_inventorySystem.PlayerCraftingStationData);
			_craftingSubScreen.ShowCraftingModule();
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUITutorialEvents.AddStepToSystem, new TutorialAddStepToSystemEvent(TutorialStep.SCHEMATICS_TAB_ACTIVE));
			break;
		case CharacterSheetTabType.ShipCraft:
		case CharacterSheetTabType.ItemCraft:
		case CharacterSheetTabType.Cooking:
		case CharacterSheetTabType.Clothing:
			_craftingSubScreen.ShowCraftingModule();
			break;
		case CharacterSheetTabType.Character:
			if (UICharacterModel.Instance != null)
			{
				UICharacterModel.Instance.gameObject.SetActive(value: true);
			}
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUITutorialEvents.AddStepToSystem, new TutorialAddStepToSystemEvent(TutorialStep.CHARACTER_TAB_ACTIVE));
			break;
		case CharacterSheetTabType.Knowledge:
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUITutorialEvents.AddStepToSystem, new TutorialAddStepToSystemEvent(TutorialStep.KNOWLEDGE_TAB_ACTIVE));
			break;
		case CharacterSheetTabType.Logbook:
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUITutorialEvents.AddStepToSystem, new TutorialAddStepToSystemEvent(TutorialStep.LOGBOOK_TAB_ACTIVE));
			break;
		case CharacterSheetTabType.StorageObject:
			LocalPlayer.Instance.playerCraftingInteraction.SetCraftingData(_inventorySystem.PlayerCraftingStationData);
			_inventorySystem.SetCurrentCraftingData(_inventorySystem.PlayerCraftingStationData);
			break;
		}
		ShowPlayerInventoryOrLockbox(LockboxTabType.PlayerInventory);
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInventoryEvents.OnOpenInventoryTab, tabType);
	}
}
