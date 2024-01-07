using System.Collections.Generic;
using UnityEngine;

namespace Travellers.UI.Utility;

public static class RectTransformHelper
{
	private static Dictionary<eAnchorDirection, Vector2> _anchorVectorLookup = new Dictionary<eAnchorDirection, Vector2>
	{
		{
			eAnchorDirection.UpLeft,
			new Vector2(0f, 1f)
		},
		{
			eAnchorDirection.UpCentre,
			new Vector2(0.5f, 1f)
		},
		{
			eAnchorDirection.UpRight,
			new Vector2(1f, 1f)
		},
		{
			eAnchorDirection.MiddleLeft,
			new Vector2(0f, 0.5f)
		},
		{
			eAnchorDirection.MiddleCentre,
			new Vector2(0.5f, 0.5f)
		},
		{
			eAnchorDirection.MiddleRight,
			new Vector2(1f, 0.5f)
		},
		{
			eAnchorDirection.DownLeft,
			new Vector2(0f, 0f)
		},
		{
			eAnchorDirection.DownCentre,
			new Vector2(0.5f, 0f)
		},
		{
			eAnchorDirection.DownRight,
			new Vector2(1f, 0f)
		}
	};

	public static RectTransform ParentAndFill(this RectTransform rect, Transform parent)
	{
		rect.SetParent(parent);
		rect.anchorMax = Vector2.one;
		rect.anchorMin = Vector2.zero;
		rect.sizeDelta = Vector2.zero;
		rect.anchoredPosition = Vector2.zero;
		rect.localScale = Vector3.one;
		return rect;
	}

	public static RectTransform ParentAndPosition(this RectTransform rect, Transform parent)
	{
		rect.ParentWithUnitScale(parent);
		rect.anchoredPosition = Vector2.zero;
		return rect;
	}

	public static RectTransform ParentWithUnitScale(this RectTransform rect, Transform parent)
	{
		rect.SetParent(parent);
		rect.localScale = Vector3.one;
		return rect;
	}

	public static RectTransform SetPivotToDirection(this RectTransform rect, eAnchorDirection direction)
	{
		rect.pivot = _anchorVectorLookup[direction];
		return rect;
	}

	public static RectTransform SetMaxAndMinAnchorToDirection(this RectTransform rect, eAnchorDirection direction)
	{
		rect.anchorMax = _anchorVectorLookup[direction];
		rect.anchorMin = _anchorVectorLookup[direction];
		return rect;
	}

	public static RectTransform SetMaxAnchorToDirection(this RectTransform rect, eAnchorDirection direction)
	{
		rect.anchorMax = _anchorVectorLookup[direction];
		return rect;
	}

	public static RectTransform SetMinAnchorToDirection(this RectTransform rect, eAnchorDirection direction)
	{
		rect.anchorMin = _anchorVectorLookup[direction];
		return rect;
	}
}
