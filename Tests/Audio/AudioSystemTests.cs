using Godot;
using GdUnit4;
using MechDefenseHalo.Audio;
using MechDefenseHalo.Core;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests.Audio
{
    /// <summary>
    /// Unit tests for the Audio system integration.
    /// Tests AudioManager, MusicController, SoundEffectPool, AudioBusController, and AudioSettingsApplier.
    /// </summary>
    [TestSuite]
    public class AudioSystemTests
    {
        private AudioBusController _audioBusController;
        private SoundEffectPool _soundEffectPool;
        private AudioSettingsApplier _audioSettingsApplier;
        
        [Before]
        public void Setup()
        {
            // Initialize audio bus controller
            _audioBusController = new AudioBusController();
            _audioBusController._Ready();
            
            // Initialize sound effect pool
            _soundEffectPool = new SoundEffectPool();
            _soundEffectPool._Ready();
            
            // Initialize audio settings applier
            _audioSettingsApplier = new AudioSettingsApplier();
            _audioSettingsApplier._Ready();
        }
        
        [After]
        public void Teardown()
        {
            if (_audioBusController != null)
            {
                _audioBusController.QueueFree();
                _audioBusController = null;
            }
            
            if (_soundEffectPool != null)
            {
                _soundEffectPool.QueueFree();
                _soundEffectPool = null;
            }
            
            if (_audioSettingsApplier != null)
            {
                _audioSettingsApplier.QueueFree();
                _audioSettingsApplier = null;
            }
        }
        
        [TestCase]
        public void AudioBusController_ShouldCreateRequiredBuses()
        {
            // Assert that all required buses exist
            AssertThat(AudioServer.GetBusIndex("Master")).IsGreaterEqual(0);
            AssertThat(AudioServer.GetBusIndex("Music")).IsGreaterEqual(0);
            AssertThat(AudioServer.GetBusIndex("SFX")).IsGreaterEqual(0);
            AssertThat(AudioServer.GetBusIndex("UI")).IsGreaterEqual(0);
        }
        
        [TestCase]
        public void AudioBusController_GetBusIndex_ShouldReturnValidIndex()
        {
            // Act
            int masterIndex = _audioBusController.GetBusIndex("Master");
            int musicIndex = _audioBusController.GetBusIndex("Music");
            
            // Assert
            AssertThat(masterIndex).IsGreaterEqual(0);
            AssertThat(musicIndex).IsGreaterEqual(0);
        }
        
        [TestCase]
        public void AudioBusController_SetBusVolume_ShouldUpdateVolume()
        {
            // Arrange
            float testVolume = -10f;
            
            // Act
            _audioBusController.SetBusVolume("Music", testVolume);
            float actualVolume = _audioBusController.GetBusVolume("Music");
            
            // Assert
            AssertThat(actualVolume).IsEqual(testVolume);
        }
        
        [TestCase]
        public void AudioBusController_SetBusMute_ShouldMuteBus()
        {
            // Arrange
            int musicIndex = AudioServer.GetBusIndex("Music");
            
            // Act
            _audioBusController.SetBusMute("Music", true);
            bool isMuted = AudioServer.IsBusMute(musicIndex);
            
            // Assert
            AssertBool(isMuted).IsTrue();
            
            // Cleanup
            _audioBusController.SetBusMute("Music", false);
        }
        
        [TestCase]
        public void SoundEffectPool_Play2D_WithNullStream_ShouldNotThrow()
        {
            // Act & Assert
            AssertThat(() => _soundEffectPool.Play2D(null, 1.0f))
                .Not().ThrowsException();
        }
        
        [TestCase]
        public void SoundEffectPool_Play3D_WithNullStream_ShouldNotThrow()
        {
            // Act & Assert
            AssertThat(() => _soundEffectPool.Play3D(null, Vector3.Zero, 1.0f))
                .Not().ThrowsException();
        }
        
        [TestCase]
        public void SoundEffectPool_PlayOnBus_WithNullStream_ShouldNotThrow()
        {
            // Act & Assert
            AssertThat(() => _soundEffectPool.PlayOnBus(null, "UI"))
                .Not().ThrowsException();
        }
        
        [TestCase]
        public void AudioSettingsApplier_SetMasterVolume_ShouldUpdateBusVolume()
        {
            // Arrange
            float testVolume = 0.5f; // 50% volume
            
            // Act
            _audioSettingsApplier.SetMasterVolume(testVolume);
            
            // Assert
            int busIndex = AudioServer.GetBusIndex("Master");
            float volumeDb = AudioServer.GetBusVolumeDb(busIndex);
            
            // Volume should be set to something less than 0 (max volume)
            AssertThat(volumeDb).IsLess(0f);
        }
        
        [TestCase]
        public void AudioSettingsApplier_SetMusicVolume_ShouldUpdateBusVolume()
        {
            // Arrange
            float testVolume = 0.7f; // 70% volume
            
            // Act
            _audioSettingsApplier.SetMusicVolume(testVolume);
            
            // Assert
            int busIndex = AudioServer.GetBusIndex("Music");
            float volumeDb = AudioServer.GetBusVolumeDb(busIndex);
            
            // Volume should be set
            AssertThat(volumeDb).IsNotEqual(0f);
        }
        
        [TestCase]
        public void AudioSettingsApplier_SetSFXVolume_ShouldUpdateBusVolume()
        {
            // Arrange
            float testVolume = 0.8f; // 80% volume
            
            // Act
            _audioSettingsApplier.SetSFXVolume(testVolume);
            
            // Assert
            int busIndex = AudioServer.GetBusIndex("SFX");
            float volumeDb = AudioServer.GetBusVolumeDb(busIndex);
            
            // Volume should be set
            AssertThat(volumeDb).IsNotEqual(0f);
        }
        
        [TestCase]
        public void AudioSettingsApplier_SetUIVolume_ShouldUpdateBusVolume()
        {
            // Arrange
            float testVolume = 0.9f; // 90% volume
            
            // Act
            _audioSettingsApplier.SetUIVolume(testVolume);
            
            // Assert
            int busIndex = AudioServer.GetBusIndex("UI");
            float volumeDb = AudioServer.GetBusVolumeDb(busIndex);
            
            // Volume should be set
            AssertThat(volumeDb).IsNotEqual(0f);
        }
        
        [TestCase]
        public void AudioSettingsApplier_SetZeroVolume_ShouldMuteBus()
        {
            // Act
            _audioSettingsApplier.SetMasterVolume(0f);
            
            // Assert
            int busIndex = AudioServer.GetBusIndex("Master");
            float volumeDb = AudioServer.GetBusVolumeDb(busIndex);
            
            // Zero volume should result in silence (-80 dB)
            AssertThat(volumeDb).IsEqual(-80f);
        }
        
        [TestCase]
        public void AudioSettingsApplier_VolumeClamp_ShouldClampToValidRange()
        {
            // Act - try to set volume above 1.0
            _audioSettingsApplier.SetMusicVolume(2.0f);
            
            // Assert - should be clamped to valid range
            int busIndex = AudioServer.GetBusIndex("Music");
            float volumeDb = AudioServer.GetBusVolumeDb(busIndex);
            
            // Volume should be at or below 0 dB (max)
            AssertThat(volumeDb).IsLessEqual(0f);
        }
        
        [TestCase]
        public void MusicController_TransitionTo_WithInvalidTrack_ShouldNotThrow()
        {
            // Arrange
            var musicController = new MusicController();
            musicController._Ready();
            
            // Act & Assert
            AssertThat(() => musicController.TransitionTo("nonexistent_track"))
                .Not().ThrowsException();
            
            // Cleanup
            musicController.QueueFree();
        }
    }
}
