using System.ComponentModel.DataAnnotations;

namespace MatrixIdent.Models
{
    public class MsisdnValidationRequestItem : BaseValidationItem
    {
        [Required]
        public string client_secret { get; set; }

        [Required]
        [Key]
        public string phone_number { get; set; }

        public string country { get; set; }
        public string? next_link { get; set; }

        [Required]
        public int send_attempt { get; set; }

        public MsisdnValidationRequestItem(string client_secret, string phone_number, string next_link, int send_attempt, string country) : base()
        {
            this.client_secret = client_secret;
            this.phone_number = phone_number;
            this.next_link = next_link;
            this.send_attempt = send_attempt;
            this.country = country;
        }

        public override object toObject()
        {
            return new { client_secret = this.client_secret, phone_number = this.phone_number, next_link = this.next_link, send_attempt = this.send_attempt };
        }
    }
}
