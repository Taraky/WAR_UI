using System.Collections;
using Bossa.Travellers.Utils;
using Travellers.UI.Framework;
using UnityEngine;

namespace Travellers.UI.DebugDisplay;

public class DebugInfoScreen : UIScreen
{
	[SerializeField]
	private TextStylerTextMeshPro _version;

	[SerializeField]
	private TextStylerTextMeshPro _playerInfo;

	[SerializeField]
	private TextStylerTextMeshPro _debugInfo;

	[SerializeField]
	private GameObject _debugInfoContainer;

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUIInGameEvents.ToggleHideFeedbackUI, OnToggleHideFeedbackUI);
		_eventList.AddEvent(WADebugEvents.UpdateDebugDisplay, OnDebugDisplayUpdate);
		_eventList.AddEvent(WADebugEvents.UpdateCharacterUidAndName, OnCharacterUidAndNameUpdate);
	}

	public static void UpdateCharacterInfo(string uid, string characterName)
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WADebugEvents.UpdateCharacterUidAndName, new UpdateDebugCharacterNameDisplayEvent(characterName, uid));
	}

	public static void UpdateTopLeftDebugInfo(string infoToShow)
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WADebugEvents.UpdateDebugDisplay, new UpdateDebugDisplayEvent(infoToShow));
	}

	private void OnCharacterUidAndNameUpdate(object[] obj)
	{
		UpdateDebugCharacterNameDisplayEvent updateDebugCharacterNameDisplayEvent = (UpdateDebugCharacterNameDisplayEvent)obj[0];
		UpdateCharacterDebugInfo(updateDebugCharacterNameDisplayEvent.Uid, updateDebugCharacterNameDisplayEvent.CharacterName);
	}

	private void OnToggleHideFeedbackUI(object[] obj)
	{
		_debugInfoContainer.SetActive(!_debugInfo.gameObject.activeSelf);
		_version.gameObject.SetActive(!_version.gameObject.activeSelf);
	}

	private void OnDebugDisplayUpdate(object[] obj)
	{
		UpdateDebugDisplayEvent updateDebugDisplayEvent = (UpdateDebugDisplayEvent)obj[0];
		_debugInfoContainer.SetActive(updateDebugDisplayEvent.Show);
		_debugInfo.SetText(updateDebugDisplayEvent.NewDebugDisplay);
	}

	protected override void ProtectedDispose()
	{
	}

	protected override void ProtectedInit()
	{
		_debugInfo.SetText(string.Empty);
		_debugInfoContainer.SetActive(value: false);
		_playerInfo.SetObjectActive(isActive: false);
		StartCoroutine(SetVersionAndBranchDisplay());
	}

	private void UpdateCharacterDebugInfo(string uid, string characterName)
	{
	}

	private IEnumerator SetVersionAndBranchDisplay()
	{
		while (!SteamManager.Initialized)
		{
			yield return null;
		}
		_version.SetText(GetBuildDisplayState().GetBuildNumber());
	}

	private BuildInfoDisplayState GetBuildDisplayState()
	{
		bool pts = SteamChecker.IsSteamBranchPTS();
		bool isDebugBuild = Debug.isDebugBuild;
		if (Application.isEditor)
		{
			return new UnityEditorBuildState(pts, isDebugBuild);
		}
		return new ProductionBuildState(pts, isDebugBuild);
	}
}
