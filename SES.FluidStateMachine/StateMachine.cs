using System;
using System.Collections.Generic;

namespace SES.FluidStateMachine
{
    public class StateMachine<TState, TEvent>
    {
        private readonly Dictionary<TState, StateConfiguration> _stateConfigurations = 
            new Dictionary<TState, StateConfiguration>();

        private TState _currentState;

        public StateMachine(TState initialState)
        {
            _currentState = initialState;
        }

        public TState CurrentState => _currentState;

        public StateConfiguration Configure(TState state)
        {
            if (!_stateConfigurations.TryGetValue(state, out var configuration))
            {
                configuration = new StateConfiguration(state);
                _stateConfigurations[state] = configuration;
            }

            return configuration;
        }

        public void Fire(TEvent trigger)
        {
            if (_stateConfigurations.TryGetValue(_currentState, out var configuration))
            {
                if (configuration.TryGetTransition(trigger, out var destinationState))
                {
                    configuration.ExecuteExitAction();
                    _currentState = destinationState;
                    if (_stateConfigurations.TryGetValue(_currentState, out var newConfig))
                    {
                        newConfig.ExecuteEntryAction();
                    }
                }
                else
                {
                    throw new InvalidOperationException($"No transition defined from state {_currentState} using trigger {trigger}");
                }
            }
        }

        public class StateConfiguration
        {
            private readonly TState _state;
            private readonly Dictionary<TEvent, TState> _transitions = new Dictionary<TEvent, TState>();
            private Action _entryAction;
            private Action _exitAction;

            public StateConfiguration(TState state)
            {
                _state = state;
            }

            public StateConfiguration Permit(TEvent trigger, TState destinationState)
            {
                _transitions[trigger] = destinationState;
                return this;
            }

            public StateConfiguration OnEntry(Action action)
            {
                _entryAction = action;
                return this;
            }

            public StateConfiguration OnExit(Action action)
            {
                _exitAction = action;
                return this;
            }

            internal bool TryGetTransition(TEvent trigger, out TState destinationState)
            {
                return _transitions.TryGetValue(trigger, out destinationState);
            }

            internal void ExecuteEntryAction()
            {
                _entryAction?.Invoke();
            }

            internal void ExecuteExitAction()
            {
                _exitAction?.Invoke();
            }
        }
    }
}