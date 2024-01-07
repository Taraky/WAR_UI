using System.Collections.Generic;
using System.Text;
using Travellers.UI.Framework;
using Travellers.UI.HUDMessaging;
using UnityEngine;

namespace Travellers.UI.Chat;

public class ChatTextBlob : UIScreenComponent
{
	[SerializeField]
	private TextStylerTextMeshPro _textComponent;

	private List<ColourParsedOSDMessage> _messages;

	protected override void AddListeners()
	{
	}

	protected override void ProtectedDispose()
	{
	}

	protected override void ProtectedInit()
	{
	}

	public void SetText(List<ColourParsedOSDMessage> messages)
	{
		_messages = new List<ColourParsedOSDMessage>(messages);
		ParseMessageList();
	}

	private void ParseMessageList()
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < _messages.Count; i++)
		{
			stringBuilder.AppendLine(_messages[i].ColourParsedMessage);
		}
		_textComponent.SetText(stringBuilder.ToString());
	}
}
