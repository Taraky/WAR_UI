using GameDBLocalization;
using Improbable;
using Travellers.UI.Framework;
using UnityEngine;

namespace Travellers.UI.PlayerInventory;

public class LockCodeUI : UIScreenComponent
{
	[SerializeField]
	private TextStyler _code;

	private EntityId _entityId;

	public void Setup(EntityId entityId)
	{
		_entityId = entityId;
		ShowShipyardCode(show: false);
	}

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUIButtonEvents.ShowShipyardCode, OnShowShipyardCode);
	}

	protected override void ProtectedInit()
	{
	}

	protected override void ProtectedDispose()
	{
		SetCodeField();
	}

	private void OnShowShipyardCode(object[] obj)
	{
		ShowShipyardCode(((ShowShipyardCodeEvent)obj[0]).Show);
	}

	private void ShowShipyardCode(bool show)
	{
		if (show)
		{
			_code.SetText(LockAgentVisualizer.VisitorShipyardCode);
		}
		else
		{
			_code.SetText(Localizer.LocalizeString(LocalizationSchema.KeySHIPYARD_CODE_HIDDEN));
		}
	}

	private void SetCodeField()
	{
		if (_entityId == LockAgentVisualizer.VisitorShipyardId)
		{
			_code.SetText(Localizer.LocalizeString(LocalizationSchema.KeySHIPYARD_CODE_HIDDEN));
		}
		else
		{
			_code.SetText(string.Empty);
		}
	}
}
