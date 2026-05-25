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

        // Verified optimal: e.g. Pizza x2, Burger x2, Sandwich x2 = 12
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
            Assert.Equal(available, used + best.Leftovers.GetValueOrDefault(ingredient, 0));
        }
    }
}
