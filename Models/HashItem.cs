using System.ComponentModel.DataAnnotations;

namespace MatrixIdent.Models
{
    public class HashItem
    {
        public string lookup_pepper { get; set; }
  
        [Key]
        public string token { get; set; }

    }
}
