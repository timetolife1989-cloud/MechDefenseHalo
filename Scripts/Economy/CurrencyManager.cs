using Godot;
using System;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Economy
{
    /// <summary>
    /// Manages in-game currencies (Credits and Cores)
    /// </summary>
    public partial class CurrencyManager : Node
    {
        #region Singleton

        private static CurrencyManager _instance;

        public static CurrencyManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GD.PrintErr("CurrencyManager accessed before initialization!");
                }
                return _instance;
            }
        }

        #endregion

        #region Public Properties

        public int Credits => _credits;
        public int Cores => _cores;

        /// <summary>
        /// Get current credits (static accessor)
        /// </summary>
        public static int CurrentCredits => Instance?._credits ?? 0;
        
        /// <summary>
        /// Get current cores (static accessor)
        /// </summary>
        public static int CurrentCores => Instance?._cores ?? 0;

        #endregion

        #region Private Fields

        private int _credits = 0;
        private int _cores = 0;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                GD.PrintErr("Multiple CurrencyManager instances detected! Removing duplicate.");
                QueueFree();
                return;
            }

            _instance = this;
            GD.Print("CurrencyManager initialized");
        }

        public override void _ExitTree()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        #endregion

        #region Public Methods - Credits

        /// <summary>
        /// Add credits to the player's wallet
        /// </summary>
        /// <param name="amount">Amount to add</param>
        /// <param name="source">Source of the credits (for logging)</param>
        /// <returns>True if successful</returns>
        public static bool AddCredits(int amount, string source = "")
        {
            if (Instance == null) return false;

            if (amount <= 0)
            {
                GD.PrintErr("Cannot add negative or zero credits");
                return false;
            }

            Instance._credits += amount;
            GD.Print($"Added {amount} credits from {source}. Total: {Instance._credits}");

            Instance.EmitCurrencyChanged("credits", amount, Instance._credits);
            return true;
        }

        /// <summary>
        /// Spend credits from the player's wallet
        /// </summary>
        /// <param name="amount">Amount to spend</param>
        /// <param name="reason">Reason for spending (for logging)</param>
        /// <returns>True if successful, false if insufficient funds</returns>
        public static bool SpendCredits(int amount, string reason = "")
        {
            if (Instance == null) return false;

            if (amount <= 0)
            {
                GD.PrintErr("Cannot spend negative or zero credits");
                return false;
            }

            if (Instance._credits < amount)
            {
                GD.PrintErr($"Insufficient credits! Need {amount}, have {Instance._credits}");
                return false;
            }

            Instance._credits -= amount;
            GD.Print($"Spent {amount} credits for {reason}. Remaining: {Instance._credits}");

            Instance.EmitCurrencyChanged("credits", -amount, Instance._credits);
            return true;
        }

        /// <summary>
        /// Check if player has enough credits
        /// </summary>
        /// <param name="amount">Amount to check</param>
        /// <returns>True if player has sufficient credits</returns>
        public static bool HasCredits(int amount)
        {
            return Instance != null && Instance._credits >= amount;
        }

        #endregion

        #region Public Methods - Cores

        /// <summary>
        /// Add cores to the player's wallet
        /// </summary>
        /// <param name="amount">Amount to add</param>
        /// <param name="source">Source of the cores (for logging)</param>
        /// <returns>True if successful</returns>
        public static bool AddCores(int amount, string source = "")
        {
            if (Instance == null) return false;

            if (amount <= 0)
            {
                GD.PrintErr("Cannot add negative or zero cores");
                return false;
            }

            Instance._cores += amount;
            GD.Print($"Added {amount} cores from {source}. Total: {Instance._cores}");

            Instance.EmitCurrencyChanged("cores", amount, Instance._cores);
            return true;
        }

        /// <summary>
        /// Spend cores from the player's wallet
        /// </summary>
        /// <param name="amount">Amount to spend</param>
        /// <param name="reason">Reason for spending (for logging)</param>
        /// <returns>True if successful, false if insufficient funds</returns>
        public static bool SpendCores(int amount, string reason = "")
        {
            if (Instance == null) return false;

            if (amount <= 0)
            {
                GD.PrintErr("Cannot spend negative or zero cores");
                return false;
            }

            if (Instance._cores < amount)
            {
                GD.PrintErr($"Insufficient cores! Need {amount}, have {Instance._cores}");
                return false;
            }

            Instance._cores -= amount;
            GD.Print($"Spent {amount} cores for {reason}. Remaining: {Instance._cores}");

            Instance.EmitCurrencyChanged("cores", -amount, Instance._cores);
            return true;
        }

        /// <summary>
        /// Check if player has enough cores
        /// </summary>
        /// <param name="amount">Amount to check</param>
        /// <returns>True if player has sufficient cores</returns>
        public static bool HasCores(int amount)
        {
            return Instance != null && Instance._cores >= amount;
        }

        #endregion

        #region Public Methods - Utility

        /// <summary>
        /// Set credits directly (for debugging/loading save data)
        /// </summary>
        /// <param name="amount">Amount to set</param>
        public static void SetCredits(int amount)
        {
            if (Instance == null) return;

            Instance._credits = Mathf.Max(0, amount);
            Instance.EmitCurrencyChanged("credits", 0, Instance._credits);
            GD.Print($"Credits set to: {Instance._credits}");
        }

        /// <summary>
        /// Set cores directly (for debugging/loading save data)
        /// </summary>
        /// <param name="amount">Amount to set</param>
        public static void SetCores(int amount)
        {
            if (Instance == null) return;

            Instance._cores = Mathf.Max(0, amount);
            Instance.EmitCurrencyChanged("cores", 0, Instance._cores);
            GD.Print($"Cores set to: {Instance._cores}");
        }

        /// <summary>
        /// Reset both currencies to zero
        /// </summary>
        public static void ResetCurrencies()
        {
            if (Instance == null) return;

            Instance._credits = 0;
            Instance._cores = 0;
            Instance.EmitCurrencyChanged("both", 0, 0);
            GD.Print("All currencies reset to 0");
        }

        #endregion

        #region Public Methods - Save/Load

        /// <summary>
        /// Get currency data for saving
        /// </summary>
        /// <returns>Currency save data</returns>
        public static SaveSystem.CurrencySaveData GetSaveData()
        {
            if (Instance == null)
            {
                return new SaveSystem.CurrencySaveData { Credits = 0, Cores = 0 };
            }

            return new SaveSystem.CurrencySaveData
            {
                Credits = Instance._credits,
                Cores = Instance._cores
            };
        }

        /// <summary>
        /// Load currency data from save
        /// </summary>
        /// <param name="saveData">Currency save data</param>
        public static void LoadFromSave(SaveSystem.CurrencySaveData saveData)
        {
            if (Instance == null || saveData == null) return;

            Instance._credits = Mathf.Max(0, saveData.Credits);
            Instance._cores = Mathf.Max(0, saveData.Cores);
            
            // Calculate the change from current to loaded values
            int creditChange = Instance._credits;
            int coreChange = Instance._cores;
            
            Instance.EmitCurrencyChanged("credits", creditChange, Instance._credits);
            Instance.EmitCurrencyChanged("cores", coreChange, Instance._cores);
            GD.Print($"Currency loaded from save: {Instance._credits} credits, {Instance._cores} cores");
        }

        #endregion

        #region Private Methods

        private void EmitCurrencyChanged(string currencyType, int change, int newTotal)
        {
            EventBus.Emit("currency_changed", new CurrencyChangedData
            {
                CurrencyType = currencyType,
                Change = change,
                NewTotal = newTotal,
                Credits = _credits,
                Cores = _cores
            });
        }

        #endregion
    }

    #region Data Structures

    /// <summary>
    /// Data for currency changed event
    /// </summary>
    public class CurrencyChangedData
    {
        public string CurrencyType { get; set; } // "credits", "cores", or "both"
        public int Change { get; set; }          // Amount changed (positive = gained, negative = spent)
        public int NewTotal { get; set; }        // New total of the changed currency
        public int Credits { get; set; }         // Current total credits
        public int Cores { get; set; }           // Current total cores
    }

    #endregion
}
