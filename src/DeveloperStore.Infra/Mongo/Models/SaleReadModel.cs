namespace DeveloperStore.Infra.Mongo.Models;

public sealed class SaleReadModel
{
    public Guid Id { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public string CustomerExternalId { get; set; } = string.Empty;
    public string CustomerDescription { get; set; } = string.Empty;
    public string BranchExternalId { get; set; } = string.Empty;
    public string BranchDescription { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public bool IsCancelled { get; set; }
    public List<SaleItemReadModel> Items { get; set; } = new();
}

public sealed class SaleItemReadModel
{
    public Guid Id { get; set; }
    public string ProductExternalId { get; set; } = string.Empty;
    public string ProductDescription { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountRate { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public bool IsCancelled { get; set; }
}
