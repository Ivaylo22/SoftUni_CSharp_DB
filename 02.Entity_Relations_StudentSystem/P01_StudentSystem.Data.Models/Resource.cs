namespace P01_StudentSystem.Data.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Common;
using Enums;

public class Resource
{
    [Key]
    public int ResourseId { get; set; }

    [MaxLength(ValidationConstants.MaxResourseNameLength)]
    public string Name { get; set; } = null!;

    public string Url { get; set; } = null!;

    public ResourceType ResourceType { get; set; }


    [ForeignKey(nameof(Courses))]
    public int CourseId { get; set; }

    public virtual Course Courses { get; set; } = null!;

}
