using System.ComponentModel.DataAnnotations;

namespace SMITFeeReimbursementSystem.ViewModels;

public class AssignStudentsViewModel
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;

    [Display(Name = "Select Students")]
    public List<string> SelectedStudentIds { get; set; } = [];

    public List<StudentOption> AvailableStudents { get; set; } = [];
}

public class StudentOption
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsEnrolled { get; set; }
}
