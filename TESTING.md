# Testing Guide - MechDefenseHalo

## 📖 Overview

This document provides comprehensive guidance on testing in the MechDefenseHalo project. We use **GdUnit4** as our testing framework for Godot 4.x with C# support.

---

## 🚀 Setup GdUnit4

### Installation Steps

1. **Open Godot Editor**
2. Navigate to **AssetLib** tab (top bar)
3. Search for **"GdUnit4"**
4. Click **Download** and then **Install**
5. Enable the plugin in **Project → Project Settings → Plugins**
6. **Restart Godot** to complete installation

### Verify Installation

After restarting Godot, you should see:
- A new **GdUnit4** menu in the top menu bar
- Test panel accessible via **GdUnit4 → Inspector**

---

## 🧪 Running Tests

### Via Godot Editor

#### Run All Tests
1. Open **GdUnit4** menu
2. Click **Run All Tests**
3. View results in the GdUnit4 Inspector panel

#### Run Specific Test Suite
1. Navigate to test file in FileSystem
2. Right-click the test file
3. Select **Run Tests**

#### Run Single Test
1. Open test file in script editor
2. Click the play button next to test method
3. Or use **GdUnit4 → Run Current Test**

### Via Command Line

#### Run all tests (headless)
```bash
godot --headless --run-tests --quit
```

#### Run tests in debug mode
```bash
godot --run-tests
```

#### Run specific test suite
```bash
godot --headless --run-tests --test-suite="EventBusTests" --quit
```

---

## 📝 Writing Tests

### Test Suite Structure

```csharp
using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests.MyNamespace
{
    /// <summary>
    /// Unit tests for MyClass
    /// </summary>
    [TestSuite]
    public class MyClassTests
    {
        private MyClass _myClass;

        [Before]
        public void Setup()
        {
            // Runs before each test
            _myClass = new MyClass();
        }

        [After]
        public void Teardown()
        {
            // Runs after each test
            _myClass = null;
        }

        [TestCase]
        public void MyMethod_Condition_ExpectedResult()
        {
            // Arrange - Set up test data
            var input = 42;
            
            // Act - Execute the method
            var result = _myClass.MyMethod(input);
            
            // Assert - Verify the result
            AssertInt(result).IsEqual(84);
        }
    }
}
```

### Naming Conventions

**Test Class Names:**
- Format: `{ClassUnderTest}Tests`
- Example: `EventBusTests`, `InventoryManagerTests`

**Test Method Names:**
- Format: `{MethodName}_{Condition}_{ExpectedResult}`
- Examples:
  - `AddItem_WithValidItem_ShouldReturnTrue`
  - `SpendCredits_InsufficientBalance_ShouldReturnFalse`
  - `EmitEvent_WithNoListeners_ShouldNotThrow`

### Common Assertions

```csharp
// Boolean assertions
AssertBool(value).IsTrue();
AssertBool(value).IsFalse();

// Integer assertions
AssertInt(value).IsEqual(42);
AssertInt(value).IsNotEqual(0);
AssertInt(value).IsGreater(10);
AssertInt(value).IsLess(100);

// Float assertions
AssertFloat(value).IsEqual(3.14f);
AssertFloat(value).IsNotEqual(0f);
AssertFloat(value).IsGreater(1.5f);

// String assertions
AssertString(value).IsEqual("expected");
AssertString(value).Contains("substring");
AssertString(value).IsEmpty();

// Object assertions
AssertObject(obj).IsNotNull();
AssertObject(obj).IsNull();
AssertThat(obj).IsEqual(expectedObj);

// Collection assertions
AssertArray(array).IsEmpty();
AssertArray(array).IsNotEmpty();
AssertArray(array).HasSize(5);

// Exception assertions
AssertThat(() => Method()).ThrowsException();
AssertThat(() => Method()).Not().ThrowsException();
```

---

## 📂 Test Organization

### Directory Structure

```
Tests/
├── Core/
│   ├── EventBusTests.cs          # Event system tests
│   └── GameManagerTests.cs        # Game state tests
├── Inventory/
│   ├── InventoryManagerTests.cs   # Inventory operations
│   └── EquipmentManagerTests.cs   # Equipment tests
├── Loot/
│   └── LootTableManagerTests.cs   # Loot generation tests
├── Combat/
│   └── DamageCalculationTests.cs  # Combat math tests
└── Economy/
    └── CurrencyManagerTests.cs    # Currency operations
```

### Test Categories

**Unit Tests:**
- Test individual methods/functions
- Fast execution (< 100ms per test)
- No external dependencies
- Location: `Tests/` folder

**Integration Tests:**
- Test interaction between systems
- May involve multiple classes
- Location: `Tests/Integration/` (when needed)

---

## 🎯 Test Coverage Goals

| Category | Coverage Target | Priority |
|----------|----------------|----------|
| Core Systems | 80%+ | High |
| Gameplay Logic | 60%+ | High |
| Economy | 70%+ | Medium |
| UI Components | 40%+ | Low |
| **Overall** | **60%+** | - |

### Measuring Coverage

Coverage reporting is integrated into CI/CD pipeline:
1. Tests run automatically on every PR
2. Coverage report generated in artifacts
3. Review coverage in CI logs

---

## 🔄 CI/CD Integration

### Automatic Test Execution

Tests run automatically on:
- Every **Pull Request** to `main`
- Every **Push** to `main`
- Manual trigger via GitHub Actions

### Workflow

1. **Build** - Godot builds the project
2. **Test** - GdUnit4 runs all tests
3. **Report** - Results uploaded as artifacts
4. **Status** - PR checks show pass/fail

### Viewing Results

1. Go to **Pull Request** page
2. Click **Checks** tab
3. Select **Godot CI/CD** workflow
4. View **test** job for results

---

## 🛠️ Best Practices

### DO ✅

- **Write tests first** (TDD when possible)
- **Test edge cases** (null, zero, negative values)
- **Keep tests focused** (one assertion per test when possible)
- **Use descriptive names** (method_condition_result)
- **Clean up resources** (use `[After]` for teardown)
- **Test public APIs** (not private methods)
- **Mock dependencies** when needed

### DON'T ❌

- **Don't test Godot internals** (Node lifecycle, rendering)
- **Don't use hardcoded paths** (use relative paths)
- **Don't share state** between tests
- **Don't write flaky tests** (no randomness without seeds)
- **Don't test implementation details** (focus on behavior)
- **Don't skip cleanup** (always teardown properly)

---

## 🐛 Debugging Tests

### Using Print Statements

```csharp
[TestCase]
public void MyTest()
{
    GD.Print("Debug: Value is " + myValue);
    AssertInt(myValue).IsEqual(42);
}
```

### Using Breakpoints

1. Set breakpoint in test code
2. Run Godot with debugger attached
3. Execute tests
4. Debugger pauses at breakpoint

### Common Issues

**Issue:** Tests not appearing in GdUnit4 panel
- **Solution:** Ensure `[TestSuite]` attribute on class
- **Solution:** Rebuild project

**Issue:** Tests fail with "Instance is null"
- **Solution:** Check singleton initialization in `[Before]`
- **Solution:** Verify Node lifecycle management

**Issue:** Tests pass locally but fail in CI
- **Solution:** Check for file path issues
- **Solution:** Verify no dependency on local files

---

## 📚 Example Tests

### Example 1: Simple Method Test

```csharp
[TestCase]
public void Add_TwoNumbers_ReturnsSum()
{
    // Arrange
    var calculator = new Calculator();
    
    // Act
    var result = calculator.Add(5, 3);
    
    // Assert
    AssertInt(result).IsEqual(8);
}
```

### Example 2: Testing with Exceptions

```csharp
[TestCase]
public void Divide_ByZero_ShouldThrowException()
{
    // Arrange
    var calculator = new Calculator();
    
    // Act & Assert
    AssertThat(() => calculator.Divide(10, 0))
        .ThrowsException<DivideByZeroException>();
}
```

### Example 3: Testing Collections

```csharp
[TestCase]
public void GetItems_EmptyInventory_ReturnsEmptyList()
{
    // Arrange
    var inventory = new Inventory();
    
    // Act
    var items = inventory.GetItems();
    
    // Assert
    AssertArray(items).IsEmpty();
}
```

---

## 📖 Additional Resources

### Documentation
- [GdUnit4 Official Docs](https://github.com/MikeSchulze/gdUnit4)
- [Godot C# Testing Guide](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/index.html)

### Example Test Suites
- `Tests/Core/EventBusTests.cs` - Event system testing
- `Tests/Inventory/InventoryManagerTests.cs` - Complex state management
- `Tests/Economy/CurrencyManagerTests.cs` - Business logic testing

---

## 🎓 Testing Philosophy

> "Write tests that give you confidence, not just coverage."

- Focus on **behavior**, not implementation
- Test **what matters** to users
- Keep tests **simple and readable**
- Make tests **fast and reliable**
- Use tests as **living documentation**

---

## 📞 Support

For questions or issues with testing:
1. Check this documentation first
2. Review existing test examples
3. Ask in project Discord/Slack
4. Create an issue on GitHub

---

**Happy Testing! 🧪✨**
