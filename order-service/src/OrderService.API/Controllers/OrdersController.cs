using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;

namespace OrderService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;

        public OrdersController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _orderRepository.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            return order is not null ? Ok(order) : NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Order order)
        {
            await _orderRepository.AddAsync(order);
            return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
        }
    }
}
