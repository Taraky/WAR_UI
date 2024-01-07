using System;

namespace Travellers.UI.Feedback;

public class FeedbackSalvageEvent : FeedbackEvent
{
	public readonly string ItemTypeId;

	public int Quantity;

	public FeedbackSalvageEvent(string itemTypeId, int quantity, float timeToLive = 3f, Action<FeedbackItem> onFeedbackItemSpawned = null)
		: base(FeedbackPrefabType.InlineIcon, FeedbackEventType.Salvage, timeToLive, onFeedbackItemSpawned)
	{
		ItemTypeId = itemTypeId;
		Quantity = quantity;
	}
}
