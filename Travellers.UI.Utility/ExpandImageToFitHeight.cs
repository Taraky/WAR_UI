using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.Utility;

public class ExpandImageToFitHeight : MonoBehaviour
{
	[SerializeField]
	private Image Icon;

	[ContextMenu("Resize rect to image width")]
	public void SetNewWidth()
	{
		RectTransform rectTransform = (RectTransform)base.transform.parent;
		RectTransform component = GetComponent<RectTransform>();
		Vector2 anchorMax = component.anchorMax;
		Vector2 anchorMin = component.anchorMin;
		component.SetMaxAndMinAnchorToDirection(eAnchorDirection.UpRight);
		Sprite sprite = Icon.sprite;
		float num = sprite.texture.height;
		float y = rectTransform.sizeDelta.y;
		float num2 = y / (float)sprite.texture.height;
		component.sizeDelta = new Vector2((float)sprite.texture.width * num2, y);
		component.anchorMax = anchorMax;
		component.anchorMin = anchorMin;
	}
}
