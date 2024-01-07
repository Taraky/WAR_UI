using System.Collections.Generic;
using Travellers.UI.Framework;
using Travellers.UI.HUDMessaging;
using UnityEngine;

namespace Travellers.UI.Chat;

public class ScrollableMessageDisplay : UIScreenComponent, IHUDMessageDisplay
{
	[SerializeField]
	protected RectTransform chatContainer;

	[SerializeField]
	private int _maxMessages;

	private int messagesPerBlob = 20;

	protected IHUDMessageQueue CurrentStateQueue;

	private readonly List<ChatTextBlob> _activeChatBlobs = new List<ChatTextBlob>();

	private readonly Queue<ChatTextBlob> _inactiveChatBlobs = new Queue<ChatTextBlob>();

	public int MaxMessages => _maxMessages;

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
	}

	protected override void ProtectedDispose()
	{
	}

	public virtual void SetMessageQueue(IHUDMessageQueue newStateQueue)
	{
		CurrentStateQueue = newStateQueue;
	}

	public void RebuildMessageView()
	{
		WriteToChatBox();
	}

	protected void WriteToChatBox()
	{
		ReturnBlobsToStore();
		List<ColourParsedOSDMessage> list = new List<ColourParsedOSDMessage>();
		if (CurrentStateQueue == null)
		{
			return;
		}
		int i = 0;
		int num = 0;
		for (; i < CurrentStateQueue.AllMessages.Count; i++)
		{
			if (CurrentStateQueue.AllMessages[i] != null)
			{
				if (num < messagesPerBlob)
				{
					list.Add(CurrentStateQueue.AllMessages[i]);
					num++;
				}
				else if (num == messagesPerBlob)
				{
					SetBlob(list);
					list.Clear();
					num = 0;
				}
			}
		}
		if (list.Count > 0)
		{
			SetBlob(list);
		}
	}

	private void SetBlob(List<ColourParsedOSDMessage> tempMessageList)
	{
		ChatTextBlob nextChatBlob = GetNextChatBlob();
		nextChatBlob.SetText(tempMessageList);
	}

	private ChatTextBlob GetNextChatBlob()
	{
		ChatTextBlob chatTextBlob = ((_inactiveChatBlobs.Count != 0) ? _inactiveChatBlobs.Dequeue() : UIObjectFactory.Create<ChatTextBlob>(UIElementType.ScreenComponent, UIFillType.KeepOriginalAnchoring, chatContainer, isObjectActive: true));
		chatTextBlob.SetObjectActive(isActive: true);
		_activeChatBlobs.Add(chatTextBlob);
		return chatTextBlob;
	}

	private void ReturnBlobsToStore()
	{
		for (int i = 0; i < _activeChatBlobs.Count; i++)
		{
			_activeChatBlobs[i].SetObjectActive(isActive: false);
			_inactiveChatBlobs.Enqueue(_activeChatBlobs[i]);
		}
		_activeChatBlobs.Clear();
	}
}
