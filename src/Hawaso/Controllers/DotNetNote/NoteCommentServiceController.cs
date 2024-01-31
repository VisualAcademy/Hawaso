using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Hawaso.Models.Notes;

namespace Hawaso.Controllers;

[Route("api/[controller]")]
public class NoteCommentServiceController(INoteCommentRepository repository) : Controller
{
    // 최근 댓글 리스트 반환
    [HttpGet]
    public IEnumerable<NoteComment> Get() => repository.GetRecentComments();
}
