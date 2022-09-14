using System.ComponentModel.DataAnnotations;

namespace MatrixIdent.Models
{
    public class ThreePidRequestItem
    {
        [Required]
        public string client_secret { get; set; }

        public string? mxid { get; set; }
        [Required]
        public string sid { get; set; }
      
        public ThreePidItem? threepid { get; set; }
     
        public ThreePidRequestItem()
        {
           
        }

    }
}
