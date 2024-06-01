using System.ComponentModel.DataAnnotations;

namespace FindMyVolunteer.Data.DataTransfer {
  public class KYCVolunteerDTO {
    public int DocumentNumber { get; set; }
    public bool IsPassport { get; set; }
    [Required]
    [FileExtensions(Extensions = "jpg,jpeg,png", ErrorMessage = "Please upload a valid image file.")]
    public IFormFile DocumentPhoto { get; set; }
    public KYCVolunteerDTO() { }
    public KYCVolunteerDTO(int docNum, bool isPassport, IFormFile docPhoto) {
      DocumentNumber = docNum;
      IsPassport = isPassport;
      DocumentPhoto = docPhoto;
    }
  }
}
