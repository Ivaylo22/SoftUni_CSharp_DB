using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Trucks.DataProcessor.ImportDto;

[XmlType("Despatcher")]
public class ImportDespatcherDto
{

    [XmlElement("Name")]
    [MinLength(2)]
    [MaxLength(40)]
    [Required]
    public string Name { get; set; } = null!;

    [XmlElement("Position")]
    [Required]
    public string Position { get; set; } = null!;

    [XmlArray("Trucks")]
    public ImportTruckDto[] Trucks { get; set; }
}
