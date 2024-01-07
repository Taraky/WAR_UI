using System;
using DG.Tweening;
using Travellers.UI.Framework;
using UnityEngine;

namespace Travellers.UI.Feedback;

public class FeedbackItem : UIScreenComponent
{
	public TextStylerTextMeshPro Title;

	public TextStylerTextMeshPro Description;

	public CanvasGroup Group;

	public RectTransform ContainerRect;

	[HideInInspector]
	public FeedbackPrefabType Type;

	[HideInInspector]
	public Vector3 TargetPosition;

	[HideInInspector]
	public float TimeToLive;

	[HideInInspector]
	public float DespawnTime;

	[HideInInspector]
	public FeedbackEvent Event;

	private Tweener _groupTweener;

	private Tweener _titleTweener;

	private Tweener _descriptionTweener;

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
	}

	protected override void ProtectedDispose()
	{
	}

	public virtual void Show()
	{
		StopTweens();
		Group.alpha = 0f;
		if (Title != null)
		{
			float titleAlpha = 0f;
			Title.SetAlpha(0f);
			_titleTweener = DOTween.To(() => titleAlpha, delegate(float x)
			{
				titleAlpha = x;
				Title.SetAlpha(x);
			}, 1f, 0.2f).SetDelay(0.2f).SetEase(Ease.InOutSine);
		}
		if (Description != null)
		{
			float descriptionAlpha = 0f;
			Description.SetAlpha(0f);
			_descriptionTweener = DOTween.To(() => descriptionAlpha, delegate(float x)
			{
				descriptionAlpha = x;
				Description.SetAlpha(x);
			}, 1f, 0.2f).SetDelay(0.3f).SetEase(Ease.InOutSine);
		}
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
		}
		_groupTweener = DOTween.To(() => Group.alpha, delegate(float x)
		{
			Group.alpha = x;
		}, 1f, 0.2f).SetEase(Ease.InOutSine);
	}

	public virtual void Hide(Action<FeedbackItem> onHideCallback)
	{
		StopTweens();
		_groupTweener = DOTween.To(() => Group.alpha, delegate(float x)
		{
			Group.alpha = x;
		}, 0f, 0.2f).SetEase(Ease.InOutSine).OnComplete(delegate
		{
			onHideCallback(this);
		});
	}

	protected virtual void StopTweens()
	{
		if (_groupTweener != null)
		{
			_groupTweener.Kill();
		}
		if (_titleTweener != null)
		{
			_titleTweener.Kill();
		}
		if (_descriptionTweener != null)
		{
			_descriptionTweener.Kill();
		}
	}

	public void KeepAlive()
	{
		DespawnTime = float.MaxValue;
	}

	public void Expire()
	{
		DespawnTime = 0f;
	}
}
