using System;
using System.Threading.Tasks;
using SES.FluidStateMachine;

namespace StateMachineDemo
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
        static async Task Main(string[] args)
        {
            // Instantiate the state machine with the initial state as "Off"
            var stateMachine = new StateMachine<States, Events>(States.Off);

            // Configure the "Off" state
            stateMachine
                .Configure(States.Off)
                .Permit(Events.PowerOn, States.On)
                .OnExit(() => Console.WriteLine("Exiting Off state..."))
                .WithExitDelay(2000); // 2 seconds delay

            // Configure the "On" state
            stateMachine
                .Configure(States.On)
                .Permit(Events.PowerOff, States.Off)
                .Permit(Events.StartHeating, States.Heating)
                .OnEntry(() => Console.WriteLine("Entering On state..."))
                .WithEntryDelay(1000) // 1 second delay
                .OnExit(() => Console.WriteLine("Exiting On state..."))
                .WithExitDelay(2000); // 2 seconds delay

            // Configure the "Heating" state
            stateMachine
                .Configure(States.Heating)
                .Permit(Events.StartCooling, States.Cooling)
                .OnEntry(() => Console.WriteLine("Entering Heating state..."))
                .WithEntryDelay(1500) // 1.5 seconds delay
                .OnExit(() => Console.WriteLine("Exiting Heating state..."))
                .WithExitDelay(1500); // 1.5 seconds delay

            // Configure the "Cooling" state
            stateMachine
                .Configure(States.Cooling)
                .Permit(Events.StartHeating, States.Heating)
                .OnEntry(() => Console.WriteLine("Entering Cooling state..."))
                .WithEntryDelay(1500) // 1.5 seconds delay
                .OnExit(() => Console.WriteLine("Exiting Cooling state..."))
                .WithExitDelay(1500); // 1.5 seconds delay

            stateMachine
                .Configure(States.Heating)
                .Permit(Events.PowerOff, States.Off)
                .OnEntry(() => Console.WriteLine("Entering Heating state..."));

            // Simulate transitions
            Console.WriteLine($"Initial State: {stateMachine.CurrentState}");
            await stateMachine.FireAsync(Events.PowerOn);
            Console.WriteLine($"Current State: {stateMachine.CurrentState}");

            await stateMachine.FireAsync(Events.StartHeating);
            Console.WriteLine($"Current State: {stateMachine.CurrentState}");

            await stateMachine.FireAsync(Events.StartCooling);
            Console.WriteLine($"Current State: {stateMachine.CurrentState}");

            await stateMachine.FireAsync(Events.StartHeating);
            Console.WriteLine($"Current State: {stateMachine.CurrentState}");

            await stateMachine.FireAsync(Events.PowerOff);
            Console.WriteLine($"Current State: {stateMachine.CurrentState}");

            Console.WriteLine("Demo complete!");
        }
    }
}