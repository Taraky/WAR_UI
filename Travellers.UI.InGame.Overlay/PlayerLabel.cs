using Travellers.UI.Framework;
using UnityEngine;
using UnityEngine.UI;
using WAUtilities.Logging;

namespace Travellers.UI.InGame.Overlay;

public class PlayerLabel : UIScreenComponent
{
	[SerializeField]
	private TextStylerTextMeshPro _playerName;

	[SerializeField]
	private TextStylerTextMeshPro _allianceName;

	[SerializeField]
	private Image _speechIndicator;

	private const int MaxDistanceForLabels = 50;

	private PlayerNameInfo _playerNameInfo;

	private RectTransform _rectTransform;

	public PlayerNameInfo PlayerNameInfo => _playerNameInfo;

	public bool IsInPlayerAlliance { get; private set; }

	public bool IsInPlayerCrew { get; private set; }

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
		_allianceName.SetColour(ColourReference.ColourType.BlueText);
	}

	protected override void ProtectedDispose()
	{
	}

	public void SetData(PlayerNameInfo playerNameInfo, string crewUidToCompare, string allianceUIdToCompare)
	{
		_rectTransform = (RectTransform)base.transform;
		_playerNameInfo = playerNameInfo;
		_playerName.SetText(_playerNameInfo.Name);
		IsInPlayerCrew = IsInCrew(playerNameInfo, crewUidToCompare);
		IsInPlayerAlliance = IsInAlliance(playerNameInfo, allianceUIdToCompare);
		ColourReference.ColourType textColour = GetTextColour();
		_playerName.SetColour(textColour);
		_allianceName.SetColour(textColour);
		_speechIndicator.gameObject.SetActive(value: false);
	}

	private bool IsInCrew(PlayerNameInfo playerNameInfo, string crewUidToCompare)
	{
		if (!string.IsNullOrEmpty(playerNameInfo.CrewUId))
		{
			return crewUidToCompare == playerNameInfo.CrewUId;
		}
		return false;
	}

	private bool IsInAlliance(PlayerNameInfo playerNameInfo, string allianceUIdToCompare)
	{
		_allianceName.SetObjectActive(!string.IsNullOrEmpty(_playerNameInfo.AllianceName));
		if (!string.IsNullOrEmpty(playerNameInfo.AllianceUid))
		{
			string text = $"< {_playerNameInfo.AllianceName} >";
			_allianceName.SetText(text);
			return allianceUIdToCompare == playerNameInfo.AllianceUid;
		}
		return false;
	}

	private ColourReference.ColourType GetTextColour()
	{
		if (IsInPlayerCrew)
		{
			return ColourReference.ColourType.CrewOSDName;
		}
		return IsInPlayerAlliance ? ColourReference.ColourType.AllianceOSDName : ColourReference.ColourType.White;
	}

	public void UpdateLabel(float positionVerticalOffset)
	{
		bool flag = true;
		Camera camera = ((!(CameraManager.MainCamera != null)) ? Camera.main : CameraManager.MainCamera);
		if (_playerNameInfo.PlayerTransform == null)
		{
			WALogger.ErrorOnce<PlayerLabel>("_playerNameInfo.PlayerTransform is null! Somehow initialization code for this is not working properly in some case.", new object[0]);
			return;
		}
		if (!Physics.Linecast(camera.transform.position, _playerNameInfo.PlayerTransform.position + _playerNameInfo.PlayerTransform.up, Layers.SolidsNoCharacters))
		{
			Vector3 position = new Vector3(_playerNameInfo.PlayerTransform.position.x, _playerNameInfo.PlayerTransform.position.y + positionVerticalOffset, _playerNameInfo.PlayerTransform.position.z);
			Vector3 vector = camera.WorldToViewportPoint(position);
			flag = vector.z > 0f && vector.z < 50f;
			_rectTransform.anchorMin = vector;
			_rectTransform.anchorMax = vector;
			_rectTransform.anchoredPosition = Vector2.zero;
		}
		else
		{
			flag = false;
		}
		UpdateSpeechIndicator();
		SetObjectActive(flag);
	}

	private void UpdateSpeechIndicator()
	{
		_speechIndicator.gameObject.SetActive(_playerNameInfo.IsTalking.HasValue && _playerNameInfo.IsTalking.Value);
	}
}
