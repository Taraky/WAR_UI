using UnityEngine;

namespace Travellers.UI.Login;

public class CharacterCreationSelectOption : MonoBehaviour
{
	[SerializeField]
	private CharacterCreationScreen.OptionType _option = CharacterCreationScreen.OptionType.NONE;

	[SerializeField]
	private CharacterCreationScreen _creationScreen;

	[SerializeField]
	private GameObject _selectedRoot;

	[SerializeField]
	private GameObject _deselectedRoot;

	public void Select()
	{
		_creationScreen.SelectOption(_option);
	}

	public void SetSelected(bool selected)
	{
		_selectedRoot.SetActive(selected);
		_deselectedRoot.SetActive(!selected);
	}
}
