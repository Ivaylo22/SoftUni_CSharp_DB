using SoftUni.Data;
using SoftUni.Models;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SoftUni;

public class StartUp
{
    static void Main(string[] args)
    {
        SoftUniContext dbContext = new SoftUniContext();

        string result = GetEmployeesByFirstNameStartingWithSa(dbContext);

        Console.WriteLine(result);
    }


    //Problem 03:
    public static string GetEmployeesFullInformation(SoftUniContext context)
    {
        StringBuilder sb = new StringBuilder();

        var employees = context.Employees
            .OrderBy(e => e.EmployeeId)
            .Select(e => new
            {
                e.FirstName,
                e.MiddleName,
                e.LastName,
                e.JobTitle,
                e.Salary
            })
            .ToArray();

        foreach (var employee in employees)
        {
            sb.AppendLine($"{employee.FirstName} {employee.LastName} {employee.MiddleName} {employee.JobTitle} {employee.Salary:f2}");
        }

        return sb.ToString().TrimEnd();
    }

    //Problem 04:
    public static string GetEmployeesWithSalaryOver50000(SoftUniContext context)
    {
        StringBuilder sb = new StringBuilder();

        var employees = context.Employees
            .Select(e => new
            {
                e.FirstName,
                e.Salary
            })
            .Where(e => e.Salary > 50000)
            .OrderBy(e => e.FirstName)
            .ToArray();

        foreach (var employee in employees)
        {
            sb
                .AppendLine($"{employee.FirstName} - {employee.Salary:f2}");
        }

        return sb.ToString().TrimEnd();
    }

    //Problem 05:
    public static string GetEmployeesFromResearchAndDevelopment(SoftUniContext context)
    {
        StringBuilder sb = new StringBuilder();

        var employeesRnD = context.Employees
            .Where(e => e.Department.Name == "Research and Development")
            .Select(e => new
            {
                e.FirstName,
                e.LastName,
                DepartmentName = e.Department.Name,
                e.Salary
            })
            .OrderBy(e => e.Salary)
            .ThenByDescending(e => e.FirstName)
            .ToArray();

        foreach (var e in employeesRnD)
        {
            sb
                .AppendLine($"{e.FirstName} {e.LastName} from {e.DepartmentName} - ${e.Salary:f2}");
        }

        return sb.ToString().TrimEnd();
    }

    //Problem 06:
    public static string AddNewAddressToEmployee(SoftUniContext context)
    {
        Address address = new Address
        {
            AddressText = "Vitoshka 15",
            TownId = 4
        };

        Employee? employee = context.Employees
            .FirstOrDefault(e => e.LastName == "Nakov");

        employee!.Address = address;

        context.SaveChanges();

        var employeesTop10 = context.Employees
            .OrderByDescending(e => e.AddressId)
            .Take(10)
            .Select(e => e.Address!.AddressText)
            .ToArray();

        return String.Join(Environment.NewLine, employeesTop10);
    }

    //Problem 07:
    public static string GetEmployeesInPeriod(SoftUniContext context)
    {
        StringBuilder sb = new StringBuilder();

        var employees = context.Employees
            .Take(10)
            .Select(e => new
            {
                e.FirstName,
                e.LastName,
                ManagerFirstName = e.Manager!.FirstName,
                ManagerLastName = e.Manager!.LastName,
                Projects = e.EmployeesProjects
                    .Where(ep => ep.Project.StartDate.Year >= 2001 &&
                                 ep.Project.StartDate.Year <= 2003)
                    .Select(ep => new
                    {
                        ProjectName = ep.Project.Name,
                        StartDate = ep.Project.StartDate.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture),
                        EndDate = ep.Project.EndDate.HasValue 
                            ? ep.Project.EndDate!.Value.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture)
                            : "not finished"
                    })
                    .ToArray()
            })
            .ToArray();

        foreach (var e in employees)
        {
            sb
                .AppendLine($"{e.FirstName} {e.LastName} - Manager: {e.ManagerFirstName} {e.ManagerLastName}");

            foreach (var project in e.Projects)
            {
                sb
                    .AppendLine($"--{project.ProjectName} - {project.StartDate} - {project.EndDate}");
            }
        }

        return sb.ToString().TrimEnd();
    }

    //Problem 08:
    public static string GetAddressesByTown(SoftUniContext context)
    {
        StringBuilder sb = new StringBuilder();

        var addresses = context.Addresses
            .OrderByDescending(a => a.Employees.Count)
            .ThenBy(a => a.Town!.Name)
            .ThenBy(a => a.AddressText)
            .Take(10)
            .Select(a => new
            {
                Address = a.AddressText,
                TownName = a.Town!.Name,
                EmployeeCount = a.Employees.Count
            })
            .ToList();

        foreach (var a in addresses)
        {
            sb
                .AppendLine($"{a.Address}, {a.TownName} - {a.EmployeeCount} employees");
        }

        return sb.ToString().TrimEnd();
    }

    //Problem 09:
    public static string GetEmployee147(SoftUniContext context)
    {
        StringBuilder sb = new StringBuilder();
        var employees = context.Employees
            .Where(e => e.EmployeeId == 147)
            .Select(e => new
            {
                e.FirstName,
                e.LastName,
                e.JobTitle,
                Projects = e.EmployeesProjects
                    .Select(p => new 
                    {
                        ProjectName = p.Project.Name
                    })
                    .OrderBy(p => p.ProjectName)
                    .ToArray()
            })
            .ToArray();

        foreach (var e in employees)
        {
            sb
                .AppendLine($"{e.FirstName} {e.LastName} - {e.JobTitle}");

            foreach (var p in e.Projects)
            {
                sb
                    .AppendLine(p.ProjectName);
            }
        }

        return sb.ToString().TrimEnd();
    }

    //Problem 10:
    public static string GetDepartmentsWithMoreThan5Employees(SoftUniContext context)
    {
        StringBuilder sb = new StringBuilder();

        var departments = context.Departments
            .Where(d => d.Employees.Count > 5)
            .OrderBy(d => d.Employees.Count)
            .ThenBy(d => d.Name)
            .Select(d => new
            {
                d.Name,
                ManagerFirstName = d.Manager.FirstName,
                ManagerLastName = d.Manager.LastName,
                Employees = d.Employees
                    .Select(e => new
                    {
                        e.FirstName,
                        e.LastName,
                        e.JobTitle
                    })
                    .OrderBy(e => e.FirstName)
                    .ThenBy(e => e.LastName)
                    .ToArray()
            })
            .ToArray();

        foreach (var d in departments)
        {
            sb
                .AppendLine($"{d.Name} - {d.ManagerFirstName} {d.ManagerLastName} ");
            foreach (var e in d.Employees)
            {
                sb
                    .AppendLine($"{e.FirstName} {e.LastName} - {e.JobTitle}");
            }
        }

        return sb.ToString().TrimEnd();
    }

    //Problem 11:
    public static string GetLatestProjects(SoftUniContext context)
    {
        StringBuilder sb = new StringBuilder();

        var projects = context.Projects
            .OrderByDescending(p => p.StartDate)
            .Take(10)
            .OrderBy(p => p.Name)
            .Select(p => new
            {
                p.Name,
                p.Description,
                p.StartDate
            })
            .ToArray();

        foreach (var p in projects)
        {
            sb
                .AppendLine(p.Name)
                .AppendLine(p.Description)
                .AppendLine($"{p.StartDate.ToString("M/d/yyyy h:mm:ss tt")}");
        }

        return sb.ToString().TrimEnd();
    }

    //Problem 12:
    public static string IncreaseSalaries(SoftUniContext context)
    {
        StringBuilder sb = new StringBuilder();
        string[] serachedDepartments = { "Engineering", "Tool Design", "Marketing", "Information Services"};

        var employees = context.Employees
            .Where(e => serachedDepartments.Contains(e.Department.Name))
            .Select(e => new
            {
                e.FirstName,
                e.LastName,
                Salary = e.Salary * 1.12m
            })
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToArray();

        context.SaveChanges();

        foreach (var e in employees)
        {
            sb
                .AppendLine($"{e.FirstName} {e.LastName} (${e.Salary:f2})");
        }

        return sb.ToString().TrimEnd();
    }

    //Problem 13:
    public static string GetEmployeesByFirstNameStartingWithSa(SoftUniContext context)
    {
        StringBuilder sb = new StringBuilder();
        var employees = context.Employees
            .Where(e => e.FirstName.StartsWith("Sa"))
            .Select(e => new
            {
                e.FirstName,
                e.LastName,
                e.JobTitle,
                e.Salary
            })
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToArray();

        foreach (var e in employees)
        {
            sb
                .AppendLine($"{e.FirstName} {e.LastName} - {e.JobTitle} - (${e.Salary:f2})");
        }
            

        return sb.ToString().TrimEnd();
    }
}
