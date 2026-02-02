using DeveloperStore.Domain.Exceptions;

namespace DeveloperStore.Domain.ValueObjects;

public readonly record struct Discount(decimal Rate)
{
    public static Discount None => new(0m);

    public static Discount FromRate(decimal rate)
    {
        if (rate < 0m || rate > 1m)
        {
            throw new DomainException("Discount rate must be between 0 and 1.");
        }

        return new Discount(rate);
    }
}
