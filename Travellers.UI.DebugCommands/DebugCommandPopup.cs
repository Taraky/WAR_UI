using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Utils;
using TMPro;
using Travellers.UI.DebugOSD;
using Travellers.UI.Framework;
using Travellers.UI.HUDMessaging;
using Travellers.UI.Chat;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Travellers.UI.DebugCommands;

[InjectableClass]
public class DebugCommandPopup : UIPopup
{
	[SerializeField]
	private TMP_InputField _inputField;

	[SerializeField]
	private DebugChatRoom _debugChatRoom;

	private InputHistory _inputHistory;

	private MessageQueueController _messageQueueController;

	private IHUDMessagingSystem _hudMessagingSystem;

	private IHelpCommandSystem _helpCommandSystem;

	private UIColourAndTextReferenceData _uiColourAndTextReferenceData;

	private Dictionary<string, List<Utils.Tuple<string, string>>> _helpCategoryCommandDictionary = new Dictionary<string, List<Utils.Tuple<string, string>>>();

	private readonly List<string> _helpCommands = new List<string>();

	private bool _updateCaret;

	private bool _waitFrame;

	private int _messageSubscriberIndex;

	[InjectableMethod]
	public void InjectDependencies(IHUDMessagingSystem hudMessagingSystem, IHelpCommandSystem helpCommandSystem, UIColourAndTextReferenceData colourAndTextReferenceData)
	{
		_hudMessagingSystem = hudMessagingSystem;
		_helpCommandSystem = helpCommandSystem;
		_uiColourAndTextReferenceData = colourAndTextReferenceData;
	}

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUIFeedbackReportingEvents.NewMessageToDisplay, OnNewMessage);
	}

	protected override void ProtectedInit()
	{
		_helpCommandSystem.RetrieveHelpCommandsAsync(AssignHelpDictionary);
		_messageQueueController = new MessageQueueController(_debugChatRoom, new CappedMessageQueue(_debugChatRoom.MaxMessages), HUDMessageType.Persistent, _hudMessagingSystem);
		_inputHistory = new InputHistory();
		_inputField.onSubmit.AddListener(OnSubmit);
		_muteListenersWhenInactive = false;
		ChangeFocus(shouldFocus: true);
		UpdateMessageDisplay();
	}

	protected override void ProtectedDispose()
	{
		_messageQueueController.UnsubscribeFromMessageSystem();
	}

	public InputHistory RetrieveCurrentInputHistory()
	{
		return _inputHistory;
	}

	public void RestoreInputHistory(InputHistory inputHistory)
	{
		_inputHistory = inputHistory;
	}

	private void OnNewMessage(object[] obj)
	{
		UpdateMessageDisplay();
	}

	private void UpdateMessageDisplay()
	{
		_messageQueueController.CheckAndPublishNewMessages(FormatMessageColours);
	}

	private string FormatMessageColours(OSDMessage message)
	{
		if (!string.IsNullOrEmpty(message.From))
		{
			return $"<color=#{_uiColourAndTextReferenceData.UIColours.GetColourHex(message.FromColourType)}>[CHEAT]{message.From}: </color><color=#{_uiColourAndTextReferenceData.UIColours.GetColourHex(message.MessageColourType)}>{message.Message}</color> ";
		}
		return $"<color=#{_uiColourAndTextReferenceData.UIColours.GetColourHex(message.MessageColourType)}>[CHEAT]{message.Message}</color> ";
	}

	private void AssignHelpDictionary(Dictionary<string, List<Utils.Tuple<string, string>>> helpDictionary)
	{
		if (_helpCategoryCommandDictionary.Count != 0)
		{
			return;
		}
		_helpCategoryCommandDictionary = helpDictionary;
		foreach (KeyValuePair<string, List<Utils.Tuple<string, string>>> item in _helpCategoryCommandDictionary)
		{
			foreach (Utils.Tuple<string, string> item2 in item.Value)
			{
				_helpCommands.Add(item2.first);
			}
		}
		_helpCommands.Sort();
	}

	private void Update()
	{
		if (_inputField.text.StartsWith("\n"))
		{
			_inputField.text = _inputField.text.Substring(1);
		}
		CheckAutoCorrect();
		if (Input.GetKeyUp(KeyCode.UpArrow))
		{
			if (_inputHistory.IsCurrentCommand)
			{
				_inputHistory.SetUnfinishedCommand(_inputField.text);
			}
			_inputField.text = _inputHistory.GetPreviousCommand();
			UpdateCaret();
		}
		else if (Input.GetKeyUp(KeyCode.DownArrow))
		{
			_inputField.text = _inputHistory.GetNextCommand();
			UpdateCaret();
		}
	}

	private void CheckAutoCorrect()
	{
		if (!Input.GetKeyUp(KeyCode.Tab) || _helpCategoryCommandDictionary.Count <= 0 || !_inputField.text.StartsWith("/") || _inputField.text.Length <= 1)
		{
			return;
		}
		string command = _inputField.text.Substring(1);
		List<string> matchingCommands = (from x in _helpCommands
			where x.StartsWith(command)
			select (x)).ToList();
		if (matchingCommands.Count <= 0)
		{
			return;
		}
		string text = new string(matchingCommands.First().Substring(0, matchingCommands.Min((string s) => s.Length)).TakeWhile((char c, int i) => matchingCommands.All((string s) => s[i] == c))
			.ToArray());
		_inputField.text = "/" + text;
		UpdateCaret();
	}

	private void UpdateCaret()
	{
		_updateCaret = true;
	}

	public void ChangeFocus(bool shouldFocus)
	{
		if (shouldFocus)
		{
			_inputField.ActivateInputField();
			_inputField.Select();
			EventSystem.current.SetSelectedGameObject(_inputField.gameObject);
			_inputField.text = string.Empty;
		}
		else
		{
			EventSystem.current.SetSelectedGameObject(null);
			_inputField.DeactivateInputField();
		}
	}

	private void OnSubmit(string inputText)
	{
		if (inputText != string.Empty)
		{
			if (inputText == "/help")
			{
				UIWindowController.PushState(DebugHelpWindowUIState.Default);
			}
			else if (!Input.GetKeyDown(KeyCode.Escape))
			{
				if (!inputText.StartsWith("/"))
				{
					inputText = "/" + inputText;
				}
				UserInputCommand debugCommand = new UserInputCommand(inputText);
				_hudMessagingSystem.SendDebugCommand(debugCommand);
				_inputHistory.AddCommand(inputText);
			}
		}
		UIWindowController.PopState<DebugCommandPopupUIState>();
	}

	private void LateUpdate()
	{
		if (!_updateCaret)
		{
			return;
		}
		if (_waitFrame)
		{
			if (_inputField.caretPosition != _inputField.text.Length)
			{
				_inputField.caretPosition = _inputField.text.Length;
				_updateCaret = false;
				_waitFrame = false;
			}
		}
		else
		{
			_waitFrame = true;
		}
	}
}
