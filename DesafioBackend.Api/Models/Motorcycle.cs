namespace DesafioBackend.Api.Models;

public class Motorcycle
{
    public int Id { get; set; }
    public string Model { get; set; } = string.Empty;
    public string Plate { get; set; } = string.Empty;
    public int Year { get; set; }
}