using System;
using SES.FluidStateMachine;

namespace FluidStateMachine.Example
{
    public enum States
    {
        Off,
        On,
        Heating,
        Cooling
    }

    public enum Events
    {
        PowerOn,
        PowerOff,
        StartHeating,
        StartCooling
    }

    class Program
    {
        static void Main(string[] args)
        {
            var stateMachine = new StateMachine<States, Events>(States.Off);

            // Define the Off state
            stateMachine.Configure(States.Off)
                .Permit(Events.PowerOn, States.On)
                .OnEntry(() => Console.WriteLine("Entering Off"))
                .OnExit(() => Console.WriteLine("Exiting Off"));

            // Define the On state (not a substate of Off)
            stateMachine.Configure(States.On)
                .Permit(Events.PowerOff, States.Off)
                .Permit(Events.StartHeating, States.Heating)
                .OnEntry(() => Console.WriteLine("Entering On"))
                .OnExit(() => Console.WriteLine("Exiting On"));

            // Define Heating and Cooling as substates of On
            stateMachine.Configure(States.Heating)
                .Permit(Events.StartCooling, States.Cooling)
                .OnEntry(() => Console.WriteLine("Entering Heating"))
                .OnExit(() => Console.WriteLine("Exiting Heating"));

            stateMachine.Configure(States.Cooling)
                .Permit(Events.StartHeating, States.Heating)
                .OnEntry(() => Console.WriteLine("Entering Cooling"))
                .OnExit(() => Console.WriteLine("Exiting Cooling"));

            // Test the state machine
            Console.WriteLine($"Current State: {stateMachine.CurrentState}");
            stateMachine.Fire(Events.PowerOn);
            Console.WriteLine($"Current State: {stateMachine.CurrentState}");
            stateMachine.Fire(Events.StartHeating);
            Console.WriteLine($"Current State: {stateMachine.CurrentState}");
            stateMachine.Fire(Events.StartCooling);
            Console.WriteLine($"Current State: {stateMachine.CurrentState}");
            stateMachine.Fire(Events.PowerOff);
            Console.WriteLine($"Current State: {stateMachine.CurrentState}");
        }
    }
}