using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Travellers.UI.Feedback;

public class FeedbackIconItem : FeedbackItem
{
	public Image Icon;

	private Tweener _iconTweener;

	private Color _iconColor;

	public override void Show()
	{
		base.Show();
		_iconColor = new Color(Icon.color.r, Icon.color.g, Icon.color.b, 0f);
		Icon.color = _iconColor;
		_iconTweener = DOTween.To(() => _iconColor.a, delegate(float x)
		{
			_iconColor.a = x;
		}, 1f, 0.2f).SetDelay(0.4f).SetEase(Ease.InOutSine)
			.OnUpdate(delegate
			{
				Icon.color = _iconColor;
			});
	}

	protected override void StopTweens()
	{
		base.StopTweens();
		if (_iconTweener != null)
		{
			_iconTweener.Kill();
		}
	}
}
