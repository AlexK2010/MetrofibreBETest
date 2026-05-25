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
