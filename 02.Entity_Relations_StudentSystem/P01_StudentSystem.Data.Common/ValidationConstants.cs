namespace P01_StudentSystem.Data.Common;

public static class ValidationConstants
{
    //Student
    public const int MaxStudentNameLength = 100;
    public const int MaxPhoneLength = 10;

    //Course
    public const int MaxCourseNameLength = 80;
    public const int MaxCourseDescriptionLength = 1024;

    //Resourse
    public const int MaxResourseNameLength = 50;
    public const int MaxResourceUrlLength = 1024;

    //Homework
    public const int MaxHomeworkContentLength = 1024;
}
