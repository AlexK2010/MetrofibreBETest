namespace FoodOptimizer;

public record Recipe(
    string Name,
    IReadOnlyDictionary<string, int> Ingredients,
    int PeopleFed);
