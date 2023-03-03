namespace P01_StudentSystem.Data;

using Microsoft.EntityFrameworkCore;

using Common;
using Models;

public class StudentSystemContext  : DbContext
{
    public StudentSystemContext()
    {

    }

    public StudentSystemContext(DbContextOptions options)
        :base(options)
    {

    }

    public DbSet<Student> Students { get; set; } = null!;

    public DbSet<Course> Courses { get; set; } = null!;

    public DbSet<Course> Homeworks { get; set; } = null!;

    public DbSet<Course> Resources { get; set; } = null!;

    public DbSet<Course> StudentsCourses { get; set; } = null!;


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(DbConfig.ConnectionString);
        }
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>(entity =>
        {
            entity
                .Property(s => s.PhoneNumber)
                .IsFixedLength(true)
                .HasColumnType("varchar");
        });


        modelBuilder.Entity<StudentCourse>(entity =>
        {
            entity.HasKey(e => new { e.StudentId, e.CourseId });
        });

        base.OnModelCreating(modelBuilder);
    }

}