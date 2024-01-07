using Travellers.UI.PlayerInventory;

namespace Travellers.UI.Events;

public class SchematicCategoryListItemPressed : UIEvent
{
	public CraftingCategory CategoryTypeEnum;

	public string SubCategoryId;

	public SchematicData Schematic;

	public SchematicSlot SchematicSlot;

	public SchematicCategoryListItemPressed(CraftingCategory categoryTypeEnum)
	{
		CategoryTypeEnum = categoryTypeEnum;
	}

	public SchematicCategoryListItemPressed(CraftingCategory categoryTypeEnum, string subCategoryId)
	{
		CategoryTypeEnum = categoryTypeEnum;
		SubCategoryId = subCategoryId;
	}

	public SchematicCategoryListItemPressed(SchematicData schematic, SchematicSlot slot)
	{
		Schematic = schematic;
		SchematicSlot = slot;
	}
}
