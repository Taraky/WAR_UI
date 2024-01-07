using Travellers.UI.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Travellers.UI.Crafting;

public class BlipTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
{
	public string Title;

	public string Description;

	public void OnPointerExit(PointerEventData eventData)
	{
		UIWindowController.PopState<ScannerToolPopupState>();
	}

	private void OnDisable()
	{
		UIWindowController.PopState<ScannerToolPopupState>();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		ScannableData scannableData = new ScannableData();
		scannableData.Title = Title;
		scannableData.description = Description;
		scannableData.FollowMouse = true;
		UIWindowController.PushState(new ScannerToolPopupState(scannableData));
	}
}
