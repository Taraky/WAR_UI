namespace Travellers.UI.Feedback;

public class FeedbackScanDescriptionBisInfo : FeedbackScanTextInfo
{
	public override bool Setup(ScannableData data)
	{
		return SetupText((!(data.descriptionBis == "noDescription")) ? data.descriptionBis : string.Empty);
	}
}
