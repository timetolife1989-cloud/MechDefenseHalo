using Godot;
using GdUnit4;
using static GdUnit4.Assertions;
using MechDefenseHalo.AI;
using MechDefenseHalo.AI.States;

namespace MechDefenseHalo.Tests.AI
{
    /// <summary>
    /// Unit tests for AI State Machine
    /// Tests state transitions, state management, and update cycles
    /// </summary>
    [TestSuite]
    public class AIStateMachineTests
    {
        private AIStateMachine _stateMachine;
        private MockEnemyAIController _mockController;
        
        [Before]
        public void Setup()
        {
            _stateMachine = new AIStateMachine();
            _mockController = new MockEnemyAIController();
        }
        
        [After]
        public void Teardown()
        {
            _stateMachine = null;
            _mockController = null;
        }
        
        [TestCase]
        public void AddState_ValidState_ShouldAddSuccessfully()
        {
            // Arrange
            var testState = new TestState(_mockController);
            
            // Act
            _stateMachine.AddState("Test", testState);
            
            // Assert - state should be added (no exception thrown)
            AssertObject(_stateMachine).IsNotNull();
        }
        
        [TestCase]
        public void ChangeState_ExistingState_ShouldTransition()
        {
            // Arrange
            var testState = new TestState(_mockController);
            _stateMachine.AddState("Test", testState);
            
            // Act
            _stateMachine.ChangeState("Test");
            
            // Assert
            AssertString(_stateMachine.CurrentStateName).IsEqual("Test");
            AssertBool(testState.EnterCalled).IsTrue();
        }
        
        [TestCase]
        public void ChangeState_NonExistingState_ShouldNotCrash()
        {
            // Act & Assert - should not throw exception
            AssertThat(() => _stateMachine.ChangeState("NonExisting")).Not().ThrowsException();
        }
        
        [TestCase]
        public void ChangeState_FromOneToAnother_ShouldCallExitAndEnter()
        {
            // Arrange
            var state1 = new TestState(_mockController);
            var state2 = new TestState(_mockController);
            _stateMachine.AddState("State1", state1);
            _stateMachine.AddState("State2", state2);
            
            _stateMachine.ChangeState("State1");
            
            // Act
            _stateMachine.ChangeState("State2");
            
            // Assert
            AssertBool(state1.ExitCalled).IsTrue();
            AssertBool(state2.EnterCalled).IsTrue();
            AssertString(_stateMachine.CurrentStateName).IsEqual("State2");
        }
        
        [TestCase]
        public void Update_WithActiveState_ShouldCallStateUpdate()
        {
            // Arrange
            var testState = new TestState(_mockController);
            _stateMachine.AddState("Test", testState);
            _stateMachine.ChangeState("Test");
            
            // Act
            _stateMachine.Update(0.016f);
            
            // Assert
            AssertBool(testState.UpdateCalled).IsTrue();
        }
        
        [TestCase]
        public void Update_WithNoActiveState_ShouldNotCrash()
        {
            // Act & Assert - should not throw exception
            AssertThat(() => _stateMachine.Update(0.016f)).Not().ThrowsException();
        }
    }
    
    // Mock classes for testing
    public class TestState : AIState
    {
        public bool EnterCalled { get; private set; }
        public bool UpdateCalled { get; private set; }
        public bool ExitCalled { get; private set; }
        
        public TestState(EnemyAIController controller) : base(controller) { }
        
        public override void Enter()
        {
            EnterCalled = true;
        }
        
        public override void Update(float delta)
        {
            UpdateCalled = true;
        }
        
        public override void Exit()
        {
            ExitCalled = true;
        }
    }
    
    public class MockEnemyAIController : EnemyAIController
    {
        // Mock implementation for testing
    }
}
