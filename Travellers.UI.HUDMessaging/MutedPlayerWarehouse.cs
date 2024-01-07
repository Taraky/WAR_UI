using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Travellers.UI.Framework;
using UnityEngine;

namespace Travellers.UI.HUDMessaging;

public class MutedPlayerWarehouse
{
	private static Dictionary<string, MutedPlayerWarehouse> _allWarehouses;

	[JsonProperty]
	private HashSet<string> _mutedPlayerCommands = new HashSet<string>();

	[JsonProperty]
	private string _characterUid;

	private const string WarehousePrefsKey = "MUTED_PLAYERS";

	public const string VoiceMuteArg = " -voice";

	public static MutedPlayerWarehouse CurrentWarehouse { get; private set; }

	static MutedPlayerWarehouse()
	{
		GetAllWarehouses();
	}

	[JsonConstructor]
	public MutedPlayerWarehouse()
	{
	}

	private MutedPlayerWarehouse(string characterUid)
	{
		_characterUid = characterUid;
	}

	private static void GetAllWarehouses()
	{
		_allWarehouses = new Dictionary<string, MutedPlayerWarehouse>();
		if (PlayerPrefs.HasKey("MUTED_PLAYERS"))
		{
			try
			{
				string @string = PlayerPrefs.GetString("MUTED_PLAYERS");
				_allWarehouses = JsonConvert.DeserializeObject<Dictionary<string, MutedPlayerWarehouse>>(@string);
				return;
			}
			catch (Exception e)
			{
				UIErrorHandler.TriggerExceptionHandler<Exception>("Can't deserialise mute lists\n Creating new lists.", e, null, null);
			}
		}
		string value = JsonConvert.SerializeObject(_allWarehouses);
		PlayerPrefs.SetString("MUTED_PLAYERS", value);
	}

	public static MutedPlayerWarehouse GetWarehouse(string characterUid)
	{
		if (string.IsNullOrEmpty(characterUid) || string.IsNullOrEmpty(characterUid.Trim()))
		{
			return null;
		}
		if (_allWarehouses.TryGetValue(characterUid, out var value))
		{
			if (string.IsNullOrEmpty(value._characterUid))
			{
				value._characterUid = characterUid;
			}
		}
		else
		{
			value = new MutedPlayerWarehouse(characterUid);
			_allWarehouses[characterUid] = value;
			string value2 = JsonConvert.SerializeObject(_allWarehouses);
			PlayerPrefs.SetString("MUTED_PLAYERS", value2);
		}
		return value;
	}

	public static void SetCurrentWarehouse(string characterUid)
	{
		CurrentWarehouse = GetWarehouse(characterUid);
	}

	public bool IsMuted(string playerName)
	{
		return !string.IsNullOrEmpty(playerName) && _mutedPlayerCommands.Contains(playerName.Trim().ToLower());
	}

	public bool IsVOIPMuted(string playerName)
	{
		string text = playerName.Trim().ToLower();
		return _mutedPlayerCommands.Contains(text) || _mutedPlayerCommands.Contains(text + " -voice");
	}

	public void ChangeMuteState(string message, bool mute)
	{
		if (!string.IsNullOrEmpty(message.Trim()))
		{
			if (mute)
			{
				MutePlayer(message);
			}
			else
			{
				UnmutePlayer(message);
			}
		}
	}

	private bool IsPlayerVOIPMutedOnly(string message)
	{
		return message.Contains(" -voice");
	}

	private string PlayerNameFromMessage(string message)
	{
		int num = message.IndexOf(" -voice");
		if (num >= 0)
		{
			return message.Substring(0, num).Trim().ToLower();
		}
		return message.Trim().ToLower();
	}

	private void MutePlayer(string message)
	{
		string text = PlayerNameFromMessage(message);
		bool flag = IsPlayerVOIPMutedOnly(message);
		if (_mutedPlayerCommands.Contains(text))
		{
			OSDMessage.SendMessage($"Player {text} already muted.");
			return;
		}
		if (flag)
		{
			if (!_mutedPlayerCommands.Add(text + " -voice"))
			{
				OSDMessage.SendMessage($"Player {text} voice already muted.");
				return;
			}
		}
		else
		{
			_mutedPlayerCommands.Add(text);
			_mutedPlayerCommands.Remove(text + " -voice");
		}
		string arg = ((!flag) ? string.Empty : "voice");
		OSDMessage.SendMessage($"Player {text} {arg} muted.");
		UpdateMuteListOnRestServer();
	}

	private void UnmutePlayer(string message)
	{
		string text = PlayerNameFromMessage(message);
		bool flag = IsPlayerVOIPMutedOnly(message);
		if (flag)
		{
			if (!_mutedPlayerCommands.Remove(text + " -voice"))
			{
				OSDMessage.SendMessage($"Could not remove player \"{text}\" voice from muted players. Double check your spelling.");
				return;
			}
		}
		else
		{
			bool flag2 = _mutedPlayerCommands.Remove(text);
			bool flag3 = _mutedPlayerCommands.Remove(text + " -voice");
			if (!flag2 && !flag3)
			{
				OSDMessage.SendMessage($"Could not remove player \"{text}\" from muted players. Double check your spelling.");
				return;
			}
		}
		string arg = ((!flag) ? string.Empty : "voice");
		OSDMessage.SendMessage($"Player {text} {arg} unmuted.");
		UpdateMuteListOnRestServer();
	}

	public void ShowMutedList()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string mutedPlayerCommand in _mutedPlayerCommands)
		{
			string text = PlayerNameFromMessage(mutedPlayerCommand);
			string text2 = ((!IsPlayerVOIPMutedOnly(mutedPlayerCommand)) ? string.Empty : " (voice only)");
			stringBuilder.AppendLine(text + text2);
		}
		OSDMessage.SendMessage($"Muted players: {stringBuilder}");
	}

	private void UpdateMuteListOnRestServer()
	{
		try
		{
			string @string = PlayerPrefs.GetString("MUTED_PLAYERS");
			Dictionary<string, MutedPlayerWarehouse> dictionary = JsonConvert.DeserializeObject<Dictionary<string, MutedPlayerWarehouse>>(@string);
			dictionary[_characterUid] = this;
			string value = JsonConvert.SerializeObject(dictionary);
			PlayerPrefs.SetString("MUTED_PLAYERS", value);
		}
		catch (Exception e)
		{
			UIErrorHandler.TriggerExceptionHandler<Exception>("Something went wrong saving muted character list", e, null, null);
		}
	}
}
