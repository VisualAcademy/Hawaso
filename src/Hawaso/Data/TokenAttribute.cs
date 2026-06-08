namespace Hawaso.Data;

public class TokenAttribute : Attribute
{
    public TokenAttribute()
    {
    }

    public TokenAttribute(string token)
    {
        Token = token;
    }

    public string Token { get; set; } = string.Empty;

    public string Prefix { get; set; } = string.Empty;
}