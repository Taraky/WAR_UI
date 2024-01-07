using System.Collections.Generic;
using Travellers.UI.Framework;

namespace Travellers.UI;

public static class UISingleton
{
	private static List<UISystem> _allSystems;

	public static void InitialiseUI()
	{
		UIStructure.Create();
		_allSystems = DependencyInjectionService.Singleton.ResolveAllSystems();
	}

	public static void ControlledUpdate()
	{
		UIWindowController.ControlledUpdate();
		foreach (UISystem allSystem in _allSystems)
		{
			allSystem.ControlledUpdate();
		}
	}

	public static void Dispose()
	{
		foreach (UISystem allSystem in _allSystems)
		{
			allSystem.BaseDispose();
		}
	}
}
