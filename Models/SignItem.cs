using System.ComponentModel.DataAnnotations;

namespace MatrixIdent.Models
{
    public class SignItem
    {
        [Key]
        public string mxid { get; set; }
        public string private_key { get; set; }
        public string token { get; set; }
    }
}
