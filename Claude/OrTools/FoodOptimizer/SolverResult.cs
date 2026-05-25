namespace FoodOptimizer;

public record SolverResult(
    IReadOnlyList<(Recipe Recipe, int Count)> RecipesUsed,
    int TotalPeopleFed,
    IReadOnlyDictionary<string, int> Leftovers);
