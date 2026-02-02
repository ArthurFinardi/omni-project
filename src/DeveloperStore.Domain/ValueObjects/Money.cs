namespace DeveloperStore.Domain.ValueObjects;

public readonly record struct Money(decimal Amount)
{
    public static Money Zero => new(0m);

    public static Money operator +(Money left, Money right) => new(left.Amount + right.Amount);
    public static Money operator *(Money left, decimal factor) => new(left.Amount * factor);
}
