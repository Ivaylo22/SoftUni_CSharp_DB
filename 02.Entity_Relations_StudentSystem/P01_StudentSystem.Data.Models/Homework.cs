namespace P01_StudentSystem.Data.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Enums;

public class Homework
{
    [Key]
    public int HomeworkId { get; set; }

    public string Content { get; set; } = null!;

    public ContentType ContentType { get; set; }

    public DateTime SubmissionTime { get; set; }

    [ForeignKey(nameof(Students))]
    public int StudentId { get; set; }

    public virtual Student Students { get; set; } = null!;

    [ForeignKey(nameof(Courses))]
    public int CourseId { get; set; }

    public virtual Course Courses { get; set; } = null!;

}
