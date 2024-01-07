using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Travellers.UI.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Screens;

public class EmoteSelectorScreen : UIScreen
{
	public PlayerCharacterAnimation.Emote selectedEmote;

	[SerializeField]
	private float itemRadius;

	[SerializeField]
	private float deadZonePixels;

	[SerializeField]
	private Image emoteSelection;

	[SerializeField]
	private Image emoteSeperatorPrefab;

	[SerializeField]
	private TextMeshProUGUI emoteItemPrefab;

	private Dictionary<int, PlayerCharacterAnimation.Emote[]> emoteCategories;

	private string[] categoryStrings;

	private List<GameObject> instantiatedGameObjects;

	private bool showingCategories;

	private Vector3 mouseStart;

	private int selectedCategory;

	private PlayerCameraController playerCameraController => (PlayerCameraController)CameraManager.Instance.GetController(CameraManager.CameraType.Player);

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
		SetEmotes(Emotes.Instance.emotes);
		NotifyStateChange(isActive: true);
	}

	protected override void ProtectedDispose()
	{
		NotifyStateChange(isActive: false);
	}

	private void NotifyStateChange(bool isActive)
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIInGameEvents.EmoteSelectorStateChanged, new EmoteSelectorScreenStateChangeEvent(isActive));
	}

	public void ShowSelector()
	{
		emoteSelection.enabled = false;
		if (instantiatedGameObjects == null)
		{
			instantiatedGameObjects = new List<GameObject>();
		}
		mouseStart = Input.mousePosition;
		float num = 360f / (float)emoteCategories.Count;
		float num2 = num * ((float)Math.PI / 180f);
		emoteSelection.fillAmount = 1f / (float)emoteCategories.Count;
		for (int i = 0; i < categoryStrings.Length; i++)
		{
			float x = itemRadius * Mathf.Sin(num2 / 2f + num2 * (float)i);
			float y = itemRadius * Mathf.Cos(num2 / 2f + num2 * (float)i);
			TextMeshProUGUI textMeshProUGUI = UnityEngine.Object.Instantiate(emoteItemPrefab, base.transform);
			textMeshProUGUI.transform.localPosition = new Vector3(x, y, 0f);
			instantiatedGameObjects.Add(textMeshProUGUI.gameObject);
			textMeshProUGUI.text = categoryStrings[i];
			Image image = UnityEngine.Object.Instantiate(emoteSeperatorPrefab, Vector3.zero, Quaternion.Euler(0f, 0f, num * (float)i), base.transform);
			image.transform.localPosition = Vector3.zero;
			instantiatedGameObjects.Add(image.gameObject);
		}
		showingCategories = true;
	}

	private void SetEmotes(PlayerCharacterAnimation.Emote[] emotes)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < emotes.Length; i++)
		{
			if (!list.Contains(emotes[i].category))
			{
				list.Add(emotes[i].category);
			}
		}
		categoryStrings = list.ToArray();
		emoteCategories = new Dictionary<int, PlayerCharacterAnimation.Emote[]>(Comparers.IntComparer);
		foreach (PlayerCharacterAnimation.Emote emote in emotes)
		{
			int key = list.IndexOf(emote.category);
			if (emoteCategories.ContainsKey(key))
			{
				emoteCategories[key] = emoteCategories[key].Concat(new PlayerCharacterAnimation.Emote[1] { emote }).ToArray();
			}
			else
			{
				emoteCategories.Add(key, new PlayerCharacterAnimation.Emote[1] { emote });
			}
		}
	}

	private void Update()
	{
		playerCameraController.rotateAlways = false;
		if (!(Vector3.Distance(mouseStart, Input.mousePosition) > deadZonePixels))
		{
			return;
		}
		emoteSelection.enabled = true;
		Vector3 vector = Input.mousePosition - mouseStart;
		float num = 360f / (float)((!showingCategories) ? emoteCategories[selectedCategory].Length : emoteCategories.Count);
		float num2 = Vector3X.SignedAngleBetween(Vector3.down, vector.normalized, Vector3.back);
		num2 += 180f;
		int num3 = Mathf.FloorToInt(num2 / num);
		emoteSelection.transform.localEulerAngles = new Vector3(0f, 0f, (float)(-num3) * num);
		if (Input.GetMouseButtonDown(0))
		{
			if (showingCategories)
			{
				SelectCategory(num3);
				return;
			}
			selectedEmote = emoteCategories[selectedCategory][num3];
			UIWindowController.PopState<EmoteScreenState>();
			UserControlCharacter.UpdateSelectedEmote(selectedEmote);
			selectedEmote = null;
		}
	}

	private void Clear()
	{
		foreach (GameObject instantiatedGameObject in instantiatedGameObjects)
		{
			UnityEngine.Object.Destroy(instantiatedGameObject);
		}
		instantiatedGameObjects.Clear();
	}

	public void SelectCategory(int category)
	{
		selectedCategory = category;
		Clear();
		PlayerCharacterAnimation.Emote[] array = emoteCategories[selectedCategory];
		float num = 360f / (float)array.Length;
		float num2 = num * ((float)Math.PI / 180f);
		emoteSelection.fillAmount = 1f / (float)array.Length;
		for (int i = 0; i < array.Length; i++)
		{
			float x = itemRadius * Mathf.Sin(num2 / 2f + num2 * (float)i);
			float y = itemRadius * Mathf.Cos(num2 / 2f + num2 * (float)i);
			TextMeshProUGUI textMeshProUGUI = UnityEngine.Object.Instantiate(emoteItemPrefab, base.transform);
			textMeshProUGUI.transform.localPosition = new Vector3(x, y, 0f);
			instantiatedGameObjects.Add(textMeshProUGUI.gameObject);
			textMeshProUGUI.text = array[i].animationStateName;
			Image image = UnityEngine.Object.Instantiate(emoteSeperatorPrefab, Vector3.zero, Quaternion.Euler(0f, 0f, num * (float)i), base.transform);
			image.transform.localPosition = Vector3.zero;
			instantiatedGameObjects.Add(image.gameObject);
		}
		showingCategories = false;
	}
}
