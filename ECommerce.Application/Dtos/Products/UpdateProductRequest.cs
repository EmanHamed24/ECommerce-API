namespace ECommerce.Application.DTOs.Products;

public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public string SKU { get; set; } = string.Empty;

    public int CategoryId { get; set; }
}
