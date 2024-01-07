using Travellers.UI.Framework;
using Travellers.UI.Login;
using UnityEngine;

namespace Travellers.UI.PlayerInventory;

[InjectableClass]
public class InventoryCharacterSubscreen : UIScreenComponent
{
	public CharacterSlot headSlot;

	public CharacterSlot bodySlot;

	public CharacterSlot feetSlot;

	public CharacterSlot utilityHeadSlot;

	public CharacterSlot utilityBodySlot;

	public CharacterSlot utilityFeetSlot;

	public CharacterSlot petSlot;

	public CharacterToolSlot grappleToolSlot;

	public CharacterToolSlot salvagerToolSlot;

	public CharacterToolSlot repairToolSlot;

	public CharacterToolSlot buildToolSlot;

	public CharacterToolSlot scannerToolSlot;

	public CanvasGroup characterSlotsCanvasGroup;

	[SerializeField]
	public TextStylerTextMeshPro _playerName;

	private LobbySystem _lobbySystem;

	[InjectableMethod]
	public void InjectDependencies(LobbySystem lobbySystem)
	{
		_lobbySystem = lobbySystem;
	}

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUIInventoryEvents.UpdateClothingSlots, OnRefreshCharacterSlots);
		_eventList.AddEvent(WAUIPlayerProfileEvents.ToolUnlocked, OnUnlockedToolsChanged);
	}

	protected override void ProtectedDispose()
	{
	}

	protected override void ProtectedInit()
	{
	}

	protected override void Activate()
	{
		_playerName.SetText(_lobbySystem.CurrentCreationData.Name);
		RefreshToolSlots();
	}

	private void OnRefreshCharacterSlots(object[] obj)
	{
		headSlot.SetInitialData();
		bodySlot.SetInitialData();
		feetSlot.SetInitialData();
		utilityHeadSlot.SetInitialData();
		utilityBodySlot.SetInitialData();
		utilityFeetSlot.SetInitialData();
		petSlot.SetInitialData();
	}

	private void OnUnlockedToolsChanged(object[] obj)
	{
		RefreshToolSlots();
	}

	private void RefreshToolSlots()
	{
		grappleToolSlot.SetInitialData();
		salvagerToolSlot.SetInitialData();
		repairToolSlot.SetInitialData();
		buildToolSlot.SetInitialData();
		scannerToolSlot.SetInitialData();
	}
}
