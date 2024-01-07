using UnityEngine;

namespace Travellers.UI.Utility;

public class RandomHelper
{
	public static string Random(string[] input)
	{
		return input[UnityEngine.Random.Range(0, input.Length)];
	}
}
