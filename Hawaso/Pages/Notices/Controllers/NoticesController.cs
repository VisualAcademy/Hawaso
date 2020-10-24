using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NoticeApp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NoticeApp.Apis.Controllers
{
    [Authorize(Roles = "Administrators")] // 최고 관리자 그룹(역할)에 포함된 사용자만 공지사항 관리
    [Produces("application/json")]
    [Route("api/Notices")]
    [ApiController]
    public class NoticesController : ControllerBase
    {
        private readonly INoticeRepository _repository;
        private readonly ILogger _logger;

        public NoticesController(INoticeRepository repository, ILoggerFactory loggerFactory)
        {
            this._repository = repository;
            this._logger = loggerFactory.CreateLogger(nameof(NoticesController));
        }

        // 입력
        // POST api/Notices
        [HttpPost]
        public async Task<IActionResult> AddAsync([FromBody]Notice model)
        {
            // model.Id = 0
            var tmpModel = new Notice();
            tmpModel.Name = model.Name;
            tmpModel.Title = model.Title;
            tmpModel.Content = model.Content; 
            tmpModel.ParentId = model.ParentId;
            tmpModel.Created = DateTime.Now;

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            try
            {
                var newModel = await _repository.AddAsync(tmpModel);
                if (newModel == null)
                {
                    return BadRequest();
                }
                //return Ok(newModel); // 200 OK
                var uri = Url.Link("GetNoticeById", new { id = newModel.Id });
                return Created(uri, newModel); // 201 Created
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest();
            }
        }

        // 출력
        // GET api/Notices
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
                return Ok(models);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(); 
            }
        }

        // 상세
        // GET api/Notices/1
        [HttpGet("{id}", Name = "GetNoticeById")]
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

        // 수정
        // PUT api/Notices/1
        [HttpPut("{id}")]
        public async Task<IActionResult> EditAsync(int id, [FromBody]Notice model)
        {
            model.Id = id;

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            try
            {
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
        // DELETE api/Notices/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            try
            {
                var status = await _repository.DeleteAsync(id);
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
        // GET api/Notices/Page/0/10
        [HttpGet("Page/{pageIndex}/{pageSize}")]
        public async Task<IActionResult> GetAll(int pageIndex, int pageSize)
        {
            try
            {
                var resultsSet = await _repository.GetAllAsync(pageIndex, pageSize);

                // 응답 헤더에 총 레코드 수를 담아서 출력 
                Response.Headers.Add("X-TotalRecordCount", resultsSet.TotalRecords.ToString());
                Response.Headers.Add("Access-Control-Expose-Headers", "X-TotalRecordCount"); 

                return Ok(resultsSet.Records);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest();
            }
        }
    }
}
