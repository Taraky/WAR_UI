using Bossa.Travellers.Social;
using TMPro;
using Travellers.UI.Framework;
using Travellers.UI.HUDMessaging;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Travellers.UI.Chat;

[InjectableClass]
public class ChatRoom : ScrollableMessageDisplay, IHUDChatMessageInput, IHUDMessageDisplay
{
	private const int MaxChatCharactersPerSecond = 50;

	private const int MaxMessageLength = 1024;

	private const int ChatRateLimitingWindowInSeconds = 30;

	[SerializeField]
	private TMP_InputField _chatInput;

	[SerializeField]
	private TextStylerTextMeshPro _roomText;

	private IHUDMessagingSystem _messagingSystem;

	private ChatRoomInputProcessor _commandProcessor;

	private InputHistory _inputHistory;

	private ChatLimiter _chatLimiter = new ChatLimiter(50, 1024, 30);

	private bool _updateCaret;

	private bool _waitFrame;

	private bool _submitAndClose;

	[InjectableMethod]
	public void InjectDependencies(IHUDMessagingSystem messagingSystem)
	{
		_messagingSystem = messagingSystem;
	}

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
		_chatInput.onSubmit.AddListener(OnSubmit);
		_inputHistory = new InputHistory();
		ChangeFocus(shouldFocus: true);
	}

	protected override void ProtectedDispose()
	{
	}

	public void SetInputProcessor(ChatRoomInputProcessor inputProcessor)
	{
		_commandProcessor = inputProcessor;
		UpdateRoomText();
	}

	private void Update()
	{
		if (_chatInput.text.StartsWith("\n"))
		{
			_chatInput.text = _chatInput.text.Substring(1);
		}
		if (Input.GetKeyUp(KeyCode.UpArrow))
		{
			if (_inputHistory.IsCurrentCommand)
			{
				_inputHistory.SetUnfinishedCommand(_chatInput.text);
			}
			_chatInput.text = _inputHistory.GetPreviousCommand();
			UpdateCaret();
		}
		else if (Input.GetKeyUp(KeyCode.DownArrow))
		{
			_chatInput.text = _inputHistory.GetNextCommand();
			UpdateCaret();
		}
		_chatLimiter.Update(Time.deltaTime);
	}

	private void UpdateCaret()
	{
		_updateCaret = true;
	}

	public void ChangeFocus(bool shouldFocus)
	{
		if (shouldFocus)
		{
			_chatInput.ActivateInputField();
			_chatInput.Select();
			EventSystem.current.SetSelectedGameObject(_chatInput.gameObject);
			UpdateRoomText();
			_chatInput.text = string.Empty;
		}
		else
		{
			EventSystem.current.SetSelectedGameObject(null);
			_chatInput.DeactivateInputField();
		}
	}

	private void UpdateRoomText()
	{
		_roomText.SetText(_commandProcessor.RoomName);
		_roomText.SetColour(_commandProcessor.RoomColour);
	}

	private void OnSubmit(string arg0)
	{
		string text = _chatInput.text;
		if (!string.IsNullOrEmpty(text))
		{
			bool flag = DoLimiting(text);
			if (!Input.GetKeyDown(KeyCode.Escape))
			{
				ProcessMessage(text);
				if (!flag)
				{
					ProcessAllowedMessage(text);
				}
			}
		}
		_submitAndClose = true;
	}

	public void OnDeselect()
	{
		_submitAndClose = true;
	}

	private void LateUpdate()
	{
		if (_updateCaret)
		{
			if (_waitFrame)
			{
				if (_chatInput.caretPosition != _chatInput.text.Length)
				{
					_chatInput.caretPosition = _chatInput.text.Length;
					_updateCaret = false;
					_waitFrame = false;
				}
			}
			else
			{
				_waitFrame = true;
			}
		}
		if (_submitAndClose)
		{
			UIWindowController.PopState<ChatRoomState>();
			_submitAndClose = false;
		}
	}

	private bool DoLimiting(string text)
	{
		return _chatLimiter.Limit(text) != ChatLimiter.Result.Allowed;
	}

	private void ProcessAllowedMessage(string text)
	{
		UserInputCommand chatMessage = _commandProcessor.ProcessInput(text);
		_messagingSystem.SendChatMessage(chatMessage);
	}

	private void ProcessMessage(string text)
	{
		_inputHistory.AddCommand(text);
	}
}
