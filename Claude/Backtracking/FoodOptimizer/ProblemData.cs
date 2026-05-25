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
