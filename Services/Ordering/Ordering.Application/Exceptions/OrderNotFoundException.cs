namespace Ordering.Application.Exceptions;

public class OrderNotFoundException(string name, Object key)
    : ApplicationException($"Entity \"{name}\" ({key}) was not found.");