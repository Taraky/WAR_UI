using UnityEngine;

namespace Travellers.UI.Components;

public class ToolbarButton : MonoBehaviour
{
	[SerializeField]
	private Toolbar toolbar;

	[SerializeField]
	private GameObject down;

	[SerializeField]
	private Transform icon;

	[SerializeField]
	private Vector3 offset;

	[SerializeField]
	private GameObject up;

	public bool startSelected;

	private Vector3 mPos;

	private bool _selected;

	public bool selected
	{
		get
		{
			return _selected;
		}
		set
		{
			_selected = value;
			down.SetActive(value);
			up.SetActive(!value);
			if ((bool)icon)
			{
				icon.localPosition = ((!value) ? mPos : (mPos + offset));
			}
		}
	}

	private void Start()
	{
		if ((bool)icon)
		{
			mPos = icon.transform.localPosition;
		}
		selected = startSelected;
	}

	private void OnHover(bool isOver)
	{
		if (!selected)
		{
			down.SetActive(isOver);
			up.SetActive(!isOver);
			if ((bool)icon)
			{
				icon.localPosition = ((!isOver) ? mPos : (mPos + offset));
			}
		}
	}

	private void OnClick()
	{
		if (!selected)
		{
			toolbar.ToolbarButtonPressed(this);
		}
		else
		{
			toolbar.SelectNone();
		}
	}
}
