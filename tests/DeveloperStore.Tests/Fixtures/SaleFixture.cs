using Bogus;
using DeveloperStore.Application.Contracts;
using DeveloperStore.Application.Sales;
using DeveloperStore.Application.Sales.Commands;

namespace DeveloperStore.Tests.Fixtures;

public static class SaleFixture
{
    public static CreateSaleCommand CreateValidCommand(int itemQuantity = 4)
    {
        var faker = new Faker("pt_BR");

        var saleNumber = $"S-{faker.Random.Number(100, 999)}";
        var customer = new ExternalIdentityDto($"cust-{faker.Random.Guid()}", faker.Company.CompanyName());
        var branch = new ExternalIdentityDto($"branch-{faker.Random.Guid()}", faker.Company.CompanyName());
        var product = new ExternalIdentityDto($"prod-{faker.Random.Guid()}", faker.Commerce.ProductName());

        return new CreateSaleCommand(
            saleNumber,
            DateTime.UtcNow,
            customer,
            branch,
            new[]
            {
                new SaleItemInput(product, itemQuantity, faker.Random.Decimal(1m, 200m))
            });
    }
}

