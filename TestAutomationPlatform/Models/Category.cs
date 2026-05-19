using TestAutomationPlatform.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public List<Script> Scripts { get; set; } = new();
}
