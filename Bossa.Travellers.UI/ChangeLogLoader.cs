using System;
using System.Collections;
using Bossa.Travellers.Utils;
using UnityEngine;

namespace Bossa.Travellers.UI;

public class ChangeLogLoader : MonoBehaviour
{
	[SerializeField]
	private TextAsset _fallbackChangelog;

	private CanvasGroup _changeLogCanvasGroup;

	[SerializeField]
	private Transform patchNotesParent;

	[SerializeField]
	private PatchNote patchNotePrefab;

	[SerializeField]
	private PatchNoteBody bodyPrefab;

	private IEnumerator Start()
	{
		WWW www = new WWW(string.Format(arg1: (!(BuildNumberUtils.BuildNumber == string.Empty)) ? BuildNumberUtils.BuildNumber : "1234", format: "{0}/{1}", arg0: WAConfig.Get<string>(ConfigKeys.ClientReleaseNotesUrl)));
		yield return www;
		if (string.IsNullOrEmpty(www.error))
		{
			ParsePatchNotes(www.text);
			yield break;
		}
		Debug.LogWarning("There was an error retrieving the change log from " + www.url + ": " + www.error);
		ParsePatchNotes(_fallbackChangelog.text);
	}

	private void ParsePatchNotes(string wwwText)
	{
		string[] array = wwwText.Split(new string[1] { "<size=14>" }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			int num = array[i].IndexOf("</size>", StringComparison.Ordinal);
			if (num < 0)
			{
				continue;
			}
			string[] array2 = array[i].Substring(0, num).Split('|');
			PatchNote patchNote = UnityEngine.Object.Instantiate(patchNotePrefab, patchNotesParent);
			string text = array[i].Substring(num + 7).Trim();
			text = text.Replace("<color=#bf9d82>", string.Empty).Replace("</color>", string.Empty);
			string[] array3 = text.Split('\n');
			string[] array4 = array3;
			foreach (string text2 in array4)
			{
				if (!string.IsNullOrEmpty(text2.Trim()))
				{
					PatchNoteBody patchNoteBody = UnityEngine.Object.Instantiate(bodyPrefab, patchNote.bodyParent);
					if (text2.IndexOf("</size>", StringComparison.Ordinal) != -1)
					{
						patchNoteBody.bulletPointImage.SetActive(value: false);
						patchNoteBody.bodyText.text = text2.Replace("<size=11>", "<color=#cfe4ff><b>").Replace("</size>", "</color></b>");
						continue;
					}
					patchNoteBody.bulletPointImage.SetActive(value: true);
					patchNoteBody.bodyText.text = text2.TrimStart(' ', '-');
				}
			}
			patchNote.version.text = array2[0];
			if (array2.Length > 1)
			{
				patchNote.date.text = array2[1];
			}
		}
	}
}
