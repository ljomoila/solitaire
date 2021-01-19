using UnityEngine;
using System.Collections;

public class SoundsButton : MonoBehaviour
{
	public GameObject disabledLine;

	float vol = 100;

	// Use this for initialization
	void Start () 
	{
		vol = AudioController.GetCategory("SFX").Volume;
	}

	// Update is called once per frame
	void Update () 
	{

	}

	void SoundsButtonPressed()
	{
		Debug.Log("SoundsButtonPressed "+disabledLine.activeSelf);

		if (!disabledLine.activeSelf)
		{
			AudioController.GetCategory("SFX").Volume = 0;
			disabledLine.SetActive(true);
		}
		else
		{
			AudioController.GetCategory("SFX").Volume = vol;
			disabledLine.SetActive(false);
		}
	}
}

