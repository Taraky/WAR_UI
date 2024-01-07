using System.Collections.Generic;
using Bossa.Travellers.Alliances;
using Bossa.Travellers.World;
using Travellers.UI.Framework;
using Travellers.UI.Chat;
using UnityEngine;
using WAUtilities.Logging;

namespace Travellers.UI.HUDMessaging;

[InjectableClass]
public class PlayerMessagingScreen : UIScreen
{
	protected enum MessageCategory
	{
		GeneralChat,
		AllianceChat,
		EventLog
	}

	[SerializeField]
	private ChatRoom _chatRoom;

	[SerializeField]
	private TextAlertArea _textAlertArea;

	[SerializeField]
	private GameObject _chatTabContainer;

	[SerializeField]
	private UIToggleGroup _toggleGroup;

	[SerializeField]
	private UIInventoryTabToggleController _generalChatToggle;

	[SerializeField]
	private UIInventoryTabToggleController _allianceChatToggle;

	private IHUDMessagingSystem _hudMessagingSystem;

	private Dictionary<MessageCategory, MessageQueueController> _messageQueueControllers = new Dictionary<MessageCategory, MessageQueueController>();

	private Dictionary<MessageCategory, ChatMessageQueueController> _chatMessageQueueControllers = new Dictionary<MessageCategory, ChatMessageQueueController>();

	private HashSet<MessageCategory> _allowedCategories = new HashSet<MessageCategory>
	{
		MessageCategory.GeneralChat,
		MessageCategory.EventLog
	};

	private ChatState _currentChatState;

	private UIColourAndTextReferenceData _uiColourAndTextReferenceData;

	private IAllianceClient _allianceClient;

	private MessageCategory _currentChatType;

	[InjectableMethod]
	public void InjectDependencies(IHUDMessagingSystem playerMessagingSystem, IAllianceClient allianceClient, UIColourAndTextReferenceData colourData)
	{
		_allianceClient = allianceClient;
		_uiColourAndTextReferenceData = colourData;
		_hudMessagingSystem = playerMessagingSystem;
	}

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUIFeedbackReportingEvents.NewMessageToDisplay, OnNewMessage);
		_eventList.AddEvent(WAUIFeedbackReportingEvents.ChangeChatState, OnChatStateChange);
		_eventList.AddEvent(WAUIFeedbackReportingEvents.CheckChatState, OnChatStateCheck);
		_eventList.AddEvent(WAUIFeedbackReportingEvents.ClearChatLog, OnClearChatLogRequested);
		_eventList.AddEvent(WAUIInGameEvents.UIFloorReceiverClicked, OnFloorReceiverClicked);
	}

	protected override void ProtectedInit()
	{
		InitialiseMessageQueues();
		SetChatType(MessageCategory.GeneralChat);
		_generalChatToggle.SetButtonEvent(OnGeneralChatPressed);
		_allianceChatToggle.SetButtonEvent(OnAllianceChatPressed);
		_toggleGroup.AddToggleToGroup(_generalChatToggle);
		_toggleGroup.AddToggleToGroup(_allianceChatToggle);
		_toggleGroup.SetActiveButton(_generalChatToggle);
		ChangeChatState(ChatState.Disabled);
	}

	protected override void ProtectedDispose()
	{
		foreach (KeyValuePair<MessageCategory, MessageQueueController> messageQueueController in _messageQueueControllers)
		{
			messageQueueController.Value.UnsubscribeFromMessageSystem();
		}
	}

	protected override void Activate()
	{
		_chatTabContainer.SetActive(value: false);
	}

	public static void MakeChatRoomVisible(bool show)
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIFeedbackReportingEvents.ChangeChatState, new ChangeChatStateEvent(show ? ChatState.Enabled : ChatState.Disabled));
		FloorClickReceiverScreen.SetFloorClickReceiverScreenState(show);
	}

	private void OnNewMessage(object[] obj)
	{
		UpdateMessageControllers();
	}

	private void OnChatStateChange(object[] obj)
	{
		ChangeChatStateEvent changeChatStateEvent = (ChangeChatStateEvent)obj[0];
		ChangeChatState(changeChatStateEvent.NewState);
	}

	private void OnChatStateCheck(object[] obj)
	{
		CheckChatState();
	}

	private void OnClearChatLogRequested(object[] obj)
	{
		_messageQueueControllers[MessageCategory.GeneralChat].ClearMessages();
		_messageQueueControllers[MessageCategory.AllianceChat].ClearMessages();
		_messageQueueControllers[MessageCategory.EventLog].ClearMessages();
	}

	private void OnFloorReceiverClicked(object[] obj)
	{
		if (_currentChatState != 0)
		{
			UIWindowController.PopState<ChatRoomState>();
		}
	}

	private void OnGeneralChatPressed()
	{
		_toggleGroup.SetActiveButton(_generalChatToggle);
		SetChatType(MessageCategory.GeneralChat);
	}

	private void OnAllianceChatPressed()
	{
		_toggleGroup.SetActiveButton(_allianceChatToggle);
		SetChatType(MessageCategory.AllianceChat);
	}

	private void InitialiseMessageQueues()
	{
		_messageQueueControllers = new Dictionary<MessageCategory, MessageQueueController> { 
		{
			MessageCategory.EventLog,
			new MessageQueueController(_textAlertArea, new CappedMessageQueue(15), HUDMessageType.Event, _hudMessagingSystem)
		} };
		_chatMessageQueueControllers = new Dictionary<MessageCategory, ChatMessageQueueController>
		{
			{
				MessageCategory.AllianceChat,
				new ChatMessageQueueController(_chatRoom, new AllianceChatFilter(), new CappedMessageQueue(200), HUDMessageType.Persistent, _hudMessagingSystem, MessageType.Alliance)
			},
			{
				MessageCategory.GeneralChat,
				new ChatMessageQueueController(_chatRoom, new StandardChatFilter(), new CappedMessageQueue(200), HUDMessageType.Persistent, _hudMessagingSystem)
			}
		};
		_chatMessageQueueControllers[MessageCategory.GeneralChat].ExcludeTypes(MessageType.Alliance);
	}

	private void ChangeChatState(ChatState newState)
	{
		_currentChatState = newState;
		CheckChatState();
	}

	private void CheckChatState()
	{
		bool shouldFocus = _currentChatState == ChatState.Enabled;
		_chatTabContainer.SetActive(value: false);
		_chatRoom.SetObjectActive(shouldFocus);
		_textAlertArea.SetObjectActive(!shouldFocus);
		if (shouldFocus)
		{
			if (WAConfig.GetOrDefault<bool>(ConfigKeys.AlliancesEnabled))
			{
				_allianceClient.IsPlayerInAlliance().Then(delegate(bool result)
				{
					_chatTabContainer.SetActive(shouldFocus && result);
					SetCategoryPermission(MessageCategory.AllianceChat, result);
					SetChatType(_currentChatType);
				});
			}
			else
			{
				_chatTabContainer.SetActive(value: false);
			}
		}
		_chatRoom.ChangeFocus(shouldFocus);
	}

	private void SetCategoryPermission(MessageCategory categoryInQuestion, bool allow)
	{
		if (allow)
		{
			_allowedCategories.Add(categoryInQuestion);
		}
		else
		{
			_allowedCategories.Remove(categoryInQuestion);
		}
	}

	private void SetChatType(MessageCategory category)
	{
		if (!_allowedCategories.Contains(category))
		{
			if (_allowedCategories.Contains(MessageCategory.GeneralChat))
			{
				OnGeneralChatPressed();
			}
			else
			{
				WALogger.Error<PlayerMessagingScreen>(LogChannel.UI, "No default chat type to resort to. Something has gone really wrong", new object[0]);
			}
			return;
		}
		foreach (KeyValuePair<MessageCategory, ChatMessageQueueController> chatMessageQueueController in _chatMessageQueueControllers)
		{
			bool flag = chatMessageQueueController.Key == category;
			chatMessageQueueController.Value.SetChatActive(flag);
			if (flag)
			{
				chatMessageQueueController.Value.CheckAndPublishNewMessages(FormatMessageColours, forceRebuild: true);
				_currentChatType = category;
			}
		}
	}

	private void UpdateMessageControllers()
	{
		foreach (KeyValuePair<MessageCategory, MessageQueueController> messageQueueController in _messageQueueControllers)
		{
			messageQueueController.Value.CheckAndPublishNewMessages(FormatMessageColours);
		}
		foreach (KeyValuePair<MessageCategory, ChatMessageQueueController> chatMessageQueueController in _chatMessageQueueControllers)
		{
			chatMessageQueueController.Value.CheckAndPublishNewMessages(FormatMessageColours);
		}
	}

	private string FormatMessageColours(OSDMessage message)
	{
		if (!string.IsNullOrEmpty(message.From))
		{
			return $"<color=#{_uiColourAndTextReferenceData.UIColours.GetColourHex(message.FromColourType)}>{message.From}: </color><color=#{_uiColourAndTextReferenceData.UIColours.GetColourHex(message.MessageColourType)}>{message.Message}</color> ";
		}
		return $"<color=#{_uiColourAndTextReferenceData.UIColours.GetColourHex(message.MessageColourType)}>{message.Message}</color> ";
	}
}
