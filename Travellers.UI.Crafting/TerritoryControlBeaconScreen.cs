using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Assets.Visualizers;
using Bossa.Travellers.Territorycontrol;
using Bossa.Travellers.World;
using Improbable;
using Improbable.Collections;
using Improbable.Unity.Core;
using Improbable.Worker;
using TMPro;
using Travellers.UI.Framework;
using Travellers.UI.HUDMessaging;
using Travellers.UI.InfoPopups;
using UnityEngine;
using UnityEngine.UI;
using WAUtilities.Logging;

namespace Travellers.UI.Crafting;

public class TerritoryControlBeaconScreen : UIScreen
{
	private class NameValidator : TMP_InputValidator
	{
		public override char Validate(ref string text, ref int pos, char ch)
		{
			if (!char.IsLetter(ch))
			{
				return '\0';
			}
			if (pos == 1 || text[pos - 1] == ' ')
			{
				return char.ToUpper(ch);
			}
			return ch;
		}
	}

	private enum Hostility
	{
		Friendly,
		Mixed,
		Hostile,
		Unclaimed
	}

	private struct WrappedShipRadarResult : IComparable<WrappedShipRadarResult>
	{
		public RadarResult ShipRadarResult;

		public Vector3 RelativePosition;

		public float Angle;

		public float Time;

		public string TooltipText;

		public Color Color;

		public int CompareTo(WrappedShipRadarResult other)
		{
			return Angle.CompareTo(other.Angle);
		}
	}

	[SerializeField]
	private TMP_InputField _nameInputField;

	[SerializeField]
	private TMP_InputField _broadcastInputField;

	[SerializeField]
	private Image _shipBlipPrefab;

	[SerializeField]
	private Image _radarFront;

	[SerializeField]
	private RectTransform _shipBlipsParent;

	[SerializeField]
	private RectTransform _currentTimeIndicator;

	[SerializeField]
	private RectTransform _vulnerabilitySliderBackground;

	[SerializeField]
	private RectTransform _vulnerabilitySliderHandle;

	[SerializeField]
	private RectTransform _wrappedVulnerabilitySliderHandle;

	[SerializeField]
	private Sprite _lightSprite;

	[SerializeField]
	private Sprite _mediumSprite;

	[SerializeField]
	private Sprite _heavySprite;

	[SerializeField]
	private Sprite _islandSprite;

	[SerializeField]
	private Text _currentServerTimeText;

	[SerializeField]
	private Text _nameText;

	[SerializeField]
	private Text _vulnerabilityHourText;

	[SerializeField]
	private Slider _vulnerabilityHourSlider;

	[SerializeField]
	private GameObject _editNameParent;

	[SerializeField]
	private GameObject _setVulnerabilityParent;

	[SerializeField]
	private Color _friendlyBlipColor;

	[SerializeField]
	private TerritoryControlLeaderboardView _territoryControlLeaderboardView;

	[SerializeField]
	private CanvasGroup[] _isUsableCanvasGroups;

	private TerritoryControlBeaconInteractable _territoryControlBeacon;

	private readonly System.Collections.Generic.List<KeyValuePair<EntityId, Image>> _shipBlipsPendingDestruction = new System.Collections.Generic.List<KeyValuePair<EntityId, Image>>();

	private readonly System.Collections.Generic.List<WrappedShipRadarResult> _pendingShipRadarResults = new System.Collections.Generic.List<WrappedShipRadarResult>();

	private readonly Dictionary<EntityId, Image> _blipMap = new Dictionary<EntityId, Image>();

	private readonly StringBuilder _alliancesStringBuilder = new StringBuilder();

	private bool _lastResponseSuccess;

	private static float _lastRequestTime;

	private float _currentRadarAngle;

	private bool _isUsable;

	private const float RadarRequestPeriod = 5f;

	private const float RadarAngleChangeRate = 72f;

	private const float AlphaReduceRate = 0.18181817f;

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
		_nameInputField.inputValidator = new NameValidator();
		Color color = _radarFront.color;
		color.a = 0f;
		_radarFront.color = color;
		_vulnerabilityHourSlider.onValueChanged.AddListener(HandleVulnerabilityHourSliderChanged);
	}

	private void HandleVulnerabilityHourSliderChanged(float arg0)
	{
		int num = Mathf.RoundToInt(arg0);
		if (num == 24)
		{
			_vulnerabilityHourSlider.value = 0f;
		}
		_vulnerabilityHourText.text = FormVulnerabilityPeriod(num);
	}

	private static string FormVulnerabilityPeriod(int hour)
	{
		return $"VULNERABLE BETWEEN {DateTime.Today.AddHours(hour): HH:mm} and {DateTime.Today.AddHours(hour + 3): HH:mm} GMT";
	}

	protected override void ProtectedDispose()
	{
	}

	public void Set(TerritoryControlBeaconInteractable territoryControlBeacon)
	{
		if (_territoryControlBeacon != null)
		{
			_territoryControlBeacon.IslandNameModel.Remove(HandleNameUpdated);
			_territoryControlBeacon.VulnerabilityHourModel.Remove(HandleVulnerabilityHourUpdated);
			_territoryControlBeacon.UsableModel.Remove(HandleIsUsableChanged);
		}
		_territoryControlBeacon = territoryControlBeacon;
		if (_territoryControlBeacon != null)
		{
			StartCoroutine(RadarRequestCoroutine());
			_territoryControlBeacon.SendLeaderboardRequest(HandleLeaderboardResponse);
			_territoryControlBeacon.IslandNameModel.AddAndInvoke(HandleNameUpdated);
			_territoryControlBeacon.VulnerabilityHourModel.AddAndInvoke(HandleVulnerabilityHourUpdated);
			_territoryControlBeacon.UsableModel.AddAndInvoke(HandleIsUsableChanged);
		}
	}

	private void HandleIsUsableChanged(bool oldValue, bool newValue)
	{
		_isUsable = newValue;
		CanvasGroup[] isUsableCanvasGroups = _isUsableCanvasGroups;
		foreach (CanvasGroup canvasGroup in isUsableCanvasGroups)
		{
			canvasGroup.alpha = ((!newValue) ? 0.25f : 1f);
			canvasGroup.interactable = newValue;
		}
	}

	private void HandleVulnerabilityHourUpdated(Option<uint> oldvalue, Option<uint> newvalue)
	{
		_setVulnerabilityParent.SetActive(!newvalue.HasValue);
		if (newvalue.HasValue)
		{
			_vulnerabilityHourSlider.value = newvalue.Value;
			_vulnerabilityHourSlider.interactable = false;
		}
		else
		{
			_vulnerabilityHourSlider.value = 8f;
			_vulnerabilityHourText.text = "Drag To Set Vulnerability Period";
			_vulnerabilityHourSlider.interactable = true;
		}
	}

	private void HandleNameUpdated(Option<string> oldvalue, Option<string> newvalue)
	{
		_editNameParent.SetActive(!newvalue.HasValue);
		_nameText.gameObject.SetActive(newvalue.HasValue);
		if (newvalue.HasValue)
		{
			_nameText.text = newvalue.Value.ToUpperInvariant();
		}
		else
		{
			_nameText.text = "Unnamed Territory Control Tower".ToUpperInvariant();
		}
	}

	private void HandleLeaderboardResponse(ICommandCallbackResponse<LeaderboardResponse> response)
	{
		if (response.Response.HasValue)
		{
			response.Response.Value.entries.Sort((LeaderboardEntry a, LeaderboardEntry b) => -a.numOwnedIslands.CompareTo(b.numOwnedIslands));
			_territoryControlLeaderboardView.SetLeaderboard(response.Response.Value);
		}
		else
		{
			_territoryControlLeaderboardView.SetError();
			WALogger.Error<TerritoryControlBeaconScreen>("Failed to retrieve leaderboard data. StatusCode: {0}, Message: {1}", new object[2] { response.StatusCode, response.ErrorMessage });
		}
	}

	protected override void Deactivate()
	{
		base.Deactivate();
		_territoryControlBeacon.StopInteraction();
		Set(null);
	}

	private IEnumerator RadarRequestCoroutine()
	{
		while (base.isActiveAndEnabled)
		{
			yield return new WaitForSeconds(5f - Mathf.Min(Time.time - _lastRequestTime, 5f));
			if (!_isUsable)
			{
				_lastResponseSuccess = false;
				continue;
			}
			_lastRequestTime = Time.time;
			_territoryControlBeacon.SendRadarRequest(delegate(ICommandCallbackResponse<RadarResponse> response)
			{
				_lastResponseSuccess = response.StatusCode == StatusCode.Success;
				if (_lastResponseSuccess)
				{
					string allianceName = LocalPlayer.Instance.PlayerNameVisualizer.AllianceName;
					RadarResponse value = response.Response.Value;
					Improbable.Collections.List<RadarResult> results = value.results;
					Improbable.Collections.List<string> allianceNames = value.allianceNames;
					foreach (RadarResult item in results)
					{
						bool flag = false;
						bool flag2 = false;
						foreach (uint allianceNameIndex in item.allianceNameIndices)
						{
							string text = allianceNames[(int)allianceNameIndex];
							if (text != "Unclaimed" && text != "No Registered Alliance")
							{
								bool flag3 = text == allianceName;
								flag = flag || flag3;
								flag2 = flag2 || !flag3;
							}
							_alliancesStringBuilder.AppendLine(text);
						}
						Hostility hostility = ((flag && flag2) ? Hostility.Mixed : ((!flag) ? ((!flag2) ? Hostility.Unclaimed : Hostility.Hostile) : Hostility.Friendly));
						Vector3 relativePosition = new Vector3(item.x, 0f, item.z);
						_pendingShipRadarResults.Add(new WrappedShipRadarResult
						{
							ShipRadarResult = item,
							RelativePosition = relativePosition,
							Angle = (360f + 57.29578f * (0f - Mathf.Atan2(relativePosition.z, relativePosition.x))) % 360f,
							Time = _lastRequestTime,
							TooltipText = _alliancesStringBuilder.ToString(),
							Color = GetColorForHostility(hostility)
						});
						_alliancesStringBuilder.Length = 0;
					}
				}
			});
		}
	}

	private Color GetColorForHostility(Hostility hostility)
	{
		return hostility switch
		{
			Hostility.Friendly => Color.green, 
			Hostility.Mixed => Color.yellow, 
			Hostility.Hostile => Color.red, 
			Hostility.Unclaimed => Color.grey, 
			_ => throw new ArgumentOutOfRangeException("hostility", hostility, null), 
		};
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		StartCoroutine(SetCurrentTimeCoroutine());
	}

	private void Update()
	{
		UpdateRadarBlips();
		UpdateSliderHandleWidth();
	}

	private void UpdateSliderHandleWidth()
	{
		int num = Mathf.RoundToInt(3f * _vulnerabilitySliderBackground.sizeDelta.x / 24f);
		_vulnerabilitySliderHandle.sizeDelta = new Vector2(num, 0f);
		_wrappedVulnerabilitySliderHandle.sizeDelta = new Vector2(num, 0f);
		_wrappedVulnerabilitySliderHandle.localPosition = new Vector2(0f - _vulnerabilitySliderBackground.sizeDelta.x, 0f);
	}

	private IEnumerator SetCurrentTimeCoroutine()
	{
		while (base.enabled)
		{
			DateTime now = Epoch.Now.ToDateTime();
			int totalMinutes = now.Hour * 60 + now.Minute;
			float progress = (float)totalMinutes / 1440f;
			_currentTimeIndicator.anchorMin = new Vector2(progress, 1f);
			_currentTimeIndicator.anchorMax = new Vector2(progress, 1f);
			_currentServerTimeText.text = $"{now: HH:mm} GMT";
			yield return new WaitForSeconds(5f);
		}
	}

	private void UpdateRadarBlips()
	{
		float deltaTime = Time.deltaTime;
		Color color = _radarFront.color;
		color.a = Mathf.Lerp(color.a, (!_lastResponseSuccess) ? 0f : 1f, deltaTime);
		_radarFront.color = color;
		float num = deltaTime * 72f;
		float num2 = _currentRadarAngle + num;
		float currentRadarAngle = num2 % 360f;
		float num3 = Time.time - 5f;
		float num4 = 0.5f * num;
		for (int num5 = _pendingShipRadarResults.Count - 1; num5 >= 0; num5--)
		{
			WrappedShipRadarResult wrappedShipRadarResult = _pendingShipRadarResults[num5];
			float num6 = Mathf.Abs(Mathf.DeltaAngle(wrappedShipRadarResult.Angle, _currentRadarAngle + num4));
			if (num6 < num4)
			{
				if (wrappedShipRadarResult.Time > num3)
				{
					Image radarBlip = GetRadarBlip(wrappedShipRadarResult.ShipRadarResult.id);
					SetBlipPosition(radarBlip, wrappedShipRadarResult.RelativePosition);
					string title;
					switch (wrappedShipRadarResult.ShipRadarResult.type)
					{
					case ShipRadarResultType.Light:
						radarBlip.sprite = _lightSprite;
						SetBlipSize(radarBlip, 12f);
						title = "Light Ship";
						break;
					case ShipRadarResultType.Medium:
						radarBlip.sprite = _mediumSprite;
						SetBlipSize(radarBlip, 14f);
						title = "Medium Ship";
						break;
					case ShipRadarResultType.Heavy:
						radarBlip.sprite = _heavySprite;
						SetBlipSize(radarBlip, 16f);
						title = "Heavy Ship";
						break;
					case ShipRadarResultType.Tower:
						radarBlip.sprite = _islandSprite;
						SetBlipSize(radarBlip, 12f);
						title = "Island";
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
					BlipTooltip component = radarBlip.GetComponent<BlipTooltip>();
					component.Title = title;
					component.Description = wrappedShipRadarResult.TooltipText;
					radarBlip.color = wrappedShipRadarResult.Color;
				}
				_pendingShipRadarResults.RemoveAt(num5);
			}
		}
		_currentRadarAngle = currentRadarAngle;
		_radarFront.rectTransform.rotation = Quaternion.Euler(0f, 0f, 0f - _currentRadarAngle);
		foreach (KeyValuePair<EntityId, Image> item in _blipMap)
		{
			Color color2 = item.Value.color;
			color2.a -= 0.18181817f * deltaTime;
			if (color2.a <= 0f)
			{
				_shipBlipsPendingDestruction.Add(item);
			}
			else
			{
				item.Value.color = color2;
			}
		}
		foreach (KeyValuePair<EntityId, Image> item2 in _shipBlipsPendingDestruction)
		{
			UnityEngine.Object.Destroy(item2.Value.gameObject);
			_blipMap.Remove(item2.Key);
		}
		_shipBlipsPendingDestruction.Clear();
	}

	public static float Distance(float a, float b)
	{
		float num = Math.Abs(a - b) % 360f;
		float num2 = ((!(num > 180f)) ? num : (360f - num));
		a = ((!(a < 0f)) ? (a % 360f) : (360f - (0f - a) % 360f));
		float num3 = (((a - b >= 0f && a - b <= 180f) || (a - b <= -180f && a - b >= -360f)) ? 1 : (-1));
		return num2 * num3;
	}

	private void SetBlipPosition(Image blip, Vector3 relativePosition)
	{
		float x = Mathf.InverseLerp(-256f, 256f, relativePosition.x);
		float y = Mathf.InverseLerp(-256f, 256f, relativePosition.z);
		RectTransform rectTransform = blip.rectTransform;
		Vector2 vector = new Vector2(x, y);
		blip.rectTransform.anchorMax = vector;
		rectTransform.anchorMin = vector;
	}

	private void SetBlipSize(Image blip, float size)
	{
		blip.rectTransform.sizeDelta = new Vector2(size, size);
	}

	private Image GetRadarBlip(EntityId id)
	{
		if (!_blipMap.TryGetValue(id, out var value))
		{
			value = UnityEngine.Object.Instantiate(_shipBlipPrefab, _shipBlipsParent);
			_blipMap.Add(id, value);
		}
		return value;
	}

	public void Close()
	{
		UIWindowController.PopState<TerritoryControlBeaconUIState>();
	}

	public void SetNameClicked()
	{
		DialogPopupFacade.ShowConfirmationDialog("Rename?", _nameInputField.text, delegate
		{
			_territoryControlBeacon.SetNameRequest(_nameInputField.text, delegate(ICommandCallbackResponse<SetNameResponse> response)
			{
				if (response.Response.TryGetValue(out var result) && result.error.HasValue)
				{
					OSDMessage.SendMessage(result.error.Value, MessageType.ClientError);
				}
			});
		}, "CONFIRM", "CANCEL", null, useSolidBackground: true, 2f);
	}

	public void SetVulnerabilityHourClicked()
	{
		DialogPopupFacade.ShowConfirmationDialog("Set Vulnerability Period?", FormVulnerabilityPeriod(Mathf.RoundToInt(_vulnerabilityHourSlider.value)), delegate
		{
			_territoryControlBeacon.SetVulnerabilityHourRequest((uint)Mathf.RoundToInt(_vulnerabilityHourSlider.value), delegate(ICommandCallbackResponse<SetVulnerabilityHourResponse> response)
			{
				if (response.Response.TryGetValue(out var result) && result.error.HasValue)
				{
					OSDMessage.SendMessage(result.error.Value, MessageType.ClientError);
				}
			});
		}, "CONFIRM", "CANCEL", null, useSolidBackground: true, 2f);
	}

	public void SendBroadcastClicked()
	{
		_territoryControlBeacon.SendTerritoryBroadcast(_broadcastInputField.text, delegate(ICommandCallbackResponse<TerritoryBroadcastResponse> response)
		{
			if (!response.Response.HasValue)
			{
				WALogger.Error<TerritoryControlBeaconScreen>("Failed to send broadcast with response: {0}", new object[1] { response.StatusCode });
			}
			else if (response.Response.Value.error.HasValue)
			{
				OSDMessage.SendMessage(response.Response.Value.error.Value, MessageType.ClientError);
			}
		});
		_broadcastInputField.text = string.Empty;
	}
}
