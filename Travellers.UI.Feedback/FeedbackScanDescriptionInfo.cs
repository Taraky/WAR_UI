namespace Travellers.UI.Feedback;

public class FeedbackScanDescriptionInfo : FeedbackScanTextInfo
{
	public override bool Setup(ScannableData data)
	{
		return SetupText((!(data.description == "noDescription")) ? data.description : string.Empty);
	}
}
