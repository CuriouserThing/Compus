namespace Compus;

public class Token
{
    private readonly string _value;

    public Token(string value)
    {
        _value = value;
    }

    public static implicit operator string(Token token)
    {
        return token._value;
    }

    public static implicit operator Token(string value)
    {
        return new(value);
    }
}
