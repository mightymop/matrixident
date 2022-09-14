using System.ComponentModel.DataAnnotations;

namespace MatrixIdent.Models
{
    public class Key
    {
        public string public_key { get; set; }
        public string private_key { get; set; }

        [Key]
        public string identifier { get; set; }

        public long expiration_timestamp { get; set; }
    }
}
