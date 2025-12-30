using Godot;
using GdUnit4;
using MechDefenseHalo.Quests;
using MechDefenseHalo.Core;
using MechDefenseHalo.Economy;
using MechDefenseHalo.Progression;
using static GdUnit4.Assertions;
using System.Collections.Generic;

namespace MechDefenseHalo.Tests.Quests
{
    /// <summary>
    /// Unit tests for QuestManager system
    /// Tests quest creation, progression, and completion
    /// </summary>
    [TestSuite]
    public class QuestManagerTests
    {
        private QuestManager _questManager;
        private EventBus _eventBus;
        private CurrencyManager _currencyManager;
        private PlayerLevel _playerLevel;

        [Before]
        public void Setup()
        {
            // Create and initialize EventBus
            _eventBus = new EventBus();
            
            // Create and initialize CurrencyManager
            _currencyManager = new CurrencyManager();
            CurrencyManager.SetCredits(0);
            CurrencyManager.SetCores(0);
            
            // Create and initialize PlayerLevel
            _playerLevel = new PlayerLevel();
            PlayerLevel.SetLevel(1, 0);
            
            // Create QuestManager
            _questManager = new QuestManager();
        }

        [After]
        public void Teardown()
        {
            _questManager = null;
            _eventBus = null;
            _currencyManager = null;
            _playerLevel = null;
        }

        [TestCase]
        public void QuestManager_ShouldHaveRegisteredQuests()
        {
            // Act - QuestManager registers quests in constructor/Ready
            
            // Assert
            AssertObject(QuestManager.Instance).IsNotNull();
            AssertThat(_questManager.AllQuests.Count).IsGreater(0);
        }

        [TestCase]
        public void StartQuest_WithValidId_ShouldActivateQuest()
        {
            // Arrange
            var quest = new Quest
            {
                Id = "test_quest",
                Name = "Test Quest",
                Description = "A test quest",
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective { Description = "Test objective", RequiredCount = 1 }
                },
                Rewards = new QuestRewards { Credits = 100 }
            };

            // Add quest directly to internal dictionary via StartQuest
            // Since we can't access private methods, we'll use reflection or test with existing quests
            
            // Act - Start an existing quest (tutorial_quest should exist)
            _questManager.StartQuest("tutorial_quest");
            
            // Assert
            var startedQuest = _questManager.GetQuest("tutorial_quest");
            AssertObject(startedQuest).IsNotNull();
            AssertThat(startedQuest.Status).IsEqual(QuestStatus.Active);
            AssertThat(_questManager.ActiveQuests.Count).IsGreater(0);
        }

        [TestCase]
        public void StartQuest_WithInvalidId_ShouldNotThrow()
        {
            // Act & Assert
            AssertThat(() => _questManager.StartQuest("invalid_quest_id"))
                .Not().ThrowsException();
        }

        [TestCase]
        public void StartQuest_AlreadyActive_ShouldNotStartAgain()
        {
            // Arrange
            _questManager.StartQuest("tutorial_quest");
            int activeCount = _questManager.ActiveQuests.Count;
            
            // Act - Try to start the same quest again
            _questManager.StartQuest("tutorial_quest");
            
            // Assert
            AssertInt(_questManager.ActiveQuests.Count).IsEqual(activeCount);
        }

        [TestCase]
        public void UpdateObjective_WithValidData_ShouldUpdateProgress()
        {
            // Arrange
            _questManager.StartQuest("tutorial_quest");
            var quest = _questManager.GetQuest("tutorial_quest");
            
            // Act
            _questManager.UpdateObjective("tutorial_quest", 0, 2);
            
            // Assert
            AssertInt(quest.Objectives[0].CurrentCount).IsEqual(2);
        }

        [TestCase]
        public void UpdateObjective_CompletingObjective_ShouldMarkAsCompleted()
        {
            // Arrange
            _questManager.StartQuest("tutorial_quest");
            var quest = _questManager.GetQuest("tutorial_quest");
            
            // Act - Complete first objective (requires 5)
            _questManager.UpdateObjective("tutorial_quest", 0, 5);
            
            // Assert
            AssertBool(quest.Objectives[0].IsCompleted).IsTrue();
        }

        [TestCase]
        public void UpdateObjective_InvalidQuestId_ShouldNotThrow()
        {
            // Act & Assert
            AssertThat(() => _questManager.UpdateObjective("invalid_id", 0, 1))
                .Not().ThrowsException();
        }

        [TestCase]
        public void UpdateObjective_InvalidObjectiveIndex_ShouldNotThrow()
        {
            // Arrange
            _questManager.StartQuest("tutorial_quest");
            
            // Act & Assert
            AssertThat(() => _questManager.UpdateObjective("tutorial_quest", 999, 1))
                .Not().ThrowsException();
        }

        [TestCase]
        public void CompleteQuest_WithAllObjectivesDone_ShouldGrantRewards()
        {
            // Arrange
            _questManager.StartQuest("tutorial_quest");
            int initialCredits = CurrencyManager.CurrentCredits;
            int initialXP = _playerLevel.CurrentXP;
            
            // Complete all objectives
            _questManager.UpdateObjective("tutorial_quest", 0, 5); // Kill 5 enemies
            _questManager.UpdateObjective("tutorial_quest", 1, 3); // Survive 3 waves
            
            // Act - Quest should auto-complete when all objectives are done
            var quest = _questManager.GetQuest("tutorial_quest");
            
            // Assert
            AssertThat(quest.Status).IsEqual(QuestStatus.Completed);
            AssertThat(CurrencyManager.CurrentCredits).IsGreater(initialCredits);
            AssertThat(_playerLevel.CurrentXP).IsGreater(initialXP);
        }

        [TestCase]
        public void CompleteQuest_ShouldRemoveFromActiveQuests()
        {
            // Arrange
            _questManager.StartQuest("tutorial_quest");
            
            // Act - Complete all objectives
            _questManager.UpdateObjective("tutorial_quest", 0, 5);
            _questManager.UpdateObjective("tutorial_quest", 1, 3);
            
            // Assert
            AssertInt(_questManager.ActiveQuests.Count).IsEqual(0);
        }

        [TestCase]
        public void FailQuest_ShouldChangeStatusToFailed()
        {
            // Arrange
            _questManager.StartQuest("tutorial_quest");
            
            // Act
            _questManager.FailQuest("tutorial_quest");
            
            // Assert
            var quest = _questManager.GetQuest("tutorial_quest");
            AssertThat(quest.Status).IsEqual(QuestStatus.Failed);
        }

        [TestCase]
        public void FailQuest_ShouldRemoveFromActiveQuests()
        {
            // Arrange
            _questManager.StartQuest("tutorial_quest");
            
            // Act
            _questManager.FailQuest("tutorial_quest");
            
            // Assert
            AssertInt(_questManager.ActiveQuests.Count).IsEqual(0);
        }

        [TestCase]
        public void GetQuestsByStatus_Active_ShouldReturnActiveQuests()
        {
            // Arrange
            _questManager.StartQuest("tutorial_quest");
            
            // Act
            var activeQuests = _questManager.GetQuestsByStatus(QuestStatus.Active);
            
            // Assert
            AssertThat(activeQuests.Count).IsGreater(0);
            AssertThat(activeQuests[0].Status).IsEqual(QuestStatus.Active);
        }

        [TestCase]
        public void GetQuestsByStatus_NotStarted_ShouldReturnAvailableQuests()
        {
            // Act
            var availableQuests = _questManager.GetQuestsByStatus(QuestStatus.NotStarted);
            
            // Assert
            AssertThat(availableQuests.Count).IsGreater(0);
            foreach (var quest in availableQuests)
            {
                AssertThat(quest.Status).IsEqual(QuestStatus.NotStarted);
            }
        }

        [TestCase]
        public void Quest_GetProgress_ShouldCalculateCorrectly()
        {
            // Arrange
            var quest = new Quest
            {
                Id = "progress_test",
                Name = "Progress Test",
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective { RequiredCount = 10, CurrentCount = 5 },
                    new QuestObjective { RequiredCount = 10, CurrentCount = 0 }
                }
            };
            
            // Act
            float progress = quest.GetProgress();
            
            // Assert - Should be 0.25 ((5/10 + 0/10) / 2)
            AssertFloat(progress).IsEqual(0.25f, 0.01f);
        }

        [TestCase]
        public void QuestObjective_GetProgress_ShouldCalculateCorrectly()
        {
            // Arrange
            var objective = new QuestObjective
            {
                RequiredCount = 10,
                CurrentCount = 7
            };
            
            // Act
            float progress = objective.GetProgress();
            
            // Assert
            AssertFloat(progress).IsEqual(0.7f, 0.01f);
        }

        [TestCase]
        public void Quest_AreAllObjectivesCompleted_WithIncompleteObjectives_ShouldReturnFalse()
        {
            // Arrange
            var quest = new Quest
            {
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective { IsCompleted = true },
                    new QuestObjective { IsCompleted = false }
                }
            };
            
            // Act
            bool allCompleted = quest.AreAllObjectivesCompleted();
            
            // Assert
            AssertBool(allCompleted).IsFalse();
        }

        [TestCase]
        public void Quest_AreAllObjectivesCompleted_WithAllComplete_ShouldReturnTrue()
        {
            // Arrange
            var quest = new Quest
            {
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective { IsCompleted = true },
                    new QuestObjective { IsCompleted = true }
                }
            };
            
            // Act
            bool allCompleted = quest.AreAllObjectivesCompleted();
            
            // Assert
            AssertBool(allCompleted).IsTrue();
        }
    }
}
