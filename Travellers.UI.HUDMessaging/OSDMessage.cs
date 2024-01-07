using System;
using Bossa.Travellers.World;
using Improbable;
using Travellers.UI.Utility;

namespace Travellers.UI.HUDMessaging;

public struct OSDMessage
{
	public static int NextId;

	public int ID;

	public long TimeStamp;

	public string ReadableTime;

	public string From;

	public ColourReference.ColourType FromColourType;

	public EntityId Entity;

	public string Message;

	public ColourReference.ColourType MessageColourType;

	private readonly MessageType _messageType;

	public MessageType MessageType => _messageType;

	public OSDMessage(string message, string from, EntityId entityId, MessageType messageType, long timeStamp)
	{
		From = from;
		Entity = entityId;
		_messageType = messageType;
		TimeStamp = timeStamp;
		ID = NextId;
		NextId++;
		Message = message.RemoveRichText();
		ReadableTime = new DateTime(TimeStamp).FormatTimeStamp();
		switch (_messageType)
		{
		case MessageType.Default:
			FromColourType = ColourReference.ColourType.DefaultOSDMessage;
			MessageColourType = ColourReference.ColourType.DefaultOSDMessage;
			break;
		case MessageType.AquireItem:
			FromColourType = ColourReference.ColourType.AcquireItemOSDMessage;
			MessageColourType = ColourReference.ColourType.AcquireItemOSDMessage;
			break;
		case MessageType.Server:
			FromColourType = ColourReference.ColourType.ServerOSDMessage;
			MessageColourType = ColourReference.ColourType.ServerOSDMessage;
			break;
		case MessageType.Chat:
			FromColourType = ColourReference.ColourType.ChatOSDName;
			MessageColourType = ColourReference.ColourType.ChatOSDMessage;
			break;
		case MessageType.PlayerDeath:
			FromColourType = ColourReference.ColourType.PlayerDeathOSDMessage;
			MessageColourType = ColourReference.ColourType.PlayerDeathOSDMessage;
			break;
		case MessageType.Admin:
			FromColourType = ColourReference.ColourType.AdminOSDMessage;
			MessageColourType = ColourReference.ColourType.AdminOSDMessage;
			break;
		case MessageType.Crew:
			FromColourType = ColourReference.ColourType.CrewOSDName;
			MessageColourType = ColourReference.ColourType.CrewOSDMessage;
			break;
		case MessageType.Alliance:
			FromColourType = ColourReference.ColourType.AllianceOSDName;
			MessageColourType = ColourReference.ColourType.AllianceOSDMessage;
			break;
		case MessageType.Debug:
			FromColourType = ColourReference.ColourType.DebugOSDName;
			MessageColourType = ColourReference.ColourType.DebugOSDMessage;
			break;
		case MessageType.ClientError:
			FromColourType = ColourReference.ColourType.ClientErrorOSDName;
			MessageColourType = ColourReference.ColourType.ClientErrorOSDMessage;
			break;
		case MessageType.TerritoryBroadcast:
			FromColourType = ColourReference.ColourType.TerritoryBroadcastOSDMessage;
			MessageColourType = ColourReference.ColourType.TerritoryBroadcastOSDMessage;
			break;
		default:
			FromColourType = ColourReference.ColourType.DefaultOSDMessage;
			MessageColourType = ColourReference.ColourType.DefaultOSDMessage;
			break;
		}
	}

	public static void SendMessage(string message)
	{
		SendMessage(message, MessageType.Default);
	}

	public static void SendMessage(string message, MessageType messageType)
	{
		SendMessage(message, string.Empty, default(EntityId), messageType, (long)Epoch.Now.Seconds);
	}

	public static void SendMessage(string message, string from, EntityId entityId, MessageType messageType, long timeStamp)
	{
		if (message != null)
		{
			if (message.StartsWith("%&") && from.Equals("God"))
			{
				Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIFeedbackReportingEvents.AddHelpMessageToSystem, new OSDMessage(message, from, entityId, messageType, timeStamp));
			}
			else
			{
				Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIFeedbackReportingEvents.AddMessageToSystem, new OSDMessage(message, from, entityId, messageType, timeStamp));
			}
		}
	}

	public bool Equals(OSDMessage other)
	{
		return ID == other.ID && TimeStamp == other.TimeStamp;
	}

	public override bool Equals(object obj)
	{
		if (object.ReferenceEquals(null, obj))
		{
			return false;
		}
		return obj is OSDMessage && Equals((OSDMessage)obj);
	}

	public override int GetHashCode()
	{
		return (ID * 397) ^ TimeStamp.GetHashCode();
	}
}
