using System.Linq;
using Data;
using RSG;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Screens;

public class AssetLoadingScreen : MonoBehaviour, ILoadingScreen
{
	[SerializeField]
	private RectTransform _dynamicLoadingScreen;

	[SerializeField]
	private GameObject _tipsRoot;

	[SerializeField]
	private Text _textObject;

	private Image _image;

	private LoadingScreenData _data;

	public void Inject(LoadingScreenData data)
	{
		_data = data;
		EnableText();
		_image = _dynamicLoadingScreen.GetComponentInChildren<Image>();
	}

	public IPromise<Object> Show(LoadingScreenDefinition.ScreenType screen)
	{
		if (!base.gameObject.activeSelf)
		{
			ShowTip();
		}
		LoadingScreenDefinition loadingScreenDefinition = _data.Data.First((LoadingScreenDefinition o) => o.Screen == screen);
		_image.sprite = loadingScreenDefinition.Asset;
		base.gameObject.SetActive(value: true);
		return Promise<Object>.Resolved(null);
	}

	public IPromise<Object> Hide()
	{
		base.gameObject.SetActive(value: false);
		return Promise<Object>.Resolved(null);
	}

	private void EnableText()
	{
		_tipsRoot.SetActive(_data.Tips != null && _data.Tips.Any());
	}

	private void ShowTip()
	{
		if (_data.Tips != null && _data.Tips.Any())
		{
			int index = Random.Range(0, _data.Tips.Count());
			_textObject.text = _data.Tips.ElementAt(index);
		}
	}
}
