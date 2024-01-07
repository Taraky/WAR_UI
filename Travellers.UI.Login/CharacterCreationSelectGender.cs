using UnityEngine;

namespace Travellers.UI.Login;

public class CharacterCreationSelectGender : MonoBehaviour
{
	[SerializeField]
	private CharacterCreationScreen _creationScreen;

	[SerializeField]
	private bool _isMale;

	[SerializeField]
	private GameObject _selectedRoot;

	[SerializeField]
	private GameObject _deselectedRoot;

	public void Select()
	{
		if (_isMale)
		{
			_creationScreen.SetMale();
		}
		else
		{
			_creationScreen.SetMale();
		}
	}

	public void SetSelected(bool selected)
	{
		_selectedRoot.SetActive(selected);
		_deselectedRoot.SetActive(!selected);
	}
}
