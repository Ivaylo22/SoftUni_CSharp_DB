namespace CarDealer.DTOs.Import;

public class ImportCustomersDto
{
    public string Name { get; set; } = null!;

    public DateTime BirthDate { get; set; }

    public bool IsYoungDriver { get; set; }
}
