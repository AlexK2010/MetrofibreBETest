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
