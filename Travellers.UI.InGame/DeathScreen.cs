using Assets.Visualizers;
using Bossa.Travellers.Player;
using Bossa.Travellers.Utils;
using Bossa.Travellers.Utils.ErrorHandling;
using Bossa.Travellers.Visualisers.Profile;
using GameDBLocalization;
using Improbable.Collections;
using Travellers.UI.Framework;
using Travellers.UI.InfoPopups;
using Travellers.UI.Login;
using UnityEngine;
using WAUtilities.Logging;

namespace Travellers.UI.InGame;

public class DeathScreen : UIPopup
{
	public class Literals
	{
		public const string AlertTitle = "Personal Reviver";

		public const string AlertMessageReviver = "This will unregister you from your Personal Reviver.";

		public const string AlertMessageRandom = "This will send you to a completely random island in your zone, and will unregister you from your Personal Reviver! Only use as a last resort!";

		public const string AlertOk = "REVIVE";

		public const string AlertCancel = "CANCEL";
	}

	[SerializeField]
	private UIButtonController _crewRespawnerButton;

	[SerializeField]
	private UIButtonController _nearestRespawnerButton;

	[SerializeField]
	private UIButtonController _randomRespawnerButton;

	[SerializeField]
	private TextStylerTextMeshPro _crewCountdown;

	[SerializeField]
	private TextStylerTextMeshPro _nearestRespawnerCountdown;

	[SerializeField]
	private TextStylerTextMeshPro _randomRespawnerCountdown;

	[SerializeField]
	private GameObject _feedbackButton;

	[SerializeField]
	private GameObject _feedbackLabel;

	[SerializeField]
	private GameObject[] _objectsToHideForHavenDeathScreen;

	private float _crewRespawnSecsRemaining = float.MaxValue;

	private float _nearestRespawnSecsRemaining = float.MaxValue;

	private float _randomRespawnSecsRemaining = float.MaxValue;

	private bool _crewRespawnForce;

	private bool _nearestRespawnForce;

	private bool _randomRespawnForce;

	private bool _noRespawnersNearby;

	private int _randomRespawnsRemaining;

	[Tooltip("This is the default respawner cool down time (in seconds)")]
	public int respawnCountdown = 10;

	private int[] _nearbyRespawnerCountdowns = new int[6] { 10, 10, 15, 25, 40, 60 };

	[SerializeField]
	private ReviverChargeBar _chargeBar;

	private DeathCameraEffect deathCameraEffect = new DeathCameraEffect();

	private bool _crewRespawnerAvailable;

	private bool _hasEnoughCharge;

	private static bool HideRespawnerCharge => !WAConfig.GetOrDefault(ConfigKeys.PersonalReviverChargeEnabledKey, defaultValue: true);

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUIInGameEvents.UpdateDeathScreen, OnUpdateDeathScreen);
		_eventList.AddEvent(WAUIInGameEvents.UpdateDeathScreenReviverData, OnUpdateDeathScreenReviverData);
		_eventList.AddEvent(WAUIInGameEvents.UpdateReviverUses, OnRandomRespawnUsesUpdated);
		_eventList.AddEvent(WAUIInGameEvents.DeathScreenRequest, OnDeathScreenRequest);
	}

	protected override void ProtectedInit()
	{
		_crewRespawnerButton.SetButtonEvent(WAUIInGameEvents.DeathScreenRequest, new DeathScreenRequestEvent(DeathScreenType.CrewReviver));
		_nearestRespawnerButton.SetButtonEvent(WAUIInGameEvents.DeathScreenRequest, new DeathScreenRequestEvent(DeathScreenType.NearbyReviver));
		_randomRespawnerButton.SetButtonEvent(WAUIInGameEvents.DeathScreenRequest, new DeathScreenRequestEvent(DeathScreenType.RandomReviver));
		if (_crewRespawnerButton == null || _nearestRespawnerButton == null || _randomRespawnerButton == null)
		{
			WALogger.Warn<DeathScreen>(LogChannel.UI, "UI -> DeathScreen -> respawn buttons need to be assigned in inpsector!", new object[0]);
		}
		if (_nearbyRespawnerCountdowns.Length == 0)
		{
			WALogger.Warn<DeathScreen>(LogChannel.UI, "UI -> DeathScreen -> nearbyRespawnerCountdowns need to be setup!", new object[0]);
		}
		CameraVisualSettings.Instance.AddVisualAction(deathCameraEffect);
		if (HideRespawnerCharge)
		{
			_chargeBar.gameObject.SetActive(value: false);
		}
		if (SteamChecker.IsSteamBranchPTS())
		{
			_feedbackButton.SetActive(value: true);
			_feedbackLabel.SetActive(value: true);
		}
		if (LocalPlayer.Instance.playerGameObject.GetComponentInChildren<NewPlayerVisualiser>().IsNew)
		{
			for (int i = 0; i < _objectsToHideForHavenDeathScreen.Length; i++)
			{
				_objectsToHideForHavenDeathScreen[i].SetActive(value: false);
			}
		}
		UpdateView();
	}

	protected override void Deactivate()
	{
		CameraVisualSettings.Instance.RemoveVisualAction(deathCameraEffect);
	}

	private void Update()
	{
		_nearestRespawnForce = false;
		_randomRespawnForce = false;
		_crewRespawnForce = false;
		PlayerPropertiesVisualiser playerPropertiesVisualiser = null;
		Transform transform = null;
		ReleaseAssert.IsTrue<InGameMenuScreen>(LocalPlayer.Exists, () => "WA-4104: Local player doesn't exist!?");
		if (LocalPlayer.Exists)
		{
			playerPropertiesVisualiser = LocalPlayer.Instance.playerProperties;
			ReleaseAssert.IsNotNull<InGameMenuScreen>(playerPropertiesVisualiser, () => "WA-4104: Local player player properties visualiser is null!");
			transform = LocalPlayer.Transform;
			ReleaseAssert.IsNotNull<InGameMenuScreen>(transform, () => "WA-4104: Local player root transform is null!");
		}
		if (playerPropertiesVisualiser != null && playerPropertiesVisualiser.IsSuperUser && Input.GetKey(KeyCode.LeftControl))
		{
			_nearestRespawnForce = true;
			_randomRespawnForce = true;
			if (_crewRespawnerAvailable)
			{
				_crewRespawnForce = true;
			}
		}
		else if (transform != null)
		{
			bool? flag = GlobalBiomeDataVisualizer.HasAncientRespawners(LocalPlayer.LocalPosition);
			if (flag.HasValue && flag.Value)
			{
				_noRespawnersNearby = false;
			}
			else
			{
				_noRespawnersNearby = true;
			}
		}
		UpdateView();
	}

	private void OnUpdateDeathScreen(object[] obj)
	{
		UpdateDeathScreenEvent updateDeathScreenEvent = (UpdateDeathScreenEvent)obj[0];
		_crewRespawnSecsRemaining = (updateDeathScreenEvent.ReviverDataExists ? respawnCountdown : 0);
		int num = ((updateDeathScreenEvent.DeathCount < _nearbyRespawnerCountdowns.Length) ? _nearbyRespawnerCountdowns[updateDeathScreenEvent.DeathCount] : _nearbyRespawnerCountdowns[_nearbyRespawnerCountdowns.Length - 1]);
		_nearestRespawnSecsRemaining = num;
		if (updateDeathScreenEvent.RandomRespawnsRemaining > 0)
		{
			_randomRespawnSecsRemaining = _nearestRespawnSecsRemaining;
		}
		else
		{
			long num2 = new Epoch(updateDeathScreenEvent.RandomRespawnResetTime) - Epoch.Now;
			_randomRespawnSecsRemaining = (int)(num2 / 1000);
		}
		_randomRespawnsRemaining = updateDeathScreenEvent.RandomRespawnsRemaining;
		UpdateRandomRespawnsRemaining();
		_crewRespawnerAvailable = updateDeathScreenEvent.ReviverDataExists;
	}

	public void ShowFeedback()
	{
		UIWindowController.PushState(FeedbackPopupUIState.Default);
	}

	public void OnUpdateDeathScreenReviverData(object[] obj)
	{
		Option<ReviverData> option = (Option<ReviverData>)obj[0];
		_crewRespawnerAvailable = option.IsAvailable();
		if (_crewRespawnSecsRemaining > 0f != _crewRespawnerAvailable)
		{
			UpdateView();
		}
		_chargeBar.OnReviverDataUpdated(option);
		_hasEnoughCharge = RespawnVisualizer.HasSufficientCharge(option);
	}

	private void OnRandomRespawnUsesUpdated(object[] obj)
	{
		UpdateDeathScreenEvent updateDeathScreenEvent = (UpdateDeathScreenEvent)obj[0];
		_randomRespawnsRemaining = updateDeathScreenEvent.RandomRespawnsRemaining;
		if (updateDeathScreenEvent.RandomRespawnsRemaining > 0)
		{
			_randomRespawnSecsRemaining = respawnCountdown;
		}
		else
		{
			long num = new Epoch(updateDeathScreenEvent.RandomRespawnResetTime) - Epoch.Now;
			_randomRespawnSecsRemaining = (float)num / 1000f;
		}
		UpdateRandomRespawnsRemaining();
	}

	private void OnDeathScreenRequest(object[] obj)
	{
		DeathScreenRequestEvent deathScreenRequestEvent = (DeathScreenRequestEvent)obj[0];
		switch (deathScreenRequestEvent.TypeToRequest)
		{
		case DeathScreenType.CrewReviver:
			SpawnAtPersonalReviver();
			break;
		case DeathScreenType.NearbyReviver:
			ShowDeregisterConfirmationNearestRespawner();
			break;
		case DeathScreenType.RandomReviver:
			ShowDeregisterConfirmationRandomRespawner();
			break;
		}
	}

	private void UpdateView()
	{
		UpdateTimers();
		UpdateButtonStates();
	}

	private void UpdateTimers()
	{
		_crewRespawnSecsRemaining = UpdateTimer(_crewRespawnSecsRemaining, _crewCountdown);
		_nearestRespawnSecsRemaining = UpdateTimer(_nearestRespawnSecsRemaining, _nearestRespawnerCountdown);
		if (_noRespawnersNearby)
		{
			_randomRespawnerCountdown.SetText("No respawners in this biome");
		}
		else
		{
			_randomRespawnSecsRemaining = UpdateTimer(_randomRespawnSecsRemaining, _randomRespawnerCountdown);
		}
	}

	private float UpdateTimer(float timer, TextStylerTextMeshPro textStyler, bool forceOff = false)
	{
		if (timer > 0f)
		{
			timer -= Time.deltaTime;
		}
		else if (timer <= 0f)
		{
			timer = 0f;
		}
		textStyler.SetText(FormatRespawnText((int)timer));
		return timer;
	}

	private void UpdateButtonStates()
	{
		bool flag = _crewRespawnForce || (_crewRespawnSecsRemaining <= 0f && _crewRespawnerAvailable);
		_crewRespawnerButton.SetButtonEnabled(flag);
		bool active = !HideRespawnerCharge && flag;
		_chargeBar.gameObject.SetActive(active);
		bool buttonEnabled = _nearestRespawnForce || _nearestRespawnSecsRemaining <= 0f;
		_nearestRespawnerButton.SetButtonEnabled(buttonEnabled);
		bool buttonEnabled2 = _randomRespawnForce || (_randomRespawnSecsRemaining <= 0f && !_noRespawnersNearby);
		_randomRespawnerButton.SetButtonEnabled(buttonEnabled2);
	}

	private void UpdateRandomRespawnsRemaining()
	{
		_randomRespawnerButton.SetText(string.Format("RANDOM REVIVAL CHAMBER\n({0} use{1} remaining)", _randomRespawnsRemaining, (_randomRespawnsRemaining != 1) ? "s" : string.Empty));
	}

	private string FormatRespawnText(int seconds)
	{
		if (seconds != 0)
		{
			int num = seconds % 60;
			int num2 = (seconds - num) / 60 % 60;
			int num3 = (seconds - num2 * 60 - num) / 3600;
			if (num2 == 0 && num3 == 0)
			{
				return string.Format("Revive in {0} second{1}", seconds, (num != 1) ? "s" : string.Empty);
			}
			return $"Revive in {((num3 <= 0) ? string.Empty : $"{num3}h ")}{((num3 <= 0 && num2 <= 0) ? string.Empty : $"{num2}min ")}{num}s";
		}
		return string.Empty;
	}

	public void ShowDeregisterConfirmationNearestRespawner()
	{
		if (_crewRespawnerAvailable)
		{
			DialogPopupFacade.ShowConfirmationDialog("Personal Reviver", "This will unregister you from your Personal Reviver.", SpawnAtAncientRespawner, "REVIVE", "CANCEL", null, useSolidBackground: true, 2f);
		}
		else
		{
			SpawnAtAncientRespawner();
		}
	}

	public void ShowDeregisterConfirmationRandomRespawner()
	{
		if (_crewRespawnerAvailable)
		{
			DialogPopupFacade.ShowConfirmationDialog("Personal Reviver", "This will send you to a completely random island in your zone, and will unregister you from your Personal Reviver! Only use as a last resort!", SpawnAtRandomAncientRespawner, "REVIVE", "CANCEL", null, useSolidBackground: true, 2f);
		}
		else
		{
			SpawnAtRandomAncientRespawner();
		}
	}

	public void SpawnAtPersonalReviver()
	{
		if (_hasEnoughCharge || HideRespawnerCharge)
		{
			DoSpawnAtPersonalReviver();
		}
		else
		{
			DialogPopupFacade.ShowConfirmationDialog("Personal Reviver", Localizer.LocalizeString(LocalizationSchema.KeyDEATH_SCREEN_PERSONAL_REVIVER_CHARGE_WARNING), DoSpawnAtPersonalReviver, "REVIVE", "CANCEL", null, useSolidBackground: true, 2f);
		}
	}

	private static void DoSpawnAtPersonalReviver()
	{
		UIWindowController.CloseAllWindows();
		ReleaseAssert.IsTrue<DeathScreen>(LocalPlayer.Exists, () => "WA-4841: Local player doesn't exist!?");
		if (LocalPlayer.Exists)
		{
			RespawnVisualizer respawnVisualizer = LocalPlayer.Instance.respawnVisualizer;
			ReleaseAssert.IsNotNull<DeathScreen>(respawnVisualizer, () => "WA-4841: Local player respawn visualiser is null!");
			if (respawnVisualizer != null)
			{
				respawnVisualizer.TriggerRespawnAtPersonalReviver();
			}
		}
		LoadingScreen.SetScreenTypeAndVisibility(LoadingScreenVisibility.Enabled);
	}

	public void SpawnAtAncientRespawner()
	{
		UIWindowController.CloseAllWindows();
		ReleaseAssert.IsTrue<DeathScreen>(LocalPlayer.Exists, () => "WA-4841: Local player doesn't exist!?");
		if (LocalPlayer.Exists)
		{
			RespawnVisualizer respawnVisualizer = LocalPlayer.Instance.respawnVisualizer;
			ReleaseAssert.IsNotNull<DeathScreen>(respawnVisualizer, () => "WA-4841: Local player respawn visualiser is null!");
			if (respawnVisualizer != null)
			{
				respawnVisualizer.TriggerRespawnAtNearestAncientRespawner();
			}
		}
		LoadingScreen.SetScreenTypeAndVisibility(LoadingScreenVisibility.Enabled);
	}

	public void SpawnAtRandomAncientRespawner()
	{
		UIWindowController.CloseAllWindows();
		ReleaseAssert.IsTrue<DeathScreen>(LocalPlayer.Exists, () => "WA-4841: Local player doesn't exist!?");
		if (LocalPlayer.Exists)
		{
			RespawnVisualizer respawnVisualizer = LocalPlayer.Instance.respawnVisualizer;
			ReleaseAssert.IsNotNull<DeathScreen>(respawnVisualizer, () => "WA-4841: Local player respawn visualiser is null!");
			if (respawnVisualizer != null)
			{
				respawnVisualizer.TriggerRespawnAtRandomAncientRespawner();
			}
		}
		LoadingScreen.SetScreenTypeAndVisibility(LoadingScreenVisibility.Enabled);
	}

	protected override void ProtectedDispose()
	{
	}
}
