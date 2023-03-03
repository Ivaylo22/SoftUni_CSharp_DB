namespace P01_StudentSystem.Data.Models;

using System.ComponentModel.DataAnnotations;

using Common;

public class Course
{
    public Course()
    {
        this.StudentsCourses = new HashSet<StudentCourse>();
        this.Resources = new HashSet<Resource>();
        this.Homeworks = new HashSet<Homework>();
    }

    [Key]
    public int CourseId { get; set; }

    [MaxLength(ValidationConstants.MaxCourseNameLength)]
    public string Name { get; set; } = null!;

    public string? Description  { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public decimal Price { get; set; }

    public virtual ICollection<StudentCourse> StudentsCourses { get; set; } = null!;

    public virtual ICollection<Resource> Resources { get; set; } = null!;

    public virtual ICollection<Homework> Homeworks { get; set; } = null!;

}
