namespace ChibitsLink.main.cs.model;

public class Character
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Speed { get; set; }
}
