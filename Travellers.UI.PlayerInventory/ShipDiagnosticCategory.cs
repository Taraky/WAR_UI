using Travellers.UI.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.PlayerInventory;

public class ShipDiagnosticCategory : UIScreenComponent
{
	[SerializeField]
	private Text _nameText;

	[SerializeField]
	private Text _quantityText;

	private int _quantity;

	[SerializeField]
	private Text _weightText;

	private float _weight;

	[SerializeField]
	private VerticalLayoutGroup _verticalGroup;

	[SerializeField]
	private Transform _contentRoot;

	[SerializeField]
	private Button _downButton;

	[SerializeField]
	private Button _upButton;

	public string Name
	{
		get
		{
			return _nameText.text;
		}
		set
		{
			_nameText.text = value;
		}
	}

	public int Quantity
	{
		get
		{
			return _quantity;
		}
		set
		{
			_quantity = value;
			_quantityText.text = $"{value}";
		}
	}

	public float Weight
	{
		get
		{
			return _weight;
		}
		set
		{
			_weight = value;
			_weightText.text = $"{value:#.#}Kg";
		}
	}

	public Transform ContentRoot => _contentRoot;

	public void ToggleShow()
	{
		Show(!ContentRoot.gameObject.activeInHierarchy);
	}

	public void Show(bool show)
	{
		ContentRoot.gameObject.SetActive(show);
		_downButton.gameObject.SetActive(!show);
		_upButton.gameObject.SetActive(show);
	}

	public void Clear()
	{
		for (int num = ContentRoot.childCount - 1; num >= 0; num--)
		{
			Object.DestroyImmediate(ContentRoot.GetChild(num).gameObject);
		}
		Quantity = 0;
		Weight = 0f;
	}

	public void AddPart(ShipDiagnosticItem part)
	{
		Quantity += part.Quantity;
		Weight += part.Weight;
	}

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
	}

	protected override void ProtectedDispose()
	{
	}
}
