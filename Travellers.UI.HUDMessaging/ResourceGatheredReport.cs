using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.HUDMessaging;

public class ResourceGatheredReport : PlayerOSDMessagingReport
{
	[SerializeField]
	private Image Icon;

	public override void SetData(OSDMessage content)
	{
		FeedbackMessageContent = content;
		TextMeshProUGUI componentInChildren = GetComponentInChildren<TextMeshProUGUI>();
		componentInChildren.text = content.Message;
	}
}
