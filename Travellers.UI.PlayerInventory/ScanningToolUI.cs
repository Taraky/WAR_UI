using System;
using System.Collections;
using System.Linq;
using Bossa.Travellers.Craftingstation;
using Bossa.Travellers.Materials;
using Bossa.Travellers.Utils.ErrorHandling;
using BossalyticsNS;
using Improbable.Collections;
using Newtonsoft.Json;
using Travellers.UI.Framework;
using Travellers.UI.Utility;
using UnityEngine;
using UnityEngine.UI;
using WAUtilities.Logging;

namespace Travellers.UI.PlayerInventory;

[InjectableClass]
public class ScanningToolUI : UIPopup
{
	private ScannableData _currentData;

	[SerializeField]
	private RectTransform _uiRoot;

	[SerializeField]
	private Image _rarityImage;

	[SerializeField]
	private LayoutElement _titleContainer;

	[SerializeField]
	private TextStyler _titleStyler;

	[SerializeField]
	private LayoutElement _titleBisContainer;

	[SerializeField]
	private Text _titleBisText;

	[SerializeField]
	private GameObject _descriptionContainer;

	[SerializeField]
	private Text _descriptionText;

	[SerializeField]
	private GameObject _descriptionBisContainer;

	[SerializeField]
	private Text _descriptionBisText;

	[SerializeField]
	private LayoutElement _craftContainer;

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

	[SerializeField]
	private LayoutElement _bonusContainer;

	[SerializeField]
	private GameObject _statsContainer;

	[SerializeField]
	private ItemAttribute[] _statBars;

	[SerializeField]
	private LayoutElement _knowledgeContainer;

	[SerializeField]
	private LayoutElement _weightContainer;

	[SerializeField]
	private Text _weightText;

	[SerializeField]
	private LayoutElement _madebyContainer;

	[SerializeField]
	private Text _madebyText;

	private Vector2 offset = new Vector2(20f, -20f);

	private UIColourAndTextReferenceData _uiRefData;

	private Material _coloredIconMaterial;

	private AspectRatioFitter _iconAspectRatioFitter;

	[InjectableMethod]
	public void InjectDependencies(UIColourAndTextReferenceData refData)
	{
		_uiRefData = refData;
	}

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
	}

	protected override void ProtectedDispose()
	{
	}

	public void Print(ScannableData data)
	{
		_currentData = data;
		if (data == null)
		{
			WALogger.Warn<ScanningToolUI>(LogChannel.UI, "Not showing scan popup because scanning data is null, returning.", new object[0]);
			return;
		}
		data.CheckSchematicOrInventoryItemData();
		_bonusContainer.gameObject.SetActive(value: false);
		_knowledgeContainer.gameObject.SetActive(value: false);
		CheckHeaderAndTitle(data);
		CheckTitleBis(data);
		CheckDescription(data);
		CheckDescriptionBis(data);
		CheckCrafting(data);
		CheckStats(data);
		CheckMadeBy(data);
		CheckWeight(data);
		CheckIcon(data);
		if (!data.FollowMouse)
		{
			_uiRoot.SetPivotToDirection(eAnchorDirection.DownRight);
			_uiRoot.SetMaxAndMinAnchorToDirection(eAnchorDirection.DownRight);
			_uiRoot.anchoredPosition = new Vector2(-20f, 20f);
		}
		else
		{
			_uiRoot.SetPivotToDirection(eAnchorDirection.UpLeft);
			_uiRoot.SetMaxAndMinAnchorToDirection(eAnchorDirection.DownLeft);
		}
		Update();
	}

	private void CheckHeaderAndTitle(ScannableData data)
	{
		SchematicsRarity rarity = (SchematicsRarity)Mathf.Max((int)data.rarity, 0);
		RarityColourSet rarityColoursForButtonStates = RarityHelper.GetRarityColoursForButtonStates(rarity);
		_rarityImage.color = _uiRefData.UIColours.GetColour(rarityColoursForButtonStates.DefaultColourType);
		if (!string.IsNullOrEmpty(data.Title))
		{
			_titleContainer.gameObject.SetActive(value: true);
			_titleStyler.SetRarityColourSet(rarityColoursForButtonStates);
			_titleStyler.SetText(data.Title);
			if (data.Title == "Unnamed Part" && data.components != null && data.components.Length > 0)
			{
				_titleStyler.SetText(data.components[0].componentName);
			}
		}
		else
		{
			_titleContainer.gameObject.SetActive(value: false);
		}
	}

	private void CheckTitleBis(ScannableData data)
	{
		if (!string.IsNullOrEmpty(data.titleBis))
		{
			_titleBisContainer.gameObject.SetActive(value: true);
			_titleBisText.text = data.titleBis;
		}
		else
		{
			_titleBisContainer.gameObject.SetActive(value: false);
		}
	}

	private void CheckDescription(ScannableData data)
	{
		if (!string.IsNullOrEmpty(data.Description) && data.Description != "noDescription")
		{
			_descriptionContainer.gameObject.SetActive(value: true);
			_descriptionText.text = data.Description;
		}
		else
		{
			_descriptionContainer.gameObject.SetActive(value: false);
		}
	}

	private void CheckDescriptionBis(ScannableData data)
	{
		if (!string.IsNullOrEmpty(data.descriptionBis) && data.descriptionBis != "noDescription")
		{
			_descriptionBisContainer.gameObject.SetActive(value: true);
			_descriptionBisText.text = data.descriptionBis;
		}
		else
		{
			_descriptionBisContainer.gameObject.SetActive(value: false);
		}
	}

	private void CheckCrafting(ScannableData data)
	{
		if (data.components != null && data.components.Length > 0 && (!string.IsNullOrEmpty(data.description) || !string.IsNullOrEmpty(data.Title) || !string.IsNullOrEmpty(data.titleBis)))
		{
			_craftContainer.gameObject.SetActive(value: true);
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
			_craftContainer.gameObject.SetActive(value: false);
		}
	}

	private void CheckStats(ScannableData data)
	{
		if (data.OrderedStats != null && data.OrderedStats.Count > 0)
		{
			_statsContainer.SetActive(value: true);
			for (int i = 0; i < _statBars.Length; i++)
			{
				try
				{
					if (i < data.statsBars.Length)
					{
						ScannableData.StatsBar statsBar = data.OrderedStats[i];
						_statBars[i].ShowValues(show: true);
						_statBars[i].SetValue(SchematicData.GetStatTitle(statsBar.statName), statsBar.baseNormalized, statsBar.modifierNormalized, statsBar.maxValue);
					}
					else
					{
						_statBars[i].ShowValues(show: false);
					}
				}
				catch
				{
					WALogger.Error<ScanningToolUI>("OrderedStats: {0}, statsBars: {1}", new object[2]
					{
						string.Join(",", data.OrderedStats.Select((ScannableData.StatsBar bar) => bar.statName).ToArray()),
						string.Join(",", data.statsBars.Select((ScannableData.StatsBar bar) => bar.statName).ToArray())
					});
					_statBars[i].ShowValues(show: false);
				}
			}
		}
		else
		{
			_statsContainer.SetActive(value: false);
		}
	}

	private void CheckWeight(ScannableData data)
	{
		if (data.weight > 0f && (data.components == null || data.components.Length == 0))
		{
			_weightContainer.gameObject.SetActive(value: true);
			_weightText.text = $"{Mathf.Round(data.weight * 10f) / 10f}kg";
		}
		else
		{
			_weightContainer.gameObject.SetActive(value: false);
		}
	}

	private void CheckMadeBy(ScannableData data)
	{
		if (!string.IsNullOrEmpty(data.crafter))
		{
			_madebyContainer.gameObject.SetActive(value: true);
			_madebyText.text = $"Made by: {data.crafter}";
		}
		else
		{
			_madebyContainer.gameObject.SetActive(value: false);
		}
	}

	private void CheckIcon(ScannableData data)
	{
		if (!(_schematicImage != null))
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

	public void Discard()
	{
		_titleContainer.gameObject.SetActive(value: false);
		_titleBisContainer.gameObject.SetActive(value: false);
		_descriptionContainer.gameObject.SetActive(value: false);
		_descriptionBisContainer.gameObject.SetActive(value: false);
		_craftContainer.gameObject.SetActive(value: false);
		_bonusContainer.gameObject.SetActive(value: false);
		_statsContainer.SetActive(value: false);
		_weightContainer.gameObject.SetActive(value: false);
		_madebyContainer.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		if (_currentData != null && _currentData.FollowMouse)
		{
			Vector2 vector = new Vector2(Input.mousePosition.x + 20f, Input.mousePosition.y - 20f) + offset;
			if (vector.x > (float)Screen.width - _uiRoot.rect.width - 20f)
			{
				vector.x = (float)Screen.width - _uiRoot.rect.width - 20f;
			}
			if (vector.y < 20f + _uiRoot.rect.height)
			{
				vector.y = 20f + _uiRoot.rect.height;
			}
			_uiRoot.anchoredPosition = vector / UIStructure.RootCanvas.scaleFactor;
		}
	}
}
