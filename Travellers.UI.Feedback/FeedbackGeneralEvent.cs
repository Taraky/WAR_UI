using System;
using UnityEngine;
using WAUtilities.Logging;

namespace Travellers.UI.Feedback;

public class FeedbackGeneralEvent : FeedbackEvent
{
	public readonly string Title;

	public readonly string Description;

	public readonly Sprite Icon;

	public FeedbackGeneralEvent(string title, string description, Sprite icon, FeedbackPrefabType prefabType, float timeToLive = 0f, Action<FeedbackItem> onFeedbackItemSpawned = null)
		: base(prefabType, FeedbackEventType.General, timeToLive, onFeedbackItemSpawned)
	{
		Title = title;
		Description = description;
		Icon = icon;
		if (prefabType == FeedbackPrefabType.NoIcon && icon != null)
		{
			WALogger.Warn<FeedbackScreen>("Passed an event with title {0}, description {1} and type {2} with an icon value but of prefab type NoIcon! Icon will be ignored, is this intended or a config error?", new object[3] { title, description, prefabType });
		}
		if ((prefabType == FeedbackPrefabType.SideIcon || prefabType == FeedbackPrefabType.InlineIcon) && icon == null)
		{
			WALogger.Error<FeedbackScreen>("Passed an event with title {0}, description {1} and type {2} without an icon value! Reverting to type NoIcon, is this intended or a config error?", new object[3] { title, description, prefabType });
			base.Type = FeedbackPrefabType.NoIcon;
		}
	}
}
