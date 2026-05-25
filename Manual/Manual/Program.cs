using Manual;

var pantry = new Dictionary<string, int>()
{
    { Ingredient.Cucumber, 2},
    { Ingredient.Olives, 2},
    { Ingredient.Lettuce, 3},
    { Ingredient.Meat, 6},
    { Ingredient.Tomato, 6},
    { Ingredient.Cheese, 8},
    { Ingredient.Dough, 10}
};

var recipes = new List<Recipe>()
{
    new()
    {
        Name = "Burger",
        Ingredients = new Dictionary<string, int>()
        {
            { Ingredient.Meat, 1 },
            { Ingredient.Lettuce, 1 },
            { Ingredient.Tomato, 1 },
            { Ingredient.Cheese, 1 },
            { Ingredient.Dough, 1 },
        },
        NoFed = 1
    },
    new()
    {
        Name = "Pie",
        Ingredients = new Dictionary<string, int>()
        {
            { Ingredient.Meat, 2 },
            { Ingredient.Dough, 2 },
        },
        NoFed = 1
    },
    new()
    {
        Name = "Sandwich",
        Ingredients = new Dictionary<string, int>()
        {
            { Ingredient.Cucumber, 1 },
            { Ingredient.Dough, 1 },
        },
        NoFed = 1
    },
    new()
    {
        Name = "Pasta",
        Ingredients = new Dictionary<string, int>()
        {
            { Ingredient.Meat, 1 },
            { Ingredient.Tomato, 1 },
            { Ingredient.Cheese, 2 },
            { Ingredient.Dough, 2 },
        },
        NoFed = 2
    },
    new()
    {
        Name = "Salad",
        Ingredients = new Dictionary<string, int>()
        {
            { Ingredient.Cucumber, 1 },
            { Ingredient.Lettuce, 2 },
            { Ingredient.Tomato, 2 },
            { Ingredient.Cheese, 2 },
            { Ingredient.Olives, 1 },
        },
        NoFed = 3
    },
    new()
    {
        Name = "Pizza",
        Ingredients = new Dictionary<string, int>()
        {
            { Ingredient.Olives, 1 },
            { Ingredient.Tomato, 2 },
            { Ingredient.Cheese, 3 },
            { Ingredient.Dough, 3 },
        },
        NoFed = 4
    },
};

var root = new Solution()
{
    Pantry = pantry,
    RecipesUsed = new List<Recipe>()
};

var solutions = new List<Solution>(){ root };
var anyAdded = true;

while (anyAdded)
{
    anyAdded = false;
    foreach (var solution in solutions.Where(x => !x.Traversed).ToList())
    {
        foreach (var recipe in recipes)
        {
            var newSolution = solution.TryAddRecipe(recipe);
            if (newSolution != null)
            {
                anyAdded = true;
                if(!solutions.Any(x => x.IsDuplicate(newSolution)))
                    solutions.Add(newSolution);
            }
        }
    }
}

var bestSolution = solutions.OrderByDescending(x => x.TotalScore).First();
var solutionsInOrder = solutions.OrderByDescending(x => x.TotalScore).ToList();

Console.WriteLine("Best solution: ");
Console.WriteLine(bestSolution.ToString());

Console.WriteLine();
Console.WriteLine("-----------------------------------------");
Console.WriteLine();
Console.WriteLine("Solutions in score order:");
var index = 0;
foreach (var solution in solutionsInOrder)
{
    Console.WriteLine($"\nSolution {index++}:");
    Console.WriteLine(solution.ToString());
}