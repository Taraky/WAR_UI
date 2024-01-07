using System;
using System.Collections;
using System.Linq;
using Assets.Scripts.Player;
using Bossa.Travellers.Rope;
using Travellers.UI.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Travellers.UI.Login;

[InjectableClass]
public class CharacterCreationIslandScreen : UIScreen
{
	[SerializeField]
	private Transform characterSelectionCamPoint;

	[SerializeField]
	private Transform cameraTarget;

	[SerializeField]
	private Transform readyForLoadingCamPoint;

	[SerializeField]
	private Transform landingScreenCamPoint;

	[SerializeField]
	private Transform loginStartCamPoint;

	[SerializeField]
	private CharacterCustomisationVisualizer customisationVisualizer;

	[SerializeField]
	private CharacterRigSwitch rigSwitch;

	[SerializeField]
	private Transform lookAtPosition;

	[SerializeField]
	private Vector3 camTargetFaceMale;

	[SerializeField]
	private Vector3 camTargetFaceFemale;

	[SerializeField]
	private Vector3 camTargetDefault;

	[SerializeField]
	private Transform playerRoot;

	[SerializeField]
	private float initPlayerHeading;

	[SerializeField]
	private float camDistanceFace;

	[SerializeField]
	private float camDistanceDefault;

	[SerializeField]
	private float camFaceFov;

	private MenuCameraController cameraController;

	[SerializeField]
	private float toEndPointSpeed;

	[SerializeField]
	private float toEndPointRotationSpeed;

	[SerializeField]
	private float lookSmoothing;

	[SerializeField]
	private float cameraSmoothing;

	private LobbySystem _lobbySys;

	private MenuCameraController CameraController => cameraController ?? (cameraController = (MenuCameraController)CameraManager.Instance.GetController(CameraManager.CameraType.Menu));

	[InjectableMethod]
	public void InjectDependencies(LobbySystem lobbySys)
	{
		_lobbySys = lobbySys;
	}

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
		_lobbySys.CamFaceFOV = camFaceFov;
		MusicPlayer.Instance.GotoMenuMusic();
		CameraManager.Instance.SetCamera(CameraManager.CameraType.Menu, cameraTarget.gameObject, overrideExistingIfSame: true);
		CameraManager.Instance.transform.position = loginStartCamPoint.position;
		CameraManager.Instance.transform.rotation = loginStartCamPoint.rotation;
		playerRoot.localEulerAngles = new Vector3(0f, initPlayerHeading, 0f);
	}

	protected override void Activate()
	{
	}

	protected override void Deactivate()
	{
	}

	public void MoveCameraToLandingInstant()
	{
		CameraController.SlerpToTarget(CameraManager.Instance.transform, landingScreenCamPoint, 10f, 10f);
		CameraController.SetSubMode(MenuCameraController.MenuCamMode.None);
	}

	public void MoveCameraToLandingFast(Action onComplete)
	{
		CameraController.SlerpToTarget(CameraManager.Instance.transform, landingScreenCamPoint, 0.3f, 0.3f, onComplete);
		CameraController.SetSubMode(MenuCameraController.MenuCamMode.None);
	}

	public void MoveCameraToLandingSlow(Action onComplete)
	{
		CameraController.SlerpToTarget(CameraManager.Instance.transform, landingScreenCamPoint, 0.6f, 0.6f, onComplete);
		CameraController.SetSubMode(MenuCameraController.MenuCamMode.None);
	}

	public void MoveCameraToCharacterSelection(Action onComplete)
	{
		CameraController.SlerpToTarget(CameraManager.Instance.transform, characterSelectionCamPoint, 0.3f, 0.3f, onComplete);
		CameraController.SetSubMode(MenuCameraController.MenuCamMode.Orbit);
	}

	public void MoveCameraToPrepareForLoading()
	{
		CursorControl.Instance.UseInGameCursor = true;
		CursorControl.Instance.MouseCursorReleased = true;
		CameraController.SlerpToTarget(CameraManager.Instance.transform, readyForLoadingCamPoint, toEndPointSpeed, toEndPointRotationSpeed);
		CameraController.SetSubMode(MenuCameraController.MenuCamMode.None);
		StartCoroutine(ToLoadingGame());
	}

	private IEnumerator ToLoadingGame()
	{
		float t = 0f;
		while (t < 1f)
		{
			yield return new WaitForEndOfFrame();
			t += 0.33f * Time.deltaTime;
			DayNightCycle.Instance.SetTime(Mathf.Lerp(0.273f, 0.5f, t));
		}
		LoadingScreen.SetScreenTypeAndVisibility(LoadingScreenVisibility.FadingIn, LoadingScreenType.BlackoutFullLogo);
		yield return new WaitForSeconds(3f);
		DayNightCycle.Instance.sunIntensity = 0.8f;
		MusicPlayer.Instance.GotoGameMusic();
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUILoginEvents.StartWorldLogin);
	}

	public void ShowCinematicIntro()
	{
		StartCoroutine(ToIntro());
	}

	private IEnumerator ToIntro()
	{
		LoadingScreen.SetScreenTypeAndVisibility(LoadingScreenVisibility.FadingIn);
		yield return new WaitForSeconds(2f);
		base.gameObject.SetActive(value: false);
		SceneManager.LoadSceneAsync("CinematicIntro", LoadSceneMode.Additive);
	}

	private void Update()
	{
		lookAtPosition.position = Vector3.Lerp(lookAtPosition.position, CameraManager.Instance.transform.position, lookSmoothing * Time.deltaTime);
		Vector3 vector = ((!_lobbySys.CurrentCreationData.isMale) ? camTargetFaceFemale : camTargetFaceMale);
		cameraTarget.position = Vector3.Lerp(cameraTarget.position, (!_lobbySys.IsZoomedInOnFace) ? camTargetDefault : vector, cameraSmoothing * Time.deltaTime);
		CameraController.CameraDistance = Mathf.Lerp(CameraController.CameraDistance, (!_lobbySys.IsZoomedInOnFace) ? camDistanceDefault : camDistanceFace, cameraSmoothing * Time.deltaTime);
		CheckIfGenderChanged();
		UpdateCharacter();
	}

	public void CheckIfGenderChanged()
	{
		if (_lobbySys.CurrentCreationData.isMale != rigSwitch.isMale)
		{
			rigSwitch.SwitchCharacter(_lobbySys.CurrentCreationData.isMale);
			_lobbySys.CanRefreshModel = true;
		}
	}

	public void UpdateCharacter()
	{
		if (_lobbySys.CanRefreshModel && _lobbySys.ShouldRefreshModel)
		{
			customisationVisualizer.SetCharacterData(_lobbySys.CurrentCreationData);
			_lobbySys.ShouldRefreshModel = false;
		}
	}

	public void RandomizeStarterGear()
	{
		_lobbySys.CurrentCreationData.Cosmetics[CharacterSlotType.Head].Prefab = CustomisationSettings.starterHeadItems.Keys.ToList()[UnityEngine.Random.Range(0, CustomisationSettings.starterHeadItems.Keys.Count)];
		_lobbySys.CurrentCreationData.Cosmetics[CharacterSlotType.Body].Prefab = CustomisationSettings.starterTorsoItems.Keys.ToList()[UnityEngine.Random.Range(0, CustomisationSettings.starterTorsoItems.Keys.Count)];
		_lobbySys.CurrentCreationData.Cosmetics[CharacterSlotType.Feet].Prefab = CustomisationSettings.starterLegItems.Keys.ToList()[UnityEngine.Random.Range(0, CustomisationSettings.starterLegItems.Keys.Count)];
		int num = UnityEngine.Random.Range(0, CustomisationSettings.skinColors.Length);
		_lobbySys.CurrentCreationData.UniversalColors.SkinColor = CustomisationSettings.skinColors[num];
		_lobbySys.CurrentCreationData.UniversalColors.LipColor = CustomisationSettings.lipColors[num];
		_lobbySys.CurrentCreationData.UniversalColors.HairColor = CustomisationSettings.hairColors[UnityEngine.Random.Range(0, CustomisationSettings.hairColors.Length)];
		_lobbySys.ChangeClothingColor(CharacterSlotType.Body, primary: true, CustomisationSettings.clothingColors[UnityEngine.Random.Range(0, CustomisationSettings.clothingColors.Length)], updateCharacter: false);
		_lobbySys.ChangeClothingColor(CharacterSlotType.Body, primary: false, CustomisationSettings.clothingColors[UnityEngine.Random.Range(0, CustomisationSettings.clothingColors.Length)], updateCharacter: false);
		_lobbySys.ChangeClothingColor(CharacterSlotType.Feet, primary: true, CustomisationSettings.clothingColors[UnityEngine.Random.Range(0, CustomisationSettings.clothingColors.Length)], updateCharacter: false);
		_lobbySys.ChangeClothingColor(CharacterSlotType.Feet, primary: false, CustomisationSettings.clothingColors[UnityEngine.Random.Range(0, CustomisationSettings.clothingColors.Length)], updateCharacter: false);
	}

	public void RandomizeAll()
	{
		_lobbySys.CurrentCreationData.Cosmetics[CharacterSlotType.Head].Prefab = CustomisationSettings.allHeads[UnityEngine.Random.Range(0, CustomisationSettings.allHeads.Length)];
		_lobbySys.CurrentCreationData.Cosmetics[CharacterSlotType.Body].Prefab = CustomisationSettings.allTorsos[UnityEngine.Random.Range(0, CustomisationSettings.allTorsos.Length)];
		_lobbySys.CurrentCreationData.Cosmetics[CharacterSlotType.Feet].Prefab = CustomisationSettings.allLegs[UnityEngine.Random.Range(0, CustomisationSettings.allLegs.Length)];
		_lobbySys.CurrentCreationData.UniversalColors.SkinColor = CustomisationSettings.skinColors[UnityEngine.Random.Range(0, CustomisationSettings.skinColors.Length)];
		_lobbySys.CurrentCreationData.UniversalColors.LipColor = CustomisationSettings.lipColors[UnityEngine.Random.Range(0, CustomisationSettings.lipColors.Length)];
		_lobbySys.CurrentCreationData.UniversalColors.HairColor = CustomisationSettings.hairColors[UnityEngine.Random.Range(0, CustomisationSettings.hairColors.Length)];
		_lobbySys.ChangeClothingColor(CharacterSlotType.Body, primary: true, CustomisationSettings.clothingColors[UnityEngine.Random.Range(0, CustomisationSettings.clothingColors.Length)], updateCharacter: false);
		_lobbySys.ChangeClothingColor(CharacterSlotType.Body, primary: false, CustomisationSettings.clothingColors[UnityEngine.Random.Range(0, CustomisationSettings.clothingColors.Length)], updateCharacter: false);
		_lobbySys.ChangeClothingColor(CharacterSlotType.Feet, primary: true, CustomisationSettings.clothingColors[UnityEngine.Random.Range(0, CustomisationSettings.clothingColors.Length)], updateCharacter: false);
		_lobbySys.ChangeClothingColor(CharacterSlotType.Feet, primary: false, CustomisationSettings.clothingColors[UnityEngine.Random.Range(0, CustomisationSettings.clothingColors.Length)], updateCharacter: false);
	}

	protected override void ProtectedDispose()
	{
	}
}
