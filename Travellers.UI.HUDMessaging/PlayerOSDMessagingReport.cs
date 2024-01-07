using UnityEngine;

namespace Travellers.UI.HUDMessaging;

public abstract class PlayerOSDMessagingReport : MonoBehaviour
{
	public OSDMessage FeedbackMessageContent;

	public abstract void SetData(OSDMessage content);
}
