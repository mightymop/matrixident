using System.ComponentModel.DataAnnotations;

namespace MatrixIdent.Models
{
    public class EmailValidationRequestItem : BaseValidationItem
    {
        [Required]
        public string client_secret { get; set; }

        [Required]
        [Key]
        public string email { get; set; }
        public string? next_link { get; set; }

        [Required]
        public int send_attempt { get; set; }

        public EmailValidationRequestItem(string client_secret, string email, string next_link, int send_attempt) : base()
        {
            this.client_secret = client_secret;
            this.email = email;
            this.next_link = next_link;
            this.send_attempt = send_attempt;
        }

        public override object toObject()
        {
            return new { client_secret = this.client_secret, email = this.email, next_link = this.next_link, send_attempt = this.send_attempt };
        }
    }
}
