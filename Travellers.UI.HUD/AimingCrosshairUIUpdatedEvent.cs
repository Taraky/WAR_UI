using Travellers.UI.Events;
using UnityEngine;

namespace Travellers.UI.HUD;

public class AimingCrosshairUIUpdatedEvent : UIEvent
{
	public bool CrosshairIsVisible;

	public Texture2D Texture;

	public float HandleDistance;

	public bool Show;

	private AimingCrosshairUIUpdatedEvent(bool show, bool crosshairIsVisible, Texture2D texture, float handleDistance)
	{
		CrosshairIsVisible = crosshairIsVisible;
		Texture = texture;
		HandleDistance = handleDistance;
		Show = show;
	}

	public static AimingCrosshairUIUpdatedEvent AimingCrosshairStateChange(bool show)
	{
		return new AimingCrosshairUIUpdatedEvent(show, crosshairIsVisible: false, null, 0f);
	}

	public static AimingCrosshairUIUpdatedEvent AimingCrosshairTextureChange(Texture2D texture)
	{
		return new AimingCrosshairUIUpdatedEvent(show: false, crosshairIsVisible: false, texture, 0f);
	}

	public static AimingCrosshairUIUpdatedEvent AimingCrosshairHandleDistanceChange(float distance)
	{
		return new AimingCrosshairUIUpdatedEvent(show: false, crosshairIsVisible: false, null, distance);
	}

	public static AimingCrosshairUIUpdatedEvent AimingCrosshairVisibilityChange(bool canSee)
	{
		return new AimingCrosshairUIUpdatedEvent(show: false, canSee, null, 0f);
	}
}
