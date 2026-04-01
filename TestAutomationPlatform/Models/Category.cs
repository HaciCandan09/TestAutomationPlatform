using System.Text.Json.Serialization;
using TestAutomationPlatform.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }

   
    public List<Script> Scripts { get; set; } = new();
}
