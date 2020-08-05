using DotNetSaleCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetSaleCore.Apis.Controllers
{
    [Authorize(Roles = "Administrators")] // 관리자만 접근하도록 설정
    //[Produces("application/json")]
    [ApiController]
    [Route("api/Customers")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerRepository _repository;
        private readonly ILogger _logger;

        public CustomersController(
            ICustomerRepository customerRepository,
            ILoggerFactory loggerFactory)
        {
            this._repository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            this._logger = loggerFactory.CreateLogger(nameof(CustomersController));
        }

        // 입력
        // POST api/Customers
        [HttpPost]
        //public async Task<IActionResult> AddAsync([FromBody]CustomerViewModel model)
        public async Task<ActionResult<Customer>> AddAsync([FromBody]CustomerViewModel dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(); // Status: 400 Bad Request 
            }

            // model.CustomerId = 0
            var temp = new Customer();
            temp.CustomerName = dto.CustomerName;
            temp.EmailAddress = dto.EmailAddress;
            temp.Created = DateTime.Now; //model.Created = DateTime.Now;

            try
            {
                var model = await _repository.AddAsync(temp);
                if (model == null)
                {
                    return BadRequest();
                }

                //[!] 다음 항목 중 원하는 방식 사용
                if (DateTime.Now.Second % 60 == 0) 
                {
                    return Ok(model); // 200 OK
                }
                else if (DateTime.Now.Second % 3 == 0)
                {
                    return CreatedAtRoute("GetCustomerById", new { id = model.CustomerId }, model); // Status: 201 Created
                }
                else if (DateTime.Now.Second % 5 == 0)
                {
                    var uri = Url.Link("GetCustomerById", new { id = model.CustomerId });
                    return Created(uri, model); // 201 Created
                }
                else
                {
                    // GetById 액션 이름 사용해서 입력된 데이터 반환 
                    return CreatedAtAction("GetCustomerById", new { id = model.CustomerId }, model);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest();
            }
        }

        #region 출력
        // 출력
        // GET api/Customers
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var models = await _repository.GetAllAsync();
                if (!models.Any())
                {
                    return new NoContentResult(); // 참고용 코드
                }
                return Ok(models); // 200 OK 
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest();
            }
        }
        #endregion

        #region 상세
        // 상세
        // GET api/Customers/1
        [HttpGet("{id}", Name = "GetCustomerById")]
        public async Task<IActionResult> GetById([FromRoute]int id)
        {
            try
            {
                var model = await _repository.GetByIdAsync(id);
                return Ok(model);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest();
            }
        } 
        #endregion

        // 수정
        // PUT api/Customers/1
        [HttpPut("{id}")]
        public async Task<IActionResult> EditAsync(int id, [FromBody]Customer model)
        {
            if (model == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            try
            {
                model.CustomerId = id;

                var status = await _repository.EditAsync(model);
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
        // DELETE api/Customers/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            try
            {
                var status = await _repository.DeleteAsync(id);
                if (!status)
                {
                    return BadRequest();
                }
                //return Ok();
                return NoContent(); // 204 NoContent
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest();
            }
        }

        // 페이징
        // GET api/Customers/Page/0/10
        [HttpGet("Page/{pageIndex}/{pageSize}")]
        public async Task<IActionResult> GetAll(int pageIndex, int pageSize)
        {
            try
            {
                var customersSet = await _repository.GetAllAsync(pageIndex, pageSize);

                // 응답 헤더에 총 레코드 수를 담아서 출력 
                Response.Headers.Add("X-TotalRecordCount", customersSet.TotalRecords.ToString());
                Response.Headers.Add("Access-Control-Expose-Headers", "X-TotalRecordCount"); // 코드 추가

                return Ok(customersSet.Records);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest();
            }
        }
    }
}
