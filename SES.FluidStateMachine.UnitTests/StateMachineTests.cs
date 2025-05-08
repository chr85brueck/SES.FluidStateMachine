using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace SES.FluidStateMachine.Tests
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

    [TestClass]
    public class StateMachineTests
    {
        [TestMethod]
        public async Task CanTransitionBetweenStates_WithDelays()
        {
            // Arrange
            var stateMachine = new StateMachine<States, Events>(States.Off);
            stateMachine.Configure(States.Off)
                .Permit(Events.PowerOn, States.On)
                .WithExitDelay(500); // 500ms delay

            stateMachine.Configure(States.On)
                .Permit(Events.PowerOff, States.Off)
                .WithEntryDelay(500); // 500ms delay

            // Act
            var startTime = DateTime.UtcNow;
            await stateMachine.FireAsync(Events.PowerOn);
            var stateAfterPowerOn = stateMachine.CurrentState;

            await stateMachine.FireAsync(Events.PowerOff);
            var stateAfterPowerOff = stateMachine.CurrentState;
            var duration = DateTime.UtcNow - startTime;

            // Assert
            Assert.AreEqual(States.On, stateAfterPowerOn);
            Assert.AreEqual(States.Off, stateAfterPowerOff);
            Assert.IsTrue(duration.TotalMilliseconds >= 1000, "Delays were not applied correctly.");
        }

        [TestMethod]
        public async Task EntryAndExitActionsAreExecuted_WithDelays()
        {
            // Arrange
            var stateMachine = new StateMachine<States, Events>(States.Off);
            bool enteredOnState = false;
            bool exitedOffState = false;

            stateMachine.Configure(States.Off)
                .Permit(Events.PowerOn, States.On)
                .OnExit(() => exitedOffState = true)
                .WithExitDelay(500); // 500ms delay

            stateMachine.Configure(States.On)
                .Permit(Events.PowerOff, States.Off)
                .OnEntry(() => enteredOnState = true)
                .WithEntryDelay(500); // 500ms delay

            // Act
            var startTime = DateTime.UtcNow;
            await stateMachine.FireAsync(Events.PowerOn);
            var duration = DateTime.UtcNow - startTime;

            // Assert
            Assert.IsTrue(enteredOnState, "Entry action was not executed.");
            Assert.IsTrue(exitedOffState, "Exit action was not executed.");
            Assert.IsTrue(duration.TotalMilliseconds >= 500, "Delays were not applied correctly.");
        }

        [TestMethod]
        public async Task CanHandleComplexStateTransitions_WithDelays()
        {
            // Arrange
            var stateMachine = new StateMachine<States, Events>(States.Off);
            stateMachine.Configure(States.Off)
                .Permit(Events.PowerOn, States.On)
                .WithExitDelay(500); // 500ms delay

            stateMachine.Configure(States.On)
                .Permit(Events.PowerOff, States.Off)
                .Permit(Events.StartHeating, States.Heating)
                .WithEntryDelay(500); // 500ms delay

            stateMachine.Configure(States.Heating)
                .Permit(Events.StartCooling, States.Cooling)
                .WithEntryDelay(500)
                .WithExitDelay(500);

            stateMachine.Configure(States.Cooling)
                .Permit(Events.StartHeating, States.Heating)
                .WithEntryDelay(500);

            // Act
            await stateMachine.FireAsync(Events.PowerOn);
            var stateAfterPowerOn = stateMachine.CurrentState;

            await stateMachine.FireAsync(Events.StartHeating);
            var stateAfterHeating = stateMachine.CurrentState;

            await stateMachine.FireAsync(Events.StartCooling);
            var stateAfterCooling = stateMachine.CurrentState;

            await stateMachine.FireAsync(Events.StartHeating);
            var stateAfterReHeating = stateMachine.CurrentState;

            // Assert
            Assert.AreEqual(States.On, stateAfterPowerOn);
            Assert.AreEqual(States.Heating, stateAfterHeating);
            Assert.AreEqual(States.Cooling, stateAfterCooling);
            Assert.AreEqual(States.Heating, stateAfterReHeating);
        }

        [TestMethod]
        public async Task CanDefineEntryAndExitActionsForStates_WithDelays()
        {
            // Arrange
            var stateMachine = new StateMachine<States, Events>(States.Off);
            bool enteredHeating = false;
            bool exitedOn = false;

            stateMachine.Configure(States.Off)
                .Permit(Events.PowerOn, States.On);

            stateMachine.Configure(States.On)
                .Permit(Events.StartHeating, States.Heating)
                .OnExit(() => exitedOn = true)
                .WithExitDelay(500); // 500ms delay

            stateMachine.Configure(States.Heating)
                .OnEntry(() => enteredHeating = true)
                .WithEntryDelay(500); // 500ms delay

            // Act
            await stateMachine.FireAsync(Events.PowerOn);
            await stateMachine.FireAsync(Events.StartHeating);

            // Assert
            Assert.IsTrue(enteredHeating, "Heating state entry action did not execute.");
            Assert.IsTrue(exitedOn, "On state exit action did not execute.");
        }
    }
}