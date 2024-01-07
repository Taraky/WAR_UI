namespace Travellers.UI.Events;

public class LoadSchematicIntoCraftingEvent : UIEvent
{
	public SchematicData SchematicData;

	public LoadSchematicIntoCraftingEvent(SchematicData schematicData)
	{
		SchematicData = schematicData;
	}
}
