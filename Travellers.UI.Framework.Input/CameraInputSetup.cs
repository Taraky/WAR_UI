using UnityEngine;

namespace Travellers.UI.Framework.Input;

public class CameraInputSetup : MonoBehaviour
{
	[SerializeField]
	private InputBehaviour _input;

	protected void Start()
	{
		_input.enabled = true;
		_input.Input.SetCapture(InputPriorityClass.PlayerMovement, new InputButtons[2]
		{
			InputButtons.SwitchShoulder,
			InputButtons.FirstPerson
		}, new InputAxes[3]
		{
			InputAxes.CameraLeftRight,
			InputAxes.CameraUpDown,
			InputAxes.CameraZoom
		});
	}
}
