using System;
using System.Collections.Generic;
using Bossa.Travellers.World;

namespace Travellers.UI.HUDMessaging;

public class MessageQueueController
{
	protected readonly IHUDMessageDisplay _messageDisplay;

	protected readonly IHUDMessageQueue _messageQueue;

	private readonly HashSet<int> _existingMessageIds = new HashSet<int>();

	private int _messageSystemSubscriberId = -1;

	private readonly HUDMessageType _messageSystemType;

	private readonly IHUDMessagingSystem _messagingSystem;

	private readonly HashSet<MessageType> _includedMessageTypes = new HashSet<MessageType>();

	protected bool _isActive;

	public MessageQueueController(IHUDMessageDisplay messageDisplay, IHUDMessageQueue messageQueue, HUDMessageType messageSystemType, IHUDMessagingSystem messagingSystem, params MessageType[] typesToInclude)
	{
		_messageDisplay = messageDisplay;
		_messageQueue = messageQueue;
		_messageSystemType = messageSystemType;
		_messagingSystem = messagingSystem;
		if (typesToInclude.Length == 0)
		{
			foreach (object value in Enum.GetValues(typeof(MessageType)))
			{
				_includedMessageTypes.Add((MessageType)value);
			}
		}
		else
		{
			foreach (MessageType item in typesToInclude)
			{
				_includedMessageTypes.Add(item);
			}
		}
		SetMessageQueue();
		SubscribeToMessageSystem();
		_isActive = true;
	}

	public void ExcludeTypes(params MessageType[] types)
	{
		foreach (MessageType item in types)
		{
			_includedMessageTypes.Remove(item);
		}
	}

	protected void SetMessageQueue()
	{
		_messageDisplay.SetMessageQueue(_messageQueue);
	}

	private void SubscribeToMessageSystem()
	{
		_messageSystemSubscriberId = _messagingSystem.SubscribeToMessages(_messageSystemType);
	}

	public void UnsubscribeFromMessageSystem()
	{
		_messagingSystem.UnsubscribeFromMessages(_messageSystemType, _messageSystemSubscriberId);
	}

	public void CheckAndPublishNewMessages(Func<OSDMessage, string> parseMessageFunc, bool forceRebuild = false)
	{
		if (!_isActive)
		{
			return;
		}
		List<OSDMessage> list = _messagingSystem.CollectMessages(_messageSystemType, _messageSystemSubscriberId);
		List<ColourParsedOSDMessage> list2 = new List<ColourParsedOSDMessage>();
		bool flag = false;
		foreach (OSDMessage item in list)
		{
			if (_includedMessageTypes.Contains(item.MessageType))
			{
				if (_existingMessageIds.Add(item.ID))
				{
					flag = true;
				}
				list2.Add(new ColourParsedOSDMessage(item, parseMessageFunc));
			}
		}
		if (flag || forceRebuild)
		{
			_messageQueue.SetMessages(list2);
			_messageDisplay.RebuildMessageView();
		}
	}

	public void ClearMessages()
	{
		_messagingSystem.ClearLogs(_messageSystemType, _messageSystemSubscriberId);
	}
}
