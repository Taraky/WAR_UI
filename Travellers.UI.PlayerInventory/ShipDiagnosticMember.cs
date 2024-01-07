using Travellers.UI.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.PlayerInventory;

public class ShipDiagnosticMember : UIScreenComponent
{
	[SerializeField]
	private Text _nameText;

	public string Name
	{
		set
		{
			_nameText.text = value;
		}
	}

	protected override void AddListeners()
	{
	}

	protected override void ProtectedDispose()
	{
	}

	protected override void ProtectedInit()
	{
	}
}
