using Godot;
using System.Collections.Generic;

namespace MechDefenseHalo.AI
{
    public class AIStateMachine
    {
        private Dictionary<string, AIState> _states = new();
        private AIState _currentState;
        
        public string CurrentStateName { get; private set; }
        
        public void AddState(string name, AIState state)
        {
            _states[name] = state;
        }
        
        public void ChangeState(string stateName)
        {
            if (!_states.ContainsKey(stateName))
            {
                GD.PrintErr($"State {stateName} not found!");
                return;
            }
            
            _currentState?.Exit();
            
            _currentState = _states[stateName];
            CurrentStateName = stateName;
            
            _currentState?.Enter();
            
            GD.Print($"AI State changed to: {stateName}");
        }
        
        public void Update(float delta)
        {
            _currentState?.Update(delta);
        }
    }
}
