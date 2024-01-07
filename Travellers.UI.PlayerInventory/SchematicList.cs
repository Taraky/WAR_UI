using System.Collections.Generic;
using Travellers.UI.Framework;
using UnityEngine;

namespace Travellers.UI.PlayerInventory;

public abstract class SchematicList : UIScreenComponent
{
	[SerializeField]
	protected Transform _schematicParent;

	[SerializeField]
	protected GameObject _inputBlocker;

	protected List<SchematicData> _allItemSchematicData = new List<SchematicData>();

	protected CraftingState _currentState;

	protected SchematicData _currentSchematic;

	protected CraftingStationData _craftingDataTemplate;

	protected bool _allSlotsAreEmptyLocally = true;

	protected abstract void BuildSchematicButtonHierarchy();

	public abstract void CraftingStarted();

	public abstract void CraftingFinished();

	public abstract void SchematicButtonPressed();

	public virtual void SubcategoryPressed(CraftingCategory categoryTypeEnum, string subCategoryId, bool forceShow = false)
	{
	}

	public virtual SchematicCategorySlot CategoryPressed(CraftingCategory categoryTypeEnum, bool forceShow = false)
	{
		return null;
	}

	public abstract void UpdateCraftingState();

	public void ChangeState(CraftingState currentlyCrafting)
	{
		if (_currentState != currentlyCrafting)
		{
			_currentState = currentlyCrafting;
			ApplyCraftingState();
		}
	}

	protected abstract void ApplyCraftingState();

	public abstract void SetCraftingDataTemplate(CraftingStationData craftingDataTemplate, bool clientOnly = false);

	public abstract void RefreshSchematicDisplay();

	public void OnCraftingSlotsEmptyUpdated(bool allSlotsAreEmptyLocally)
	{
		if (_allSlotsAreEmptyLocally != allSlotsAreEmptyLocally)
		{
			_allSlotsAreEmptyLocally = allSlotsAreEmptyLocally;
			if (!_allSlotsAreEmptyLocally)
			{
				UpdateCraftingState();
			}
		}
	}

	protected virtual bool IsCurrentlyCrafting()
	{
		return !_allSlotsAreEmptyLocally || !_craftingDataTemplate.AllSlotsAreEmptyRemotely || _craftingDataTemplate.CraftingInProgress;
	}
}
