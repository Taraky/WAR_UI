namespace Travellers.UI.PlayerInventory;

public interface IInventoryActionSource
{
	InventorySlotData InventorySlotSourceData { get; }

	InventoryContents ParentInventoryContents { get; }

	void Use();

	void Learn();

	bool TryEquip();

	void Unequip(bool letUserPositionInInventory);

	void Split();

	void Destroy();
}
