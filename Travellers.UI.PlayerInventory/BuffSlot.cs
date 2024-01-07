using TMPro;
using Travellers.UI.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.PlayerInventory;

public class BuffSlot : UIScreenComponent
{
	[SerializeField]
	private RawImage _rawImage;

	[SerializeField]
	private AspectRatioFitter _aspectRatioFitter;

	[SerializeField]
	private TextMeshProUGUI _timeLeftText;

	[SerializeField]
	private Image _timeLeftOverlay;

	private int _duration;

	public int timeLeft { get; set; }

	public string itemTypeId { get; set; }

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
	}

	protected override void ProtectedDispose()
	{
	}

	public void Setup(string _itemTypeId, int duration)
	{
		_duration = duration;
		itemTypeId = _itemTypeId;
		InventoryItemData inventoryItemData = InventoryItemManager.Instance.LookupItem(itemTypeId);
		Texture iconTexture = InventoryIconManager.Instance.GetIconTexture(inventoryItemData.iconName);
		float aspectRatio = (float)iconTexture.width / (float)iconTexture.height;
		_rawImage.texture = iconTexture;
		_aspectRatioFitter.aspectRatio = aspectRatio;
	}

	public void UpdateTimer(int _timeLeft)
	{
		timeLeft = _timeLeft;
		if (timeLeft < 60)
		{
			_timeLeftText.text = timeLeft + "s";
		}
		else
		{
			_timeLeftText.text = Mathf.FloorToInt((float)timeLeft / 60f) + "m";
		}
		_timeLeftOverlay.fillAmount = (float)_timeLeft / (float)_duration;
	}
}
