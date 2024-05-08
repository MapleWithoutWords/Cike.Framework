namespace Cike.Core.Exceptions;

public class FriendlyException : Exception
{
    public string Code { get; set; }
    public FriendlyException(string message, string code = "") : base(message)
    {
        this.Code = code;
    }
}
