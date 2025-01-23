using System.ComponentModel.DataAnnotations;

namespace DonationBox.BlazorWebAssembly.Models;

public class DonationModel
{
    public FileUploadInfo File { get; set; }

    public string CompanyId { get; set; }
    public string CompanyName { get; set; }

    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    [Required]
    public string Phone { get; set; }

    [Required]
    public string Email { get; set; }
    public string StreetAddress { get; set; }
    public string StreetAddress2 { get; set; }
    public string City { get; set; }
    public string Region { get; set; }
    public string PostalCode { get; set; }

    [Required]
    public string Amount { get; set; }
    public string Message { get; set; }
}
