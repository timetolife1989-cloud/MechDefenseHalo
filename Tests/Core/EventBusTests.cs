using Godot;
using GdUnit4;
using MechDefenseHalo.Core;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests.Core
{
    /// <summary>
    /// Unit tests for EventBus system
    /// 
    /// SETUP:
    /// 1. Install GdUnit4 from Asset Library
    /// 2. Enable plugin in Project Settings
    /// 3. Run: GdUnit4 > Run Tests
    /// </summary>
    [TestSuite]
    public class EventBusTests
    {
        private EventBus _eventBus;

        [Before]
        public void Setup()
        {
            _eventBus = new EventBus();
        }

        [After]
        public void Teardown()
        {
            _eventBus = null;
        }

        [TestCase]
        public void EmitEvent_WithNoListeners_ShouldNotThrow()
        {
            // Act & Assert
            AssertThat(() => _eventBus.Emit("test_event", null))
                .Not().ThrowsException();
        }

        [TestCase]
        public void OnEvent_ShouldRegisterListener()
        {
            // Arrange
            bool callbackInvoked = false;
            _eventBus.On("test_event", (data) => callbackInvoked = true);

            // Act
            _eventBus.Emit("test_event", null);

            // Assert
            AssertBool(callbackInvoked).IsTrue();
        }

        [TestCase]
        public void EmitEvent_WithData_ShouldPassDataToListener()
        {
            // Arrange
            object receivedData = null;
            var testData = new { Value = 42 };
            _eventBus.On("test_event", (data) => receivedData = data);

            // Act
            _eventBus.Emit("test_event", testData);

            // Assert
            AssertObject(receivedData).IsNotNull();
            AssertThat(receivedData).IsEqual(testData);
        }

        [TestCase]
        public void OffEvent_ShouldUnregisterListener()
        {
            // Arrange
            int callCount = 0;
            System.Action<object> callback = (data) => callCount++;
            _eventBus.On("test_event", callback);

            // Act
            _eventBus.Emit("test_event", null);
            _eventBus.Off("test_event", callback);
            _eventBus.Emit("test_event", null);

            // Assert
            AssertInt(callCount).IsEqual(1);
        }

        [TestCase]
        public void EmitEvent_WithMultipleListeners_ShouldCallAll()
        {
            // Arrange
            int counter = 0;
            _eventBus.On("test_event", (data) => counter++);
            _eventBus.On("test_event", (data) => counter++);
            _eventBus.On("test_event", (data) => counter++);

            // Act
            _eventBus.Emit("test_event", null);

            // Assert
            AssertInt(counter).IsEqual(3);
        }

        [TestCase]
        public void ClearEvent_ShouldRemoveAllListeners()
        {
            // Arrange
            int counter = 0;
            _eventBus.On("test_event", (data) => counter++);
            _eventBus.On("test_event", (data) => counter++);

            // Act
            _eventBus.ClearEvent("test_event");
            _eventBus.Emit("test_event", null);

            // Assert
            AssertInt(counter).IsEqual(0);
        }

        [TestCase]
        public void ClearAll_ShouldRemoveAllEventListeners()
        {
            // Arrange
            int counter = 0;
            _eventBus.On("event1", (data) => counter++);
            _eventBus.On("event2", (data) => counter++);

            // Act
            _eventBus.ClearAll();
            _eventBus.Emit("event1", null);
            _eventBus.Emit("event2", null);

            // Assert
            AssertInt(counter).IsEqual(0);
        }
    }
}
