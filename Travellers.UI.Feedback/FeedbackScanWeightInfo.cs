using UnityEngine;

namespace Travellers.UI.Feedback;

public class FeedbackScanWeightInfo : FeedbackScanTextInfo
{
	public override bool Setup(ScannableData data)
	{
		return SetupText((!(data.weight > 0f) || (data.components != null && data.components.Length != 0)) ? string.Empty : $"{Mathf.Round(data.weight * 10f) / 10f}kg");
	}
}
