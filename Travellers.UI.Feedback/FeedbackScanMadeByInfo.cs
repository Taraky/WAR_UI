namespace Travellers.UI.Feedback;

public class FeedbackScanMadeByInfo : FeedbackScanTextInfo
{
	public override bool Setup(ScannableData data)
	{
		return SetupText((!string.IsNullOrEmpty(data.crafter)) ? $"Made by: {data.crafter}" : string.Empty);
	}
}
