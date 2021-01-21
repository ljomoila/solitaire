using UnityEngine;

public class StateBase : MonoBehaviour
{

	public StateBase parentState, previousState;


	public virtual void UpdateState()
	{
	}

	public virtual void FixedUpdateState()
	{

	}

	public virtual void LateUpdateState()
	{

	}

	public virtual void OnActivateState()
	{
		
	}
	public virtual void OnDeactivateState()
	{
		
	}

	public virtual void EndState()
	{

	}

	public virtual void BackButtonPressed()
	{
		if (previousState != null)
		{
			StateManager.Instance.ActivateState(previousState);
		}
	}

	public void SetCollidersEnabled(bool v)
	{
		SetCollidersEnabled(v, transform);
	}

	public void SetCollidersEnabled(bool v, Transform t)
	{
		BoxCollider c = t.GetComponent<BoxCollider>();

		if (c != null)
		{
			c.enabled = v;
		}

		foreach (Transform child in t)
		{
			if (child == t)
				continue;

			SetCollidersEnabled(v, child);
		}
	}


	// Use this for initialization
	public virtual void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}
}

