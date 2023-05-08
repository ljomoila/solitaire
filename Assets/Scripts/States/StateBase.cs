using UnityEngine;

public class StateBase : MonoBehaviour
{
    public StateBase parentState,
        previousState;

    public virtual void OnActivateState() { }

    public virtual void OnActivateState(StateBase parentState) { }

    public virtual void OnDeactivateState() { }

    public virtual void UpdateState() { }

    public virtual void FixedUpdateState() { }

    public virtual void LateUpdateState() { }

    public virtual void EndState() { }

    public virtual void BackButtonPressed()
    {
        if (previousState != null)
        {
            StateManager.Instance.ActivateState(previousState);
        }
    }
}
