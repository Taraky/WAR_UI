using System;
using System.Collections.Generic;
using Bossa.Travellers.Utils.ErrorHandling;
using Bossa.Travellers.World;
using Bossa.Travellers.World.Observer;
using Travellers.UI.DebugExtensions;
using Travellers.UI.Framework;
using Travellers.UI.Chat;
using UnityEngine;

namespace Travellers.UI.HUDMessaging;

[InjectedSystem(InjectionType.Real)]
public class HUDMessagingSystem : UISystem, IHUDMessagingSystem
{
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

	private ClientMessagingCommandRouter _clientMessagingCommandRouter;

	private ChatSpeak _chatSpeak;

	private readonly IDebugExtensionsSystem _uiDebugExtensionSystem;

	public bool UnityDevConsoleVisibility { get; set; }

	public HUDMessagingSystem(IDebugExtensionsSystem uiDebugExtensionSystem)
	{
		_uiDebugExtensionSystem = uiDebugExtensionSystem;
		UnityDevConsoleVisibility = true;
		_clientMessagingCommandRouter = new ClientMessagingCommandRouter();
	}

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUIFeedbackReportingEvents.AddMessageToSystem, OnAddMessageEvent);
		_eventList.AddEvent(WAUIFeedbackReportingEvents.RegisterChatVisualiser, OnRegisterChatToServerWriter);
		_eventList.AddEvent(WAUIFeedbackReportingEvents.DeregisterChatVisualiser, OnDeregisterChatToServerWriter);
	}

	public override void Init()
	{
	}

	private void OnAddMessageEvent(object[] obj)
	{
		OSDMessage msg = (OSDMessage)obj[0];
		AddMessage(msg);
	}

	private void OnRegisterChatToServerWriter(object[] obj)
	{
		ChatSpeak chatSpeak = (ChatSpeak)obj[0];
		_chatSpeak = chatSpeak;
	}

	private void OnDeregisterChatToServerWriter(object[] obj)
	{
		_chatSpeak = null;
	}

	public void InitialiseForCharacter(string characterUid)
	{
		MutedPlayerWarehouse.SetCurrentWarehouse(characterUid);
	}

	public bool IsPlayerAdmin()
	{
		return _chatSpeak != null && _chatSpeak.IsAdmin;
	}

	public void AddMessage(OSDMessage msg)
	{
		DumpToLogs(msg);
		if (IsMuted(msg))
		{
			return;
		}
		_masterMessageList.Add(msg);
		foreach (KeyValuePair<HUDMessageType, MessageReferenceCentre> item in _messageListLookup)
		{
			item.Value.AddMessage(msg);
		}
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIFeedbackReportingEvents.NewMessageToDisplay, null);
	}

	private bool IsMuted(OSDMessage msg)
	{
		if (!UserCommandReference.ServerMessageTypeIsChat(msg.MessageType))
		{
			return false;
		}
		return _clientMessagingCommandRouter.IsMessageSourceMuted(msg.From);
	}

	public void SendChatMessage(UserInputCommand userCommand)
	{
		if (!userCommand.Command.IsDevOnlyDebugCommand)
		{
			SendUserCommand(userCommand);
		}
	}

	public void SendDebugCommand(UserInputCommand userCommand)
	{
		if (userCommand.Command.IsDebugCommand)
		{
			SendUserCommand(userCommand);
		}
	}

	private void SendUserCommand(UserInputCommand userCommand)
	{
		if (userCommand.Command.IsClientOnlyGameCommand)
		{
			_clientMessagingCommandRouter.CheckMessageForCommands(userCommand);
		}
		else
		{
			if (_chatSpeak == null)
			{
				return;
			}
			if (userCommand.Command.IsDevOnlyDebugCommand)
			{
				if (!_chatSpeak.IsAdmin)
				{
					return;
				}
				List<string> list = _uiDebugExtensionSystem.Process(userCommand.FullMessage);
				if (list.Count > 0)
				{
					foreach (string item in list)
					{
						_chatSpeak.SendDebugCommand(item);
					}
					return;
				}
				_chatSpeak.SendDebugCommand(userCommand.FullMessage);
			}
			else if (userCommand.Command.IsDebugCommand)
			{
				_chatSpeak.SendDebugCommand(userCommand.FullMessage);
			}
			else if (!userCommand.EmptyMessage && !userCommand.OnlyRoomSwitch)
			{
				_chatSpeak.SendSocialWorkerMessage(userCommand.FullMessage);
			}
		}
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
		ClearAllMessages();
		MutedPlayerWarehouse.SetCurrentWarehouse(null);
	}

	public List<OSDMessage> CollectMessages(HUDMessageType messageSubscriptionType, int subscriberIndex)
	{
		if (!_messageListLookup.TryGetValue(messageSubscriptionType, out var value))
		{
			throw new ArgumentException("HUDMessage subscription type doesn't exist. Cannot collect messages");
		}
		return value.CollectMessages(subscriberIndex);
	}

	private void DumpToLogs(OSDMessage message)
	{
		if (message.MessageType == MessageType.ServerError)
		{
			ReportServerError(message);
		}
	}

	private void ReportServerError(OSDMessage message)
	{
		string errorMsg = $"The server is in an unexpected state and has returned an error message. Please grab the client logs and notify the dev team so that they can grab the appropriate server logs.\n\nError: {message.Message}";
		ReleaseAssert.IsTrue<HUDMessagingSystem>(condition: false, () => errorMsg);
	}

	protected override void Dispose()
	{
	}

	public override void ControlledUpdate()
	{
		if (Debug.isDebugBuild)
		{
			Debug.developerConsoleVisible = UnityDevConsoleVisibility;
		}
	}
}
