using System.ComponentModel.DataAnnotations;

namespace MatrixIdent.Models
{
    public class InvitationRequestItem
    {
        [Required]
        [Key]
        public string address { get; set; }

        [Required]
        public string medium { get; set; }

        public string? room_alias { get; set; }

        public string? room_avatar_url { get; set; }

        [Required]
        public string room_id { get; set; }

        public string? room_join_rules { get; set; }

        public string? room_name { get; set; }

        public string? room_type { get; set; }

        [Required]
        public string sender { get; set; }

        public string? sender_avatar_url { get; set; }

        public string? sender_display_name { get; set; }

        public string? token { get; set; }

        public string? key { get; set; }

    }
}
