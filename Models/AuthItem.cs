using System.ComponentModel.DataAnnotations;

namespace MatrixIdent.Models
{
    public class AuthItem
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string matrix_server_name { get; set; }
        public long expires_in { get; set; }

        public string? user_id { get; set; }
                
        [Key]
        public string? token { get; set; }
    }
}
