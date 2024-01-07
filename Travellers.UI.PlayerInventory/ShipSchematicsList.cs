using System;
using System.Collections.Generic;
using Bossa.Travellers.Craftingstation;
using Bossa.Travellers.World;
using Improbable.Collections;
using Travellers.UI.Framework;
using Travellers.UI.HUDMessaging;
using Travellers.UI.InfoPopups;
using Travellers.UI.Utility;
using UnityEngine;

namespace Travellers.UI.PlayerInventory;

public class ShipSchematicsList : UIScreenComponent
{
	[SerializeField]
	private RectTransform _slotsParent;

	[SerializeField]
	private RectTransform _hullsCategoryTransform;

	[SerializeField]
	private RectTransform _blueprintsCategoryTransform;

	[SerializeField]
	private ShipSchematicSlot _shipSchematicSlotPrefab;

	private readonly Dictionary<string, ShipSchematicSlot> _hullShipSchematicSlots = new Dictionary<string, ShipSchematicSlot>();

	private readonly Dictionary<string, ShipSchematicSlot> _blueprintSchematicSlots = new Dictionary<string, ShipSchematicSlot>();

	private ShipSchematicSlot _currentlySelectedSchematicSlot;

	private LoadingInputBlocker _loadingInputBlocker;

	public event Action<SchematicData> ShipHullSchematicSelected;

	public event Action<string> ShipBlueprintSelected;

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
		_loadingInputBlocker = UIObjectFactory.Create<LoadingInputBlocker>(UIElementType.ScreenComponent, UIFillType.FillParentTransform, base.transform, isObjectActive: false);
		LocalPlayer.Instance.PlayerShipBlueprintInteractionBehaviour.BusyModel.AddAndInvoke(HandleBusyUpdated);
	}

	private void HandleBusyUpdated(bool oldData, bool newData)
	{
		_loadingInputBlocker.SetObjectActive(newData);
	}

	protected override void ProtectedDispose()
	{
		if (LocalPlayer.Exists)
		{
			LocalPlayer.Instance.PlayerShipBlueprintInteractionBehaviour.BusyModel.Remove(HandleBusyUpdated);
		}
	}

	public void SetSchematics(System.Collections.Generic.List<SchematicData> hullSchematics, Option<ShipBlueprintList> shipBlueprintList)
	{
		Clear();
		int num = 0;
		_hullsCategoryTransform.SetSiblingIndex(num++);
		for (int i = 0; i < hullSchematics.Count; i++)
		{
			SchematicData hullSchematicData = hullSchematics[i];
			ShipSchematicSlot shipSchematicSlot = CreateShipSchematicSlot();
			shipSchematicSlot.transform.SetSiblingIndex(num++);
			shipSchematicSlot.Setup(hullSchematicData.title, delegate
			{
				this.ShipHullSchematicSelected.SafeInvoke(hullSchematicData);
			}, delegate
			{
				RenameSchematic(hullSchematicData);
			}, null);
			_hullShipSchematicSlots.Add(hullSchematicData.title, shipSchematicSlot);
		}
		_blueprintsCategoryTransform.SetSiblingIndex(num++);
		if (!shipBlueprintList.HasValue)
		{
			return;
		}
		Improbable.Collections.List<string> availableBlueprints = shipBlueprintList.Value.availableBlueprints;
		for (int j = 0; j < availableBlueprints.Count; j++)
		{
			string blueprintName = availableBlueprints[j];
			ShipSchematicSlot shipSchematicSlot2 = CreateShipSchematicSlot();
			shipSchematicSlot2.transform.SetSiblingIndex(num++);
			shipSchematicSlot2.Setup(blueprintName, delegate
			{
				this.ShipBlueprintSelected.SafeInvoke(blueprintName);
			}, delegate
			{
				RenameBlueprint(blueprintName);
			}, delegate
			{
				DeleteBlueprint(blueprintName);
			});
			_blueprintSchematicSlots.Add(blueprintName, shipSchematicSlot2);
		}
	}

	private ShipSchematicSlot CreateShipSchematicSlot()
	{
		return UIObjectFactory.Create<ShipSchematicSlot>(UIElementType.ScreenComponent, UIFillType.KeepOriginalAnchoring, _slotsParent, isObjectActive: true);
	}

	public void SelectShipSchematicSlot(Option<string> id)
	{
		if (_currentlySelectedSchematicSlot != null)
		{
			_currentlySelectedSchematicSlot.SetSelected(selected: false);
			_currentlySelectedSchematicSlot = null;
		}
		if (id.HasValue && (_hullShipSchematicSlots.TryGetValue(id.Value, out var value) || _blueprintSchematicSlots.TryGetValue(id.Value, out value)))
		{
			_currentlySelectedSchematicSlot = value;
			_currentlySelectedSchematicSlot.SetSelected(selected: true);
		}
	}

	private void Clear()
	{
		foreach (KeyValuePair<string, ShipSchematicSlot> hullShipSchematicSlot in _hullShipSchematicSlots)
		{
			UnityEngine.Object.Destroy(hullShipSchematicSlot.Value.gameObject);
		}
		foreach (KeyValuePair<string, ShipSchematicSlot> blueprintSchematicSlot in _blueprintSchematicSlots)
		{
			UnityEngine.Object.Destroy(blueprintSchematicSlot.Value.gameObject);
		}
		_hullShipSchematicSlots.Clear();
		_blueprintSchematicSlots.Clear();
		_currentlySelectedSchematicSlot = null;
	}

	private void RenameSchematic(SchematicData schematic)
	{
		DialogPopupFacade.ShowInputWithTwoButtons("EDIT SHIP NAME", string.Empty, schematic.title, "OK", "CANCEL", delegate(string newName)
		{
			if (!newName.IsNullOrEmptyOrOnlySpaces())
			{
				LocalPlayer.Instance.shipHullAgentVisualizer.RenameSchematic(schematic.UniqueID, newName, delegate(bool success)
				{
					if (success)
					{
						Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInventoryEvents.SchematicsUpdated, null);
					}
				});
			}
		});
	}

	private void RenameBlueprint(string blueprintId)
	{
		DialogPopupFacade.ShowInputWithTwoButtons("EDIT SHIP NAME", string.Empty, blueprintId, "OK", "CANCEL", delegate(string newName)
		{
			if (newName.IsNullOrEmptyOrOnlySpaces())
			{
				OSDMessage.SendMessage("Ship blueprints must have non empty names.", MessageType.ClientError);
			}
			else
			{
				LocalPlayer.Instance.PlayerShipBlueprintInteractionBehaviour.TriggerRenameBlueprint(blueprintId, newName);
			}
		});
	}

	private void DeleteBlueprint(string blueprintId)
	{
		DialogPopupFacade.ShowInputWithTwoButtons("DELETE SHIP?", "Type DELETE to confirm!", blueprintId, "DELETE!", "CANCEL", delegate(string input)
		{
			if (input != "DELETE")
			{
				OSDMessage.SendMessage("You must type DELETE in uppercase to confirm ship blueprint deletion.", MessageType.ClientError);
			}
			else
			{
				LocalPlayer.Instance.PlayerShipBlueprintInteractionBehaviour.TriggerDeleteBlueprint(blueprintId);
			}
		});
	}
}
