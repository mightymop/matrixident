using System.ComponentModel.DataAnnotations;

namespace MatrixIdent.Models
{
    public class ThreePidItem
    {
        [Required]
        public string address { get; set; }
        [Required]
        public string medium { get; set; }
      
        public ThreePidItem()
        {
           
        }

    }
}
