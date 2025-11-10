using System;

namespace Systems.StateMachine
{
    public class StateMachine
    {
        private IState _currentState;
        private IState _previousState;

        public IState CurrentState => _currentState;
        
        public IState PreviousState => _previousState;

        public event Action<IState, IState> OnStateChanged;

        public void ChangeState(IState newState)
        {
            if (_currentState == newState)
            {
                return;
            }

            _previousState = _currentState;
            
            _currentState?.Exit();
            
            _currentState = newState;
            
            _currentState?.Enter();
            
            OnStateChanged?.Invoke(_previousState, _currentState);
        }

        public void Update()
        {
            _currentState?.Update();
        }

        public void RevertToPreviousState()
        {
            if (_previousState != null)
            {
                ChangeState(_previousState);
            }
        }
    }
}

