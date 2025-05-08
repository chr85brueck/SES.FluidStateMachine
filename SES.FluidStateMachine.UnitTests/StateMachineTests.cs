using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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
        public void CanTransitionBetweenStates()
        {
            // Arrange
            var stateMachine = new StateMachine<States, Events>(States.Off);
            stateMachine.Configure(States.Off).Permit(Events.PowerOn, States.On);
            stateMachine.Configure(States.On).Permit(Events.PowerOff, States.Off);

            // Act
            stateMachine.Fire(Events.PowerOn);
            var stateAfterPowerOn = stateMachine.CurrentState;

            stateMachine.Fire(Events.PowerOff);
            var stateAfterPowerOff = stateMachine.CurrentState;

            // Assert
            Assert.AreEqual(States.On, stateAfterPowerOn);
            Assert.AreEqual(States.Off, stateAfterPowerOff);
        }

        [TestMethod]
        public void EntryAndExitActionsAreExecuted()
        {
            // Arrange
            var stateMachine = new StateMachine<States, Events>(States.Off);
            bool enteredOnState = false;
            bool exitedOffState = false;

            stateMachine.Configure(States.Off)
                .Permit(Events.PowerOn, States.On)
                .OnExit(() => exitedOffState = true);

            stateMachine.Configure(States.On)
                .Permit(Events.PowerOff, States.Off)
                .OnEntry(() => enteredOnState = true);

            // Act
            stateMachine.Fire(Events.PowerOn);

            // Assert
            Assert.IsTrue(enteredOnState);
            Assert.IsTrue(exitedOffState);
        }

        [TestMethod]
        public void ThrowsExceptionForUndefinedTransition()
        {
            // Arrange
            var stateMachine = new StateMachine<States, Events>(States.Off);

            // Act & Assert
            var exception = Assert.ThrowsException<InvalidOperationException>(() =>
            {
                stateMachine.Fire(Events.PowerOff);
            });

            Assert.AreEqual("No transition defined from state Off using trigger PowerOff", exception.Message);
        }

        [TestMethod]
        public void CanHandleComplexStateTransitions()
        {
            // Arrange
            var stateMachine = new StateMachine<States, Events>(States.Off);
            stateMachine.Configure(States.Off)
                .Permit(Events.PowerOn, States.On);

            stateMachine.Configure(States.On)
                .Permit(Events.PowerOff, States.Off)
                .Permit(Events.StartHeating, States.Heating);

            stateMachine.Configure(States.Heating)
                .Permit(Events.StartCooling, States.Cooling);

            stateMachine.Configure(States.Cooling)
                .Permit(Events.StartHeating, States.Heating);

            // Act
            stateMachine.Fire(Events.PowerOn);
            var stateAfterPowerOn = stateMachine.CurrentState;

            stateMachine.Fire(Events.StartHeating);
            var stateAfterHeating = stateMachine.CurrentState;

            stateMachine.Fire(Events.StartCooling);
            var stateAfterCooling = stateMachine.CurrentState;

            stateMachine.Fire(Events.StartHeating);
            var stateAfterReHeating = stateMachine.CurrentState;

            // Assert
            Assert.AreEqual(States.On, stateAfterPowerOn);
            Assert.AreEqual(States.Heating, stateAfterHeating);
            Assert.AreEqual(States.Cooling, stateAfterCooling);
            Assert.AreEqual(States.Heating, stateAfterReHeating);
        }

        [TestMethod]
        public void CanDefineEntryAndExitActionsForStates()
        {
            // Arrange
            var stateMachine = new StateMachine<States, Events>(States.Off);
            bool enteredHeating = false;
            bool exitedOn = false;

            stateMachine.Configure(States.Off)
                .Permit(Events.PowerOn, States.On);

            stateMachine.Configure(States.On)
                .Permit(Events.StartHeating, States.Heating)
                .OnExit(() => exitedOn = true);

            stateMachine.Configure(States.Heating)
                .OnEntry(() => enteredHeating = true);

            // Act
            stateMachine.Fire(Events.PowerOn);
            stateMachine.Fire(Events.StartHeating);

            // Assert
            Assert.IsTrue(enteredHeating);
            Assert.IsTrue(exitedOn);
        }
    }
}