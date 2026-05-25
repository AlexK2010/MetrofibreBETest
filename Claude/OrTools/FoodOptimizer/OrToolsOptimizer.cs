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
        var solver = new CpSolver();
        solver.StringParameters = "enumerate_all_solutions:true";
        solver.Solve(model, callback);

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
    private readonly List<SolverResult> _results = new();

    public IReadOnlyList<SolverResult> Results => _results;

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
        var counts = _vars.Select((v, i) => (Recipe: _recipes[i], Count: (int)Value(v))).ToList();

        var leftovers = new Dictionary<string, int>(_pantry);
        foreach (var (recipe, count) in counts)
            foreach (var (ingredient, amount) in recipe.Ingredients)
                if (leftovers.ContainsKey(ingredient))
                    leftovers[ingredient] -= amount * count;

        var used = counts
            .Where(c => c.Count > 0)
            .Select(c => (c.Recipe, c.Count))
            .ToList();

        _results.Add(new SolverResult(
            used,
            counts.Sum(c => c.Recipe.PeopleFed * c.Count),
            leftovers));
    }
}
