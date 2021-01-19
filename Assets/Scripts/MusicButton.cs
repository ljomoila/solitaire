using UnityEngine;
using System.Collections;


public class MusicButton : MonoBehaviour
{
	public GameObject disabledLine;

	float vol = 50;

	// Use this for initialization
	void Start () 
	{
		vol = AudioController.GetCategory("Music").Volume;
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	void MusicButtonPressed()
	{
		Debug.Log("MusicButtonPressed "+disabledLine.activeSelf);

		if (!disabledLine.activeSelf)
		{
			AudioController.SetCategoryVolume("Music", 0);
			disabledLine.SetActive(true);
		}
		else
		{
			AudioController.SetCategoryVolume("Music", vol);
			disabledLine.SetActive(false);
		}
	}
}
