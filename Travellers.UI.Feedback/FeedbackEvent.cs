using System;
using Travellers.UI.Events;

namespace Travellers.UI.Feedback;

public class FeedbackEvent : UIEvent
{
	public readonly FeedbackEventType EventType;

	public readonly float TimeToLive;

	public readonly Action<FeedbackItem> OnFeedbackItemSpawned;

	public FeedbackPrefabType Type { get; protected set; }

	public FeedbackEvent(FeedbackPrefabType prefabType, FeedbackEventType eventType, float timeToLive = 0f, Action<FeedbackItem> onFeedbackItemSpawned = null)
	{
		Type = prefabType;
		TimeToLive = timeToLive;
		EventType = eventType;
		OnFeedbackItemSpawned = onFeedbackItemSpawned;
	}
}
