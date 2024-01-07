using System;
using System.Collections.Generic;
using Travellers.UI.Framework;
using Travellers.UI.Utility;
using UnityEngine;

namespace Travellers.UI;

public class UIStructure
{
	private static Dictionary<UILayer, Transform> _priorityTransformLookup;

	private static GameObject _parentContainer;

	public static Canvas RootCanvas { get; private set; }

	public static void Create()
	{
		GameObject gameObject = UIObjectFactory.Instantiate(Resources.Load("UI/Prefabs/UIRoot") as GameObject);
		RootCanvas = gameObject.GetComponent<Canvas>();
		int num = Enum.GetNames(typeof(UILayer)).Length;
		_parentContainer = new GameObject("PriorityHandler", typeof(RectTransform));
		RectTransform component = _parentContainer.GetComponent<RectTransform>();
		component.ParentAndFill(RootCanvas.transform);
		_priorityTransformLookup = new Dictionary<UILayer, Transform>();
		for (int i = 0; i < num; i++)
		{
			UILayer key = (UILayer)i;
			gameObject = new GameObject(key.ToString(), typeof(RectTransform));
			gameObject.AddComponent<CanvasGroup>();
			RectTransform component2 = gameObject.GetComponent<RectTransform>();
			component2.ParentAndFill(component);
			_priorityTransformLookup[key] = component2;
		}
	}

	public static Transform GetPriorityParent(UILayer priority)
	{
		return _priorityTransformLookup[priority];
	}

	public static void SetLayersActive(bool isActive, params UILayer[] layers)
	{
		foreach (UILayer key in layers)
		{
			CanvasGroup component = _priorityTransformLookup[key].GetComponent<CanvasGroup>();
			component.alpha = (isActive ? 1 : 0);
			component.blocksRaycasts = isActive;
		}
	}

	public static void ChangeVisibilityOfEntireUI(bool isActive)
	{
		_parentContainer.SetActive(isActive);
	}

	public static void ToggleVisibilityOfEntireUI()
	{
		ChangeVisibilityOfEntireUI(!_parentContainer.activeSelf);
	}
}
