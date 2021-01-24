using System.Xml.Linq;
using UnityEngine;

public class StateManager : MonoBehaviour, IBackButtonListener
{
    public static StateManager Instance;

    public StateBase firstState;

    public StateBase activeState;

    float t = 0;

    void Awake()
    {
        Instance = this;
    }

    public void ActivateState(StateBase s)
    {
        if (activeState != null)
        {
            activeState.OnDeactivateState();

            s.previousState = activeState;
        }

        activeState = s;

        s.OnActivateState();
    }

    public bool BackButtonPressed()
    {
        if (activeState != null)
        {
            activeState.BackButtonPressed();
        }

        return true;
    }

    // Update is called once per frame
    void Update()
    {
        if (activeState == null && firstState != null)
        {
            ActivateState(firstState);
        }

        if (activeState != null)
        {
            activeState.UpdateState();

            ActiveStateCheck();
        }
    }

    void ActiveStateCheck()
    {
        t += Time.deltaTime;

        if (t >= 1f) // making sure activestate shows, if escape has been räpytetty
        {
            t = 0;
        }
    }

    void FixedUpdate()
    {
        if (activeState != null)
        {
            activeState.FixedUpdateState();
        }
    }

    void LateUpdate()
    {
        if (activeState != null)
        {
            activeState.LateUpdateState();
        }
    }

    public bool IsActiveState(StateBase sb)
    {
        if (activeState != null && sb.gameObject == activeState.gameObject)
            return true;

        return false;
    }
}
