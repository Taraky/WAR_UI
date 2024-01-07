using System.Collections.Generic;
using System.Text;
using TMPro;
using Travellers.UI.Framework;
using UnityEngine;

namespace Travellers.UI.HUDMessaging;

[InjectableClass]
public class TextAlertArea : UIScreenComponent, IHUDMessageDisplay
{
	protected struct MessageDeathClock
	{
		public int MessageID;

		public string Message;

		public double StartTime;

		public MessageDeathClock(ColourParsedOSDMessage message)
		{
			MessageID = message.InnerMessage.ID;
			Message = message.ColourParsedMessage;
			StartTime = Epoch.Now.Seconds;
		}
	}

	[SerializeField]
	private TextMeshProUGUI textField;

	[SerializeField]
	private float messageTimeout;

	private List<MessageDeathClock> _messages = new List<MessageDeathClock>();

	private HashSet<int> _allAddedMessageIds = new HashSet<int>();

	private IHUDMessageQueue _messageQueue;

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
	}

	protected override void ProtectedDispose()
	{
	}

	public void SetMessageQueue(IHUDMessageQueue messageQueue)
	{
		_messageQueue = messageQueue;
		RefreshMessageList();
	}

	public void RebuildMessageView()
	{
		RefreshMessageList();
		StringBuilder stringBuilder = new StringBuilder();
		foreach (MessageDeathClock message in _messages)
		{
			stringBuilder.AppendLine(message.Message);
		}
		textField.text = stringBuilder.ToString();
	}

	private void RefreshMessageList()
	{
		foreach (ColourParsedOSDMessage allMessage in _messageQueue.AllMessages)
		{
			if (!_allAddedMessageIds.Contains(allMessage.InnerMessage.ID))
			{
				_messages.Add(new MessageDeathClock(allMessage));
				_allAddedMessageIds.Add(allMessage.InnerMessage.ID);
			}
		}
	}

	private void Update()
	{
		double seconds = Epoch.Now.Seconds;
		bool flag = false;
		for (int num = _messages.Count - 1; num >= 0; num--)
		{
			if (_messages[num].StartTime + (double)messageTimeout <= seconds)
			{
				_messages.RemoveAt(num);
				flag = true;
			}
		}
		if (flag)
		{
			RebuildMessageView();
		}
	}
}
