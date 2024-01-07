using Data;
using RSG;
using UnityEngine;

namespace UI;

public interface ILoadingScreen
{
	void Inject(LoadingScreenData data);

	IPromise<Object> Show(LoadingScreenDefinition.ScreenType screen);

	IPromise<Object> Hide();
}
