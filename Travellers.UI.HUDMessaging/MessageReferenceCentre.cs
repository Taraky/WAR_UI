using System.Collections.Generic;
using WAUtilities.Logging;

namespace Travellers.UI.HUDMessaging;

public class MessageReferenceCentre
{
	private readonly bool _burnAfterReading;

	private readonly Dictionary<int, List<OSDMessage>> _messageLookup;

	private readonly List<OSDMessage> _masterMessageList;

	private int _currentSubscriberIndex;

	public MessageReferenceCentre(bool burnAfterReading)
	{
		_burnAfterReading = burnAfterReading;
		_messageLookup = new Dictionary<int, List<OSDMessage>>(Comparers.IntComparer);
		_masterMessageList = new List<OSDMessage>();
	}

	public void AddMessage(OSDMessage message)
	{
		if (!_burnAfterReading)
		{
			_masterMessageList.Add(message);
		}
		foreach (KeyValuePair<int, List<OSDMessage>> item in _messageLookup)
		{
			item.Value.Add(message);
		}
	}

	public int AddSubscriber()
	{
		_messageLookup[_currentSubscriberIndex] = new List<OSDMessage>(_masterMessageList);
		return _currentSubscriberIndex++;
	}

	public bool RemoveSubscriber(int subscriberIndex)
	{
		if (_messageLookup.ContainsKey(subscriberIndex))
		{
			_messageLookup.Remove(subscriberIndex);
			_currentSubscriberIndex--;
			return true;
		}
		return false;
	}

	public List<OSDMessage> CollectMessages(int subscriberIndex)
	{
		List<OSDMessage> value = new List<OSDMessage>();
		if (!_messageLookup.TryGetValue(subscriberIndex, out value))
		{
			WALogger.Error<HUDMessagingSystem>("Attempting to get a message for an invalid subscriber number");
			return new List<OSDMessage>();
		}
		if (_burnAfterReading)
		{
			_messageLookup[subscriberIndex] = new List<OSDMessage>();
		}
		return value;
	}

	public void ClearSubscribersMessages(int subscriberIndex)
	{
		List<OSDMessage> value = new List<OSDMessage>();
		if (!_messageLookup.TryGetValue(subscriberIndex, out value))
		{
			WALogger.Error<HUDMessagingSystem>("Attempting to clear logs for an invalid subscriber number");
		}
		else
		{
			value.Clear();
		}
	}

	public void ClearAllSubscribersAndMessages()
	{
		for (int i = 0; i < _currentSubscriberIndex; i++)
		{
			RemoveSubscriber(i);
		}
		_masterMessageList.Clear();
	}
}
