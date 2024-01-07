using System.Collections.Generic;

namespace Travellers.UI.HUDMessaging;

public class CappedMessageQueue : IHUDMessageQueue
{
	private readonly int _maxMessagesToStore;

	private readonly List<ColourParsedOSDMessage> _allMessages;

	public List<ColourParsedOSDMessage> AllMessages => _allMessages;

	public CappedMessageQueue(int maxMessages)
	{
		_maxMessagesToStore = maxMessages;
		_allMessages = new List<ColourParsedOSDMessage>();
	}

	public void SetMessages(List<ColourParsedOSDMessage> osdMessageList)
	{
		_allMessages.Clear();
		int num = 0;
		if (osdMessageList.Count > _maxMessagesToStore)
		{
			num = osdMessageList.Count - _maxMessagesToStore;
		}
		for (int i = num; i < osdMessageList.Count; i++)
		{
			_allMessages.Add(osdMessageList[i]);
		}
	}
}
