using System.Net;

namespace Domain.Exceptions;
public class CardRequestException : DomainException
{
    public CardRequestException(string message)
        : base(message, null, HttpStatusCode.Forbidden)
    {
    }
}