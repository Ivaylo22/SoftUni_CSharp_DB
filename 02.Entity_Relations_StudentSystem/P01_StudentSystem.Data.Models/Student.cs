﻿namespace P01_StudentSystem.Data.Models;

using System.ComponentModel.DataAnnotations;

using Common;

public class Student
{

    public Student()
    {
        this.StudentsCourses = new HashSet<StudentCourse>();
        this.Homeworks = new HashSet<Homework>();
    }

    [Key]
    public int StudentId { get; set; }

    [MaxLength(ValidationConstants.MaxStudentNameLength)]
    public string Name { get; set; } = null!;

    [MaxLength(ValidationConstants.MaxPhoneLength)]
    public string? PhoneNumber { get; set; }

    public DateTime RegisteredOn { get; set; }

    public DateTime? Birthday  { get; set; }

    public virtual ICollection<StudentCourse> StudentsCourses { get; set; } = null!;

    public virtual ICollection<Homework> Homeworks { get; set; } = null!;
}