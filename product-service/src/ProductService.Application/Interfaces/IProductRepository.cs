using ProductService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Interfaces
{
    public interface IProductRepository
    {
        Task<List<Product>> GetAllAsync(CancellationToken ct = default);
        Task<Product?> GetByIdAsync(string id, CancellationToken ct = default);
        Task CreateAsync(Product product, CancellationToken ct = default);
        Task<bool> UpdateStockAsync(string productId, int delta, CancellationToken ct = default);
    }
}
