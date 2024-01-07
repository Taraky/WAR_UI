using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Travellers.UI.Framework;
using Travellers.UI.Utility;
using UnityEngine;

namespace Travellers.UI.Tutorial;

public class TutorialPopup : UIPopup
{
	[SerializeField]
	private TextStyler _popupParagraph;

	[SerializeField]
	private GameObject _leftLine;

	[SerializeField]
	private GameObject _rightLine;

	[SerializeField]
	private GameObject _topLine;

	[SerializeField]
	private GameObject _bottomLine;

	[SerializeField]
	private RectTransform _arrowRect;

	private List<GameObject> _allGameObjects;

	private Dictionary<eAnchorDirection, List<GameObject>> _activeObjectLookup;

	private static Dictionary<eAnchorDirection, Vector3> _padding;

	private static Dictionary<eAnchorDirection, Vector3> _arrowRotation;

	public TutorialContent.AnchoredPopup AnchoredContent { get; private set; }

	public TutorialTransformAnchor ParentAnchor { get; private set; }

	private RectTransform _thisRect => (RectTransform)base.transform;

	static TutorialPopup()
	{
		_padding = new Dictionary<eAnchorDirection, Vector3>
		{
			{
				eAnchorDirection.UpLeft,
				new Vector3(0f, -7f, 0f)
			},
			{
				eAnchorDirection.UpCentre,
				new Vector3(0f, -7f, 0f)
			},
			{
				eAnchorDirection.UpRight,
				new Vector3(0f, -7f, 0f)
			},
			{
				eAnchorDirection.MiddleLeft,
				new Vector3(7f, 0f, 0f)
			},
			{
				eAnchorDirection.MiddleCentre,
				new Vector3(0f, 0f, 0f)
			},
			{
				eAnchorDirection.MiddleRight,
				new Vector3(-7f, 0f, 0f)
			},
			{
				eAnchorDirection.DownLeft,
				new Vector3(0f, 7f, 0f)
			},
			{
				eAnchorDirection.DownCentre,
				new Vector3(0f, 7f, 0f)
			},
			{
				eAnchorDirection.DownRight,
				new Vector3(0f, 7f, 0f)
			}
		};
		_arrowRotation = new Dictionary<eAnchorDirection, Vector3>
		{
			{
				eAnchorDirection.UpLeft,
				new Vector3(0f, 0f, 135f)
			},
			{
				eAnchorDirection.UpCentre,
				new Vector3(0f, 0f, 90f)
			},
			{
				eAnchorDirection.UpRight,
				new Vector3(0f, 0f, 45f)
			},
			{
				eAnchorDirection.MiddleLeft,
				new Vector3(0f, 0f, 180f)
			},
			{
				eAnchorDirection.MiddleCentre,
				new Vector3(0f, 0f, 0f)
			},
			{
				eAnchorDirection.MiddleRight,
				new Vector3(0f, 0f, 0f)
			},
			{
				eAnchorDirection.DownLeft,
				new Vector3(0f, 0f, -135f)
			},
			{
				eAnchorDirection.DownCentre,
				new Vector3(0f, 0f, -90f)
			},
			{
				eAnchorDirection.DownRight,
				new Vector3(0f, 0f, -45f)
			}
		};
	}

	protected override void AddListeners()
	{
	}

	protected override void ProtectedInit()
	{
		_allGameObjects = new List<GameObject> { _leftLine, _rightLine, _topLine, _bottomLine };
		_activeObjectLookup = new Dictionary<eAnchorDirection, List<GameObject>>
		{
			{
				eAnchorDirection.UpLeft,
				new List<GameObject> { _leftLine, _topLine }
			},
			{
				eAnchorDirection.UpCentre,
				new List<GameObject> { _topLine }
			},
			{
				eAnchorDirection.UpRight,
				new List<GameObject> { _rightLine, _topLine }
			},
			{
				eAnchorDirection.MiddleLeft,
				new List<GameObject> { _leftLine }
			},
			{
				eAnchorDirection.MiddleCentre,
				new List<GameObject>()
			},
			{
				eAnchorDirection.MiddleRight,
				new List<GameObject> { _rightLine }
			},
			{
				eAnchorDirection.DownLeft,
				new List<GameObject> { _leftLine, _bottomLine }
			},
			{
				eAnchorDirection.DownCentre,
				new List<GameObject> { _bottomLine }
			},
			{
				eAnchorDirection.DownRight,
				new List<GameObject> { _bottomLine, _rightLine }
			}
		};
	}

	protected override void ProtectedDispose()
	{
	}

	public void SetData(TutorialContent.AnchoredPopup anchoredContent, TutorialTransformAnchor anchor)
	{
		string message = anchoredContent.Message;
		string text = ((!Localizer.HasKey(message)) ? message.AddLineBreaks(15) : Localizer.LocalizeString(message));
		_popupParagraph.SetText(text);
		AnchoredContent = anchoredContent;
		ParentAnchor = anchor;
		SetObjectActive(isActive: true);
		SetMode();
		StartCoroutine(SetPosition());
	}

	private void SetMode()
	{
		if (!_activeObjectLookup.TryGetValue(AnchoredContent.HookDirection, out var value))
		{
			Debug.LogFormat("<color=yellow>[UI]</color> Attempting to get tutorial popup layout for grouping that hasn't been set [{0}]", AnchoredContent.HookDirection);
		}
		List<GameObject> list = _allGameObjects.Except(value).ToList();
		foreach (GameObject item in value)
		{
			item.SetActive(value: true);
		}
		foreach (GameObject item2 in list)
		{
			item2.SetActive(value: false);
		}
		_thisRect.SetPivotToDirection(AnchoredContent.HookDirection);
		_arrowRect.eulerAngles = _arrowRotation[AnchoredContent.HookDirection];
	}

	private IEnumerator SetPosition()
	{
		int iterations = 0;
		if (ParentAnchor.transform == null)
		{
			yield return null;
		}
		while (iterations < 5)
		{
			if (_arrowRect.position != ParentAnchor.transform.position)
			{
				_arrowRect.position = ParentAnchor.transform.position;
			}
			if (_thisRect.position != ParentAnchor.transform.position + _padding[AnchoredContent.HookDirection])
			{
				_thisRect.position = ParentAnchor.transform.position + _padding[AnchoredContent.HookDirection];
			}
			iterations++;
			yield return null;
		}
	}
}
