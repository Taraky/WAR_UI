using System;
using Improbable;
using Travellers.UI.Events;
using Travellers.UI.Framework;
using Travellers.UI.HUDMessaging;
using Travellers.UI.InfoPopups;
using Travellers.UI.Tutorial;
using UnityEngine.UI;
using WAUtilities.Logging;

namespace Travellers.UI.PlayerInventory;

public class ShipCraftingUIHelper : UIMonoLifetimeController
{
	private CraftingStationData _craftingData;

	public Button saveButton;

	public Button resetButton;

	public Button editButton;

	public Text editText;

	private int _slotIndex;

	private int _previousSlotIndex;

	private SchematicData _lastSchematicDataObject;

	private EntityId _shipyardId;

	private ShipHullEditorVisualizer _shipHullEditorVisualiser;

	private bool _isCancelPressed;

	public bool IsModified
	{
		get
		{
			if (_shipHullEditorVisualiser == null)
			{
				return false;
			}
			return _shipHullEditorVisualiser.IsModified();
		}
	}

	protected override void Activate()
	{
		_isCancelPressed = false;
		if (_shipHullEditorVisualiser != null)
		{
			UpdateButtons();
		}
	}

	protected override void Deactivate()
	{
		_isCancelPressed = false;
	}

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUIInGameEvents.ShipHullEditorSchematicModified, OnShipSchematicModified);
	}

	private void OnShipSchematicModified(object[] obj)
	{
		UpdateButtons();
	}

	protected override void ProtectedInit()
	{
	}

	protected override void ProtectedDispose()
	{
	}

	public void SetDependencies(CraftingStationData craftingData)
	{
		_craftingData = craftingData;
		if (craftingData == null)
		{
			WALogger.Error<ShipCraftingUIHelper>("Crafting data is null");
		}
		if (craftingData.ShipHullEditorVisualiser == null)
		{
			WALogger.Error<ShipCraftingUIHelper>("Ship hull editor visualiser is null");
		}
		_shipHullEditorVisualiser = craftingData.ShipHullEditorVisualiser;
		_shipyardId = _craftingData.CraftingEntityId;
	}

	public void CheckIfShipLoaded()
	{
		if (_shipHullEditorVisualiser.HasShipLoaded())
		{
			SetSchematic(_shipHullEditorVisualiser.GetSlotId());
			_craftingData.LoadSchematic(_shipHullEditorVisualiser.GetSchematic(), CharacterData.Instance.userName);
			_craftingData.SetCurrentHullModifiedStatus(_shipHullEditorVisualiser.IsModified());
		}
		else
		{
			_craftingData.LoadSchematic(null, CharacterData.Instance.userName);
			_craftingData.SetCurrentHullModifiedStatus(isModified: false);
		}
	}

	public void TryLoad(SchematicData schematicData)
	{
		if (_shipHullEditorVisualiser.GetSchematic() != null)
		{
			Unload();
		}
		if (schematicData != null)
		{
			int schematicSlotIndex = LocalPlayer.Instance.shipHullAgentVisualizer.GetSchematicSlotIndex(schematicData.UniqueID);
			SetSchematic(schematicSlotIndex);
			Load();
		}
	}

	public void Load()
	{
		if (_slotIndex != -1)
		{
			DisableButtons();
			LocalPlayer.Instance.shipHullAgentVisualizer.LoadSchematic(_shipHullEditorVisualiser, _slotIndex, DoUpdateSchematic);
		}
	}

	public void Unload(Action returnResourceFromSlotsIfSuccess = null)
	{
		if (!_isCancelPressed)
		{
			if (_shipHullEditorVisualiser.IsModified())
			{
				DialogPopupFacade.ShowConfirmationDialog(string.Empty, "This schematic is not saved.\nAre you sure you want to unload it?", delegate
				{
					DoUnload(returnResourceFromSlotsIfSuccess);
				}, "CONFIRM", "CANCEL", delegate
				{
					DoSwitchHighlightedSchematic();
				});
			}
			else
			{
				DoUnload(returnResourceFromSlotsIfSuccess);
			}
		}
		else
		{
			_isCancelPressed = false;
		}
	}

	private void DoSwitchHighlightedSchematic()
	{
		SetSchematic(_previousSlotIndex);
		_isCancelPressed = true;
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInventoryEvents.ShipSchematicsCancelAndReloadLastShipSchematic, _lastSchematicDataObject);
	}

	private void DoUnload(Action returnResourceFromSlotsIfSuccess = null)
	{
		DisableButtons();
		LocalPlayer.Instance.shipHullAgentVisualizer.UnloadSchematic(_shipHullEditorVisualiser, delegate(bool success)
		{
			if (success)
			{
				if (returnResourceFromSlotsIfSuccess != null)
				{
					returnResourceFromSlotsIfSuccess();
				}
			}
			else
			{
				OSDMessage.SendMessage("Schematic couldn't be unloaded.");
				UpdateButtons();
			}
			DoUpdateButtons(success);
		});
	}

	public void ForceUnload()
	{
		DisableButtons();
		LocalPlayer.Instance.shipHullAgentVisualizer.UnloadSchematic(_shipHullEditorVisualiser, delegate(bool success)
		{
			DoUpdateButtons(success);
		});
	}

	public void UnloadAndLoad(SchematicData schematicData)
	{
		int slotIndex = LocalPlayer.Instance.shipHullAgentVisualizer.GetSchematicSlotIndex(schematicData.UniqueID);
		DisableButtons();
		LocalPlayer.Instance.shipHullAgentVisualizer.UnloadSchematic(_shipHullEditorVisualiser, delegate(bool success)
		{
			if (success)
			{
				SetSchematic(slotIndex);
				if (_slotIndex != -1)
				{
					DisableButtons();
					LocalPlayer.Instance.shipHullAgentVisualizer.LoadSchematic(_shipHullEditorVisualiser, _slotIndex, DoUpdateSchematic);
				}
			}
			else
			{
				OSDMessage.SendMessage("Schematic couldn't be unloaded.");
				UpdateButtons();
			}
		});
	}

	public void Save()
	{
		DisableButtons();
		LocalPlayer.Instance.shipHullAgentVisualizer.SaveSchematic(_shipHullEditorVisualiser, _slotIndex, delegate(bool success)
		{
			if (success)
			{
				Load();
			}
			else
			{
				OSDMessage.SendMessage("Schematic couldn't be saved.");
			}
		});
		UpdateButtons();
	}

	public void Reset()
	{
		DisableButtons();
		LocalPlayer.Instance.shipHullAgentVisualizer.ResetSchematic(_shipHullEditorVisualiser, _slotIndex, DoUpdateSchematic);
		UpdateButtons();
	}

	public void Copy()
	{
		DisableButtons();
		LocalPlayer.Instance.shipHullAgentVisualizer.SaveSchematic(_shipHullEditorVisualiser, _slotIndex, DoUpdateButtons);
		UpdateButtons();
	}

	public void Edit()
	{
		LocalPlayer.Instance.shipHullAgentVisualizer.OpenShipEditor(_shipyardId);
	}

	public void Leave()
	{
		LocalPlayer.Instance.shipHullAgentVisualizer.LeaveShipEditor(_shipyardId);
		LocalPlayer.Instance.shipHullAgentVisualizer.LoadSchematic(_shipHullEditorVisualiser, _slotIndex, DoUpdateSchematic);
	}

	public void Show(bool show)
	{
		base.gameObject.SetActive(show);
		if (show)
		{
			UpdateButtons();
			return;
		}
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUITutorialEvents.RemoveStepFromSystem, new TutorialRemoveStepFromSystemEvent(TutorialStep.LOAD_SHIP));
	}

	private void DoUpdateSchematic(SchematicData schematic)
	{
		if (schematic != null)
		{
			SetSchematic(_shipHullEditorVisualiser.GetSlotId());
		}
		UpdateButtons();
		_craftingData.SetCurrentHullModifiedStatus(_shipHullEditorVisualiser.IsModified());
		SchematicData schematic2 = _shipHullEditorVisualiser.GetSchematic();
		_craftingData.LoadSchematicWithEvent(schematic2, CharacterData.Instance.userName);
		if (!_shipHullEditorVisualiser.IsModified())
		{
			_lastSchematicDataObject = schematic2;
		}
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInventoryEvents.SchematicsUpdated, null);
	}

	private void DoUpdateButtons(bool success)
	{
		UpdateButtons();
	}

	private void SetSchematic(int newSlotIndex)
	{
		if (_previousSlotIndex != newSlotIndex)
		{
			_previousSlotIndex = _slotIndex;
		}
		_slotIndex = newSlotIndex;
		UpdateButtons();
	}

	public void UpdateButtons()
	{
		if (!(_shipHullEditorVisualiser == null) && _craftingData != null)
		{
			bool flag = _shipHullEditorVisualiser.HasShipLoaded();
			bool flag2 = _shipHullEditorVisualiser.GetOwnerId() == LocalPlayer.Instance.PlayerId;
			bool flag3 = _slotIndex != -1;
			bool flag4 = _shipHullEditorVisualiser.IsModified();
			bool craftingInProgress = _craftingData.CraftingInProgress;
			saveButton.interactable = flag && flag2 && flag3 && flag4 && !craftingInProgress;
			resetButton.interactable = flag && flag2 && flag3 && flag4 && !craftingInProgress;
			editButton.interactable = flag && !craftingInProgress;
			CheckHullEditValidity();
			editText.text = ((!flag4) ? "Click EDIT to customize the shape of your ship." : "Click SAVE to craft the ship.");
			editText.transform.parent.gameObject.SetActive(!_craftingData.CraftingInProgress);
			if (editButton.interactable && !saveButton.interactable)
			{
				Singleton<WAEventSystem>.Instance.TriggerEvent(WAUITutorialEvents.AddStepToSystem, new TutorialAddStepToSystemEvent(TutorialStep.LOAD_SHIP));
			}
			else
			{
				Singleton<WAEventSystem>.Instance.TriggerEvent(WAUITutorialEvents.RemoveStepFromSystem, new TutorialRemoveStepFromSystemEvent(TutorialStep.LOAD_SHIP));
			}
			if (saveButton.interactable)
			{
				Singleton<WAEventSystem>.Instance.TriggerEvent(WAUITutorialEvents.AddStepToSystem, new TutorialAddStepToSystemEvent(TutorialStep.SAVE_SHIP));
			}
			else
			{
				Singleton<WAEventSystem>.Instance.TriggerEvent(WAUITutorialEvents.RemoveStepFromSystem, new TutorialRemoveStepFromSystemEvent(TutorialStep.SAVE_SHIP));
			}
		}
	}

	private void CheckHullEditValidity()
	{
		if (CraftingMaterialSlot.IsModifiedSchematicNotSaved() && !saveButton.interactable && _shipHullEditorVisualiser.GetOwnerId() == LocalPlayer.Instance.PlayerId)
		{
			bool flag = _shipHullEditorVisualiser.HasShipLoaded();
			bool flag2 = _slotIndex != -1;
			bool flag3 = _shipHullEditorVisualiser.IsModified();
			bool craftingInProgress = _craftingData.CraftingInProgress;
			WALogger.Error<ShipCraftingUIHelper>("save schematic error: hasLoaded: {0}, isOwner: true, hasSlotSelected: {1}, isModified: {2}, !isCraftingStarted: {3}, GetOwnerId(): {4}, _slotIndex: {5}", new object[6]
			{
				flag,
				flag2,
				flag3,
				!craftingInProgress,
				_shipHullEditorVisualiser.GetOwnerId(),
				_slotIndex
			});
		}
	}

	public void DockedShip(bool docked)
	{
		if (docked)
		{
			Show(show: false);
		}
	}

	public void DisableButtons()
	{
		saveButton.interactable = false;
		resetButton.interactable = false;
		editButton.interactable = false;
	}
}
