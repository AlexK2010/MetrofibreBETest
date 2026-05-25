# Food Optimizer Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Two independent C# .NET 10 console apps that enumerate every valid recipe combination from a pantry of ingredients and print them ranked by total people fed.

**Architecture:** Both solutions expose an identical `Solve(pantry, recipes)` contract. The Backtracking solution uses ordered DFS enumeration (no external deps). The OR-Tools solution models it as a CP-SAT satisfaction problem and uses `SearchAllSolutions` to enumerate all feasible assignments.

**Tech Stack:** C# 13 / .NET 10, xUnit 2.x, Google.OrTools (OR-Tools solution only)

---

## File Map

```
Claude/
  Backtracking/
    FoodOptimizer.sln
    FoodOptimizer/
      FoodOptimizer.csproj       net10.0, no external deps
      Ingredient.cs              const string ingredient keys
      Recipe.cs                  record Recipe(Name, Ingredients, PeopleFed)
      SolverResult.cs            record SolverResult(RecipesUsed, TotalPeopleFed, Leftovers)
      ProblemData.cs             static pantry + recipes from the assessment
      BacktrackingOptimizer.cs   DFS enumerate all multisets, sort by score
      Program.cs                 wire up + ranked output
    FoodOptimizer.Tests/
      FoodOptimizer.Tests.csproj net10.0, xUnit
      BacktrackingOptimizerTests.cs

  OrTools/
    FoodOptimizer.sln
    FoodOptimizer/
      FoodOptimizer.csproj       net10.0, Google.OrTools
      Ingredient.cs              (same content as Backtracking)
      Recipe.cs                  (same content)
      SolverResult.cs            (same content)
      ProblemData.cs             (same content)
      OrToolsOptimizer.cs        CP-SAT model + SolutionCollector callback
      Program.cs                 (same output logic as Backtracking)
    FoodOptimizer.Tests/
      FoodOptimizer.Tests.csproj net10.0, xUnit, Google.OrTools
      OrToolsOptimizerTests.cs
```

---

## Task 1: Scaffold the Backtracking solution

**Files:**
- Create: `Claude/Backtracking/` directory tree

- [ ] **Step 1: Create solution and projects**

```bash
cd /mnt/d/Repo/Assessment/Claude
mkdir Backtracking && cd Backtracking
dotnet new sln -n FoodOptimizer
dotnet new console -n FoodOptimizer -f net10.0
dotnet new xunit -n FoodOptimizer.Tests -f net10.0
dotnet sln add FoodOptimizer/FoodOptimizer.csproj
dotnet sln add FoodOptimizer.Tests/FoodOptimizer.Tests.csproj
dotnet add FoodOptimizer.Tests/FoodOptimizer.Tests.csproj reference FoodOptimizer/FoodOptimizer.csproj
```

- [ ] **Step 2: Verify clean build**

```bash
cd /mnt/d/Repo/Assessment/Claude/Backtracking && dotnet build
```

Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

---

## Task 2: Domain model — Backtracking

**Files:**
- Create: `Claude/Backtracking/FoodOptimizer/Ingredient.cs`
- Create: `Claude/Backtracking/FoodOptimizer/Recipe.cs`
- Create: `Claude/Backtracking/FoodOptimizer/SolverResult.cs`
- Create: `Claude/Backtracking/FoodOptimizer/ProblemData.cs`

- [ ] **Step 1: Create Ingredient.cs**

```csharp
// Claude/Backtracking/FoodOptimizer/Ingredient.cs
namespace FoodOptimizer;

public static class Ingredient
{
    public const string Cucumber = "Cucumber";
    public const string Olives   = "Olives";
    public const string Lettuce  = "Lettuce";
    public const string Meat     = "Meat";
    public const string Tomato   = "Tomato";
    public const string Cheese   = "Cheese";
    public const string Dough    = "Dough";
}
```

- [ ] **Step 2: Create Recipe.cs**

```csharp
// Claude/Backtracking/FoodOptimizer/Recipe.cs
namespace FoodOptimizer;

public record Recipe(
    string Name,
    IReadOnlyDictionary<string, int> Ingredients,
    int PeopleFed);
```

- [ ] **Step 3: Create SolverResult.cs**

```csharp
// Claude/Backtracking/FoodOptimizer/SolverResult.cs
namespace FoodOptimizer;

public record SolverResult(
    IReadOnlyList<(Recipe Recipe, int Count)> RecipesUsed,
    int TotalPeopleFed,
    IReadOnlyDictionary<string, int> Leftovers);
```

- [ ] **Step 4: Create ProblemData.cs**

```csharp
// Claude/Backtracking/FoodOptimizer/ProblemData.cs
namespace FoodOptimizer;

public static class ProblemData
{
    public static readonly IReadOnlyDictionary<string, int> Pantry =
        new Dictionary<string, int>
        {
            [Ingredient.Cucumber] = 2,
            [Ingredient.Olives]   = 2,
            [Ingredient.Lettuce]  = 3,
            [Ingredient.Meat]     = 6,
            [Ingredient.Tomato]   = 6,
            [Ingredient.Cheese]   = 8,
            [Ingredient.Dough]    = 10
        };

    public static readonly IReadOnlyList<Recipe> Recipes = new List<Recipe>
    {
        new("Burger", new Dictionary<string, int>
        {
            [Ingredient.Meat]    = 1,
            [Ingredient.Lettuce] = 1,
            [Ingredient.Tomato]  = 1,
            [Ingredient.Cheese]  = 1,
            [Ingredient.Dough]   = 1
        }, 1),
        new("Pie", new Dictionary<string, int>
        {
            [Ingredient.Meat]  = 2,
            [Ingredient.Dough] = 2
        }, 1),
        new("Sandwich", new Dictionary<string, int>
        {
            [Ingredient.Cucumber] = 1,
            [Ingredient.Dough]    = 1
        }, 1),
        new("Pasta", new Dictionary<string, int>
        {
            [Ingredient.Meat]   = 1,
            [Ingredient.Tomato] = 1,
            [Ingredient.Cheese] = 2,
            [Ingredient.Dough]  = 2
        }, 2),
        new("Salad", new Dictionary<string, int>
        {
            [Ingredient.Cucumber] = 1,
            [Ingredient.Lettuce]  = 2,
            [Ingredient.Tomato]   = 2,
            [Ingredient.Cheese]   = 2,
            [Ingredient.Olives]   = 1
        }, 3),
        new("Pizza", new Dictionary<string, int>
        {
            [Ingredient.Olives] = 1,
            [Ingredient.Tomato] = 2,
            [Ingredient.Cheese] = 3,
            [Ingredient.Dough]  = 3
        }, 4)
    };
}
```

- [ ] **Step 5: Delete the generated Program.cs placeholder content and replace**

Replace the generated `Claude/Backtracking/FoodOptimizer/Program.cs` with a temporary stub so it compiles:

```csharp
// Claude/Backtracking/FoodOptimizer/Program.cs
```

(Empty file — we fill this in Task 4.)

- [ ] **Step 6: Verify build**

```bash
cd /mnt/d/Repo/Assessment/Claude/Backtracking && dotnet build
```

Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

---

## Task 3: BacktrackingOptimizer — TDD

**Files:**
- Create: `Claude/Backtracking/FoodOptimizer/BacktrackingOptimizer.cs`
- Modify: `Claude/Backtracking/FoodOptimizer.Tests/BacktrackingOptimizerTests.cs`

- [ ] **Step 1: Replace the generated test file with real tests**

```csharp
// Claude/Backtracking/FoodOptimizer.Tests/BacktrackingOptimizerTests.cs
using FoodOptimizer;

namespace FoodOptimizer.Tests;

public class BacktrackingOptimizerTests
{
    private readonly BacktrackingOptimizer _sut = new();

    [Fact]
    public void Solve_EmptyRecipes_ReturnsEmpty()
    {
        var result = _sut.Solve(
            new Dictionary<string, int> { ["Flour"] = 5 },
            new List<Recipe>());

        Assert.Empty(result);
    }

    [Fact]
    public void Solve_SingleAffordableRecipe_ReturnsSingleResult()
    {
        var pantry   = new Dictionary<string, int> { ["Flour"] = 2 };
        var recipes  = new List<Recipe>
        {
            new("Cake", new Dictionary<string, int> { ["Flour"] = 2 }, 3)
        };

        var results = _sut.Solve(pantry, recipes);

        Assert.Single(results);
        Assert.Equal(3, results[0].TotalPeopleFed);
    }

    [Fact]
    public void Solve_SingleUnaffordableRecipe_ReturnsEmpty()
    {
        var pantry   = new Dictionary<string, int> { ["Flour"] = 1 };
        var recipes  = new List<Recipe>
        {
            new("Cake", new Dictionary<string, int> { ["Flour"] = 2 }, 3)
        };

        var results = _sut.Solve(pantry, recipes);

        Assert.Empty(results);
    }

    [Fact]
    public void Solve_CanMakeRecipeTwice_ReturnsBothCounts()
    {
        var pantry   = new Dictionary<string, int> { ["Flour"] = 4 };
        var recipes  = new List<Recipe>
        {
            new("Bread", new Dictionary<string, int> { ["Flour"] = 2 }, 2)
        };

        var results = _sut.Solve(pantry, recipes);

        // 1x Bread (feeds 2) and 2x Bread (feeds 4)
        Assert.Equal(2, results.Count);
        Assert.Equal(4, results[0].TotalPeopleFed);
        Assert.Equal(2, results[1].TotalPeopleFed);
    }

    [Fact]
    public void Solve_ResultsSortedDescendingByPeopleFed()
    {
        var results = _sut.Solve(ProblemData.Pantry, ProblemData.Recipes);

        for (var i = 0; i < results.Count - 1; i++)
            Assert.True(results[i].TotalPeopleFed >= results[i + 1].TotalPeopleFed);
    }

    [Fact]
    public void Solve_AssessmentData_BestSolutionFeeds12()
    {
        var results = _sut.Solve(ProblemData.Pantry, ProblemData.Recipes);

        Assert.Equal(12, results[0].TotalPeopleFed);
    }

    [Fact]
    public void Solve_LeftoversCorrectForBestSolution()
    {
        var results = _sut.Solve(ProblemData.Pantry, ProblemData.Recipes);
        var best = results[0];

        // All leftovers must be non-negative
        Assert.All(best.Leftovers.Values, v => Assert.True(v >= 0));

        // Total ingredients consumed + leftovers must equal pantry
        foreach (var (ingredient, available) in ProblemData.Pantry)
        {
            var used = best.RecipesUsed.Sum(r =>
                r.Recipe.Ingredients.TryGetValue(ingredient, out var amt)
                    ? amt * r.Count
                    : 0);
            Assert.Equal(available, used + best.Leftovers[ingredient]);
        }
    }
}
```

- [ ] **Step 2: Run tests — expect compile failure (class doesn't exist yet)**

```bash
cd /mnt/d/Repo/Assessment/Claude/Backtracking && dotnet test FoodOptimizer.Tests
```

Expected: build error — `The type or namespace name 'BacktrackingOptimizer' could not be found`.

- [ ] **Step 3: Create BacktrackingOptimizer.cs**

```csharp
// Claude/Backtracking/FoodOptimizer/BacktrackingOptimizer.cs
namespace FoodOptimizer;

public class BacktrackingOptimizer
{
    public IReadOnlyList<SolverResult> Solve(
        IReadOnlyDictionary<string, int> pantry,
        IReadOnlyList<Recipe> recipes)
    {
        if (!pantry.Any() || !recipes.Any())
            return Array.Empty<SolverResult>();

        var results   = new List<SolverResult>();
        var remaining = new Dictionary<string, int>(pantry);
        var counts    = new Dictionary<Recipe, int>();

        Enumerate(0, remaining, counts, results, recipes);

        return results.OrderByDescending(r => r.TotalPeopleFed).ToList();
    }

    private static void Enumerate(
        int minIdx,
        Dictionary<string, int> remaining,
        Dictionary<Recipe, int> counts,
        List<SolverResult> results,
        IReadOnlyList<Recipe> recipes)
    {
        if (counts.Count > 0)
            results.Add(Snapshot(counts, remaining));

        for (var i = minIdx; i < recipes.Count; i++)
        {
            var recipe = recipes[i];
            if (!CanAfford(remaining, recipe)) continue;

            Deduct(remaining, recipe);
            counts[recipe] = counts.GetValueOrDefault(recipe) + 1;

            Enumerate(i, remaining, counts, results, recipes);

            counts[recipe]--;
            if (counts[recipe] == 0) counts.Remove(recipe);
            Restore(remaining, recipe);
        }
    }

    private static bool CanAfford(Dictionary<string, int> remaining, Recipe recipe) =>
        recipe.Ingredients.All(kvp =>
            remaining.TryGetValue(kvp.Key, out var available) && available >= kvp.Value);

    private static void Deduct(Dictionary<string, int> remaining, Recipe recipe)
    {
        foreach (var (k, v) in recipe.Ingredients)
            remaining[k] -= v;
    }

    private static void Restore(Dictionary<string, int> remaining, Recipe recipe)
    {
        foreach (var (k, v) in recipe.Ingredients)
            remaining[k] += v;
    }

    private static SolverResult Snapshot(
        Dictionary<Recipe, int> counts,
        Dictionary<string, int> remaining) =>
        new(
            counts.Select(kvp => (kvp.Key, kvp.Value)).ToList(),
            counts.Sum(kvp => kvp.Key.PeopleFed * kvp.Value),
            new Dictionary<string, int>(remaining));
}
```

- [ ] **Step 4: Run tests — expect all pass**

```bash
cd /mnt/d/Repo/Assessment/Claude/Backtracking && dotnet test FoodOptimizer.Tests
```

Expected: `Passed! - Failed: 0, Passed: 7, Skipped: 0, Total: 7`

---

## Task 4: Program.cs — Backtracking output

**Files:**
- Modify: `Claude/Backtracking/FoodOptimizer/Program.cs`

- [ ] **Step 1: Implement Program.cs**

```csharp
// Claude/Backtracking/FoodOptimizer/Program.cs
using FoodOptimizer;

var results = new BacktrackingOptimizer().Solve(ProblemData.Pantry, ProblemData.Recipes);
PrintRanked(results);

static void PrintRanked(IReadOnlyList<SolverResult> results)
{
    if (!results.Any())
    {
        Console.WriteLine("No solutions found.");
        return;
    }

    Console.WriteLine("Best solution:");
    PrintSolution(results[0]);
    Console.WriteLine();
    Console.WriteLine("--- All solutions (ranked) ---");

    for (var i = 0; i < results.Count; i++)
    {
        Console.WriteLine();
        Console.WriteLine($"Rank {i + 1} — {results[i].TotalPeopleFed} people fed:");
        PrintSolution(results[i]);
    }
}

static void PrintSolution(SolverResult result)
{
    var recipes  = string.Join(", ", result.RecipesUsed.Select(r => $"{r.Recipe.Name} x{r.Count}"));
    var leftover = result.Leftovers.Where(kvp => kvp.Value > 0).Select(kvp => $"{kvp.Key} x{kvp.Value}");
    Console.WriteLine($"  Recipes:  {recipes}");
    Console.WriteLine($"  Fed:      {result.TotalPeopleFed}");
    Console.WriteLine($"  Leftover: {string.Join(", ", leftover)}");
}
```

- [ ] **Step 2: Run and verify output**

```bash
cd /mnt/d/Repo/Assessment/Claude/Backtracking && dotnet run --project FoodOptimizer
```

Expected first lines:
```
Best solution:
  Recipes:  ...
  Fed:      12
  Leftover: ...
```

---

## Task 5: Scaffold the OR-Tools solution

**Files:**
- Create: `Claude/OrTools/` directory tree

- [ ] **Step 1: Create solution and projects**

```bash
cd /mnt/d/Repo/Assessment/Claude
mkdir OrTools && cd OrTools
dotnet new sln -n FoodOptimizer
dotnet new console -n FoodOptimizer -f net10.0
dotnet new xunit -n FoodOptimizer.Tests -f net10.0
dotnet sln add FoodOptimizer/FoodOptimizer.csproj
dotnet sln add FoodOptimizer.Tests/FoodOptimizer.Tests.csproj
dotnet add FoodOptimizer.Tests/FoodOptimizer.Tests.csproj reference FoodOptimizer/FoodOptimizer.csproj
```

- [ ] **Step 2: Add Google.OrTools to both projects**

```bash
cd /mnt/d/Repo/Assessment/Claude/OrTools
dotnet add FoodOptimizer/FoodOptimizer.csproj package Google.OrTools
dotnet add FoodOptimizer.Tests/FoodOptimizer.Tests.csproj package Google.OrTools
```

- [ ] **Step 3: Verify clean build**

```bash
cd /mnt/d/Repo/Assessment/Claude/OrTools && dotnet build
```

Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

---

## Task 6: Domain model — OR-Tools

**Files:**
- Create: `Claude/OrTools/FoodOptimizer/Ingredient.cs`
- Create: `Claude/OrTools/FoodOptimizer/Recipe.cs`
- Create: `Claude/OrTools/FoodOptimizer/SolverResult.cs`
- Create: `Claude/OrTools/FoodOptimizer/ProblemData.cs`

- [ ] **Step 1: Create Ingredient.cs**

```csharp
// Claude/OrTools/FoodOptimizer/Ingredient.cs
namespace FoodOptimizer;

public static class Ingredient
{
    public const string Cucumber = "Cucumber";
    public const string Olives   = "Olives";
    public const string Lettuce  = "Lettuce";
    public const string Meat     = "Meat";
    public const string Tomato   = "Tomato";
    public const string Cheese   = "Cheese";
    public const string Dough    = "Dough";
}
```

- [ ] **Step 2: Create Recipe.cs**

```csharp
// Claude/OrTools/FoodOptimizer/Recipe.cs
namespace FoodOptimizer;

public record Recipe(
    string Name,
    IReadOnlyDictionary<string, int> Ingredients,
    int PeopleFed);
```

- [ ] **Step 3: Create SolverResult.cs**

```csharp
// Claude/OrTools/FoodOptimizer/SolverResult.cs
namespace FoodOptimizer;

public record SolverResult(
    IReadOnlyList<(Recipe Recipe, int Count)> RecipesUsed,
    int TotalPeopleFed,
    IReadOnlyDictionary<string, int> Leftovers);
```

- [ ] **Step 4: Create ProblemData.cs**

```csharp
// Claude/OrTools/FoodOptimizer/ProblemData.cs
namespace FoodOptimizer;

public static class ProblemData
{
    public static readonly IReadOnlyDictionary<string, int> Pantry =
        new Dictionary<string, int>
        {
            [Ingredient.Cucumber] = 2,
            [Ingredient.Olives]   = 2,
            [Ingredient.Lettuce]  = 3,
            [Ingredient.Meat]     = 6,
            [Ingredient.Tomato]   = 6,
            [Ingredient.Cheese]   = 8,
            [Ingredient.Dough]    = 10
        };

    public static readonly IReadOnlyList<Recipe> Recipes = new List<Recipe>
    {
        new("Burger", new Dictionary<string, int>
        {
            [Ingredient.Meat]    = 1,
            [Ingredient.Lettuce] = 1,
            [Ingredient.Tomato]  = 1,
            [Ingredient.Cheese]  = 1,
            [Ingredient.Dough]   = 1
        }, 1),
        new("Pie", new Dictionary<string, int>
        {
            [Ingredient.Meat]  = 2,
            [Ingredient.Dough] = 2
        }, 1),
        new("Sandwich", new Dictionary<string, int>
        {
            [Ingredient.Cucumber] = 1,
            [Ingredient.Dough]    = 1
        }, 1),
        new("Pasta", new Dictionary<string, int>
        {
            [Ingredient.Meat]   = 1,
            [Ingredient.Tomato] = 1,
            [Ingredient.Cheese] = 2,
            [Ingredient.Dough]  = 2
        }, 2),
        new("Salad", new Dictionary<string, int>
        {
            [Ingredient.Cucumber] = 1,
            [Ingredient.Lettuce]  = 2,
            [Ingredient.Tomato]   = 2,
            [Ingredient.Cheese]   = 2,
            [Ingredient.Olives]   = 1
        }, 3),
        new("Pizza", new Dictionary<string, int>
        {
            [Ingredient.Olives] = 1,
            [Ingredient.Tomato] = 2,
            [Ingredient.Cheese] = 3,
            [Ingredient.Dough]  = 3
        }, 4)
    };
}
```

- [ ] **Step 5: Empty out generated Program.cs**

```csharp
// Claude/OrTools/FoodOptimizer/Program.cs
```

- [ ] **Step 6: Verify build**

```bash
cd /mnt/d/Repo/Assessment/Claude/OrTools && dotnet build
```

Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

---

## Task 7: OrToolsOptimizer — TDD

**Files:**
- Create: `Claude/OrTools/FoodOptimizer/OrToolsOptimizer.cs`
- Modify: `Claude/OrTools/FoodOptimizer.Tests/OrToolsOptimizerTests.cs`

- [ ] **Step 1: Replace generated test file with real tests**

```csharp
// Claude/OrTools/FoodOptimizer.Tests/OrToolsOptimizerTests.cs
using FoodOptimizer;

namespace FoodOptimizer.Tests;

public class OrToolsOptimizerTests
{
    private readonly OrToolsOptimizer _sut = new();

    [Fact]
    public void Solve_EmptyRecipes_ReturnsEmpty()
    {
        var result = _sut.Solve(
            new Dictionary<string, int> { ["Flour"] = 5 },
            new List<Recipe>());

        Assert.Empty(result);
    }

    [Fact]
    public void Solve_SingleAffordableRecipe_ReturnsSingleResult()
    {
        var pantry  = new Dictionary<string, int> { ["Flour"] = 2 };
        var recipes = new List<Recipe>
        {
            new("Cake", new Dictionary<string, int> { ["Flour"] = 2 }, 3)
        };

        var results = _sut.Solve(pantry, recipes);

        Assert.Single(results);
        Assert.Equal(3, results[0].TotalPeopleFed);
    }

    [Fact]
    public void Solve_SingleUnaffordableRecipe_ReturnsEmpty()
    {
        var pantry  = new Dictionary<string, int> { ["Flour"] = 1 };
        var recipes = new List<Recipe>
        {
            new("Cake", new Dictionary<string, int> { ["Flour"] = 2 }, 3)
        };

        var results = _sut.Solve(pantry, recipes);

        Assert.Empty(results);
    }

    [Fact]
    public void Solve_CanMakeRecipeTwice_ReturnsBothCounts()
    {
        var pantry  = new Dictionary<string, int> { ["Flour"] = 4 };
        var recipes = new List<Recipe>
        {
            new("Bread", new Dictionary<string, int> { ["Flour"] = 2 }, 2)
        };

        var results = _sut.Solve(pantry, recipes);

        // 1x Bread (feeds 2) and 2x Bread (feeds 4)
        Assert.Equal(2, results.Count);
        Assert.Equal(4, results[0].TotalPeopleFed);
        Assert.Equal(2, results[1].TotalPeopleFed);
    }

    [Fact]
    public void Solve_ResultsSortedDescendingByPeopleFed()
    {
        var results = _sut.Solve(ProblemData.Pantry, ProblemData.Recipes);

        for (var i = 0; i < results.Count - 1; i++)
            Assert.True(results[i].TotalPeopleFed >= results[i + 1].TotalPeopleFed);
    }

    [Fact]
    public void Solve_AssessmentData_BestSolutionFeeds12()
    {
        var results = _sut.Solve(ProblemData.Pantry, ProblemData.Recipes);

        Assert.Equal(12, results[0].TotalPeopleFed);
    }

    [Fact]
    public void Solve_LeftoversCorrectForBestSolution()
    {
        var results = _sut.Solve(ProblemData.Pantry, ProblemData.Recipes);
        var best = results[0];

        Assert.All(best.Leftovers.Values, v => Assert.True(v >= 0));

        foreach (var (ingredient, available) in ProblemData.Pantry)
        {
            var used = best.RecipesUsed.Sum(r =>
                r.Recipe.Ingredients.TryGetValue(ingredient, out var amt)
                    ? amt * r.Count
                    : 0);
            Assert.Equal(available, used + best.Leftovers[ingredient]);
        }
    }
}
```

- [ ] **Step 2: Run tests — expect compile failure**

```bash
cd /mnt/d/Repo/Assessment/Claude/OrTools && dotnet test FoodOptimizer.Tests
```

Expected: build error — `The type or namespace name 'OrToolsOptimizer' could not be found`.

- [ ] **Step 3: Create OrToolsOptimizer.cs**

```csharp
// Claude/OrTools/FoodOptimizer/OrToolsOptimizer.cs
using Google.OrTools.Sat;

namespace FoodOptimizer;

public class OrToolsOptimizer
{
    public IReadOnlyList<SolverResult> Solve(
        IReadOnlyDictionary<string, int> pantry,
        IReadOnlyList<Recipe> recipes)
    {
        if (!pantry.Any() || !recipes.Any())
            return Array.Empty<SolverResult>();

        var model = new CpModel();

        // One integer variable per recipe: how many times it is made
        var vars = recipes.Select((recipe, i) =>
        {
            var upperBound = recipe.Ingredients
                .Where(kvp => pantry.ContainsKey(kvp.Key))
                .Select(kvp => (long)(pantry[kvp.Key] / kvp.Value))
                .DefaultIfEmpty(0L)
                .Min();
            return model.NewIntVar(0, upperBound, $"recipe_{i}");
        }).ToArray();

        // Ingredient capacity constraints
        foreach (var (ingredient, available) in pantry)
        {
            var relevant = recipes
                .Select((r, i) => (r, i))
                .Where(x => x.r.Ingredients.ContainsKey(ingredient))
                .ToList();

            if (!relevant.Any()) continue;

            var termVars  = relevant.Select(x => vars[x.i]).ToArray();
            var coeffs    = relevant.Select(x => (long)x.r.Ingredients[ingredient]).ToArray();
            model.Add(LinearExpr.WeightedSum(termVars, coeffs) <= available);
        }

        var callback = new SolutionCollector(vars, recipes, pantry);
        new CpSolver().SearchAllSolutions(model, callback);

        return callback.Results
            .Where(r => r.TotalPeopleFed > 0)
            .OrderByDescending(r => r.TotalPeopleFed)
            .ToList();
    }
}

internal sealed class SolutionCollector : CpSolverSolutionCallback
{
    private readonly IntVar[] _vars;
    private readonly IReadOnlyList<Recipe> _recipes;
    private readonly IReadOnlyDictionary<string, int> _pantry;

    public List<SolverResult> Results { get; } = new();

    public SolutionCollector(
        IntVar[] vars,
        IReadOnlyList<Recipe> recipes,
        IReadOnlyDictionary<string, int> pantry)
    {
        _vars    = vars;
        _recipes = recipes;
        _pantry  = pantry;
    }

    public override void OnSolutionCallback()
    {
        var counts = _vars.Select((v, i) => (_recipes[i], (int)Value(v))).ToList();

        var leftovers = new Dictionary<string, int>(_pantry);
        foreach (var (recipe, count) in counts)
            foreach (var (ingredient, amount) in recipe.Ingredients)
                leftovers[ingredient] -= amount * count;

        var used = counts
            .Where(c => c.Item2 > 0)
            .Select(c => (c.Item1, c.Item2))
            .ToList();

        Results.Add(new SolverResult(
            used,
            counts.Sum(c => c.Item1.PeopleFed * c.Item2),
            leftovers));
    }
}
```

- [ ] **Step 4: Run tests — expect all pass**

```bash
cd /mnt/d/Repo/Assessment/Claude/OrTools && dotnet test FoodOptimizer.Tests
```

Expected: `Passed! - Failed: 0, Passed: 7, Skipped: 0, Total: 7`

---

## Task 8: Program.cs — OR-Tools output

**Files:**
- Modify: `Claude/OrTools/FoodOptimizer/Program.cs`

- [ ] **Step 1: Implement Program.cs**

```csharp
// Claude/OrTools/FoodOptimizer/Program.cs
using FoodOptimizer;

var results = new OrToolsOptimizer().Solve(ProblemData.Pantry, ProblemData.Recipes);
PrintRanked(results);

static void PrintRanked(IReadOnlyList<SolverResult> results)
{
    if (!results.Any())
    {
        Console.WriteLine("No solutions found.");
        return;
    }

    Console.WriteLine("Best solution:");
    PrintSolution(results[0]);
    Console.WriteLine();
    Console.WriteLine("--- All solutions (ranked) ---");

    for (var i = 0; i < results.Count; i++)
    {
        Console.WriteLine();
        Console.WriteLine($"Rank {i + 1} — {results[i].TotalPeopleFed} people fed:");
        PrintSolution(results[i]);
    }
}

static void PrintSolution(SolverResult result)
{
    var recipes  = string.Join(", ", result.RecipesUsed.Select(r => $"{r.Recipe.Name} x{r.Count}"));
    var leftover = result.Leftovers.Where(kvp => kvp.Value > 0).Select(kvp => $"{kvp.Key} x{kvp.Value}");
    Console.WriteLine($"  Recipes:  {recipes}");
    Console.WriteLine($"  Fed:      {result.TotalPeopleFed}");
    Console.WriteLine($"  Leftover: {string.Join(", ", leftover)}");
}
```

- [ ] **Step 2: Run and verify output**

```bash
cd /mnt/d/Repo/Assessment/Claude/OrTools && dotnet run --project FoodOptimizer
```

Expected first lines:
```
Best solution:
  Recipes:  ...
  Fed:      12
  Leftover: ...
```

- [ ] **Step 3: Confirm both solutions agree on the best score**

The Backtracking and OR-Tools best `Fed:` values must both be `12`.
