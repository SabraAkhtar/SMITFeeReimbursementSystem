using System.ComponentModel.DataAnnotations;

namespace SMITFeeReimbursementSystem.ViewModels;

public class AdminEditStudentViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [StringLength(50)]
    [Display(Name = "Roll Number")]
    public string? RollNumber { get; set; }

    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Phone Number")]
    public string? Phone { get; set; }
}
