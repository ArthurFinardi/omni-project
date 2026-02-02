using DeveloperStore.Domain.Exceptions;

namespace DeveloperStore.Domain.ValueObjects;

public readonly record struct Quantity(int Value)
{
    public static Quantity From(int value)
    {
        if (value <= 0)
        {
            throw new DomainException("Quantity must be greater than zero.");
        }

        return new Quantity(value);
    }
}
