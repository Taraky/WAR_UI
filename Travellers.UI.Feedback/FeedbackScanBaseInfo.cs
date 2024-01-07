using UnityEngine;

namespace Travellers.UI.Feedback;

public abstract class FeedbackScanBaseInfo : MonoBehaviour
{
	[SerializeField]
	private GameObject _divisorLine;

	public abstract bool Setup(ScannableData data);

	public void SetLineVisibility(bool visible)
	{
		_divisorLine.SetActive(visible);
	}
}
