# Metrofibre Backend Assessment — Food Optimizer

## Problem

Given a pantry of ingredients and a set of recipes, find the combination of recipes that feeds the most people without exceeding the available ingredients.

**Pantry:** Cucumber ×2, Olives ×2, Lettuce ×3, Meat ×6, Tomato ×6, Cheese ×8, Dough ×10

| Recipe   | Ingredients                                     | Feeds |
|----------|-------------------------------------------------|-------|
| Burger   | Meat ×1, Lettuce ×1, Tomato ×1, Cheese ×1, Dough ×1 | 1 |
| Pie      | Meat ×2, Dough ×2                               | 1     |
| Sandwich | Cucumber ×1, Dough ×1                           | 1     |
| Pasta    | Meat ×1, Tomato ×1, Cheese ×2, Dough ×2        | 2     |
| Salad    | Cucumber ×1, Lettuce ×2, Tomato ×2, Cheese ×2, Olives ×1 | 3 |
| Pizza    | Olives ×1, Tomato ×2, Cheese ×3, Dough ×3      | 4     |

**Optimal answer:** 12 people fed (Pizza ×1, Salad ×1, Pasta ×1, Burger ×1, Pie ×1, Sandwich ×1)

---

## Solutions

### Manual — `Manual/`

> Written entirely by me, without any AI assistance.

An iterative breadth-first expansion approach. Starting from a root state (full pantry, no recipes used), each iteration attempts to extend every non-terminal `Solution` node by adding one more recipe. New states that are not duplicates of existing ones are appended to the list. The loop terminates once no new states can be added, at which point all states are ranked by total people fed.

```
Manual/
└── Manual/
    ├── Ingredient.cs     — ingredient name constants
    ├── Recipe.cs         — recipe record (name, ingredients, people fed)
    ├── Solution.cs       — state node: pantry snapshot + recipes used + expansion logic
    └── Program.cs        — wires up pantry/recipes, runs the expansion loop, prints results
```

**Run:**
```bash
dotnet run --project Manual/Manual/Manual/Manual.csproj
```

---

### Claude — Backtracking — `Claude/Backtracking/`

> Written with the assistance of Claude Code.

A recursive backtracking search that enumerates every valid multiset of recipes. At each call it records the current combination as a result, then tries extending it with each recipe from `minIdx` onwards (allowing repeats of the same recipe). Ingredient state is mutated in-place and restored after each recursive call, avoiding unnecessary allocations.

```
Claude/Backtracking/
├── FoodOptimizer/
│   ├── BacktrackingOptimizer.cs   — recursive enumerate/backtrack engine
│   ├── ProblemData.cs             — pantry and recipe definitions
│   ├── Ingredient.cs / Recipe.cs / SolverResult.cs
│   └── Program.cs
└── FoodOptimizer.Tests/
    └── BacktrackingOptimizerTests.cs
```

**Run:**
```bash
dotnet run --project Claude/Backtracking/FoodOptimizer/FoodOptimizer.csproj
```

**Test:**
```bash
dotnet test Claude/Backtracking/FoodOptimizer.sln
```

---

### Claude — OR-Tools — `Claude/OrTools/`

> Written with the assistance of Claude Code.

Uses Google OR-Tools CP-SAT (constraint programming / SAT) solver. Each recipe becomes an integer decision variable bounded by how many times it can be made given the pantry. Ingredient availability is expressed as linear inequality constraints. The solver is run in `enumerate_all_solutions` mode so every feasible assignment is collected via a `SolutionCollector` callback, then sorted by people fed.

```
Claude/OrTools/
├── FoodOptimizer/
│   ├── OrToolsOptimizer.cs        — CP-SAT model + SolutionCollector callback
│   ├── ProblemData.cs
│   ├── Ingredient.cs / Recipe.cs / SolverResult.cs
│   └── Program.cs
└── FoodOptimizer.Tests/
    └── OrToolsOptimizerTests.cs
```

**Run:**
```bash
dotnet run --project Claude/OrTools/FoodOptimizer/FoodOptimizer.csproj
```

**Test:**
```bash
dotnet test Claude/OrTools/FoodOptimizer.sln
```

---

## Approach Comparison

| | Manual | Claude — Backtracking | Claude — OR-Tools |
|---|---|---|---|
| Algorithm | Iterative BFS expansion | Recursive backtracking | CP-SAT constraint solver |
| AI assistance | None | Claude Code | Claude Code |
| Unit tests | No | Yes | Yes |
| External dependencies | None | None | Google.OrTools |
| Solution enumeration | All multisets via BFS | All multisets via DFS | All feasible assignments |
