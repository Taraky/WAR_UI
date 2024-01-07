namespace Travellers.UI.Feedback;

public class FeedbackScanTitleBisInfo : FeedbackScanTextInfo
{
	public override bool Setup(ScannableData data)
	{
		return SetupText(data.titleBis);
	}
}
