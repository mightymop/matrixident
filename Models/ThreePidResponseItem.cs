using System.ComponentModel.DataAnnotations;

namespace MatrixIdent.Models
{
    public class ThreePidResponseItem
    {
        [Required]
        [Key]
        public string address { get; set; }
        [Required]
        public string medium { get; set; }
        [Required]
        public string mxid { get; set; }
        [Required]
        public long not_after { get; set; }
        [Required]
        public long not_before { get; set; }
        [Required]
        public long ts { get; set; }

        public ThreePidResponseItem()
        {
           
        }
    }
}
