# Food Optimizer — Design Spec

**Date:** 2026-05-24  
**Problem:** Given a fixed pantry of ingredients and a list of recipes (each consuming ingredients and feeding N people), find all valid recipe combinations and rank them by total people fed.  
**Goal:** Two independent C# .NET 10 console apps in `Claude/` — one using DFS backtracking (no dependencies), one using Google OR-Tools (CP-SAT).

---

## Project Structure

```
Claude/
  Backtracking/
    FoodOptimizer.sln
    FoodOptimizer/
      FoodOptimizer.csproj   (net10.0, no external deps)
      Ingredient.cs
      Recipe.cs
      SolverResult.cs
      ProblemData.cs
      BacktrackingOptimizer.cs
      Program.cs
  OrTools/
    FoodOptimizer.sln
    FoodOptimizer/
      FoodOptimizer.csproj   (net10.0, Google.OrTools NuGet)
      Ingredient.cs
      Recipe.cs
      SolverResult.cs
      ProblemData.cs
      OrToolsOptimizer.cs
      Program.cs
```

---

## Domain Model (identical shape in both solutions)

### `Ingredient.cs`
Static class of `const string` constants — one per ingredient name. Avoids magic strings throughout.

### `Recipe.cs`
```csharp
record Recipe(string Name, IReadOnlyDictionary<string, int> Ingredients, int PeopleFed);
```

### `SolverResult.cs`
```csharp
record SolverResult(
    IReadOnlyList<(Recipe Recipe, int Count)> RecipesUsed,
    int TotalPeopleFed,
    IReadOnlyDictionary<string, int> Leftovers);
```

### `ProblemData.cs`
Static class defining the assessment's specific pantry and recipe list:

**Pantry:**
| Ingredient | Amount |
|---|---|
| Cucumber | 2 |
| Olives | 2 |
| Lettuce | 3 |
| Meat | 6 |
| Tomato | 6 |
| Cheese | 8 |
| Dough | 10 |

**Recipes:**
| Name | Feeds | Ingredients |
|---|---|---|
| Burger | 1 | 1 Meat, 1 Lettuce, 1 Tomato, 1 Cheese, 1 Dough |
| Pie | 1 | 2 Meat, 2 Dough |
| Sandwich | 1 | 1 Cucumber, 1 Dough |
| Pasta | 2 | 1 Meat, 1 Tomato, 2 Cheese, 2 Dough |
| Salad | 3 | 1 Cucumber, 2 Lettuce, 2 Tomato, 2 Cheese, 1 Olives |
| Pizza | 4 | 1 Olives, 2 Tomato, 3 Cheese, 3 Dough |

### Optimizer contract (both solutions)
```csharp
IReadOnlyList<SolverResult> Solve(
    IReadOnlyDictionary<string, int> pantry,
    IReadOnlyList<Recipe> recipes);
```
Returns all valid solutions, sorted descending by `TotalPeopleFed`.

---

## Solution A — Backtracking (pure .NET)

### Algorithm
Recursive DFS that enumerates all valid recipe multisets. Recipes are iterated in fixed index order (each recursive call passes `minIndex = i`, not `i+1`, to allow repetition of the same recipe). This structure guarantees each multiset is visited exactly once — no deduplication needed.

```
Enumerate(minIdx, remainingPantry, currentCounts, results):
  record the current state as a SolverResult (zero or more recipes used)
  for i = minIdx to recipes.Count - 1:
    recipe = recipes[i]
    if CanAfford(remainingPantry, recipe):
      Deduct recipe from remainingPantry
      Increment currentCounts[recipe]
      Enumerate(i, remainingPantry, currentCounts, results)
      Restore remainingPantry and currentCounts   // backtrack
```

Every node in the tree is a valid solution (partial combinations count). The root node (no recipes) is excluded from results since it feeds 0 people.

### `BacktrackingOptimizer.cs`
- Public `Solve(pantry, recipes)` allocates mutable working copies and calls the private `Enumerate` recursive method.
- Returns collected results sorted descending by `TotalPeopleFed`.
- No static state; fully re-entrant.

---

## Solution B — OR-Tools (CP-SAT)

### Algorithm
Model as a constraint satisfaction problem via CP-SAT (no objective — score-based ranking is done externally):

- **Variables:** one `IntVar` per recipe, domain `[0, upperBound]` where `upperBound = min over required ingredients of floor(pantry[ingredient] / recipe[ingredient])`
- **Constraints:** for each ingredient: `Σ (count[r] × amount[r][ingredient]) ≤ pantry[ingredient]`
- **Enumeration:** call `solver.SearchAllSolutions(model, callback)` — CP-SAT enumerates every feasible assignment when no objective is set. The callback records each solution as a `SolverResult`. Results are sorted descending by `TotalPeopleFed` after search completes.

Note: setting an objective with `EnumerateAllSolutions` only fires the callback on improving solutions (not suboptimal ones), so the satisfaction approach is required for a full ranked list.

### `OrToolsOptimizer.cs`
- Builds the CP model from the generic `pantry` / `recipes` inputs.
- Runs solver with all-solutions enumeration enabled.
- Converts each callback snapshot (variable assignments) into a `SolverResult`.
- Returns all collected results sorted descending by `TotalPeopleFed`.

---

## Output Format (both solutions)

```
Best solution:
  Pizza x1, Salad x1, Sandwich x1, Burger x1, Pasta x1, Pie x1
  Total people fed: 12
  Leftovers: Meat x2

--- All solutions (ranked) ---

Rank 1 — 12 people fed:
  Pizza x1, Salad x1, ...

Rank 2 — 11 people fed:
  ...
```

---

## Error Handling

- If `pantry` or `recipes` is empty, return an empty list.
- No recipe can produce a negative ingredient count (validated at model construction).
- No external config or file I/O — all data is in-memory.
