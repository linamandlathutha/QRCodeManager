using System.ComponentModel.DataAnnotations;

namespace QRCodeManager.Models
{
    public class QRCodeModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }  // Link to the user

        [Required]
        public string Data { get; set; }    // Text or URL in the QR code
        public string ImageUrl { get; set; } // Path to the generated image

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
