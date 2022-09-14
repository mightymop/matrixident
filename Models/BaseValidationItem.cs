using System.ComponentModel.DataAnnotations;

namespace MatrixIdent.Models
{
    public class BaseValidationItem
    {
        [Required]
        public string client_secret { get; set; }

        public string? sid { get; set; }

        public string? token { get; set; }

        public bool? success { get; set; }

        public long? expire_after { get; set; }

        public string? mxid { get; set; }

        public BaseValidationItem()
        {
            success=false;
        }


        public virtual object toObject()
        {
            return new { client_secret = this.client_secret, sid = this.sid, token = this.token };
        }
    }
}
