using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace SMITFeeReimbursementSystem.Models;

public class ApplicationUser : IdentityUser
{
    [StringLength(100)]
    [Display(Name = "Full Name")]
    public string? FullName { get; set; }

    [StringLength(50)]
    [Display(Name = "Roll Number")]
    public string? RollNumber { get; set; }
}
