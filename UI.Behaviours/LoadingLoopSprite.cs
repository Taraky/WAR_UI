using UnityEngine;

namespace UI.Behaviours;

public class LoadingLoopSprite : MonoBehaviour
{
	[SerializeField]
	private float loopPoint;

	[SerializeField]
	private float speed;

	[SerializeField]
	private Vector3 offset;

	private Vector3 pos;

	private void Start()
	{
		pos = base.transform.localPosition;
	}

	private void Update()
	{
		pos.x += speed * Time.deltaTime;
		pos.x %= loopPoint;
		base.transform.localPosition = offset + pos;
	}
}
