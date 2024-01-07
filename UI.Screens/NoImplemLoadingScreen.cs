using Data;
using RSG;
using UnityEngine;

namespace UI.Screens;

public class NoImplemLoadingScreen : ILoadingScreen
{
	public void Inject(LoadingScreenData data)
	{
	}

	public IPromise<Object> Show(LoadingScreenDefinition.ScreenType screen)
	{
		return Promise<Object>.Resolved(null);
	}

	public IPromise<Object> Hide()
	{
		return Promise<Object>.Resolved(null);
	}
}
