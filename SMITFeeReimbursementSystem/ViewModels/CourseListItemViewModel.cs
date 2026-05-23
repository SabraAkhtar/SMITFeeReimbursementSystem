namespace SMITFeeReimbursementSystem.ViewModels;

public class CourseListItemViewModel
{
    public int Id { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public decimal FeeAmount { get; set; }
    public string Duration { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int EnrolledCount { get; set; }
}
