using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bossa.Travellers.BossaNet;
using Travellers.UI.Framework;
using Travellers.UI.InfoPopups;
using UnityEngine;
using UnityEngine.UI;
using WAUtilities.Logging;

namespace Travellers.UI.Login;

[InjectableClass]
public class CharacterCreationScreen : UIScreen
{
	[Serializable]
	public enum OptionType
	{
		NONE = -1,
		FACE,
		HAIR,
		MARKING,
		CLOTHING_BODY,
		CLOTHING_LEGS,
		TOTAL
	}

	[Serializable]
	public class CustomisationSelection
	{
		public int PrimaryItem = -1;

		public int SecondaryItem = -1;

		public int PrimaryColor = -1;

		public int SecondaryColor = -1;
	}

	[SerializeField]
	private CharacterCreationItemsList _smallItemsListPrimary;

	[SerializeField]
	private CharacterCreationItemsList _largeItemsListPrimary;

	[SerializeField]
	private CharacterCreationItemsList _smallItemsListSecondary;

	[SerializeField]
	private CharacterCreationItemsList _largeItemsListSecondary;

	[SerializeField]
	private CharacterCreationColorPicker _colorPickerPrimary;

	[SerializeField]
	private CharacterCreationColorPicker _colorPickerSecondary;

	[SerializeField]
	private InputField _nameInputField;

	[SerializeField]
	private CharacterCreationServersList _serverList;

	[SerializeField]
	private bool _isMale = true;

	[SerializeField]
	private CharacterCreationSelectGender _maleOption;

	[SerializeField]
	private CharacterCreationSelectGender _femaleOption;

	[SerializeField]
	private CharacterCreationSelectOption[] _allOptionButtons;

	private OptionType _currentOption = OptionType.NONE;

	[SerializeField]
	private Button _saveCharacterButton;

	[SerializeField]
	private Button _cancelButton;

	[SerializeField]
	private CharacterCreationItem _listItemPrefab;

	private Dictionary<OptionType, CustomisationSelection> _selectedOptionsByType;

	[SerializeField]
	private VerticalLayoutGroup _layoutGroup;

	private ContentSizeFitter[] _layoutGroupContentSizeFitters;

	[SerializeField]
	private int _deploymentStatusUpdatePeriod = 30;

	private int _lastPolled;

	private LobbySystem _lobbySys;

	public bool IsMale
	{
		get
		{
			return _isMale;
		}
		set
		{
			_isMale = value;
		}
	}

	public CharacterCreationItem ListItemPrefab => _listItemPrefab;

	[InjectableMethod]
	public void InjectDependencies(LobbySystem lobbySys)
	{
		_lobbySys = lobbySys;
	}

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
		_selectedOptionsByType = new Dictionary<OptionType, CustomisationSelection>();
		_smallItemsListPrimary.Init();
		_largeItemsListPrimary.Init();
		_smallItemsListPrimary.IsPrimary = true;
		_largeItemsListPrimary.IsPrimary = true;
		_smallItemsListSecondary.Init();
		_largeItemsListSecondary.Init();
		_smallItemsListSecondary.IsPrimary = false;
		_largeItemsListSecondary.IsPrimary = false;
		_colorPickerPrimary.Init();
		_colorPickerSecondary.Init();
		_colorPickerPrimary.IsPrimary = true;
		_colorPickerSecondary.IsPrimary = false;
		_allOptionButtons = GetComponentsInChildren<CharacterCreationSelectOption>(includeInactive: true);
		RefreshScreen();
	}

	private void RefreshScreen()
	{
		_lobbySys.CheckModelDisplayState();
		Randomise();
		SelectOption(OptionType.FACE);
		RefreshServerList();
		_serverList.Select(0);
		if (!string.IsNullOrEmpty(_lobbySys.CurrentCreationData.Name))
		{
			_nameInputField.text = _lobbySys.GetUserName();
			_nameInputField.interactable = false;
		}
		else
		{
			_nameInputField.text = string.Empty;
			_nameInputField.interactable = true;
		}
	}

	public void RefreshServerList()
	{
		_serverList.RefreshServersList();
	}

	[ContextMenu("Randomise")]
	public void Randomise()
	{
		_lobbySys.SetCharacterModelRefresh(canRefresh: false);
		if (UnityEngine.Random.value >= 0.5f)
		{
			SetMale();
		}
		else
		{
			SetFemale();
		}
		_maleOption.SetSelected(_isMale);
		_femaleOption.SetSelected(!_isMale);
		if (_currentOption == OptionType.FACE)
		{
			int iColor = UnityEngine.Random.Range(0, CustomisationSettings.skinColors.Length);
			_colorPickerPrimary.Select(iColor);
			_colorPickerSecondary.Select(iColor);
			_smallItemsListPrimary.Select(UnityEngine.Random.Range(0, CustomisationSettings.starterFaceItems.Count));
		}
		else
		{
			int id = UnityEngine.Random.Range(0, CustomisationSettings.skinColors.Length);
			SetOption(OptionType.FACE, UnityEngine.Random.Range(0, CustomisationSettings.starterFaceItems.Count), isPrimary: true);
			SetColor(OptionType.FACE, id, isPrimary: true);
			SetColor(OptionType.FACE, id, isPrimary: false);
		}
		if (_currentOption == OptionType.HAIR)
		{
			_smallItemsListPrimary.Select(UnityEngine.Random.Range(0, CustomisationSettings.starterHeadItems.Count));
			if (_isMale)
			{
				_smallItemsListSecondary.Select(UnityEngine.Random.Range(0, CustomisationSettings.starterFacialHairItems.Count));
			}
			_colorPickerPrimary.Select(UnityEngine.Random.Range(0, CustomisationSettings.hairColors.Length));
		}
		else
		{
			SetOption(OptionType.HAIR, UnityEngine.Random.Range(0, CustomisationSettings.starterHeadItems.Count), isPrimary: true);
			if (_isMale)
			{
				SetOption(OptionType.HAIR, UnityEngine.Random.Range(0, CustomisationSettings.starterFacialHairItems.Count), isPrimary: false);
			}
			SetColor(OptionType.HAIR, UnityEngine.Random.Range(0, CustomisationSettings.hairColors.Length), isPrimary: true);
		}
		if (_currentOption == OptionType.MARKING)
		{
		}
		if (_currentOption == OptionType.CLOTHING_BODY)
		{
			_largeItemsListPrimary.Select(UnityEngine.Random.Range(0, CustomisationSettings.starterTorsoItems.Count));
			_colorPickerPrimary.Select(UnityEngine.Random.Range(0, CustomisationSettings.clothingColors.Length));
			_colorPickerSecondary.Select(UnityEngine.Random.Range(0, CustomisationSettings.clothingColors.Length));
		}
		else
		{
			SetOption(OptionType.CLOTHING_BODY, UnityEngine.Random.Range(0, CustomisationSettings.starterTorsoItems.Count), isPrimary: true);
			SetColor(OptionType.CLOTHING_BODY, UnityEngine.Random.Range(0, CustomisationSettings.clothingColors.Length), isPrimary: true);
			SetColor(OptionType.CLOTHING_BODY, UnityEngine.Random.Range(0, CustomisationSettings.clothingColors.Length), isPrimary: false);
		}
		if (_currentOption == OptionType.CLOTHING_LEGS)
		{
			_largeItemsListPrimary.Select(UnityEngine.Random.Range(0, CustomisationSettings.starterLegItems.Count));
			_colorPickerPrimary.Select(UnityEngine.Random.Range(0, CustomisationSettings.clothingColors.Length));
			_colorPickerSecondary.Select(UnityEngine.Random.Range(0, CustomisationSettings.clothingColors.Length));
		}
		else
		{
			SetOption(OptionType.CLOTHING_LEGS, UnityEngine.Random.Range(0, CustomisationSettings.starterLegItems.Count), isPrimary: true);
			SetColor(OptionType.CLOTHING_LEGS, UnityEngine.Random.Range(0, CustomisationSettings.clothingColors.Length), isPrimary: true);
			SetColor(OptionType.CLOTHING_LEGS, UnityEngine.Random.Range(0, CustomisationSettings.clothingColors.Length), isPrimary: false);
		}
		_lobbySys.SetCharacterModelRefresh(canRefresh: true);
	}

	public void SelectOption(OptionType option)
	{
		_currentOption = option;
		if (!_lobbySys.CanRefreshModel)
		{
			return;
		}
		_lobbySys.FaceView(option == OptionType.FACE || option == OptionType.HAIR);
		string title = string.Empty;
		bool flag = true;
		Dictionary<string, CustomisationSettings.GenderItem> dictionary = null;
		bool flag2 = true;
		bool flag3 = true;
		CharacterCreationItemsList characterCreationItemsList = null;
		string title2 = string.Empty;
		bool flag4 = true;
		Dictionary<string, CustomisationSettings.GenderItem> dictionary2 = null;
		bool flag5 = true;
		bool flag6 = true;
		CharacterCreationItemsList characterCreationItemsList2 = null;
		string title3 = "COLOUR";
		Color[] array = null;
		string title4 = "COLOUR";
		Color[] array2 = null;
		switch (option)
		{
		case OptionType.FACE:
			title = "FACE";
			dictionary = CustomisationSettings.starterFaceItems;
			title3 = "SKIN COLOUR";
			array = CustomisationSettings.skinColors;
			title4 = "LIP COLOUR";
			array2 = CustomisationSettings.lipColors;
			break;
		case OptionType.HAIR:
			title = "HAIR";
			dictionary = CustomisationSettings.starterHeadItems;
			if (_isMale)
			{
				title2 = "FACIAL HAIR";
				dictionary2 = CustomisationSettings.starterFacialHairItems;
			}
			title3 = "HAIR COLOUR";
			array = CustomisationSettings.hairColors;
			break;
		case OptionType.MARKING:
			title = "MARKING";
			dictionary = CustomisationSettings.starterHeadItems;
			array = CustomisationSettings.skinColors;
			break;
		case OptionType.CLOTHING_BODY:
			flag = false;
			title = "CLOTHING - BODY";
			dictionary = CustomisationSettings.starterTorsoItems;
			title3 = "PRIMARY COLOUR";
			array = CustomisationSettings.clothingColors;
			title4 = "SECONDARY COLOUR";
			array2 = CustomisationSettings.clothingColors;
			break;
		case OptionType.CLOTHING_LEGS:
			flag = false;
			title = "CLOTHING - LEGS";
			dictionary = CustomisationSettings.starterLegItems;
			title3 = "PRIMARY COLOUR";
			array = CustomisationSettings.clothingColors;
			title4 = "SECONDARY COLOUR";
			array2 = CustomisationSettings.clothingColors;
			break;
		}
		if (dictionary != null)
		{
			characterCreationItemsList = ((!flag) ? _largeItemsListPrimary : _smallItemsListPrimary);
		}
		if (dictionary2 != null)
		{
			characterCreationItemsList2 = ((!flag4) ? _largeItemsListSecondary : _smallItemsListSecondary);
		}
		bool flag7 = dictionary != null && ((IsMale && flag2) || (!IsMale && flag3));
		bool flag8 = dictionary2 != null && ((IsMale && flag5) || (!IsMale && flag6));
		_smallItemsListPrimary.gameObject.SetActive(flag7 && flag);
		_largeItemsListPrimary.gameObject.SetActive(flag7 && !flag);
		_smallItemsListSecondary.gameObject.SetActive(flag8 && flag4);
		_largeItemsListSecondary.gameObject.SetActive(flag8 && !flag4);
		_colorPickerPrimary.gameObject.SetActive(array != null);
		_colorPickerSecondary.gameObject.SetActive(array2 != null);
		if (flag7)
		{
			if (flag)
			{
				_smallItemsListPrimary.SetData(title, dictionary);
			}
			else
			{
				_largeItemsListPrimary.SetData(title, dictionary);
			}
		}
		if (flag8)
		{
			if (flag4)
			{
				_smallItemsListSecondary.SetData(title2, dictionary2);
			}
			else
			{
				_largeItemsListSecondary.SetData(title2, dictionary2);
			}
		}
		if (array != null)
		{
			_colorPickerPrimary.SetData(title3, array);
		}
		if (array2 != null)
		{
			_colorPickerSecondary.SetData(title4, array2);
		}
		if (_selectedOptionsByType.ContainsKey(option))
		{
			CustomisationSelection customisationSelection = _selectedOptionsByType[option];
			if (dictionary != null)
			{
				characterCreationItemsList.Select((customisationSelection.PrimaryItem != -1) ? customisationSelection.PrimaryItem : 0);
			}
			if (dictionary2 != null)
			{
				characterCreationItemsList2.Select((customisationSelection.SecondaryItem != -1) ? customisationSelection.SecondaryItem : 0);
			}
			if (array != null)
			{
				_colorPickerPrimary.Select((customisationSelection.PrimaryColor != -1) ? customisationSelection.PrimaryColor : 0);
			}
			if (array2 != null)
			{
				_colorPickerSecondary.Select((customisationSelection.SecondaryColor != -1) ? customisationSelection.SecondaryColor : 0);
			}
		}
		else
		{
			if (dictionary != null)
			{
				characterCreationItemsList.Select(0);
			}
			if (dictionary2 != null)
			{
				characterCreationItemsList2.Select(0);
			}
			if (array != null)
			{
				_colorPickerPrimary.Select(0);
			}
			if (array2 != null)
			{
				_colorPickerSecondary.Select(0);
			}
		}
		for (int i = 0; i < _allOptionButtons.Length; i++)
		{
			_allOptionButtons[i].SetSelected(_currentOption == (OptionType)i);
		}
		if (_layoutGroup != null)
		{
			if (_layoutGroupContentSizeFitters == null)
			{
				_layoutGroupContentSizeFitters = _layoutGroup.GetComponentsInChildren<ContentSizeFitter>(includeInactive: true);
			}
			UILayoutGroupHelper.UpdateLayoutGroupImmediately(_layoutGroupContentSizeFitters);
			_layoutGroup.enabled = false;
			_layoutGroup.enabled = true;
		}
		Canvas.ForceUpdateCanvases();
	}

	public void SetMale()
	{
		IsMale = true;
		_lobbySys.ChangeGender(_isMale);
		RefreshItems();
	}

	public void SetFemale()
	{
		IsMale = false;
		_lobbySys.ChangeGender(_isMale);
		RefreshItems();
	}

	public void SetOption(int id, bool isPrimary)
	{
		SetOption(_currentOption, id, isPrimary);
	}

	private void SetOption(OptionType option, int id, bool isPrimary)
	{
		if (!_selectedOptionsByType.ContainsKey(option))
		{
			_selectedOptionsByType.Add(option, new CustomisationSelection());
		}
		if (isPrimary)
		{
			_selectedOptionsByType[option].PrimaryItem = id;
		}
		else
		{
			_selectedOptionsByType[option].SecondaryItem = id;
		}
		switch (option)
		{
		case OptionType.FACE:
		{
			string faceName = CustomisationSettings.starterFaceItems.Keys.ToList()[id];
			_lobbySys.ChangeFace(faceName);
			break;
		}
		case OptionType.HAIR:
			if (isPrimary)
			{
				string headName = CustomisationSettings.starterHeadItems.Keys.ToList()[id];
				_lobbySys.ChangeHead(headName);
			}
			else
			{
				string facialHairName = CustomisationSettings.starterFacialHairItems.Keys.ToList()[id];
				_lobbySys.ChangeFacialHair(facialHairName);
			}
			break;
		case OptionType.MARKING:
			break;
		case OptionType.CLOTHING_BODY:
		{
			string torsoName = CustomisationSettings.starterTorsoItems.Keys.ToList()[id];
			_lobbySys.ChangeTorso(torsoName);
			break;
		}
		case OptionType.CLOTHING_LEGS:
		{
			string legsName = CustomisationSettings.starterLegItems.Keys.ToList()[id];
			_lobbySys.ChangeLegs(legsName);
			break;
		}
		}
	}

	public void SetColor(int id, bool isPrimary)
	{
		SetColor(_currentOption, id, isPrimary);
	}

	private void SetColor(OptionType option, int id, bool isPrimary)
	{
		if (!_selectedOptionsByType.ContainsKey(option))
		{
			_selectedOptionsByType.Add(option, new CustomisationSelection());
		}
		if (isPrimary)
		{
			_selectedOptionsByType[option].PrimaryColor = id;
		}
		else
		{
			_selectedOptionsByType[option].SecondaryColor = id;
		}
		switch (option)
		{
		case OptionType.FACE:
			if (isPrimary)
			{
				Color color4 = CustomisationSettings.skinColors[id];
				_lobbySys.ChangeSkinColor(color4);
				SetColor(OptionType.FACE, id, isPrimary: false);
			}
			else
			{
				Color color5 = CustomisationSettings.lipColors[id];
				_lobbySys.ChangeLipColor(color5);
			}
			break;
		case OptionType.HAIR:
		{
			Color color3 = CustomisationSettings.hairColors[id];
			_lobbySys.ChangeHairColor(color3);
			break;
		}
		case OptionType.MARKING:
			break;
		case OptionType.CLOTHING_BODY:
			if (isPrimary)
			{
				Color color6 = CustomisationSettings.clothingColors[id];
				_lobbySys.ChangeClothingColor(CharacterSlotType.Body, isPrimary, color6);
			}
			else
			{
				Color color7 = CustomisationSettings.clothingColors[id];
				_lobbySys.ChangeClothingColor(CharacterSlotType.Body, isPrimary, color7);
			}
			break;
		case OptionType.CLOTHING_LEGS:
			if (isPrimary)
			{
				Color color = CustomisationSettings.clothingColors[id];
				_lobbySys.ChangeClothingColor(CharacterSlotType.Feet, isPrimary, color);
			}
			else
			{
				Color color2 = CustomisationSettings.clothingColors[id];
				_lobbySys.ChangeClothingColor(CharacterSlotType.Feet, isPrimary, color2);
			}
			break;
		}
	}

	private void RefreshItems()
	{
		if (_lobbySys.CanRefreshModel)
		{
			_maleOption.SetSelected(_isMale);
			_femaleOption.SetSelected(!_isMale);
			SelectOption(_currentOption);
		}
	}

	public void Cancel()
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIStateEvents.BackButton);
	}

	public void CreateCharacter()
	{
		if (string.IsNullOrEmpty(_nameInputField.text))
		{
			DialogPopupFacade.ShowOkDialog("No name.", "Please give a name to your character!");
		}
		else
		{
			ConfirmCreation();
		}
	}

	private void NameReserveFail(List<string> reasons)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string reason in reasons)
		{
			stringBuilder.AppendLine(reason);
		}
		stringBuilder.Remove(stringBuilder.Length - 1, 1);
		DialogPopupFacade.ShowOkDialog("Name not valid", stringBuilder.ToString(), delegate
		{
			_saveCharacterButton.interactable = true;
			_cancelButton.interactable = true;
		});
	}

	private void NameReserveError(string error)
	{
		WALogger.Error<CharacterCreationScreen>("Error reserving name: {0}", new object[1] { error });
		DialogPopupFacade.ShowOkDialog("Error", "There was an error creating your character please try again in a few moments", delegate
		{
			_saveCharacterButton.interactable = true;
			_cancelButton.interactable = true;
		});
	}

	private void ConfirmCreation()
	{
		string text = _nameInputField.text;
		CharacterCreationSelectServer selectedServerRecord = _serverList.SelectedServerRecord;
		DialogPopupFacade.ShowConfirmationDialog("Confirm Character", $"Are you sure you want to create your character {text} on server {selectedServerRecord.ServerName}?\nThis cannot be changed later.", SkipTutorial, "CONFIRM");
	}

	private void SkipTutorial()
	{
		if (_lobbySys.HavenFinished)
		{
			DialogPopupFacade.ShowConfirmationDialog("Skip Tutorial Zone?", "If you already know how to play Worlds Adrift you can skip the tutorial.", ReserveName, "PLAY", "SKIP", ConfirmSkipTutorial, useSolidBackground: true, 2f);
		}
		else
		{
			ReserveName();
		}
	}

	private void ConfirmSkipTutorial()
	{
		_lobbySys.CurrentCreationData.skippedTutorial = true;
		ReserveName();
	}

	private void ReserveName()
	{
		_saveCharacterButton.interactable = false;
		_cancelButton.interactable = false;
		bool flag = string.IsNullOrEmpty(BossaNetBootstrap.Instance.ScreenName);
		bool flag2 = !string.IsNullOrEmpty(_lobbySys.CurrentCreationData.Name) || !_lobbySys.HasMainCharacter;
		if (!WAConfig.Get<bool>(ConfigKeys.UseBossaNet))
		{
			DoCreate(_nameInputField.text, _serverList.SelectedServerIdentifier);
		}
		else if (flag2 && flag)
		{
			BossaNetBootstrap.Instance.ReserveName(_lobbySys.CurrentCreationData.characterUid, _nameInputField.text, isMainCharacterName: true, delegate
			{
				DoCreate(_nameInputField.text, _serverList.SelectedServerIdentifier);
			}, NameReserveFail, NameReserveError);
		}
		else if (!flag2)
		{
			BossaNetBootstrap.Instance.ReserveName(_lobbySys.CurrentCreationData.characterUid, _nameInputField.text, isMainCharacterName: false, delegate
			{
				DoCreate(_nameInputField.text, _serverList.SelectedServerIdentifier);
			}, NameReserveFail, NameReserveError);
		}
		else
		{
			DoCreate(_nameInputField.text, _serverList.SelectedServerIdentifier);
		}
	}

	private void DoCreate(string characterName, string serverIdentifier)
	{
		_lobbySys.CharacterName = characterName;
		_lobbySys.CharacterMale = _isMale;
		_lobbySys.ServerIdentifier = serverIdentifier;
		_lobbySys.SaveData();
	}

	private void RefreshServerStatus()
	{
		_serverList.UpdateDeploymentStatus();
	}

	private void Update()
	{
		if (WAConfig.Get<bool>(ConfigKeys.UseBossaNet) && (int)Time.timeSinceLevelLoad % _deploymentStatusUpdatePeriod == 0 && _lastPolled != (int)Time.timeSinceLevelLoad)
		{
			BossaNetBootstrap.Instance.QueryServerStatus(delegate
			{
				RefreshServerStatus();
			}, delegate
			{
			});
			_lastPolled = (int)Time.timeSinceLevelLoad;
		}
		if (_lobbySys.ShouldUpdateServerLists)
		{
			RefreshServerList();
			_lobbySys.ShouldUpdateServerLists = false;
		}
	}

	protected override void ProtectedDispose()
	{
	}
}
