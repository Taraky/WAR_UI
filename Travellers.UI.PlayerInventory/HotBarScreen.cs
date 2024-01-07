using System.Collections.Generic;
using Travellers.UI.Events;
using Travellers.UI.Framework;
using Travellers.UI.Models;
using Travellers.UI.Tutorial;
using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.PlayerInventory;

[InjectableClass]
public class HotBarScreen : UIScreen
{
	public class LocalPlayerSpeechStateEvent : UIEvent
	{
		public bool Talking;

		public LocalPlayerSpeechStateEvent(bool talking)
		{
			Talking = talking;
		}
	}

	public class VOIPDisabledStateEvent : UIEvent
	{
		public bool Show;

		public VOIPDisabledStateEvent(bool showIndicator)
		{
			Show = showIndicator;
		}
	}

	[SerializeField]
	public GameObject HotbarSlotParent;

	[SerializeField]
	private HealthBar _healthBar;

	[SerializeField]
	private Image _localPlayerSpeechIndicator;

	[SerializeField]
	private Image _voipDisabledIndicator;

	private Dictionary<HotbarSlotType, HotbarSlot> _slotByType;

	private HotbarSlot _lastSelectedSlot;

	private InventorySystem _inventorySystem;

	private ISchematicSystem _schematicSystem;

	private int currentHealth;

	private int maxHealth;

	private readonly List<TutorialStep> _hotbarTutorialSteps = new List<TutorialStep>
	{
		TutorialStep.EQUIP_GAUNTLET_SALVAGE,
		TutorialStep.EQUIP_GAUNTLET_REPAIR,
		TutorialStep.EQUIP_SHIP_BUILDER,
		TutorialStep.EQUIP_SCANNER,
		TutorialStep.EQUIP_RANGED_WEAPON
	};

	[InjectableMethod]
	public void InjectDependencies(InventorySystem inventorySystem, ISchematicSystem schematicSystem)
	{
		_schematicSystem = schematicSystem;
		_inventorySystem = inventorySystem;
	}

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUIInventoryEvents.EquipCurrentSelectedInFirstHotbarSlot, OnEquipSelectedItemInFirstSlot);
		_eventList.AddEvent(WAUIPlayerProfileEvents.ToolUnlocked, OnUnlockedToolsChanged);
		_eventList.AddEvent(WAUIFeedbackReportingEvents.ChangeLocalPlayerSpeechState, OnLocalPlayerSpeechStateChange);
		_eventList.AddEvent(WAUIFeedbackReportingEvents.VOIPDisabled, OnVOIPDisabled);
		_eventList.AddEvent(WAUIInGameEvents.HotbarSlotSelected, OnHotbarSlotSelected);
	}

	public static void ToggleSpeechIndicator(bool show)
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIFeedbackReportingEvents.ChangeLocalPlayerSpeechState, new LocalPlayerSpeechStateEvent(show));
	}

	public static void ToggleVOIPFailureIndicator(bool show)
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIFeedbackReportingEvents.VOIPDisabled, new VOIPDisabledStateEvent(show));
	}

	public static void SelectHotBarSlot(HotbarSlotType hotbarSlotType, bool enabled = true)
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInGameEvents.HotbarSlotSelected, new HotBarStateChangeEvent(hotbarSlotType, enabled));
	}

	private void Update()
	{
		if (_inventorySystem.UpdateHotbarSlots)
		{
			RefreshHotbarSlots();
		}
		if (_inventorySystem.CurrentHealth != currentHealth || _inventorySystem.MaxHealth != maxHealth)
		{
			UpdateHealthBar();
		}
	}

	protected override void ProtectedInit()
	{
		_slotByType = new Dictionary<HotbarSlotType, HotbarSlot>();
		HotbarSlot[] componentsInChildren = HotbarSlotParent.GetComponentsInChildren<HotbarSlot>(includeInactive: true);
		foreach (HotbarSlot hotbarSlot in componentsInChildren)
		{
			_slotByType[hotbarSlot.SlotType] = hotbarSlot;
		}
		UpdateHealthBar();
		CheckSocialSlot();
		RefreshHotbarSlots();
		ToggleLocalPlayerSpeechIndicator(on: false);
		ToggleVOIPDisabledIndicator(disabled: false);
	}

	private void ToggleLocalPlayerSpeechIndicator(bool on)
	{
		_localPlayerSpeechIndicator.gameObject.SetActive(on);
	}

	private void ToggleVOIPDisabledIndicator(bool disabled)
	{
		_voipDisabledIndicator.gameObject.SetActive(disabled);
		if (disabled)
		{
			_localPlayerSpeechIndicator.gameObject.SetActive(value: false);
		}
	}

	private void OnHotbarSlotSelected(object[] obj)
	{
		HotBarStateChangeEvent hotBarStateChangeEvent = (HotBarStateChangeEvent)obj[0];
		SetHotbarState(hotBarStateChangeEvent.SlotType, hotBarStateChangeEvent.Enabled);
	}

	private void OnLocalPlayerSpeechStateChange(object[] obj)
	{
		LocalPlayerSpeechStateEvent localPlayerSpeechStateEvent = (LocalPlayerSpeechStateEvent)obj[0];
		ToggleLocalPlayerSpeechIndicator(localPlayerSpeechStateEvent.Talking);
	}

	private void OnVOIPDisabled(object[] obj)
	{
		VOIPDisabledStateEvent vOIPDisabledStateEvent = (VOIPDisabledStateEvent)obj[0];
		ToggleVOIPDisabledIndicator(vOIPDisabledStateEvent.Show);
	}

	private void OnUnlockedToolsChanged(object[] newTools)
	{
		RefreshToolSlots();
	}

	private void RefreshToolSlots()
	{
		CheckToolSlot(ToolType.Salvage, HotbarSlotType.Salvage);
		CheckToolSlot(ToolType.Repair, HotbarSlotType.Repair);
		CheckToolSlot(ToolType.Build, HotbarSlotType.Lifter);
		CheckToolSlot(ToolType.Scan, HotbarSlotType.Scanner);
	}

	private void CheckToolSlot(ToolType type, HotbarSlotType slotType)
	{
		bool toolState = ToolBehaviour.IsToolUnlocked(type);
		if (_slotByType.TryGetValue(slotType, out var value))
		{
			value.SetToolState(toolState);
		}
	}

	private void CheckSocialSlot()
	{
		if (_slotByType.TryGetValue(HotbarSlotType.Social, out var value))
		{
			value.SetObjectActive(WAConfig.GetOrDefault<bool>(ConfigKeys.AlliancesEnabled));
		}
	}

	private void SetHotbarState(HotbarSlotType selectedType, bool enabled)
	{
		HotbarSlot.currentSelected = selectedType;
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInventoryEvents.HotbarSelected, new HotBarStateChangeEvent(selectedType, enabled));
		if (_lastSelectedSlot != null)
		{
			SelectedHotbarSlotChanged(selectedType);
		}
		foreach (KeyValuePair<HotbarSlotType, HotbarSlot> item in _slotByType)
		{
			if (item.Key != selectedType && item.Value.IsInGroup(selectedType))
			{
				item.Value.SetSelected(enabled: false);
			}
			else if (item.Key == selectedType)
			{
				item.Value.SetSelected(enabled: true);
				_lastSelectedSlot = item.Value;
			}
		}
	}

	public HotbarSlot GetEmptyHotBarSlot()
	{
		foreach (KeyValuePair<HotbarSlotType, HotbarSlot> item in _slotByType)
		{
			if (HotbarSlot.EquippableSlotGroup.Contains(item.Key) && item.Value.IsEmpty)
			{
				return item.Value;
			}
		}
		return null;
	}

	public void RefreshHotbarSlots()
	{
		RefreshToolSlots();
		foreach (KeyValuePair<HotbarSlotType, HotbarSlot> item in _slotByType)
		{
			if (HotbarSlot.EquippableSlotGroup.Contains(item.Key))
			{
				item.Value.ClearItem();
			}
		}
		if (CharacterData.Instance == null)
		{
			return;
		}
		InventoryContents playerInventory = _inventorySystem.PlayerInventory;
		foreach (KeyValuePair<InventoryItemKey, InventorySlotData> item2 in playerInventory.AllSlotDataLookup)
		{
			InventorySlotData value = item2.Value;
			if (value.hotBarSlotNum != -1 && _slotByType.TryGetValue((HotbarSlotType)value.hotBarSlotNum, out var value2))
			{
				value2.Setup(value);
			}
		}
		SetHotbarState(HotbarSlot.currentSelected, enabled: true);
	}

	public void UpdateHealthBar()
	{
		currentHealth = _inventorySystem.CurrentHealth;
		maxHealth = _inventorySystem.MaxHealth;
		_healthBar.UpdateHealth(currentHealth, maxHealth);
	}

	private void SelectedHotbarSlotChanged(HotbarSlotType newType)
	{
		switch (newType)
		{
		case HotbarSlotType.Salvage:
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUITutorialEvents.AddStepToSystem, new TutorialAddStepToSystemEvent(TutorialStep.EQUIP_GAUNTLET_SALVAGE));
			return;
		case HotbarSlotType.Repair:
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUITutorialEvents.AddStepToSystem, new TutorialAddStepToSystemEvent(TutorialStep.EQUIP_GAUNTLET_REPAIR));
			return;
		case HotbarSlotType.Lifter:
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUITutorialEvents.AddStepToSystem, new TutorialAddStepToSystemEvent(TutorialStep.EQUIP_SHIP_BUILDER));
			return;
		case HotbarSlotType.Scanner:
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUITutorialEvents.AddStepToSystem, new TutorialAddStepToSystemEvent(TutorialStep.EQUIP_SCANNER));
			return;
		}
		if (_schematicSystem.IsPlayerAndReferenceDataLoaded)
		{
			if (_slotByType.TryGetValue(newType, out var value) && value.equippedItem.itemTypeId == "pistol")
			{
				Singleton<WAEventSystem>.Instance.TriggerEvent(WAUITutorialEvents.AddStepToSystem, new TutorialAddStepToSystemEvent(TutorialStep.EQUIP_RANGED_WEAPON));
			}
			else
			{
				Singleton<WAEventSystem>.Instance.TriggerEvent(WAUITutorialEvents.RemoveStepFromSystem, new TutorialRemoveStepFromSystemEvent(_hotbarTutorialSteps));
			}
		}
	}

	private void OnEquipSelectedItemInFirstSlot(object[] obj)
	{
		HotbarSlot emptyHotBarSlot = GetEmptyHotBarSlot();
		if (!(emptyHotBarSlot == null))
		{
			emptyHotBarSlot.Equip();
		}
	}

	protected override void ProtectedDispose()
	{
	}
}
