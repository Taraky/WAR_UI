using System;
using System.Collections;
using Assets.Scripts.Utils;
using Improbable.Unity.Core;
using Travellers.UI.Framework;
using Travellers.UI.Login;
using UnityEngine;

namespace Travellers.UI.InfoPopups;

public class ModalPopupButtonActions
{
	public static void Quit()
	{
		Application.Quit();
	}

	public static void ForceCleanBookkeepingAppOnNextLogin()
	{
		LocalPlayer.SetForceCleanBookkeepingDataOnNextLogin(value: true);
	}

	public static void DisconnectFromSpatial(Action onDisconnectedCallback)
	{
		LoadingScreen.SetScreenTypeAndVisibility(LoadingScreenVisibility.Enabled);
		UIWindowController.CloseAllWindows();
		VOIPManager.Shutdown(onDestroy: false, forced: true);
		if (LocalPlayer.Exists)
		{
			LocalPlayer.Instance.logoutBehaviour.TriggerLogoutCleanly(isExit: false);
		}
		JobManager.instance.StartCoroutine(WaitToDisconnectCoroutine(onDisconnectedCallback));
	}

	private static IEnumerator WaitToDisconnectCoroutine(Action onDisconnectedCallback)
	{
		yield return null;
		if (SpatialOS.IsConnected)
		{
			SpatialOS.Disconnect();
		}
		onDisconnectedCallback?.Invoke();
	}
}
