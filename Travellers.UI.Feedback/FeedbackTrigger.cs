using System;
using System.Collections;
using GameDBClient;
using UnityEngine;
using WAUtilities.Logging;

namespace Travellers.UI.Feedback;

public class FeedbackTrigger : MonoBehaviour
{
	[SerializeField]
	private string _gameDBFeedbackKey;

	[SerializeField]
	private FeedbackTriggerType _type;

	[SerializeField]
	private bool _onlyOnce;

	private FeedbackSystem _gameDbRow;

	private Action<FeedbackItem> _itemSpawnedCallback;

	private FeedbackEvent _triggerStayFeedbackEvent;

	private FeedbackItem _triggerStayFeedbackItem;

	private const string PlayerTag = "Player";

	private const string UniqueIdFormat = "FeedbackTrigger-{0}-{1}-{2}-{3}";

	private string _uniqueIdentifier;

	private void Awake()
	{
		_gameDbRow = Singleton<GameDBAccessor>.Instance.ClientGameDB.FeedbackSystemTable.GetByKey(_gameDBFeedbackKey);
		if (_type == FeedbackTriggerType.Stay)
		{
			_itemSpawnedCallback = OnFeedbackItemSpawned;
		}
	}

	private void OnEnable()
	{
		if (_onlyOnce && string.IsNullOrEmpty(_uniqueIdentifier))
		{
			StartCoroutine(GetUniqueIdentifier());
		}
	}

	private IEnumerator GetUniqueIdentifier()
	{
		while (!LocalPlayer.Exists)
		{
			yield return null;
		}
		string playerId = LocalPlayer.Instance.PlayerId;
		_uniqueIdentifier = $"FeedbackTrigger-{playerId}-{_gameDBFeedbackKey}-{_type.ToString()}-{base.transform.position.sqrMagnitude}";
		if (PlayerPrefs.HasKey(_uniqueIdentifier))
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (_type != FeedbackTriggerType.Exit && (!_onlyOnce || !string.IsNullOrEmpty(_uniqueIdentifier)) && CheckIfColliderIsValid(other))
		{
			SendFeedback();
			if (_onlyOnce && _type == FeedbackTriggerType.Enter)
			{
				PlayerPrefs.SetInt(_uniqueIdentifier, 1);
				base.gameObject.SetActive(value: false);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (_type == FeedbackTriggerType.Enter || (_onlyOnce && string.IsNullOrEmpty(_uniqueIdentifier)) || !CheckIfColliderIsValid(other))
		{
			return;
		}
		if (_type == FeedbackTriggerType.Stay)
		{
			if (_triggerStayFeedbackEvent != null)
			{
				Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIFeedbackReportingEvents.CancelFeedbackItem, _triggerStayFeedbackEvent);
				_triggerStayFeedbackEvent = null;
			}
			else if (_triggerStayFeedbackItem != null)
			{
				_triggerStayFeedbackItem.Expire();
				_triggerStayFeedbackItem = null;
			}
		}
		else
		{
			SendFeedback();
		}
		if (_onlyOnce)
		{
			PlayerPrefs.SetInt(_uniqueIdentifier, 1);
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnFeedbackItemSpawned(FeedbackItem item)
	{
		_triggerStayFeedbackEvent = null;
		_triggerStayFeedbackItem = item;
	}

	private bool CheckIfColliderIsValid(Collider other)
	{
		if (other.attachedRigidbody.gameObject != LocalPlayer.Instance.playerGameObject)
		{
			return false;
		}
		if (other.tag != "Player")
		{
			return false;
		}
		return true;
	}

	private void SendFeedback()
	{
		if (_gameDbRow == null)
		{
			WALogger.Error<FeedbackTrigger>("Could not find Game DB Row with key {0}. Please verify this key exists and has been typed correctly.", new object[1] { _gameDBFeedbackKey });
			return;
		}
		Sprite icon = null;
		if (!string.IsNullOrEmpty(_gameDbRow.IconPathVal))
		{
			icon = _gameDbRow.IconObjectVal as Sprite;
		}
		FeedbackGeneralEvent feedbackGeneralEvent = new FeedbackGeneralEvent(_gameDbRow.TitleVal, _gameDbRow.DescriptionVal, icon, _gameDbRow.PrefabTypeVal, _gameDbRow.DurationVal, _itemSpawnedCallback);
		if (_type == FeedbackTriggerType.Stay)
		{
			_triggerStayFeedbackEvent = feedbackGeneralEvent;
		}
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIFeedbackReportingEvents.FeedbackItemToDisplay, feedbackGeneralEvent);
	}
}
