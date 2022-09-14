using System.ComponentModel.DataAnnotations;

namespace MatrixIdent.Models
{
    public class SearchRequestItem
    {
        [Required]
        public string search_term { get; set; }
        public int? limit { get; set; }
    }
}
