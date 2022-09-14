namespace MatrixIdent.Models
{
    public class LookupRequest
    {
        public string[] addresses { get; set; }

        public string algorithm { get; set; }

        public string pepper { get; set; }
    }
}
