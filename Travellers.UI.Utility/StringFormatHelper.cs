using System;
using System.Text;
using System.Text.RegularExpressions;
using Bossa.Travellers.Social;
using GameDBClient;
using UnityEngine;

namespace Travellers.UI.Utility;

public static class StringFormatHelper
{
	public static bool IsNullOrEmptyOrOnlySpaces(this string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return true;
		}
		if (string.IsNullOrEmpty(str.Trim()))
		{
			return true;
		}
		return false;
	}

	public static string CapitaliseFirstLetter(this string str)
	{
		if (str.Length > 0)
		{
			return $"{char.ToUpper(str[0])}{((str.Length <= 1) ? string.Empty : str.Substring(1))}";
		}
		return str;
	}

	public static string LowerCaseFirstLetter(this string str)
	{
		if (str.Length > 0)
		{
			return $"{char.ToLower(str[0])}{((str.Length <= 1) ? string.Empty : str.Substring(1))}";
		}
		return str;
	}

	public static string RemoveRichText(this string inputStr)
	{
		string empty = string.Empty;
		string pattern = "(\\<).*?(\\>)";
		Regex regex = new Regex(pattern);
		return regex.Replace(inputStr, empty);
	}

	public static string AddLineBreaks(this string inputStr, int lineCharacterLength)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		int num2 = 1;
		while (num < inputStr.Length)
		{
			stringBuilder.Append(inputStr[num]);
			if (num2 >= lineCharacterLength && char.IsWhiteSpace(inputStr[num]))
			{
				stringBuilder.Append("\n");
				num2 = 0;
			}
			num++;
			num2++;
		}
		return stringBuilder.ToString();
	}

	public static string FormatTimeStamp(this DateTime timestamp)
	{
		return $"[{timestamp:H:mm:ss}]";
	}

	public static string FormatTimeForBeaconCountdown(this int input)
	{
		int num = input % 60;
		int num2 = (input - num) / 60;
		int num3 = num2 % 60;
		int num4 = (num2 - num3) / 60;
		return $"{num4:00}:{num3:00}:{num:00}";
	}

	public static string GetStackTraceFrames(string stackTrace, int framesToGet)
	{
		if (!string.IsNullOrEmpty(stackTrace))
		{
			StringBuilder stringBuilder = new StringBuilder();
			string[] array = stackTrace.Split('\n');
			for (int i = 0; i < framesToGet && i < array.Length; i++)
			{
				stringBuilder.AppendLine(array[i]);
			}
			return stringBuilder.ToString();
		}
		return "No stacktrace";
	}

	public static string TryHighlightSelection(this string stringToCheck, string query, Color highlightColour, bool returnEmptyIfNotFound, int lettersEitherSide = 0)
	{
		string text = stringToCheck;
		if (!string.IsNullOrEmpty(query) && !string.IsNullOrEmpty(stringToCheck))
		{
			int num = stringToCheck.IndexOf(query, StringComparison.InvariantCultureIgnoreCase);
			if (num == -1)
			{
				return (!returnEmptyIfNotFound) ? stringToCheck : string.Empty;
			}
			if (lettersEitherSide > 0)
			{
				int num2 = num - lettersEitherSide;
				int num3 = num + query.Length + lettersEitherSide;
				num2 = ((num2 >= 0) ? num2 : 0);
				num3 = ((num3 <= stringToCheck.Length) ? num3 : stringToCheck.Length);
				text = stringToCheck.Substring(num2, num3 - num2);
				text = text.Insert(text.Length, "...").Insert(0, "...");
				num = text.IndexOf(query, StringComparison.InvariantCultureIgnoreCase);
			}
			if (num >= 0)
			{
				text = text.Insert(num + query.Length, "</color>").Insert(num, $"<color={highlightColour.ToHex(includeHash: true)}>");
			}
			return text;
		}
		return (!returnEmptyIfNotFound) ? stringToCheck : string.Empty;
	}

	public static void TryAllianceName(string name)
	{
		StringBuilder stringBuilder = CheckRules(name);
		if (stringBuilder.Length > 0)
		{
			string message = "Cannot accept name for following reasons:\n" + stringBuilder;
			throw new InvalidAllianceNameException(message);
		}
	}

	private static StringBuilder CheckRules(string name)
	{
		StringBuilder stringBuilder = new StringBuilder();
		SocialInputCharacterLimits byKey = Singleton<GameDBAccessor>.Instance.ClientGameDB.SocialInputCharacterLimitsTable.GetByKey(SocialInputCharacterLimitsSchema.KeyALLIANCE_NAME);
		if (string.IsNullOrEmpty(name) || name.Length < byKey.MINVal || name.Length > byKey.MAXVal)
		{
			stringBuilder.AppendLine($"- Must be {byKey.MINVal}-{byKey.MAXVal} characters long");
			return stringBuilder;
		}
		Regex regex = new Regex("[^A-Z a-z']");
		if (regex.IsMatch(name))
		{
			stringBuilder.AppendLine("- Must only use letters, spaces and apostrophes");
		}
		Regex regex2 = new Regex("[a-z][A-Z]");
		if (regex2.IsMatch(name))
		{
			stringBuilder.AppendLine("- Capitals cannot follow another letter");
		}
		if (name[0] == ' ' || name[name.Length - 1] == ' ')
		{
			stringBuilder.AppendLine("- No spaces at the start or end of your alliance name");
		}
		Regex regex3 = new Regex("[ ][ ]");
		if (regex3.IsMatch(name))
		{
			stringBuilder.AppendLine("- Spaces cannot follow each other");
		}
		if (name[0] == '\'' || name[name.Length - 1] == '\'')
		{
			stringBuilder.AppendLine("- No apostrophes at the start or end of your alliance name");
		}
		Regex regex4 = new Regex("['][']");
		if (regex4.IsMatch(name))
		{
			stringBuilder.AppendLine("- Apostrophes cannot follow each other");
		}
		return stringBuilder;
	}
}
