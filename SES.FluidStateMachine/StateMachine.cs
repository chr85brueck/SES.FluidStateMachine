using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        public async Task FireAsync(TEvent trigger)
        {
            if (_stateConfigurations.TryGetValue(_currentState, out var configuration))
            {
                if (configuration.TryGetTransition(trigger, out var destinationState))
                {
                    await configuration.ExecuteExitActionAsync();
                    _currentState = destinationState;
                    if (_stateConfigurations.TryGetValue(_currentState, out var newConfig))
                    {
                        await newConfig.ExecuteEntryActionAsync();
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
            private int _entryDelay; // Delay in milliseconds
            private int _exitDelay;  // Delay in milliseconds

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

            public StateConfiguration WithEntryDelay(int milliseconds)
            {
                _entryDelay = milliseconds;
                return this;
            }

            public StateConfiguration WithExitDelay(int milliseconds)
            {
                _exitDelay = milliseconds;
                return this;
            }

            internal bool TryGetTransition(TEvent trigger, out TState destinationState)
            {
                return _transitions.TryGetValue(trigger, out destinationState);
            }

            internal async Task ExecuteEntryActionAsync()
            {
                if (_entryDelay > 0)
                {
                    await Task.Delay(_entryDelay);
                }
                _entryAction?.Invoke();
            }

            internal async Task ExecuteExitActionAsync()
            {
                if (_exitDelay > 0)
                {
                    await Task.Delay(_exitDelay);
                }
                _exitAction?.Invoke();
            }
        }
    }
}