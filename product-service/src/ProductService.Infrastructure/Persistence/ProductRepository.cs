using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Infrastructure.Persistence
{
    public class ProductRepository : IProductRepository
    {
        private readonly IMongoCollection<Product> _collection;

        public ProductRepository(IOptions<MongoDbSettings> options)
        {
            var client = new MongoClient(options.Value.ConnectionString);
            var db = client.GetDatabase(options.Value.DatabaseName);
            _collection = db.GetCollection<Product>(options.Value.ProductsCollection);
        }

        public async Task<List<Product>> GetAllAsync(CancellationToken ct = default) =>
            await _collection.Find(_ => true).ToListAsync(ct);

        public async Task<Product?> GetByIdAsync(string id, CancellationToken ct = default) =>
            await _collection.Find(x => x.Id == id).FirstOrDefaultAsync(ct);

        public async Task CreateAsync(Product product, CancellationToken ct = default) =>
            await _collection.InsertOneAsync(product, cancellationToken: ct);

        public async Task<bool> UpdateStockAsync(string productId, int delta, CancellationToken ct = default)
        {
            var update = Builders<Product>.Update.Inc(p => p.Stock, delta);
            var result = await _collection.UpdateOneAsync(p => p.Id == productId, update, cancellationToken: ct);
            return result.ModifiedCount > 0;
        }
    }
}
