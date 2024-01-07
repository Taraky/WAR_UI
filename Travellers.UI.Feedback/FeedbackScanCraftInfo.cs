using System;
using System.Collections;
using Bossa.Travellers.Craftingstation;
using Bossa.Travellers.Materials;
using Bossa.Travellers.Utils.ErrorHandling;
using BossalyticsNS;
using Improbable.Collections;
using Newtonsoft.Json;
using Travellers.UI.PlayerInventory;
using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.Feedback;

public class FeedbackScanCraftInfo : FeedbackScanBaseInfo
{
	[SerializeField]
	private CraftingMaterialSlot[] _craftingMaterialSlots;

	[SerializeField]
	private RawImage _schematicImage;

	[SerializeField]
	private TextStyler _schematicAmount;

	[SerializeField]
	private Text _schematicWeight;

	[SerializeField]
	private Text _schematicsCoolingFactor;

	private Material _coloredIconMaterial;

	private AspectRatioFitter _iconAspectRatioFitter;

	public override bool Setup(ScannableData data)
	{
		bool flag = CheckCrafting(data);
		if (flag)
		{
			CheckIcon(data);
		}
		return flag;
	}

	private bool CheckCrafting(ScannableData data)
	{
		if (data.components != null && data.components.Length > 0 && (!string.IsNullOrEmpty(data.description) || !string.IsNullOrEmpty(data.Title) || !string.IsNullOrEmpty(data.titleBis)))
		{
			base.gameObject.SetActive(value: true);
			for (int i = 0; i < _craftingMaterialSlots.Length; i++)
			{
				if (i < data.components.Length)
				{
					_craftingMaterialSlots[i].SetNewScannableComponentData(data.components[i]);
				}
				else
				{
					_craftingMaterialSlots[i].ResetSlot();
				}
			}
			if (data.weight > 0f)
			{
				_schematicWeight.gameObject.SetActive(value: true);
				_schematicWeight.text = $"{Mathf.Round(data.weight)}kg";
			}
			else
			{
				_schematicWeight.gameObject.SetActive(value: false);
			}
			if (data.conductivityFinalStat > 0f)
			{
				_schematicsCoolingFactor.gameObject.SetActive(value: true);
				_schematicsCoolingFactor.text = $"CF: {Mathf.RoundToInt(data.conductivityFinalStat * 100f)}";
			}
			else
			{
				_schematicsCoolingFactor.gameObject.SetActive(value: false);
			}
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
		return base.gameObject.activeSelf;
	}

	private void CheckIcon(ScannableData data)
	{
		if (_schematicImage == null)
		{
			return;
		}
		_schematicAmount.SetText($"x{data.amountToCraft}");
		_schematicAmount.gameObject.SetActive(data.amountToCraft > 0);
		string text = string.Empty;
		if (!string.IsNullOrEmpty(data.iconId))
		{
			text = data.iconId;
		}
		if (data.Title == "Unnamed Part")
		{
			text = data.components[0].iconId;
		}
		if (!string.IsNullOrEmpty(text))
		{
			Texture iconTexture = InventoryIconManager.Instance.GetIconTexture(text);
			float aspectRatio = (float)iconTexture.width / (float)iconTexture.height;
			_schematicImage.enabled = true;
			_schematicImage.texture = iconTexture;
			if (ProcColoredIconHelper.ShouldUseProcColoredShader(data.inventoryItemData))
			{
				ProcColoredIconHelper.SetProcColoredIconWithoutColor(ref _coloredIconMaterial);
				ProcColoredIconHelper.SetAspectRatio(_schematicImage.gameObject, ref _iconAspectRatioFitter, data.inventoryItemData);
				_schematicImage.material = _coloredIconMaterial;
			}
			else
			{
				_schematicImage.GetComponent<AspectRatioFitter>().aspectRatio = aspectRatio;
				_schematicImage.material = null;
			}
		}
		else if (!string.IsNullOrEmpty(data.Title) && !string.IsNullOrEmpty(data.blueprintId) && data.components != null)
		{
			_schematicImage.material = null;
			List<SlottedMaterial> list = new List<SlottedMaterial>();
			for (int i = 0; i < data.components.Length; i++)
			{
				list.Add(new SlottedMaterial(i, new RawMaterial(data.components[i].materialId, (int)data.components[i].quality, data.components[i].materialType, new Map<string, string>()), data.components[i].quantity, null));
			}
			ScannableData.ProcShipPartBluePrint procShipPartBluePrint = JsonConvert.DeserializeObject<ScannableData.ProcShipPartBluePrint>(data.blueprintId);
			if (procShipPartBluePrint != null)
			{
				try
				{
					IDictionary dictionary = (IDictionary)MiniJSON.jsonDecode(procShipPartBluePrint.prefabId);
					string identifier = (string)dictionary["prefabName"];
					IDictionary dictionary2 = (IDictionary)dictionary["prefabModules"];
					Map<string, string> map = new Map<string, string>();
					foreach (string key in dictionary2.Keys)
					{
						map.Add(key, (string)dictionary2[key]);
					}
					if (!ProcIconRenderer.SetProcIcon(identifier, isUpdate: false, map, list, _schematicImage))
					{
						ReleaseError.Raise<ScanningToolUI>(() => "Failed showing procedual ship part scannong tool for " + data.Title + " with blurprintid " + data.blueprintId);
					}
				}
				catch (Exception ex)
				{
					Exception ex2 = ex;
					Exception e = ex2;
					ReleaseError.Raise<ScanningToolUI>(() => "Error deserializing prefab modules from blueprintId: " + data.blueprintId + "\n" + e.Message);
				}
			}
			_schematicImage.GetComponent<AspectRatioFitter>().aspectRatio = 1f;
		}
		else
		{
			Texture iconTexture2 = InventoryIconManager.Instance.GetIconTexture("item_schematics");
			float aspectRatio2 = (float)iconTexture2.width / (float)iconTexture2.height;
			_schematicImage.material = null;
			_schematicImage.enabled = true;
			_schematicImage.texture = iconTexture2;
			_schematicImage.GetComponent<AspectRatioFitter>().aspectRatio = aspectRatio2;
		}
	}
}
