namespace Manual;
public class Solution
{
    public Dictionary<string, int> Pantry = new();
    public List<Recipe> RecipesUsed = new();
    public int TotalScore => RecipesUsed.Sum(r => r.NoFed);
    public bool Traversed;

    public override string ToString() =>
        "Recipes used: " + string.Join(", ", RecipesUsed.Select(r => r.Name)) +
        "\nTotal score: " + TotalScore +
        "\nIngredients left over: " + string.Join(", ", Pantry);

    public bool IsDuplicate(Solution solution)
    {
        if(RecipesUsed.Count == 0)
            return false;

        var solutionRecipes = new List<Recipe>(solution.RecipesUsed);
        foreach (var recipe in RecipesUsed)
        {
            if (!(solutionRecipes.Count > 0))
                return false;
            if (!solutionRecipes.Contains(recipe))
                return false;
            solutionRecipes.Remove(recipe);
        }
        return !(solutionRecipes.Count > 0);
    }

    public Solution Clone()
    {
        return new Solution()
        {
            Pantry = new Dictionary<string, int>(Pantry),
            RecipesUsed = new List<Recipe>(RecipesUsed),
            Traversed = false,
        };
    }
    
    public Solution? TryAddRecipe(Recipe recipe)
    {
        var clone = Clone();
        if (!clone.PantryContainsIngredients(recipe))
            return null;
        clone.RecipesUsed.Add(recipe);
        clone.UseIngredients(recipe);
        Traversed = true;
        return clone;
    }

    private bool PantryContainsIngredients(Recipe recipe)
    {
        return recipe.Ingredients.All(
            ingredientInRecipe => 
                Pantry.ContainsKey(ingredientInRecipe.Key) &&
                Pantry[ingredientInRecipe.Key] - ingredientInRecipe.Value >= 0);
    }

    private void UseIngredients(Recipe recipe)
    {
        foreach (var ingredient in recipe.Ingredients) 
            Pantry[ingredient.Key] -= ingredient.Value;
    }
}