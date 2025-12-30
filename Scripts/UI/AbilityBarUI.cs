using Godot;
using System.Collections.Generic;
using MechDefenseHalo.Abilities;

namespace MechDefenseHalo.UI
{
    /// <summary>
    /// UI component that displays player abilities with icons, cooldowns, and energy costs.
    /// Shows 4 ability slots with visual feedback for cooldown progress and availability.
    /// 
    /// SETUP:
    /// Add to the HUD scene as a Control node.
    /// Assign the AbilitySystem node reference in the inspector or via code.
    /// 
    /// UI STRUCTURE:
    /// AbilityBarUI (HBoxContainer)
    /// ├── AbilitySlot1 (Panel)
    /// │   ├── Icon (TextureRect)
    /// │   ├── Cooldown (ProgressBar)
    /// │   ├── Hotkey (Label)
    /// │   └── EnergyCost (Label)
    /// ├── AbilitySlot2 (Panel)
    /// └── ... (4 total slots)
    /// </summary>
    public partial class AbilityBarUI : Control
    {
        #region Exported Properties
        
        [Export] public NodePath AbilitySystemPath;
        [Export] public bool ShowHotkeys = true;
        [Export] public bool ShowEnergyCosts = true;
        
        #endregion
        
        #region Private Fields
        
        private AbilitySystem _abilitySystem;
        private List<AbilitySlotUI> _slots = new();
        private HBoxContainer _container;
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            // Get ability system reference
            if (!string.IsNullOrEmpty(AbilitySystemPath.ToString()))
            {
                _abilitySystem = GetNode<AbilitySystem>(AbilitySystemPath);
            }
            else
            {
                // Try to find it in the scene tree as fallback
                // NOTE: If multiple AbilitySystem nodes exist, this may not find the correct one.
                // It's recommended to explicitly set AbilitySystemPath in the inspector.
                _abilitySystem = GetTree().Root.FindChild("AbilitySystem", true, false) as AbilitySystem;
            }
            
            if (_abilitySystem == null)
            {
                GD.PrintErr("[AbilityBarUI] AbilitySystem not found!");
                return;
            }
            
            // Create UI container
            CreateUI();
            
            GD.Print("[AbilityBarUI] Initialized with ability system");
        }
        
        public override void _Process(double delta)
        {
            if (_abilitySystem == null)
                return;
                
            // Update all ability slots
            UpdateSlots();
        }
        
        #endregion
        
        #region UI Creation
        
        private void CreateUI()
        {
            // Create horizontal container for ability slots
            _container = new HBoxContainer
            {
                Name = "AbilityContainer"
            };
            _container.AddThemeConstantOverride("separation", 10);
            AddChild(_container);
            
            // Create 4 ability slots
            string[] hotkeys = { "1", "2", "3", "4" };
            
            for (int i = 0; i < 4; i++)
            {
                var ability = _abilitySystem.GetAbility(i);
                if (ability != null)
                {
                    var slot = CreateAbilitySlot(ability, hotkeys[i], i);
                    _slots.Add(slot);
                    _container.AddChild(slot.Panel);
                }
            }
        }
        
        private AbilitySlotUI CreateAbilitySlot(AbilityBase ability, string hotkey, int index)
        {
            var slot = new AbilitySlotUI();
            
            // Create panel
            slot.Panel = new Panel
            {
                Name = $"AbilitySlot{index}",
                CustomMinimumSize = new Vector2(80, 80)
            };
            
            // Style the panel
            var styleBox = new StyleBoxFlat
            {
                BgColor = new Color(0.1f, 0.1f, 0.1f, 0.8f),
                BorderWidthAll = 2,
                BorderColor = new Color(0.3f, 0.5f, 0.7f, 1.0f),
                CornerRadiusAll = 8
            };
            slot.Panel.AddThemeStyleboxOverride("panel", styleBox);
            
            // Create icon (placeholder texture rect)
            slot.Icon = new TextureRect
            {
                Name = "Icon",
                Size = new Vector2(60, 60),
                Position = new Vector2(10, 10),
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered
            };
            
            // Try to load icon, fallback to placeholder
            if (!string.IsNullOrEmpty(ability.IconPath) && ResourceLoader.Exists(ability.IconPath))
            {
                slot.Icon.Texture = GD.Load<Texture2D>(ability.IconPath);
            }
            else
            {
                // Create a simple colored rectangle as placeholder
                var image = Image.Create(64, 64, false, Image.Format.Rgba8);
                image.Fill(GetAbilityColor(ability.AbilityId));
                slot.Icon.Texture = ImageTexture.CreateFromImage(image);
            }
            
            slot.Panel.AddChild(slot.Icon);
            
            // Create cooldown overlay
            slot.CooldownOverlay = new ColorRect
            {
                Name = "CooldownOverlay",
                Size = new Vector2(60, 60),
                Position = new Vector2(10, 10),
                Color = new Color(0, 0, 0, 0.7f),
                Visible = false
            };
            slot.Panel.AddChild(slot.CooldownOverlay);
            
            // Create cooldown progress bar
            slot.CooldownBar = new ProgressBar
            {
                Name = "CooldownBar",
                Size = new Vector2(60, 8),
                Position = new Vector2(10, 65),
                MaxValue = 1.0,
                Value = 1.0,
                ShowPercentage = false
            };
            slot.Panel.AddChild(slot.CooldownBar);
            
            // Create hotkey label
            if (ShowHotkeys)
            {
                slot.HotkeyLabel = new Label
                {
                    Name = "Hotkey",
                    Text = hotkey,
                    Position = new Vector2(5, 5)
                };
                slot.HotkeyLabel.AddThemeFontSizeOverride("font_size", 12);
                slot.HotkeyLabel.AddThemeColorOverride("font_color", new Color(1, 1, 1, 0.8f));
                slot.Panel.AddChild(slot.HotkeyLabel);
            }
            
            // Create energy cost label
            if (ShowEnergyCosts)
            {
                slot.EnergyCostLabel = new Label
                {
                    Name = "EnergyCost",
                    Text = $"-{ability.EnergyCost:F0}",
                    Position = new Vector2(48, 5)
                };
                slot.EnergyCostLabel.AddThemeFontSizeOverride("font_size", 10);
                slot.EnergyCostLabel.AddThemeColorOverride("font_color", new Color(0.3f, 0.7f, 1.0f, 0.9f));
                slot.Panel.AddChild(slot.EnergyCostLabel);
            }
            
            // Create cooldown timer label
            slot.CooldownLabel = new Label
            {
                Name = "CooldownTimer",
                Position = new Vector2(28, 30),
                Visible = false
            };
            slot.CooldownLabel.AddThemeFontSizeOverride("font_size", 16);
            slot.CooldownLabel.AddThemeColorOverride("font_color", new Color(1, 1, 1, 1));
            slot.Panel.AddChild(slot.CooldownLabel);
            
            // Tooltip
            slot.Panel.TooltipText = $"{ability.AbilityName}\n{ability.Description}\n\nCooldown: {ability.Cooldown}s\nEnergy: {ability.EnergyCost}";
            
            return slot;
        }
        
        private Color GetAbilityColor(string abilityId)
        {
            return abilityId switch
            {
                "dash" => new Color(0.3f, 0.8f, 0.3f), // Green
                "shield" => new Color(0.3f, 0.5f, 1.0f), // Blue
                "emp" => new Color(0.8f, 0.8f, 0.3f), // Yellow
                "time_slow" => new Color(0.8f, 0.3f, 0.8f), // Purple
                _ => new Color(0.5f, 0.5f, 0.5f)
            };
        }
        
        #endregion
        
        #region Update Logic
        
        private void UpdateSlots()
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                UpdateSlot(_slots[i], i);
            }
        }
        
        private void UpdateSlot(AbilitySlotUI slot, int index)
        {
            bool isReady = _abilitySystem.GetAbilityReady(index);
            float progress = _abilitySystem.GetAbilityCooldownProgress(index);
            float remaining = _abilitySystem.GetAbilityRemainingCooldown(index);
            
            // Update cooldown overlay visibility
            slot.CooldownOverlay.Visible = !isReady;
            
            // Update cooldown bar
            slot.CooldownBar.Value = progress;
            
            // Update cooldown timer text
            if (!isReady && remaining > 0)
            {
                slot.CooldownLabel.Visible = true;
                slot.CooldownLabel.Text = $"{remaining:F1}";
            }
            else
            {
                slot.CooldownLabel.Visible = false;
            }
            
            // Dim icon when on cooldown
            if (slot.Icon != null)
            {
                slot.Icon.Modulate = isReady 
                    ? new Color(1, 1, 1, 1) 
                    : new Color(0.5f, 0.5f, 0.5f, 1);
            }
        }
        
        #endregion
        
        #region Helper Class
        
        private class AbilitySlotUI
        {
            public Panel Panel;
            public TextureRect Icon;
            public ColorRect CooldownOverlay;
            public ProgressBar CooldownBar;
            public Label HotkeyLabel;
            public Label EnergyCostLabel;
            public Label CooldownLabel;
        }
        
        #endregion
    }
}
