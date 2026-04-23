namespace HotelBooking.Application.Common.Exceptions;

public class NotFoundException(string message) : Exception(message)
{
    public NotFoundException(string name, object key)
        : this($"{name} with id '{key}' was not found.") { }
}
