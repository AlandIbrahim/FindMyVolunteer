using System.ComponentModel.DataAnnotations;

namespace FindMyVolunteer.Data.DataTransfer {
  public class KYCOrganizationDTO {
    public int BusinessLicense { get; set; }
    // a photo of the business license.
    //[Required]
    //[FileExtensions(Extensions = "jpg,jpeg,png", ErrorMessage = "Please upload a valid image file.")]
    //public IFormFile BusinessLicensePhoto { get; set; }
  }
}
