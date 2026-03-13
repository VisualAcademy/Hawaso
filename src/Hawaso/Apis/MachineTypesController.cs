using MachineTypeApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace Hawaso.Apis;

[ApiController]
[Route("/api/[controller]")]
[Produces("application/json")]
public class MachineTypesController : ControllerBase
{
    private readonly IMachineTypeRepository _repository;
    private readonly ILogger _logger;

    public MachineTypesController(IMachineTypeRepository repository, ILoggerFactory loggerFactory)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = loggerFactory.CreateLogger(nameof(MachineTypesController));
    }

    #region 시험
    [HttpGet("Test")]
    public IEnumerable<MachineType> Get() => Enumerable.Empty<MachineType>();
    #endregion

    #region 입력
    [HttpPost]
    public async Task<IActionResult> AddAsync([FromBody] MachineType dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var temp = new MachineType
        {
            Name = dto.Name
        };

        try
        {
            var model = await _repository.AddMachineTypeAsync(temp);
            if (model == null)
            {
                return BadRequest();
            }

            if (DateTime.Now.Second % 60 == 0)
            {
                return Ok(model);
            }
            else if (DateTime.Now.Second % 3 == 0)
            {
                return CreatedAtRoute(nameof(GetMachineTypeById), new { id = model.Id }, model);
            }
            else if (DateTime.Now.Second % 2 == 0)
            {
                var uri = Url.Link(nameof(GetMachineTypeById), new { id = model.Id });
                return Created(uri!, model);
            }
            else
            {
                return CreatedAtAction(nameof(GetMachineTypeById), new { id = model.Id }, model);
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
    [HttpGet]
    public async ValueTask<ActionResult<IEnumerable<MachineType>>> GetAll()
    {
        try
        {
            var models = await _repository.GetMachineTypesAsync();
            if (!models.Any())
            {
                return new NoContentResult();
            }

            return new JsonResult(models);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return BadRequest();
        }
    }
    #endregion

    #region 상세
    [HttpGet("{id:int}", Name = nameof(GetMachineTypeById))]
    public async Task<IActionResult> GetMachineTypeById([FromRoute] int id)
    {
        try
        {
            var model = await _repository.GetMachineTypeAsync(id);
            if (model == null)
            {
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
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync([FromRoute] int? id, [FromBody] MachineType dto)
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

        var origin = await _repository.GetMachineTypeAsync(id.Value);
        if (origin == null)
        {
            return NotFound();
        }

        origin.Id = id.Value;
        origin.Name = dto.Name;
        // origin.Title = dto.Title;
        // origin.Content = dto.Content;
        // --TODO--

        try
        {
            var status = await _repository.EditMachineTypeAsync(origin);

            // if (!status)
            // {
            //     return BadRequest();
            // }

            return NoContent();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return BadRequest();
        }
    }
    #endregion

    #region 삭제
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        try
        {
            await _repository.DeleteMachineTypeAsync(id);
            return NoContent();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return BadRequest("삭제할 수 없습니다.");
        }
    }
    #endregion

    #region 검색
    [HttpGet("Page/{pageNumber:int}/{pageSize:int}")]
    public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            int pageIndex = pageNumber > 0 ? pageNumber - 1 : 0;

            var resultSet = await _repository.GetAllAsync(pageIndex, pageSize);
            if (resultSet.Items == null)
            {
                return NotFound("아무런 데이터가 없습니다.");
            }

            SetResponseHeader("X-TotalRecordCount", resultSet.TotalCount.ToString());
            SetResponseHeader("Access-Control-Expose-Headers", "X-TotalRecordCount");

            var ʘ‿ʘ = resultSet.Items;
            return Ok(ʘ‿ʘ);
        }
        catch (Exception ಠ_ಠ)
        {
            _logger.LogError($"ERROR({nameof(GetAll)}): {ಠ_ಠ.Message}");
            return BadRequest();
        }
    }

    private void SetResponseHeader(string key, string value)
    {
        Response.Headers[key] = value;
    }
    #endregion
}
