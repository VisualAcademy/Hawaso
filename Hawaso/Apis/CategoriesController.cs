using DotNetSaleCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DotNetSaleCore.Apis.Controllers
{
    [ApiController]
    [Route("api/Categories")]
    public class CategoriesController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger _logger;

        public CategoriesController(ICategoryRepository categoryRepository, ILoggerFactory loggerFactory)
        {
            this._categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            this._logger = loggerFactory.CreateLogger(nameof(CategoriesController));
        }

        // 입력
        // POST api/Categories
        [HttpPost]
        public async Task<IActionResult> AddAsync([FromBody]Category category)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            try
            {
                var newCategory = await _categoryRepository.AddAsync(category);
                if (newCategory == null)
                {
                    return BadRequest();
                }
                var uri = Url.Link("GetCategoryById", new { id = newCategory.CategoryId });
                return Created(uri, newCategory); // 201 Created
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest();
            }
        }

        // 출력
        // GET api/Categories
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var customers = await _categoryRepository.GetAllAsync();
                return Ok(customers);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest();
            }
        }

        // 상세
        // GET api/Categories/1
        [HttpGet("{id}", Name = "GetCategoryById")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                return Ok(category);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest();
            }
        }

        // 수정
        // PUT api/Categories/1
        [HttpPut("{id}")]
        public async Task<IActionResult> EditAsync(int id, [FromBody]Category category)
        {
            category.CategoryId = id;

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            try
            {
                var status = await _categoryRepository.EditAsync(category);
                if (!status)
                {
                    return BadRequest();
                }
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest();
            }
        }

        // 삭제
        // DELETE api/Categories/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            try
            {
                var status = await _categoryRepository.DeleteAsync(id);
                if (!status)
                {
                    return BadRequest();
                }
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest();
            }
        }

        // 페이징
        // GET api/Categories/Page/0/10
        [HttpGet("Page/{pageIndex}/{pageSize}")]
        public async Task<IActionResult> GetAll(int pageIndex, int pageSize)
        {
            try
            {
                var categorySet = await _categoryRepository.GetAllAsync(pageIndex, pageSize);

                // 응답 헤더에 총 레코드 수를 담아서 출력 
                Response.Headers.Add("X-TotalRecordCount", categorySet.TotalRecords.ToString());
                Response.Headers.Add("Access-Control-Expose-Headers", "X-TotalRecordCount"); // 코드 추가

                return Ok(categorySet.Records);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest();
            }
        }
    }
}
