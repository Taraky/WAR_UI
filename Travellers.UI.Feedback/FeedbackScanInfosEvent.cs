using System;

namespace Travellers.UI.Feedback;

public class FeedbackScanInfosEvent : FeedbackEvent
{
	public readonly ScannableData ScannableData;

	public FeedbackScanInfosEvent(ScannableData scannableData, float timeToLive = 0f, Action<FeedbackItem> onFeedbackItemSpawned = null)
		: base(FeedbackPrefabType.ScanDetails, FeedbackEventType.ScanDetails, timeToLive, onFeedbackItemSpawned)
	{
		ScannableData = scannableData;
	}
}
