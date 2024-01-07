using Bossa.Travellers.Game;
using Improbable.Unity.Visualizer;
using UnityEngine;

namespace Bossa.Travellers.UI.Behaviours;

public class ModalFabricMessagePopupVisualiser : MonoBehaviour
{
	[Require]
	private ModalPopupMessageState.Reader _state;

	private void OnEnable()
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUIStateEvents.ModalPopupRequested, _state.Data.messageWrapper);
	}
}
