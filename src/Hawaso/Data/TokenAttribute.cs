namespace Hawaso.Data
{
    public class TokenAttribute : Attribute
    {
        public TokenAttribute()
        {
        }

        public TokenAttribute(string token)
        {
            this.Token = token;
        }

        public string Token { get; set; }

        public string Prefix { get; set; }
    }
}
