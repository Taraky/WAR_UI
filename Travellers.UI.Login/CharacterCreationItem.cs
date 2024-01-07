using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.Login;

public class CharacterCreationItem : MonoBehaviour
{
	[SerializeField]
	private RawImage _image;

	private int _id;

	private CharacterCreationItemsList _list;

	public int Id => _id;

	public CharacterCreationItemsList List
	{
		set
		{
			_list = value;
		}
	}

	public void SetData(int id, string name)
	{
		Texture iconTexture = InventoryIconManager.Instance.GetIconTexture("Character Customisation/" + name);
		_image.texture = iconTexture;
		_id = id;
	}

	public void Select()
	{
		_list.Select(this);
	}
}
