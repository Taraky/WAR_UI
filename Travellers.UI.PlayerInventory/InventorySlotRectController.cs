using Travellers.UI.Framework;
using UnityEngine;

namespace Travellers.UI.PlayerInventory;

public class InventorySlotRectController : UIScreenComponent
{
	[SerializeField]
	private RectTransform _mainRect;

	public Vector2 ServerAdjustedCoords;

	private bool _isRotated;

	private Vector2 _currentItemCoords;

	private Vector2 _lastNonBlockedGridCoords;

	private float _individualSlotSize;

	private Vector2 _originalItemDimensions;

	private Vector2 _rotationAdjustedItemDimensions;

	private Vector2 _scaledItemDimensions;

	private Vector2 _parentGridDimensions;

	private RectTransform _parentRectTransform;

	private Vector2 _scaledParentGridDimensions;

	private InventorySlotData _inventorySlotData;

	private InventorySpaceChecker _blockerArray;

	public bool IsRotated
	{
		get
		{
			return _isRotated;
		}
		private set
		{
			if (_isRotated != value)
			{
				_isRotated = value;
				Resize();
			}
		}
	}

	protected override void AddListeners()
	{
	}

	protected override void ProtectedDispose()
	{
	}

	protected override void ProtectedInit()
	{
	}

	public void SetRectTransform(RectTransform rectTransform)
	{
		_mainRect = rectTransform;
		IsRotated = false;
	}

	public void SetupAsInventoryItem(InventorySlotData currentSelectedSlotData, RectTransform boundaryRectTransform, float slotSize)
	{
		Setup(currentSelectedSlotData, boundaryRectTransform, slotSize);
	}

	public void SetupAsPlaceholder(InventorySlotData currentSelectedSlotData, RectTransform boundaryRectTransform, float slotSize, InventorySpaceChecker blockerArray)
	{
		_blockerArray = blockerArray;
		Setup(currentSelectedSlotData, boundaryRectTransform, slotSize);
	}

	private void Setup(InventorySlotData currentSelectedSlotData, RectTransform boundaryRectTransform, float slotSize)
	{
		_individualSlotSize = slotSize;
		if (_individualSlotSize == 0f)
		{
		}
		_inventorySlotData = currentSelectedSlotData;
		_originalItemDimensions = new Vector2(currentSelectedSlotData.InventoryItemData.numOfSlotsWidth, _inventorySlotData.InventoryItemData.numOfSlotsHeight);
		_parentRectTransform = boundaryRectTransform;
		_scaledParentGridDimensions = boundaryRectTransform.sizeDelta;
		_parentGridDimensions = new Vector2(Mathf.FloorToInt(_scaledParentGridDimensions.x / _individualSlotSize), (float)Mathf.FloorToInt(_scaledParentGridDimensions.y / _individualSlotSize) * -1f);
		float x = Mathf.FloorToInt((float)_inventorySlotData.xPosition * _individualSlotSize);
		float y = (float)Mathf.FloorToInt((float)_inventorySlotData.yPosition * _individualSlotSize) * -1f;
		_currentItemCoords = new Vector2(x, y);
		_lastNonBlockedGridCoords = new Vector2(_inventorySlotData.xPosition, (float)_inventorySlotData.yPosition * -1f);
		IsRotated = _inventorySlotData.rotated;
		Resize();
		SetPosition();
	}

	public void TryRotateItem()
	{
		if (_originalItemDimensions.x != _originalItemDimensions.y)
		{
			_inventorySlotData.rotated = !_inventorySlotData.rotated;
			ApplyDataRotation();
			Vector2 gridCoordsFromMousePos = GetGridCoordsFromMousePos();
			Vector2 newAdjustedGridCoords = CheckIfCoordsAreInBounds(gridCoordsFromMousePos);
			if (_blockerArray.IsItemBlocked(_inventorySlotData, newAdjustedGridCoords))
			{
				_inventorySlotData.rotated = !_inventorySlotData.rotated;
				ApplyDataRotation();
			}
		}
	}

	public void ApplyDataRotation()
	{
		IsRotated = _inventorySlotData.rotated;
	}

	private void Resize()
	{
		_rotationAdjustedItemDimensions = ((!IsRotated) ? _originalItemDimensions : _originalItemDimensions.Transpose());
		_scaledItemDimensions = _rotationAdjustedItemDimensions * _individualSlotSize;
		_mainRect.sizeDelta = _scaledItemDimensions;
	}

	public void FollowMouseUsingInventoryGrid()
	{
		Vector2 gridCoordsFromMousePos = GetGridCoordsFromMousePos();
		Vector2 vector = (_lastNonBlockedGridCoords = CheckNewGridCoordsAreValid(gridCoordsFromMousePos));
		float x = vector.x * _individualSlotSize;
		float y = vector.y * _individualSlotSize;
		_currentItemCoords = new Vector2(x, y);
		SetPosition();
	}

	public void FollowMouseFreely(RectTransform parentRectTransform)
	{
		Vector2 screenAdjustedMousePos = GetScreenAdjustedMousePos(parentRectTransform);
		float num = _individualSlotSize * (_rotationAdjustedItemDimensions.x * 0.5f);
		float num2 = _individualSlotSize * (_rotationAdjustedItemDimensions.y * 0.5f);
		_currentItemCoords = new Vector2(screenAdjustedMousePos.x - num, screenAdjustedMousePos.y + num2);
		SetPosition();
	}

	private Vector2 GetGridCoordsFromMousePos()
	{
		Vector2 screenAdjustedMousePos = GetScreenAdjustedMousePos(_parentRectTransform);
		Vector2 vector = new Vector2(_rotationAdjustedItemDimensions.x * 0.5f * _individualSlotSize * -1f, _rotationAdjustedItemDimensions.y * 0.5f * _individualSlotSize);
		float x = Mathf.RoundToInt((screenAdjustedMousePos.x + vector.x) / _individualSlotSize);
		float y = Mathf.RoundToInt((screenAdjustedMousePos.y + vector.y) / _individualSlotSize);
		return new Vector2(x, y);
	}

	private Vector2 GetScreenAdjustedMousePos(RectTransform parentRectTransform)
	{
		Vector3[] array = new Vector3[4];
		parentRectTransform.GetWorldCorners(array);
		Vector2 vector = new Vector2(Input.mousePosition.x - array[1].x, Input.mousePosition.y - array[1].y);
		return vector / UIStructure.RootCanvas.scaleFactor;
	}

	private Vector2 CheckNewGridCoordsAreValid(Vector2 newGridCoords)
	{
		Vector2 vector = CheckIfCoordsAreInBounds(newGridCoords);
		if (_blockerArray.IsItemBlocked(_inventorySlotData, vector))
		{
			return _lastNonBlockedGridCoords;
		}
		return vector;
	}

	private Vector2 CheckIfCoordsAreInBounds(Vector2 newGridCoords)
	{
		float x = newGridCoords.x;
		float y = newGridCoords.y;
		if (newGridCoords.y > 0f)
		{
			y = 0f;
		}
		if (newGridCoords.x < 0f)
		{
			x = 0f;
		}
		if (newGridCoords.x + _rotationAdjustedItemDimensions.x > _parentGridDimensions.x)
		{
			x = _parentGridDimensions.x - _rotationAdjustedItemDimensions.x;
		}
		if (newGridCoords.y - _rotationAdjustedItemDimensions.y <= _parentGridDimensions.y)
		{
			y = _parentGridDimensions.y + _rotationAdjustedItemDimensions.y;
		}
		return new Vector2(x, y);
	}

	private void SetPosition()
	{
		_mainRect.anchoredPosition = _currentItemCoords;
		ServerAdjustedCoords = new Vector2(_currentItemCoords.x / _individualSlotSize, Mathf.Abs(_currentItemCoords.y / _individualSlotSize));
	}
}
