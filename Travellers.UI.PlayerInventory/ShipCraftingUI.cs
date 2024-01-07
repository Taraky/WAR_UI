using Bossa.Travellers.Craftingstation;
using Bossa.Travellers.CraftingStation;
using Bossa.Travellers.World;
using Improbable.Collections;
using Travellers.UI.Events;
using Travellers.UI.Framework;
using Travellers.UI.HUDMessaging;
using Travellers.UI.InfoPopups;
using Travellers.UI.Models;
using Travellers.UI.Sound;
using UnityEngine;

namespace Travellers.UI.PlayerInventory;

public class ShipCraftingUI : CraftingUI
{
	[SerializeField]
	private GameObject _noSchematicSelectedRoot;

	[SerializeField]
	private GameObject _schematicSelectedRoot;

	[SerializeField]
	private GameObject _shipBlueprintCraftingRoot;

	[SerializeField]
	private GameObject _leftPanelRoot;

	[SerializeField]
	private ShipDiagnosticsUI _shipDiagnostics;

	[SerializeField]
	protected ShipSchematicsList _shipSchematicsList;

	[SerializeField]
	protected LockCodeUI _lockCodeUI;

	[SerializeField]
	protected ShipBlueprintCraftingUI _shipBlueprintCraftingUI;

	public ShipCraftingUIState CurrentState;

	private readonly LazyUIInterface<ISchematicSystem> _schematicSystem = new LazyUIInterface<ISchematicSystem>();

	private List<ShipBlueprintSchematic> _shipBlueprintSchematics;

	private ShipCraftingUIState _currentState;

	protected override void AddListeners()
	{
		base.AddListeners();
		_eventList.AddEvent(WAUICraftingEvents.CloseShipHullEditor, OnCloseShipHullEditor);
		_eventList.AddEvent(WAUIInventoryEvents.ShipDockedChanged, OnShipDockChanged);
		_eventList.AddEvent(WAUIInventoryEvents.SchematicsUpdated, HandleHullSchematicsUpdated);
	}

	protected override void ProtectedInit()
	{
		_muteListenersWhenInactive = false;
		_craftingMaterialsRoot.gameObject.SetActive(value: false);
		_currentSlots = new CraftingMaterialSlot[0];
		_craftButton.SetButtonEvent(WAUIButtonEvents.CraftButtonPressed);
		_leftPanelRoot.SetActive(value: false);
	}

	public override void SetAsActiveCraftingSection(bool willBeActive)
	{
	}

	protected override void Activate()
	{
		base.Activate();
		_leftPanelRoot.SetActive(value: true);
		_shipSchematicsList.ShipHullSchematicSelected += HandleShipHullSchematicSelected;
		_shipSchematicsList.ShipBlueprintSelected += HandleShipBlueprintSelected;
		PlayerShipBlueprintInteractionBehaviour playerShipBlueprintInteractionBehaviour = LocalPlayer.Instance.PlayerShipBlueprintInteractionBehaviour;
		playerShipBlueprintInteractionBehaviour.ShipBlueprintListModel.AddAndInvoke(HandleBlueprintsUpdated);
		playerShipBlueprintInteractionBehaviour.ShipBlueprintCraftingBehaviourModelWriter.Model.AddAndInvoke(HandleShipBlueprintCraftingBehaviourChanged);
		playerShipBlueprintInteractionBehaviour.TriggerShipBlueprintsRefresh();
		UpdateSchematicsList();
	}

	private void HandleShipBlueprintCraftingBehaviourChanged(ShipBlueprintCraftingBehaviour oldValue, ShipBlueprintCraftingBehaviour newValue)
	{
		if (oldValue != null)
		{
			oldValue.ShipBlueprintSchematicsModel.Remove(HandleShipBlueprintSchematicsChanged);
			newValue.SelectedBlueprintNameModel.Remove(HandleSelectedBlueprintNameChanged);
		}
		if (newValue != null)
		{
			newValue.ShipBlueprintSchematicsModel.AddAndInvoke(HandleShipBlueprintSchematicsChanged);
			newValue.SelectedBlueprintNameModel.AddAndInvoke(HandleSelectedBlueprintNameChanged);
		}
	}

	private void HandleSelectedBlueprintNameChanged(Option<string> oldvalue, Option<string> newvalue)
	{
		UpdateSelectedSchematic();
	}

	protected override void Deactivate()
	{
		base.Deactivate();
		_leftPanelRoot.SetActive(value: false);
		_shipSchematicsList.ShipHullSchematicSelected -= HandleShipHullSchematicSelected;
		_shipSchematicsList.ShipBlueprintSelected -= HandleShipBlueprintSelected;
		PlayerShipBlueprintInteractionBehaviour playerShipBlueprintInteractionBehaviour = LocalPlayer.Instance.PlayerShipBlueprintInteractionBehaviour;
		playerShipBlueprintInteractionBehaviour.ShipBlueprintListModel.Remove(HandleBlueprintsUpdated);
		playerShipBlueprintInteractionBehaviour.ShipBlueprintCraftingBehaviourModelWriter.Model.Remove(HandleShipBlueprintCraftingBehaviourChanged);
	}

	public void HandleSaveBlueprintButtonClicked()
	{
		DialogPopupFacade.ShowInputWithTwoButtons("SAVE BLUEPRINT", string.Empty, "Enter Blueprint Name", "OK", "Cancel", HandleBlueprintNamed);
	}

	private void HandleBlueprintNamed(string blueprintName)
	{
		if (!string.IsNullOrEmpty(blueprintName))
		{
			LocalPlayer.Instance.PlayerShipBlueprintInteractionBehaviour.TriggerSaveShipBlueprint(blueprintName);
		}
	}

	private void HandleShipHullSchematicSelected(SchematicData schematicData)
	{
		if (CheckCanSwitchSchematic())
		{
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIButtonEvents.SchematicItemPressed, new SchematicCategoryListItemPressed(schematicData, null));
			LocalPlayer.Instance.PlayerShipBlueprintInteractionBehaviour.TriggerSetBlueprintId(default(Option<string>));
			UpdateSelectedSchematic();
		}
	}

	private void HandleShipBlueprintSelected(string id)
	{
		if (CheckCanSwitchSchematic())
		{
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIButtonEvents.SchematicItemPressed, new SchematicCategoryListItemPressed(null, null));
			SelectNewSchematic();
			UnloadShipDesignFromHullEditor();
			LocalPlayer.Instance.PlayerShipBlueprintInteractionBehaviour.TriggerSetBlueprintId(id);
		}
	}

	private bool CheckCanSwitchSchematic()
	{
		PlayerShipBlueprintInteractionBehaviour playerShipBlueprintInteractionBehaviour = LocalPlayer.Instance.PlayerShipBlueprintInteractionBehaviour;
		if ((base._craftingStationData.HasLoadedSchematic && !base._craftingStationData.AllSlotsAreEmptyRemotely) || (playerShipBlueprintInteractionBehaviour.HasBlueprintLoaded() && playerShipBlueprintInteractionBehaviour.HasAnyMaterialsLoaded()))
		{
			OSDMessage.SendMessage("This shipyard still has materials in it. Please remove them and try again.", MessageType.ClientError);
			return false;
		}
		if (_currentState == ShipCraftingUIState.ShipDocked)
		{
			OSDMessage.SendMessage("This shipyard has a ship docked with it. Please undock the ship and try again.", MessageType.ClientError);
			return false;
		}
		return true;
	}

	private void HandleBlueprintsUpdated(Option<ShipBlueprintList> oldData, Option<ShipBlueprintList> newData)
	{
		UpdateSchematicsList();
	}

	private void HandleShipBlueprintSchematicsChanged(List<ShipBlueprintSchematic> oldData, List<ShipBlueprintSchematic> newData)
	{
		_shipBlueprintSchematics = newData;
		CheckState();
	}

	private void HandleHullSchematicsUpdated(object[] obj)
	{
		UpdateSchematicsList();
	}

	private void UpdateSelectedSchematic()
	{
		if (base._craftingStationData != null && base._craftingStationData.LoadedSchematic != null)
		{
			_shipSchematicsList.SelectShipSchematicSlot(base._craftingStationData.LoadedSchematic.title);
			return;
		}
		PlayerShipBlueprintInteractionBehaviour playerShipBlueprintInteractionBehaviour = LocalPlayer.Instance.PlayerShipBlueprintInteractionBehaviour;
		ShipBlueprintCraftingBehaviour data = playerShipBlueprintInteractionBehaviour.ShipBlueprintCraftingBehaviourModelWriter.Model.Data;
		if (data != null)
		{
			Option<string> data2 = data.SelectedBlueprintNameModel.Data;
			_shipSchematicsList.SelectShipSchematicSlot(data2);
		}
	}

	private void UpdateSchematicsList()
	{
		PlayerShipBlueprintInteractionBehaviour playerShipBlueprintInteractionBehaviour = LocalPlayer.Instance.PlayerShipBlueprintInteractionBehaviour;
		_shipSchematicsList.SetSchematics(_schematicSystem.Value.GetShipSchematics(), playerShipBlueprintInteractionBehaviour.ShipBlueprintListModel.Data);
		CheckState();
		UpdateSelectedSchematic();
	}

	private void OnShipDockChanged(object[] obj)
	{
		CheckState();
	}

	public override void SetCraftingDataTemplate(CraftingStationData craftingStationData)
	{
		hullEditorUI.SetDependencies(craftingStationData);
		_lockCodeUI.Setup(craftingStationData.CraftingEntityId);
		base.SetCraftingDataTemplate(craftingStationData);
	}

	public override void CheckIfSchematicLoaded()
	{
		hullEditorUI.CheckIfShipLoaded();
		CheckState();
		if (base._craftingStationData.HasLoadedSchematic)
		{
			LoadSchematicFromCraftingData();
			hullEditorUI.Show(show: true);
		}
	}

	public override void SelectNewSchematic()
	{
		ReturnAnySlottedMaterials();
		LoadSchematicFromCraftingData();
		SetupShipHullEditor();
	}

	public override void UpdateLoadedSchematic()
	{
	}

	public override void LoadSchematicFromCraftingData()
	{
		SetForCrafting(base._craftingStationData.CurrentSchematicCanBeCraftedHere);
		SetupSlots();
		CheckIcon();
		SetWeight();
		SetName();
		CheckCraftButtonState();
		UpdateCraftProgressBarsAndTimers();
		CheckState();
		CheckSoundState();
	}

	protected override void SetForCrafting(bool craft)
	{
		_craftingMaterialsRoot.gameObject.SetActive(craft);
		_currentSlots = craftingOnlySlots;
	}

	protected override void CheckIcon()
	{
		if (base._craftingStationData.CraftingSlotData != null && base._craftingStationData.CraftingSlotData.Count != 0)
		{
			string iconName = "shipyard_placeholder_icon";
			Sprite iconSprite = InventoryIconManager.Instance.GetIconSprite(iconName);
			_mainSchematicIcon.enabled = true;
			_mainSchematicIcon.sprite = iconSprite;
		}
	}

	private void SetupShipHullEditor()
	{
		hullEditorUI.Show(show: true);
		hullEditorUI.TryLoad(base._craftingStationData.LoadedSchematic);
	}

	private void SetShipCraftingUIState(ShipCraftingUIState newState)
	{
		_currentState = newState;
		switch (newState)
		{
		case ShipCraftingUIState.NoSchematic:
			_noSchematicSelectedRoot.SetActive(value: true);
			_schematicSelectedRoot.SetActive(value: false);
			_shipDiagnostics.gameObject.SetActive(value: false);
			_shipBlueprintCraftingRoot.gameObject.SetActive(value: false);
			break;
		case ShipCraftingUIState.ShipDocked:
			_noSchematicSelectedRoot.SetActive(value: false);
			_schematicSelectedRoot.SetActive(value: false);
			_shipDiagnostics.gameObject.SetActive(value: true);
			_shipBlueprintCraftingRoot.gameObject.SetActive(value: false);
			break;
		case ShipCraftingUIState.ShipCrafting:
			_noSchematicSelectedRoot.SetActive(value: false);
			_schematicSelectedRoot.SetActive(value: true);
			_shipDiagnostics.gameObject.SetActive(value: false);
			_shipBlueprintCraftingRoot.gameObject.SetActive(value: false);
			break;
		case ShipCraftingUIState.SchematicLoaded:
			_noSchematicSelectedRoot.SetActive(value: false);
			_schematicSelectedRoot.SetActive(value: true);
			_shipDiagnostics.gameObject.SetActive(value: false);
			_shipBlueprintCraftingRoot.gameObject.SetActive(value: false);
			break;
		case ShipCraftingUIState.ShipBlueprintLoaded:
			_noSchematicSelectedRoot.SetActive(value: false);
			_schematicSelectedRoot.SetActive(value: false);
			_shipDiagnostics.gameObject.SetActive(value: false);
			_shipBlueprintCraftingRoot.gameObject.SetActive(value: true);
			break;
		}
		CurrentState = newState;
	}

	private void OnCloseShipHullEditor(object[] obj)
	{
		hullEditorUI.Leave();
	}

	protected override void CraftingStarted()
	{
		CheckCraftButtonState();
		UpdateSlotStates();
		PlayStartCraftingSound();
		hullEditorUI.DisableButtons();
	}

	protected override void CraftingFinished()
	{
		UpdateCraftProgressBarsAndTimers();
		SetupSlots();
		CheckCraftButtonState();
		PlayStopCraftingSound();
		hullEditorUI.ForceUnload();
		CheckState();
	}

	public void LoadShipDesignIntoHullEditor()
	{
		hullEditorUI.UnloadAndLoad(base._craftingStationData.LoadedSchematic);
	}

	public void UnloadShipDesignFromHullEditor()
	{
		if (CheckCanSwitchSchematic())
		{
			hullEditorUI.Unload(delegate
			{
				ReturnAnySlottedMaterials();
				UpdateSelectedSchematic();
				CheckState();
			});
		}
	}

	public void UnloadShipBlueprint()
	{
		if (CheckCanSwitchSchematic())
		{
			LocalPlayer.Instance.PlayerShipBlueprintInteractionBehaviour.TriggerSetBlueprintId(default(Option<string>));
		}
	}

	public void SaveShipDesignFromHullEditor()
	{
		hullEditorUI.Save();
	}

	public void ResetShipDesignInHullEditor()
	{
		hullEditorUI.Reset();
	}

	public void CopyShipDesignInHullEditor()
	{
		hullEditorUI.Copy();
	}

	public void EditShipDesignInHullEditor()
	{
		ReturnAnySlottedMaterials();
		hullEditorUI.Edit();
	}

	public void CheckState()
	{
		if (base._craftingStationData != null)
		{
			bool flag = base._craftingStationData.ShipyardVisualizer.DockedShip != null;
			if (base._craftingStationData.CraftingInProgress)
			{
				SetShipCraftingUIState(ShipCraftingUIState.ShipCrafting);
			}
			else if (flag)
			{
				ShipInfoRefreshRequired();
				SetShipCraftingUIState(ShipCraftingUIState.ShipDocked);
			}
			else if (_shipBlueprintSchematics != null && _shipBlueprintSchematics.Count > 0)
			{
				SetShipCraftingUIState(ShipCraftingUIState.ShipBlueprintLoaded);
			}
			else if (base._craftingStationData.HasLoadedSchematic)
			{
				SetShipCraftingUIState(ShipCraftingUIState.SchematicLoaded);
			}
			else
			{
				SetShipCraftingUIState(ShipCraftingUIState.NoSchematic);
			}
			hullEditorUI.DockedShip(flag);
		}
	}

	private void CheckSoundState()
	{
		if (base._craftingStationData.CraftingInProgress)
		{
			PlayStartCraftingSound();
		}
	}

	public void ShipInfoRefreshRequired()
	{
		ShipPartShipyardInformationVisualizer.UpdateUI(_shipDiagnostics);
	}

	protected override void UpdateSchematicListState()
	{
		CheckState();
	}

	protected override void PlayStartCraftingSound()
	{
		if (base._craftingStationData.HasLoadedSchematic)
		{
			SoundScreen.PlayASound("Play_Build_Shipyard");
		}
	}

	protected override void PlayStopCraftingSound()
	{
		if (base._craftingStationData.HasLoadedSchematic)
		{
			SoundScreen.PlayASound("Stop_Build_Shipyard");
		}
	}

	public override void LowLightAttributes()
	{
	}

	public override void HighlightAttributes(string text)
	{
	}
}
