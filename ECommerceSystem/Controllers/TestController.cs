using ECommerceSystem.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public TestController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [Authorize(Roles ="Admin")]
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            return Ok(categories);
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory()
        {
            var category = new Domain.Entities.Category
            {
                Name = "Electronics"
            };
            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.CompleteAsync();
            return Ok("Category added successfully");

        }

    }
}
