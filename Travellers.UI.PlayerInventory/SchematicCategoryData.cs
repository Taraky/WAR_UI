using System.Collections.Generic;

namespace Travellers.UI.PlayerInventory;

public class SchematicCategoryData
{
	public CraftingCategory CategoryTypeEnum;

	public string SubCategoryId;

	public string SubCategoryDisplayName;

	public Dictionary<string, SchematicCategoryData> ChildSchematicCategories = new Dictionary<string, SchematicCategoryData>();

	public List<SchematicData> ChildItemSchematics = new List<SchematicData>();

	public List<SchematicData> ChildShipSchematics = new List<SchematicData>();

	public string Description;

	public float AmountOwned;

	public float TotalAmount;

	public SchematicCategoryData(CraftingCategory categoryEnum, string description)
	{
		CategoryTypeEnum = categoryEnum;
		SubCategoryId = string.Empty;
		SubCategoryDisplayName = string.Empty;
		Description = description;
	}

	public SchematicCategoryData(CraftingCategory categoryEnum, string subCategory, string subCategoryDisplayName, string description)
	{
		CategoryTypeEnum = categoryEnum;
		SubCategoryId = subCategory;
		SubCategoryDisplayName = subCategoryDisplayName;
		Description = description;
	}

	public void CleanLists()
	{
		ChildSchematicCategories.Clear();
		ChildItemSchematics.Clear();
		ChildShipSchematics.Clear();
	}
}
