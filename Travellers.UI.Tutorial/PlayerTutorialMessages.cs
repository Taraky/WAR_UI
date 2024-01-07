using System.Collections.Generic;
using Travellers.UI.Events;
using UnityEngine;

namespace Travellers.UI.Tutorial;

public class PlayerTutorialMessages : MonoBehaviour
{
	[SerializeField]
	public PlayerMove playerMove;

	[SerializeField]
	public GrapplingHookNew grapplingHook;

	private bool _playerWasInRespawner;

	private bool _previousGrapplingHookActive;

	private bool _previousIsNearWall;

	private bool _previousIsClimbing;

	private readonly List<TutorialStep> _playerTutorialSteps = new List<TutorialStep>
	{
		TutorialStep.FIRST_REVIVAL_CHAMBER,
		TutorialStep.GRAPLING_HOOK_CAN_CLIMB,
		TutorialStep.GRAPLING_HOOK,
		TutorialStep.CLIMBABLE_SURFACE,
		TutorialStep.CLIMBING
	};

	private void Awake()
	{
		if (!WorldsAdrift.IsOnStack)
		{
			Object.Destroy(this);
		}
	}

	private void Update()
	{
		if (playerMove.isInRespawner && !_playerWasInRespawner && !grapplingHook.rope.IsActive && !playerMove.IsClimbing)
		{
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUITutorialEvents.AddStepToSystem, new TutorialAddStepToSystemEvent(TutorialStep.FIRST_REVIVAL_CHAMBER));
			_playerWasInRespawner = true;
		}
		else if (!playerMove.isInRespawner && _playerWasInRespawner)
		{
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUITutorialEvents.AddStepToSystem, new TutorialAddStepToSystemEvent(TutorialStep.LEAVE_REVIVAL_CHAMBER));
			_playerWasInRespawner = false;
		}
		else if (grapplingHook.rope.IsActive)
		{
			if (!_previousGrapplingHookActive || playerMove.IsNearWall != _previousIsNearWall)
			{
				if (playerMove.IsNearWall)
				{
					Singleton<WAEventSystem>.Instance.TriggerEvent(WAUITutorialEvents.AddStepToSystem, new TutorialAddStepToSystemEvent(TutorialStep.GRAPLING_HOOK_CAN_CLIMB));
				}
				else
				{
					Singleton<WAEventSystem>.Instance.TriggerEvent(WAUITutorialEvents.AddStepToSystem, new TutorialAddStepToSystemEvent(TutorialStep.GRAPLING_HOOK));
				}
			}
		}
		else if (playerMove.IsNearWall && !playerMove.IsClimbing)
		{
			if (!_previousIsNearWall || _previousIsClimbing)
			{
				Singleton<WAEventSystem>.Instance.TriggerEvent(WAUITutorialEvents.AddStepToSystem, new TutorialAddStepToSystemEvent(TutorialStep.CLIMBABLE_SURFACE));
			}
		}
		else if (playerMove.IsClimbing)
		{
			if (!_previousIsClimbing)
			{
				Singleton<WAEventSystem>.Instance.TriggerEvent(WAUITutorialEvents.AddStepToSystem, new TutorialAddStepToSystemEvent(TutorialStep.CLIMBING));
			}
		}
		else
		{
			Singleton<WAEventSystem>.Instance.TriggerEvent(WAUITutorialEvents.RemoveStepFromSystem, new TutorialRemoveStepFromSystemEvent(_playerTutorialSteps));
		}
		_previousGrapplingHookActive = grapplingHook.rope.IsActive;
		_previousIsNearWall = playerMove.IsNearWall;
		_previousIsClimbing = playerMove.IsClimbing;
	}
}
