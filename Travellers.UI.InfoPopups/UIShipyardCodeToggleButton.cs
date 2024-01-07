using UnityEngine;
using UnityEngine.EventSystems;

namespace Travellers.UI.InfoPopups;

public class UIShipyardCodeToggleButton : MonoBehaviour, IPointerDownHandler, IPointerExitHandler, IPointerUpHandler, IEventSystemHandler
{
	private bool _released = true;

	public void OnPointerDown(PointerEventData eventData)
	{
		if (_released)
		{
			ToggleVisibility();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Release();
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		Release();
	}

	private void Release()
	{
		if (!_released)
		{
			ToggleVisibility();
		}
	}

	private void ToggleVisibility()
	{
		_released = !_released;
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIButtonEvents.ShowShipyardCode, new ShowShipyardCodeEvent(!_released));
	}
}
