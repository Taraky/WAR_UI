using System;
using System.Collections.Generic;
using Bossa.Travellers.World;
using Travellers.UI.Framework;
using UnityEngine;
using WAUtilities.Logging;

namespace Travellers.UI.Feedback;

public class FeedbackScreen : UIScreen
{
	[SerializeField]
	private int _maxNumberOfFeedbacks = 3;

	[SerializeField]
	private float _distanceBetweenElements = 20f;

	[SerializeField]
	private float _elementExtraBorderSize = 33f;

	[SerializeField]
	private float _defaultFeedbackTimeToLive = 10f;

	[SerializeField]
	private float _delayBetweenFeedbacks = 0.2f;

	[SerializeField]
	private Transform _containerTransform;

	[SerializeField]
	private FeedbackPrefabByType[] _prefabs;

	private bool _updatePositions;

	private float _nextTimeToAddFeedback;

	private List<FeedbackItem> _activeItems;

	private Dictionary<int, Stack<FeedbackItem>> _pool;

	private List<FeedbackEvent> _feedbackQueue;

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUIFeedbackReportingEvents.FeedbackItemToDisplay, OnFeedbackItemToDisplay);
		_eventList.AddEvent(WAUIFeedbackReportingEvents.FeedbackSalvagedReceived, OnFeedbackSalvagedReceived);
		_eventList.AddEvent(WAUIFeedbackReportingEvents.FeedbackScanReceived, OnFeedbackScanReceived);
		_eventList.AddEvent(WAUIFeedbackReportingEvents.FeedbackScanDetailsReceived, OnFeedbackScanDetailsReceived);
		_eventList.AddEvent(WAUIFeedbackReportingEvents.CancelFeedbackItem, OnCancelFeedbackItem);
	}

	protected override void ProtectedInit()
	{
		_activeItems = new List<FeedbackItem>(_maxNumberOfFeedbacks);
		_feedbackQueue = new List<FeedbackEvent>(10);
		_pool = new Dictionary<int, Stack<FeedbackItem>>(_prefabs.Length, Comparers.IntComparer);
	}

	protected override void ProtectedDispose()
	{
		_pool.Clear();
		_pool = null;
		_activeItems.Clear();
		_activeItems = null;
		_feedbackQueue.Clear();
		_feedbackQueue = null;
	}

	private void OnFeedbackScanReceived(object[] feedbackReceived)
	{
		if (feedbackReceived == null || feedbackReceived.Length != 1)
		{
			WALogger.Error<FeedbackScreen>("Trying to trigger a feedback scan to display event without passing any scan feedback events! Please pass one instance of FeedbackScanEvent class to properly display data!");
			return;
		}
		FeedbackScanEvent evt = (FeedbackScanEvent)feedbackReceived[0];
		if (!TryIncrementingActiveScanFeedback(evt))
		{
			EnqueueScanFeedback(evt);
		}
	}

	private bool TryIncrementingActiveScanFeedback(FeedbackScanEvent evt)
	{
		for (int i = 0; i < _activeItems.Count; i++)
		{
			FeedbackItem feedbackItem = _activeItems[i];
			if (feedbackItem.Event.EventType == FeedbackEventType.Scan)
			{
				FeedbackScanEvent feedbackScanEvent = feedbackItem.Event as FeedbackScanEvent;
				feedbackScanEvent.Quantity += evt.Quantity;
				feedbackItem.Title.SetText($"You gained {feedbackScanEvent.Quantity} knowledge.");
				feedbackItem.DespawnTime = Time.time + feedbackItem.TimeToLive;
				return true;
			}
		}
		return false;
	}

	private void EnqueueScanFeedback(FeedbackScanEvent evt)
	{
		for (int i = 0; i < _feedbackQueue.Count; i++)
		{
			FeedbackEvent feedbackEvent = _feedbackQueue[i];
			if (feedbackEvent.EventType == FeedbackEventType.Scan)
			{
				FeedbackScanEvent feedbackScanEvent = feedbackEvent as FeedbackScanEvent;
				feedbackScanEvent.Quantity += evt.Quantity;
				return;
			}
		}
		_feedbackQueue.Add(new FeedbackScanEvent(evt.Quantity));
	}

	private void OnFeedbackSalvagedReceived(object[] feedbackReceived)
	{
		if (feedbackReceived == null || feedbackReceived.Length != 1)
		{
			WALogger.Error<FeedbackScreen>("Trying to trigger a feedback salvage to display event without passing any salvage feedback events! Please pass one instance of ReceiveSalvageFeedback class to properly display data!");
			return;
		}
		ReceiveSalvageFeedback evt = (ReceiveSalvageFeedback)feedbackReceived[0];
		if (!TryIncrementingActiveSalvageFeedback(evt))
		{
			EnqueueSalvageFeedback(evt);
		}
	}

	private bool TryIncrementingActiveSalvageFeedback(ReceiveSalvageFeedback evt)
	{
		for (int i = 0; i < _activeItems.Count; i++)
		{
			FeedbackItem feedbackItem = _activeItems[i];
			if (feedbackItem.Event.EventType == FeedbackEventType.Salvage)
			{
				FeedbackSalvageEvent feedbackSalvageEvent = feedbackItem.Event as FeedbackSalvageEvent;
				if (!(feedbackSalvageEvent.ItemTypeId != evt.itemTypeId))
				{
					InventoryItemData inventoryItemData = InventoryItemManager.Instance.LookupItem(evt.itemTypeId);
					feedbackSalvageEvent.Quantity += evt.quantity;
					feedbackItem.Title.SetText($"Salvaged {inventoryItemData.name} x{feedbackSalvageEvent.Quantity}");
					feedbackItem.DespawnTime = Time.time + feedbackItem.TimeToLive;
					return true;
				}
			}
		}
		return false;
	}

	private void EnqueueSalvageFeedback(ReceiveSalvageFeedback evt)
	{
		for (int i = 0; i < _feedbackQueue.Count; i++)
		{
			FeedbackEvent feedbackEvent = _feedbackQueue[i];
			if (feedbackEvent.EventType == FeedbackEventType.Salvage)
			{
				FeedbackSalvageEvent feedbackSalvageEvent = feedbackEvent as FeedbackSalvageEvent;
				if (!(feedbackSalvageEvent.ItemTypeId != evt.itemTypeId))
				{
					feedbackSalvageEvent.Quantity += evt.quantity;
					return;
				}
			}
		}
		_feedbackQueue.Add(new FeedbackSalvageEvent(evt.itemTypeId, evt.quantity));
	}

	private void OnFeedbackItemToDisplay(object[] feedbackEvents)
	{
		if (feedbackEvents == null || feedbackEvents.Length == 0)
		{
			WALogger.Error<FeedbackScreen>("Trying to trigger a feedback item to display event without passing any feedback events! Please pass one or more instances of FeedbackEvent class to properly display data!");
			return;
		}
		foreach (object obj in feedbackEvents)
		{
			if (obj == null)
			{
				WALogger.Error<FeedbackScreen>("Trying to trigger a feedback item to display event with a null event value!");
			}
			else if (!(obj is FeedbackGeneralEvent item))
			{
				WALogger.Error<FeedbackScreen>("Passed a parameter to the FeedbackItemToDisplay event that is not of type FeedbackGeneralEvent! This is not supported!");
			}
			else
			{
				_feedbackQueue.Add(item);
			}
		}
	}

	private void OnFeedbackScanDetailsReceived(object[] feedbackEvents)
	{
		if (feedbackEvents == null || feedbackEvents.Length == 0)
		{
			WALogger.Error<FeedbackScreen>("Trying to trigger a scan details feedback event without passing any feedback events! Please pass one or more instances of FeedbackEvent class to properly display data!");
			return;
		}
		foreach (object obj in feedbackEvents)
		{
			if (obj == null)
			{
				WALogger.Error<FeedbackScreen>("Trying to trigger a scan details feedback item to display event with a null event value!");
			}
			else if (!(obj is FeedbackScanInfosEvent item))
			{
				WALogger.Error<FeedbackScreen>("Passed a parameter to the FeedbackScanDetailsReceived event that is not of type FeedbackScanInfosEvent! This is not supported!");
			}
			else
			{
				_feedbackQueue.Add(item);
			}
		}
	}

	private void OnCancelFeedbackItem(object[] feedbackEvent)
	{
		if (feedbackEvent == null || feedbackEvent.Length != 1)
		{
			WALogger.Error<FeedbackScreen>("Trying to cancel a feedback item to display event without passing any feedback events! Please pass one instance of FeedbackEvent class to cancel it!");
			return;
		}
		FeedbackEvent feedbackEvent2 = (FeedbackEvent)feedbackEvent[0];
		if (feedbackEvent2 == null)
		{
			WALogger.Error<FeedbackScreen>("Trying to cancel a feedback event but did not pass an instance of FeedbackEvent to the WAUIFeedbackReportingEvents.CancelFeedbackItem event! Please verify this event is being fired correctly!");
		}
		if (!_feedbackQueue.Contains(feedbackEvent2))
		{
			WALogger.Error<FeedbackScreen>("Trying to cancel a feedback event that is not currently in the _feedbackQueue list! Is this a race condition, removing it twice?");
		}
		else
		{
			_feedbackQueue.Remove(feedbackEvent2);
		}
	}

	private void Update()
	{
		float time = Time.time;
		if (_updatePositions)
		{
			UpdateTargetPositions();
			_updatePositions = false;
		}
		if (_nextTimeToAddFeedback < time && _feedbackQueue.Count > 0 && _activeItems.Count < _maxNumberOfFeedbacks)
		{
			SpawnFeedbackItem();
			_updatePositions = true;
			_nextTimeToAddFeedback = time + _delayBetweenFeedbacks;
		}
		for (int i = 0; i < _activeItems.Count; i++)
		{
			FeedbackItem feedbackItem = _activeItems[i];
			if (feedbackItem.DespawnTime < time)
			{
				feedbackItem.Hide(DespawnFeedbackItem);
				_activeItems.RemoveAt(i);
				UpdateTargetPositions();
				i--;
			}
			else
			{
				feedbackItem.transform.localPosition = Vector3.Lerp(feedbackItem.transform.localPosition, feedbackItem.TargetPosition, 0.2f);
			}
		}
	}

	private void SpawnFeedbackItem()
	{
		FeedbackEvent data = _feedbackQueue[0];
		_feedbackQueue.RemoveAt(0);
		int type = (int)data.Type;
		if (!_pool.ContainsKey(type))
		{
			_pool.Add(type, new Stack<FeedbackItem>(_maxNumberOfFeedbacks + 1));
		}
		FeedbackItem feedbackItem;
		if (_pool[type].Count > 0)
		{
			feedbackItem = _pool[type].Pop();
		}
		else
		{
			FeedbackPrefabByType feedbackPrefabByType = Array.Find(_prefabs, (FeedbackPrefabByType p) => p.Type == data.Type);
			feedbackItem = UIObjectFactory.Create<FeedbackItem>(UIElementType.ScreenComponent, UIFillType.KeepOriginalAnchoring, _containerTransform, isObjectActive: true, feedbackPrefabByType.PrefabName);
			feedbackItem.Type = data.Type;
		}
		float num = data.TimeToLive;
		if (data.OnFeedbackItemSpawned != null)
		{
			num = -1f;
		}
		else if (num == 0f)
		{
			num = _defaultFeedbackTimeToLive;
		}
		feedbackItem.transform.localPosition = new Vector3(0f, -500f, 0f);
		feedbackItem.TimeToLive = num;
		feedbackItem.DespawnTime = Time.time + num;
		feedbackItem.Event = data;
		SetFeedbackProperties(data, feedbackItem);
		feedbackItem.Show();
		_activeItems.Add(feedbackItem);
		if (data.OnFeedbackItemSpawned != null)
		{
			feedbackItem.KeepAlive();
			data.OnFeedbackItemSpawned(feedbackItem);
		}
	}

	private void SetFeedbackProperties(FeedbackEvent evt, FeedbackItem item)
	{
		switch (evt.EventType)
		{
		case FeedbackEventType.General:
		{
			FeedbackGeneralEvent feedbackGeneralEvent = evt as FeedbackGeneralEvent;
			item.Title.SetText(feedbackGeneralEvent.Title);
			item.Description.SetText(feedbackGeneralEvent.Description);
			if (evt.Type != 0)
			{
				FeedbackIconItem feedbackIconItem2 = item as FeedbackIconItem;
				feedbackIconItem2.Icon.sprite = feedbackGeneralEvent.Icon;
			}
			break;
		}
		case FeedbackEventType.Salvage:
		{
			FeedbackSalvageEvent feedbackSalvageEvent = evt as FeedbackSalvageEvent;
			InventoryItemData inventoryItemData = InventoryItemManager.Instance.LookupItem(feedbackSalvageEvent.ItemTypeId);
			item.Title.SetText($"Salvaged {inventoryItemData.name} x{feedbackSalvageEvent.Quantity}");
			item.Description.SetText(string.Empty);
			FeedbackIconItem feedbackIconItem = item as FeedbackIconItem;
			feedbackIconItem.Icon.sprite = InventoryIconManager.Instance.GetIconSprite(inventoryItemData.iconName);
			break;
		}
		case FeedbackEventType.Scan:
		{
			FeedbackScanEvent feedbackScanEvent = evt as FeedbackScanEvent;
			item.Title.SetText($"You gained {feedbackScanEvent.Quantity} knowledge.");
			item.Description.SetText(string.Empty);
			break;
		}
		case FeedbackEventType.ScanDetails:
		{
			FeedbackScanInfosEvent feedbackScanInfosEvent = evt as FeedbackScanInfosEvent;
			FeedbackScanInfosItem feedbackScanInfosItem = item as FeedbackScanInfosItem;
			feedbackScanInfosItem.Setup(feedbackScanInfosEvent.ScannableData);
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void UpdateTargetPositions()
	{
		float num = 0f;
		for (int i = 0; i < _activeItems.Count; i++)
		{
			num += _activeItems[i].ContainerRect.rect.height;
			num += _elementExtraBorderSize;
			if (i != _activeItems.Count - 1)
			{
				num += _distanceBetweenElements;
			}
		}
		float num2 = num * 0.5f;
		for (int j = 0; j < _activeItems.Count; j++)
		{
			FeedbackItem feedbackItem = _activeItems[j];
			float num3 = (feedbackItem.ContainerRect.rect.height + _elementExtraBorderSize) * 0.5f;
			feedbackItem.TargetPosition.y = num2 - num3;
			num2 = feedbackItem.TargetPosition.y - num3 - _distanceBetweenElements;
		}
	}

	private void DespawnFeedbackItem(FeedbackItem item)
	{
		item.gameObject.SetActive(value: false);
		_pool[(int)item.Type].Push(item);
	}
}
