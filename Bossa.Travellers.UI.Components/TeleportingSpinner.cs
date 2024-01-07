using UnityEngine;
using UnityEngine.UI;

namespace Bossa.Travellers.UI.Components;

public class TeleportingSpinner : MonoBehaviour
{
	private SpriteRenderer _spriteRenderer;

	private Image _image;

	private void Start()
	{
		_spriteRenderer = GetComponent<SpriteRenderer>();
		_image = GetComponent<Image>();
	}

	private void Update()
	{
		_image.sprite = _spriteRenderer.sprite;
	}
}
