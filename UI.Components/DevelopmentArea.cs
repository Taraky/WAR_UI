using System.Collections;
using GameDBClient;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace UI.Components;

public class DevelopmentArea : MonoBehaviour
{
	[SerializeField]
	private RawImage thumbnailImage;

	[SerializeField]
	private Button videoButton;

	[SerializeField]
	private TextStylerTextMeshPro _forumAreaTitle;

	[SerializeField]
	private TextStylerTextMeshPro _videoAreaTitle;

	[SerializeField]
	private TextStylerTextMeshPro _videoTitle;

	[SerializeField]
	private GameObject patchNotesArea;

	private string _latestVidUrl;

	private string _youtubeAreaTitle;

	private string _forumLink;

	private string _forumTitle;

	private Coroutine checkingOverlay;

	private string videoId;

	private void Start()
	{
		PopulateLinks();
		FillTextComponents();
		StartCoroutine(GetVidInfo());
	}

	private void PopulateLinks()
	{
		LandingScreenLiteralsAndLinksTable landingScreenLiteralsAndLinksTable = Singleton<GameDBAccessor>.Instance.ClientGameDB.LandingScreenLiteralsAndLinksTable;
		LandingScreenLiteralsAndLinks byKey = landingScreenLiteralsAndLinksTable.GetByKey(LandingScreenLiteralsAndLinksSchema.KeyYOUTUBE_LINK);
		LandingScreenLiteralsAndLinks byKey2 = landingScreenLiteralsAndLinksTable.GetByKey(LandingScreenLiteralsAndLinksSchema.KeyFORUM_LINK);
		_latestVidUrl = byKey.URLVal;
		_youtubeAreaTitle = byKey.TitleVal;
		_forumLink = byKey2.URLVal;
		_forumTitle = byKey2.TitleVal;
	}

	private void FillTextComponents()
	{
		_forumAreaTitle.SetText(_forumTitle);
		_videoAreaTitle.SetText(_youtubeAreaTitle);
	}

	public void GotoForum()
	{
		GotoUrl(_forumLink);
	}

	public void TogglePatchNotes(bool show)
	{
		patchNotesArea.SetActive(show);
	}

	private void GotoUrl(string url)
	{
		try
		{
			SteamFriends.ActivateGameOverlayToWebPage(url);
			if (checkingOverlay != null)
			{
				StopCoroutine(checkingOverlay);
			}
			checkingOverlay = StartCoroutine(CheckOverlay(url));
		}
		catch
		{
			Application.OpenURL(url);
		}
	}

	private IEnumerator CheckOverlay(string url)
	{
		float t = 0f;
		while (t < 0.5f)
		{
			if (SteamUtils.IsOverlayEnabled())
			{
				yield break;
			}
			t += Time.deltaTime;
			yield return null;
		}
		Application.OpenURL(url);
	}

	public void GotoVideo()
	{
		GotoUrl(_latestVidUrl);
	}

	private IEnumerator GetVidInfo()
	{
		UnityWebRequest www = UnityWebRequest.Get(_latestVidUrl);
		yield return www.Send();
		if (!www.isError)
		{
			string text = www.downloadHandler.text;
			int startIndex = text.IndexOf("<title>") + 7;
			int length = 100;
			string text2 = text.Substring(startIndex, length);
			text2 = text2.Substring(0, text2.IndexOf("</title>"));
			_videoTitle.SetText(text2);
			videoButton.interactable = true;
		}
	}
}
