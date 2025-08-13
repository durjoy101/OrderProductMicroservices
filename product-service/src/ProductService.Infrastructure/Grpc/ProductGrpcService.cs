using ProductService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;

namespace ProductService.Infrastructure.Grpc
{
    public class ProductGrpcService : ProductGrpc.ProductGrpcBase
    {
        private readonly IProductRepository _repo;

        public ProductGrpcService(IProductRepository repo)
        {
            _repo = repo;
        }

        public override async Task<StockReply> CheckProductStock(StockRequest request, ServerCallContext context)
        {
            var product = await _repo.GetByIdAsync(request.ProductId, context.CancellationToken);
            var ok = product is not null && product.Stock >= request.Quantity;
            var remaining = product?.Stock ?? 0;
            return new StockReply { IsAvailable = ok, Remaining = remaining };
        }
    }
}
