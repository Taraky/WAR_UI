using UnityEngine;

namespace Travellers.UI.Feedback;

public abstract class FeedbackScanTextInfo : FeedbackScanBaseInfo
{
	[SerializeField]
	protected TextStylerTextMeshPro _label;

	protected bool SetupText(string text)
	{
		if (!string.IsNullOrEmpty(text))
		{
			base.gameObject.SetActive(value: true);
			_label.SetText(text);
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
		return base.gameObject.activeSelf;
	}
}
