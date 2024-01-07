using System.Collections.Generic;
using Assets.Scripts.Utils;
using TMPro;
using Travellers.UI.DebugOSD;
using Travellers.UI.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.DebugCommands;

[InjectableClass]
public class DebugCommandHelpScreen : UIScreen
{
	[SerializeField]
	private ScrollRect _scrollRect;

	[SerializeField]
	private GameObject _item;

	[SerializeField]
	private GameObject _command;

	[SerializeField]
	private VerticalLayoutGroup _verticalLayoutGroup;

	[SerializeField]
	private VerticalLayoutGroup _innerVerticalLayoutGroup;

	[SerializeField]
	private RectTransform _scrollContent;

	[SerializeField]
	private Button _categoryButton;

	[SerializeField]
	private Button _closeButton;

	private IHelpCommandSystem _helpCommandSystem;

	private readonly Dictionary<string, GameObject> _gameObjectsCategories = new Dictionary<string, GameObject>();

	private readonly Dictionary<string, List<GameObject>> _gameObjectsCategoryCommandObjects = new Dictionary<string, List<GameObject>>();

	private Dictionary<string, List<Utils.Tuple<string, string>>> _helpCategoryCommandDictionary = new Dictionary<string, List<Utils.Tuple<string, string>>>();

	[InjectableMethod]
	public void InjectDependencies(IHelpCommandSystem helpCommandSystem)
	{
		_helpCommandSystem = helpCommandSystem;
		_helpCommandSystem.RetrieveHelpCommandsAsync(AssignHelpDictionary);
	}

	private void AssignHelpDictionary(Dictionary<string, List<Utils.Tuple<string, string>>> helpDictionary)
	{
		if (_helpCategoryCommandDictionary.Count != 0)
		{
			return;
		}
		_helpCategoryCommandDictionary = helpDictionary;
		foreach (KeyValuePair<string, List<Utils.Tuple<string, string>>> item in _helpCategoryCommandDictionary)
		{
			InitializeCategories(item.Key);
			InitializeList(item);
		}
	}

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
		_closeButton.onClick.AddListener(OnCloseButtonClicked);
		_muteListenersWhenInactive = false;
	}

	protected override void ProtectedDispose()
	{
	}

	private void InitializeList(KeyValuePair<string, List<Utils.Tuple<string, string>>> pair)
	{
		for (int i = 0; i < pair.Value.Count; i++)
		{
			InitializeNewItem(pair.Value[i], pair.Key);
		}
	}

	private void InitializeCategories(string name)
	{
		GameObject newCategory = Object.Instantiate(_item);
		newCategory.name = name;
		newCategory.transform.SetParent(_verticalLayoutGroup.transform, worldPositionStays: false);
		newCategory.transform.localScale = Vector3.one;
		TextMeshProUGUI component = newCategory.GetComponent<TextMeshProUGUI>();
		component.text = name;
		Button component2 = newCategory.GetComponent<Button>();
		component2.onClick.AddListener(delegate
		{
			OnButtonClick(newCategory.name);
		});
		newCategory.SetActive(value: true);
		_gameObjectsCategories.Add(newCategory.name, newCategory);
	}

	private void InitializeNewItem(Utils.Tuple<string, string> name, string category)
	{
		GameObject gameObject = Object.Instantiate(_command);
		gameObject.name = name.first;
		GameObject gameObject2 = _gameObjectsCategories[category];
		VerticalLayoutGroup component = gameObject2.GetComponent<VerticalLayoutGroup>();
		gameObject.transform.SetParent(component.transform, worldPositionStays: false);
		gameObject.transform.localScale = Vector3.one;
		TextMeshProUGUI component2 = gameObject.GetComponent<TextMeshProUGUI>();
		component2.text = "/" + name.first + " : " + name.second;
		gameObject.SetActive(value: false);
		if (_gameObjectsCategoryCommandObjects.ContainsKey(category))
		{
			_gameObjectsCategoryCommandObjects[category].Add(gameObject);
			return;
		}
		_gameObjectsCategoryCommandObjects.Add(category, new List<GameObject> { gameObject });
	}

	private void OnButtonClick(string category)
	{
		List<GameObject> list = _gameObjectsCategoryCommandObjects[category];
		foreach (GameObject item in list)
		{
			if (item != null)
			{
				item.SetActive(!item.activeSelf);
			}
		}
	}

	private void OnCloseButtonClicked()
	{
		UIWindowController.PopState<DebugHelpWindowUIState>();
	}
}
