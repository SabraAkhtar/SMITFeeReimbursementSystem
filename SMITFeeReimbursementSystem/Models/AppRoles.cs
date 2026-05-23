namespace SMITFeeReimbursementSystem.Models;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Teacher = "Teacher";
    public const string Student = "Student";

    public static readonly string[] All = [Admin, Teacher, Student];

    /// <summary>Roles available during public self-registration (Admin is assigned to the first user).</summary>
    public static readonly string[] Registerable = [Teacher, Student];
}
