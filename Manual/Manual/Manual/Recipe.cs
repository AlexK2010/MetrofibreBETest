namespace Manual;
public record Recipe
{
    public required string Name { get; set; }
    public required Dictionary<string, int> Ingredients { get; set; }
    public int NoFed { get; set; }
}