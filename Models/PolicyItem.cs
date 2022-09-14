namespace MatrixIdent.Models
{
    public class PolicyItem
    {
        public string language { get; set; }
        public string name { get; set; }
        public string url { get; set; }

        public PolicyItem(string language, string name, string url)
        {
            this.language = language;
            this.name = name;
            this.url = url;
        }

        public object toObject()
        {
            return new { name = this.name, url = this.url };
        }
    }
}
