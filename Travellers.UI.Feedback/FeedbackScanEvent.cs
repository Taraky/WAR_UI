using System;

namespace Travellers.UI.Feedback;

public class FeedbackScanEvent : FeedbackEvent
{
	public long Quantity;

	public FeedbackScanEvent(long quantity, float timeToLive = 3f, Action<FeedbackItem> onFeedbackItemSpawned = null)
		: base(FeedbackPrefabType.NoIcon, FeedbackEventType.Scan, timeToLive, onFeedbackItemSpawned)
	{
		Quantity = quantity;
	}
}
