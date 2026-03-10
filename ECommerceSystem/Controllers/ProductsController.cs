using AutoMapper;
using ECommerceSystem.Application.DTOs;
using ECommerceSystem.Application.Interface;
using ECommerceSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ProductsController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pagesize = 10, [FromQuery] string? search = null)
        {
            var products = await _unitOfWork.Products.GetAllAsync();
            if (!string.IsNullOrEmpty(search))
                products = products.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase));

            var pagedProducts = products.Skip((page - 1) * pagesize).Take(pagesize);

            var productDtos = _mapper.Map<IEnumerable<Product>, IEnumerable<ProductDto>>(pagedProducts);
            return Ok(new { success = true, data = productDtos });


        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                return NotFound(new { success = false, message = "Product not found" });
            var productDto = _mapper.Map<Product, ProductDto>(product);
            return Ok(new { success = true, data = productDto });
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Product product)
        {
            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.CompleteAsync();
            var productDto = _mapper.Map<Product, ProductDto>(product);
            return Ok(new { success = true, data = productDto });
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, Product product)
        {
            var existingProduct = await _unitOfWork.Products.GetByIdAsync(id);

            if (existingProduct == null)
                return NotFound(new { success = false, message = "Product not found" });
            //map
           _mapper.Map(product, existingProduct);

            _unitOfWork.Products.Update(existingProduct);
            await _unitOfWork.CompleteAsync();

            var updatedProductDto = _mapper.Map<Product, ProductDto>(existingProduct);

            return Ok(new { success = true, data = updatedProductDto });

        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var existingProduct = await _unitOfWork.Products.GetByIdAsync(id);
            if (existingProduct == null)
                return NotFound(new { success = false, message = "Product not found" });

            _unitOfWork.Products.Delete(existingProduct);
            await _unitOfWork.CompleteAsync();

            return Ok(new { success = true, message = "Product deleted successfully" });











        }
    }
}