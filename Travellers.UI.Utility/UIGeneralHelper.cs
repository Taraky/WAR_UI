using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using WAUtilities.Logging;

namespace Travellers.UI.Utility;

public static class UIGeneralHelper
{
	public static void SetGameobjectStates(List<GameObject> allObject, List<GameObject> objectsToTurnOn)
	{
		List<GameObject> list = allObject.Except(objectsToTurnOn).ToList();
		foreach (GameObject item in objectsToTurnOn)
		{
			item.SetActive(value: true);
		}
		foreach (GameObject item2 in list)
		{
			item2.SetActive(value: false);
		}
	}

	public static string Dump<T>(object instance)
	{
		try
		{
			JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
			jsonSerializerSettings.Formatting = Formatting.Indented;
			JsonSerializerSettings settings = jsonSerializerSettings;
			return JsonConvert.SerializeObject(instance, typeof(T), settings);
		}
		catch
		{
			WALogger.Warn<T>(LogChannel.UI, "Trying to serialise data, but can't", new object[0]);
			return "UNABLE TO SERIALISE DATA";
		}
	}
}
