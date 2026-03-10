using System.Security.Claims;
using AutoMapper;
using ECommerceSystem.Application.DTOs;
using ECommerceSystem.Application.Interface;
using ECommerceSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrdersController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateOrder(List<OrderItemDto> itemsDto)
        {
            var order = new Order
            {
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                OrderDate = DateTime.UtcNow
            };

            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.CompleteAsync();

            foreach (var itemDto in itemsDto)
            {
                var item = _mapper.Map<OrderItem>(itemDto);
                item.OrderId = order.Id;
                await _unitOfWork.OrderItems.AddAsync(item);
            }

            await _unitOfWork.CompleteAsync();

            order.TotalAmount = itemsDto.Sum(i => i.Quantity * i.UnitPrice);
            _unitOfWork.Orders.Update(order);
            await _unitOfWork.CompleteAsync();

            var orderDto = _mapper.Map<OrderDto>(order);
            return Ok(new { success = true, data = orderDto });
        }
    }
}