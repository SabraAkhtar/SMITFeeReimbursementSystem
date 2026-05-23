namespace SMITFeeReimbursementSystem.ViewModels;

public class StudentListItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string RollNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int EnrolledCourses { get; set; }
}
