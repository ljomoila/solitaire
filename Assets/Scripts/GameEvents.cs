using UnityEngine;
using System.Collections.Generic;

public class GameEvents : MonoBehaviour {

	public static Dictionary<string, object> staticData;
	
	public static void SetData(string k, object v)
	{
		if (staticData == null)
		{
			staticData = new Dictionary<string, object>();
		}
		staticData[k] = v;
	}
	public static object GetData(string k)
	{
		object v = null;
		if (staticData != null && staticData.ContainsKey(k))
		{
			v = staticData[k];
		}
		return v;
	}

	public const string OnStoredStateLoaded = "OnStoredStateLoaded";

	public const string PauseRequest = "PauseRequest";
	
	public const string FoundationMoveDone = "FoundationMoveDone";
	
}
