using Hawaso.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MemoApp.Apis.Controllers
{
    [Authorize]
    //[ApiVersion("1.0")] //[Route("api/v{v:apiVersion}/Memos")]
    [ApiController] // @RestController 
    [Route("api/[controller]")] // [Route("api/Memos")] // @RequestMapping
    [Produces("application/json")]
    public class MemosController : ControllerBase
    {
        private readonly IMemoRepository _repository;
        private readonly ILogger _logger;

        public MemosController(IMemoRepository repository, ILoggerFactory loggerFactory)
        {
            this._repository = repository ?? throw new ArgumentNullException(nameof(MemosController));
            this._logger = loggerFactory.CreateLogger(nameof(MemosController));
        }

        #region 시험
        [HttpGet("[action]")] // api/Memos/Test
        public IEnumerable<Memo> Test() => Enumerable.Empty<Memo>();
        #endregion

        #region 입력
        // 입력
        // POST api/Memos
        [HttpPost] // @PostMapping
        public async Task<IActionResult> AddAsync([FromBody] Memo dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            // <>
            var temp = new Memo();
            temp.Name = dto.Name;
            temp.Title = dto.Title;
            temp.Content = dto.Content;
            temp.Created = DateTime.UtcNow;
            // --TODO-- 
            // </>

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
                    return CreatedAtRoute("GetMemoById", new { id = model.Id }, model); // Status: 201 Created
                }
                else if (DateTime.Now.Second % 2 == 0)
                {
                    var uri = Url.Link("GetMemoById", new { id = model.Id });
                    return Created(uri, model); // 201 Created
                }
                else
                {
                    // GetById 액션 이름 사용해서 입력된 데이터 반환 
                    return CreatedAtAction(nameof(GetMemoById), new { id = model.Id }, model);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest();
            }
        }
        #endregion

        #region 출력
        // 출력
        // GET api/Memos
        [HttpGet] // [HttpGet("[action]")] // @GetMapping
        public async ValueTask<ActionResult<IOrderedEnumerable<Memo>>> GetAll()
        {
            try
            {
                var models = await _repository.GetAllAsync();

                if (models == null)
                    return NotFound(); // 학습용 코드 

                if (!models.Any())
                {
                    return new NoContentResult(); // 참고용 코드
                }
                return new JsonResult(models); //return Ok(models); // 200 OK
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
        // GET api/Memos/123
        [HttpGet("{id:int}", Name = nameof(GetMemoById))] // Name 속성으로 RouteName 설정
        public async Task<IActionResult> GetMemoById([FromRoute] int id)
        {
            try
            {
                var model = await _repository.GetByIdAsync(id);
                if (model == null)
                {
                    //return new NoContentResult(); // 204 No Content
                    return NotFound();
                }
                return Ok(model);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest();
            }
        }
        #endregion

        #region 수정
        // 수정
        // PUT api/Memos/123
        [HttpPut("{id}")] // @PutMapping
        public async Task<IActionResult> UpdateAsync([FromRoute] int? id, [FromBody] Memo dto)
        {
            if (id is null)
            {
                return NotFound();
            }

            if (dto == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            // <>
            var origin = await _repository.GetByIdAsync(id ?? default);
            if (origin != null)
            {
                origin.Name = dto.Name;
                origin.Title = dto.Title;
                origin.Content = dto.Content;
                // --TODO--
            }
            // </>

            try
            {
                origin.Id = id ?? default;
                var status = await _repository.UpdateAsync(origin);
                if (!status)
                {
                    return BadRequest();
                }

                // 204 No Content
                return NoContent(); // 이미 전송된 정보에 모든 값 가지고 있기에...
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest();
            }
        }
        #endregion

        #region 삭제
        // 삭제
        // DELETE api/Memos/1
        [HttpDelete("{id:int}")] // @DeleteMapping 
        public async Task<IActionResult> DeleteAsync(int id)
        {
            try
            {
                var status = await _repository.DeleteAsync(id);
                if (!status)
                {
                    return BadRequest();
                }

                return NoContent(); // 204 NoContent
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest("삭제할 수 없습니다.");
            }
        }
        #endregion

        #region 페이징
        // 페이징
        // GET api/Memos/Page/1/10
        [HttpGet("Page/{pageNumber:int}/{pageSize:int}")]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                // 페이지 번호는 1, 2, 3 사용, 리포지토리에서는 0, 1, 2 사용
                int pageIndex = (pageNumber > 0) ? pageNumber - 1 : 0;

                var resultSet = await _repository.GetAllAsync(pageIndex, pageSize);
                if (resultSet.Records == null)
                {
                    return NotFound($"아무런 데이터가 없습니다.");
                }

                // 응답 헤더에 총 레코드 수를 담아서 출력
                Response.Headers.Add("X-TotalRecordCount", resultSet.TotalRecords.ToString());
                Response.Headers.Add("Access-Control-Expose-Headers", "X-TotalRecordCount");

                //return Ok(resultSet.Records);
                var ʘ‿ʘ = resultSet.Records; // 재미를 위해서 
                return Ok(ʘ‿ʘ); // Look of Approval
            }
            catch (Exception ಠ_ಠ) // Look of Disapproval
            {
                _logger?.LogError($"ERROR({nameof(GetAll)}): {ಠ_ಠ.Message}");
                return BadRequest();
            }
        }
        #endregion
    }

    //[Authorize]
    ////[ApiVersion("2.0")] //[Route("api/v{v:apiVersion}/Memos")]
    //[ApiController] // @RestController 
    //[Route("api/[controller]")] // [Route("api/Memos")] // @RequestMapping
    //[Produces("application/json")]
    //public class MemosV2_0Controller : ControllerBase
    //{
    //    private readonly IMemoRepository _repository;
    //    private readonly ILogger _logger;

    //    public MemosV2_0Controller(IMemoRepository repository, ILoggerFactory loggerFactory)
    //    {
    //        this._repository = repository ?? throw new ArgumentNullException(nameof(MemosController));
    //        this._logger = loggerFactory.CreateLogger(nameof(MemosController));
    //    }

    //    #region 시험
    //    [HttpGet("[action]")] // api/Memos/Test
    //    public IEnumerable<Memo> Test() => Enumerable.Empty<Memo>();
    //    #endregion

    //    #region 입력
    //    // 입력
    //    // POST api/Memos
    //    [HttpPost] // @PostMapping
    //    public async Task<IActionResult> AddAsync([FromBody] Memo dto)
    //    {
    //        if (!ModelState.IsValid)
    //        {
    //            return BadRequest();
    //        }

    //        // <>
    //        var temp = new Memo();
    //        temp.Name = dto.Name;
    //        temp.Title = dto.Title;
    //        temp.Content = dto.Content;
    //        temp.Created = DateTime.Now;
    //        // --TODO-- 
    //        // </>

    //        try
    //        {
    //            var model = await _repository.AddAsync(temp);
    //            if (model == null)
    //            {
    //                return BadRequest();
    //            }

    //            //[!] 다음 항목 중 원하는 방식 사용
    //            if (DateTime.Now.Second % 60 == 0)
    //            {
    //                return Ok(model); // 200 OK
    //            }
    //            else if (DateTime.Now.Second % 3 == 0)
    //            {
    //                return CreatedAtRoute("GetMemoById", new { id = model.Id }, model); // Status: 201 Created
    //            }
    //            else if (DateTime.Now.Second % 2 == 0)
    //            {
    //                var uri = Url.Link("GetMemoById", new { id = model.Id });
    //                return Created(uri, model); // 201 Created
    //            }
    //            else
    //            {
    //                // GetById 액션 이름 사용해서 입력된 데이터 반환 
    //                return CreatedAtAction(nameof(GetMemoById), new { id = model.Id }, model);
    //            }
    //        }
    //        catch (Exception e)
    //        {
    //            _logger.LogError(e.Message);
    //            return BadRequest();
    //        }
    //    }
    //    #endregion

    //    #region 출력
    //    // 출력
    //    // GET api/Memos
    //    [HttpGet] // [HttpGet("[action]")] // @GetMapping
    //    public async Task<IActionResult> GetAll()
    //    {
    //        try
    //        {
    //            var models = await _repository.GetAllAsync();
    //            if (!models.Any())
    //            {
    //                return new NoContentResult(); // 참고용 코드
    //            }
    //            return Ok(models.OrderBy(m => m.Id)); // 200 OK
    //        }
    //        catch (Exception e)
    //        {
    //            _logger.LogError(e.Message);
    //            return BadRequest();
    //        }
    //    }
    //    #endregion

    //    #region 상세
    //    // 상세
    //    // GET api/Memos/123
    //    [HttpGet("{id:int}", Name = "GetMemoById")] // Name 속성으로 RouteName 설정: nameof 연산자 사용 권장
    //    public async Task<IActionResult> GetMemoById([FromRoute] int id)
    //    {
    //        try
    //        {
    //            var model = await _repository.GetByIdAsync(id);
    //            if (model == null)
    //            {
    //                //return new NoContentResult(); // 204 No Content
    //                return NotFound();
    //            }
    //            return Ok(model);
    //        }
    //        catch (Exception e)
    //        {
    //            _logger.LogError(e.Message);
    //            return BadRequest();
    //        }
    //    }
    //    #endregion

    //    #region 수정
    //    // 수정
    //    // PUT api/Memos/123
    //    [HttpPut("{id}")] // @PutMapping 
    //    public async Task<IActionResult> UpdateAsync([FromRoute] int? id, [FromBody] Memo dto)
    //    {
    //        if (id is null)
    //        {
    //            return NotFound(); 
    //        }

    //        if (dto == null)
    //        {
    //            return BadRequest();
    //        }

    //        if (!ModelState.IsValid)
    //        {
    //            return BadRequest();
    //        }

    //        // <>
    //        var origin = await _repository.GetByIdAsync(id ?? default);
    //        if (origin != null)
    //        {
    //            origin.Name = dto.Name;
    //            origin.Title = dto.Title;
    //            origin.Content = dto.Content;
    //            // --TODO--
    //        }
    //        // </>

    //        try
    //        {
    //            origin.Id = id ?? default;
    //            var status = await _repository.UpdateAsync(origin);
    //            if (!status)
    //            {
    //                return BadRequest();
    //            }

    //            // 204 No Content
    //            return NoContent(); // 이미 전송된 정보에 모든 값 가지고 있기에...
    //        }
    //        catch (Exception e)
    //        {
    //            _logger.LogError(e.Message);
    //            return BadRequest();
    //        }
    //    }
    //    #endregion

    //    #region 삭제
    //    // 삭제
    //    // DELETE api/Memos/1
    //    [HttpDelete("{id:int}")] // @DeleteMapping 
    //    public async Task<IActionResult> DeleteAsync(int id)
    //    {
    //        try
    //        {
    //            var status = await _repository.DeleteAsync(id);
    //            if (!status)
    //            {
    //                return BadRequest();
    //            }

    //            return NoContent(); // 204 NoContent
    //        }
    //        catch (Exception e)
    //        {
    //            _logger.LogError(e.Message);
    //            return BadRequest("삭제할 수 없습니다.");
    //        }
    //    }
    //    #endregion

    //    #region 페이징
    //    // 페이징
    //    // GET api/Memos/Page/1/10
    //    [HttpGet("Page/{pageNumber:int}/{pageSize:int}")]
    //    public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
    //    {
    //        try
    //        {
    //            // 페이지 번호는 1, 2, 3 사용, 리포지토리에서는 0, 1, 2 사용
    //            int pageIndex = (pageNumber > 0) ? pageNumber - 1 : 0;

    //            var resultSet = await _repository.GetAllAsync(pageIndex, pageSize);
    //            if (resultSet.Records == null)
    //            {
    //                return NotFound($"아무런 데이터가 없습니다.");
    //            }

    //            // 응답 헤더에 총 레코드 수를 담아서 출력
    //            Response.Headers.Add("X-TotalRecordCount", resultSet.TotalRecords.ToString());
    //            Response.Headers.Add("Access-Control-Expose-Headers", "X-TotalRecordCount");

    //            return Ok(resultSet.Records);
    //        }
    //        catch (Exception e)
    //        {
    //            _logger.LogError(e.Message);
    //            return BadRequest();
    //        }
    //    }
    //    #endregion
    //}
}
