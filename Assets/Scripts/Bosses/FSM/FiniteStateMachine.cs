using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiniteStateMachine
{
    State _currentState = null;

    Dictionary<ObsidianGod.ObsidianStates, State> _allStates = new Dictionary<ObsidianGod.ObsidianStates, State>();
    public void Update()
    {
        _currentState?.OnUpdate();
    }

    public void AddState(ObsidianGod.ObsidianStates name, State state)
    {
        if (!_allStates.ContainsKey(name))
            _allStates.Add(name, state);
        else
            _allStates[name] = state;

        state.fsm = this;
    }

    public void ChangeState(ObsidianGod.ObsidianStates state)
    {
        _currentState?.OnExit();
        if (_allStates.ContainsKey(state))
            _currentState = _allStates[state];
        _currentState.OnEnter();
    }

    public void RestartState(ObsidianGod.ObsidianStates state)
    {
        if (_allStates.ContainsKey(state))
            _currentState = _allStates[state];
    }
}
