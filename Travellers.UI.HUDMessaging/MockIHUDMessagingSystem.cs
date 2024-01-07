using System;
using System.Collections.Generic;
using Bossa.Travellers.World;
using Bossa.Travellers.World.Observer;
using Improbable;
using Travellers.UI.Framework;
using Travellers.UI.Chat;
using Travellers.UI.Utility;
using UnityEngine;

namespace Travellers.UI.HUDMessaging;

[InjectedSystem(InjectionType.Mock)]
public class MockIHUDMessagingSystem : UISystem, IHUDMessagingSystem
{
	private static readonly string[] _messages = new string[12]
	{
		"hello", "sup", "innit", "wat?!", "is anybody out there?", "yes", "no", "alliances are awesome", "no way", "yup",
		"who wants to build a ship?", "i just found a bug!"
	};

	private static readonly string[] _fromArray = new string[11]
	{
		"Brian", "Legsy", "Old Smooty", "Negative Dave", "Hardnuts", "Liam Jumpers", "Small Brian", "Tiny Brian", "Absolutely miniscule Brian", "Holy Baby Jesus",
		"Ten tons of Craig"
	};

	private List<OSDMessage> _masterMessageList = new List<OSDMessage>();

	private Dictionary<HUDMessageType, MessageReferenceCentre> _messageListLookup = new Dictionary<HUDMessageType, MessageReferenceCentre>
	{
		{
			HUDMessageType.Event,
			new MessageReferenceCentre(burnAfterReading: true)
		},
		{
			HUDMessageType.Persistent,
			new MessageReferenceCentre(burnAfterReading: false)
		}
	};

	private ChatSpeak _chatSpeak;

	private bool _allianceRadioActive;

	public bool UnityDevConsoleVisibility { get; set; }

	protected override void AddListeners()
	{
	}

	public override void Init()
	{
	}

	public void InitialiseForCharacter(string characterUid)
	{
		throw new NotImplementedException();
	}

	public bool IsPlayerAdmin()
	{
		return true;
	}

	public void SendChatMessage(UserInputCommand chatMessage)
	{
		throw new NotImplementedException();
	}

	public void SendDebugCommand(UserInputCommand debugCommand)
	{
		throw new NotImplementedException();
	}

	public void AddMessage(OSDMessage msg)
	{
		_masterMessageList.Add(msg);
		foreach (KeyValuePair<HUDMessageType, MessageReferenceCentre> item in _messageListLookup)
		{
			item.Value.AddMessage(msg);
		}
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIFeedbackReportingEvents.NewMessageToDisplay, null);
	}

	public int SubscribeToMessages(HUDMessageType messageSubscriptionType)
	{
		if (!_messageListLookup.TryGetValue(messageSubscriptionType, out var value))
		{
			throw new ArgumentException("HUDMessage subscription type doesn't exist. Cannot add subscriber");
		}
		return value.AddSubscriber();
	}

	public void UnsubscribeFromMessages(HUDMessageType messageSubscriptionType, int subscriberIndex)
	{
		if (_messageListLookup.TryGetValue(messageSubscriptionType, out var value))
		{
			value.RemoveSubscriber(subscriberIndex);
		}
	}

	public void ClearLogs(HUDMessageType messageSubscriptionType, int subscriberIndex)
	{
		if (_messageListLookup.TryGetValue(messageSubscriptionType, out var value))
		{
			value.ClearSubscribersMessages(subscriberIndex);
		}
	}

	public void ClearAllMessages()
	{
		foreach (KeyValuePair<HUDMessageType, MessageReferenceCentre> item in _messageListLookup)
		{
			item.Value.ClearAllSubscribersAndMessages();
		}
		_masterMessageList.Clear();
	}

	public void ResetCharacter()
	{
		throw new NotImplementedException();
	}

	public List<OSDMessage> CollectMessages(HUDMessageType messageSubscriptionType, int subscriberIndex)
	{
		if (!_messageListLookup.TryGetValue(messageSubscriptionType, out var value))
		{
			throw new ArgumentException("HUDMessage subscription type doesn't exist. Cannot collect messages");
		}
		return value.CollectMessages(subscriberIndex);
	}

	protected override void Dispose()
	{
	}

	public override void ControlledUpdate()
	{
		if (Time.frameCount % 20 == 0)
		{
			int messageType = UnityEngine.Random.Range(0, Enum.GetNames(typeof(MessageType)).Length);
			OSDMessage msg = new OSDMessage(RandomHelper.Random(_messages), RandomHelper.Random(_fromArray), default(EntityId), (MessageType)messageType, (long)Epoch.Now.Seconds);
			AddMessage(msg);
		}
	}
}
