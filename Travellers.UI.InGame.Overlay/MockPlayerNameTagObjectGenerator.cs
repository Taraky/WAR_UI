using UnityEngine;

namespace Travellers.UI.InGame.Overlay;

public class MockPlayerNameTagObjectGenerator : MonoBehaviour
{
	private int _numberCreated;

	private void Update()
	{
		if (Time.frameCount % 30 == 0 && _numberCreated < 20)
		{
			Vector3 position = new Vector3(Random.Range(-20f, 20f), Random.Range(-20f, 20f), Random.Range(0, 40));
			GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
			gameObject.AddComponent<MockPlayerNameTagTestObject>();
			gameObject.transform.position = position;
			_numberCreated++;
		}
	}
}
