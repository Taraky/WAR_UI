using Travellers.UI.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.PlayerInventory;

public class ShipDiagnosticItem : UIScreenComponent
{
	[SerializeField]
	private Text _nameText;

	[SerializeField]
	private Text _quantityText;

	private int _quantity;

	[SerializeField]
	private Text _weightText;

	private float _weight;

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
			_weightText.text = $"{value}Kg";
		}
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
