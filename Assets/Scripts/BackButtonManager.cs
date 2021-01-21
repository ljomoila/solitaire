using UnityEngine;
using System.Collections.Generic;

public class BackButtonManager : MonoBehaviour
{

	private static BackButtonManager _instance = null;
	public static BackButtonManager Instance
	{
		get
		{
			if (_instance == null)
			{
				GameObject go = new GameObject("BackButtonManager");
				_instance = go.AddComponent<BackButtonManager>();
			}
			return _instance;
		}
	}

	void OnDisable()
	{
		_instance = null;
	}

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
#if UNITY_EDITOR || UNITY_ANDROID
		if (Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.Escape))
		{
			BackButtonPressed();
		}
#endif
	}

	public void AddListener(IBackButtonListener l)
	{
		if (buttonListeners.Contains(l) == false)
			buttonListeners.Add(l);
	}
	public void RemoveListener(IBackButtonListener l)
	{
		if (buttonListeners.Contains(l))
			buttonListeners.Remove(l);
	}

	public void BackButtonPressed()
	{
		if (buttonListeners.Count > 0)
		{
			IBackButtonListener l = buttonListeners[buttonListeners.Count - 1];

			bool handled = l.BackButtonPressed();

			if (handled)
			{
				RemoveListener(l);
			}
		}
		else
		{
			HandleQuit();
		}

	}

	void HandleQuit()
	{

	}


	private List<IBackButtonListener> _l = null;
	public List<IBackButtonListener> buttonListeners
	{
		get
		{
			if (_l == null)
				_l = new List<IBackButtonListener>();

			return _l;
		}
	}
}
