using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.Login;

public class CharacterCreationColorItem : MonoBehaviour
{
	private CharacterCreationColorPicker _colorPicker;

	private int _id;

	[SerializeField]
	private Image _image;

	public CharacterCreationColorPicker ColorPicker
	{
		set
		{
			_colorPicker = value;
		}
	}

	public int Id => _id;

	public void SetData(CharacterCreationColorPicker colorPicker, int id, Color color)
	{
		_colorPicker = colorPicker;
		_id = id;
		_image.color = color;
	}

	public void Select()
	{
		_colorPicker.Select(this);
	}
}
