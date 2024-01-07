using Travellers.UI.Framework;

namespace Travellers.UI.Sound;

public class SoundScreen : UIScreen
{
	private AudioPlayer _sound;

	protected override void AddListeners()
	{
		_eventList.AddEvent(WAUISoundEvents.PlaySound, OnPlaySoundRequested);
	}

	protected override void ProtectedInit()
	{
		_sound = AudioPlayer.Initialize(base.gameObject);
	}

	protected override void ProtectedDispose()
	{
	}

	public static void PlayASound(string sound)
	{
		Singleton<WAEventSystem>.Instance.TriggerEvent(WAUISoundEvents.PlaySound, new PlayUiSoundEvent(sound));
	}

	private void OnPlaySoundRequested(object[] obj)
	{
		PlayUiSoundEvent playUiSoundEvent = (PlayUiSoundEvent)obj[0];
		PlaySound(playUiSoundEvent.SoundId);
	}

	private void PlaySound(string soundId)
	{
		if (_sound != null)
		{
			_sound.PostEvent(soundId);
		}
	}
}
