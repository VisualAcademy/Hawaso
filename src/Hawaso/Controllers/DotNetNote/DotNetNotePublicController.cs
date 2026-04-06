using Dul.Board;
using Hawaso.Models.Notes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace Hawaso.Controllers;

public class DotNetNotePublicController(
    IWebHostEnvironment environment,
    INoteRepository repository,
    INoteCommentRepository commentRepository,
    ILogger<DotNetNoteController> logger) : Controller
{
    // 공통 속성: 검색 모드: 검색 모드이면 true, 그렇지 않으면 false.
    public bool SearchMode { get; set; } = false;

    // null 경고 방지를 위해 기본값 부여
    public string SearchField { get; set; } = string.Empty; // 필드: Name, Title, Content
    public string SearchQuery { get; set; } = string.Empty; // 검색 내용

    /// <summary>
    /// 현재 보여줄 페이지 번호
    /// </summary>
    public int PageIndex { get; set; } = 0;

    /// <summary>
    /// 총 레코드 갯수(글번호 순서 정렬용)
    /// </summary>
    public int TotalRecordCount { get; set; } = 0;

    /// <summary>
    /// 게시판 리스트 페이지
    /// </summary>
    public IActionResult Index()
    {
        // 로깅
        logger.LogInformation("게시판 리스트 페이지 로딩");

        // 검색 모드 결정: ?SearchField=Name&SearchQuery=닷넷코리아
        SearchMode =
            !string.IsNullOrEmpty(Request.Query["SearchField"]) &&
            !string.IsNullOrEmpty(Request.Query["SearchQuery"]);

        // 검색 환경이면 속성에 저장
        if (SearchMode)
        {
            SearchField = Request.Query["SearchField"].ToString();
            SearchQuery = Request.Query["SearchQuery"].ToString();
        }

        //[1] 쿼리스트링에 따른 페이지 보여주기
        if (!string.IsNullOrEmpty(Request.Query["Page"].ToString()))
        {
            // Page는 보여지는 쪽은 1, 2, 3, ... 코드단에서는 0, 1, 2, ...
            PageIndex = Convert.ToInt32(Request.Query["Page"]) - 1;
        }

        //[2] 쿠키를 사용한 리스트 페이지 번호 유지 적용(Optional):
        //    100번째 페이지 보고 있다가 다시 리스트 왔을 때 100번째 페이지 표시
        if (!string.IsNullOrEmpty(Request.Cookies["DotNetNotePageNum"]))
        {
            if (!string.IsNullOrEmpty(Request.Cookies["DotNetNotePageNum"]))
            {
                PageIndex = Convert.ToInt32(Request.Cookies["DotNetNotePageNum"]);
            }
            else
            {
                PageIndex = 0;
            }
        }

        // 게시판 리스트 정보 가져오기
        List<Note> notes;
        if (!SearchMode)
        {
            TotalRecordCount = repository.GetCountAll();
            notes = repository.GetAll(PageIndex);
        }
        else
        {
            TotalRecordCount = repository.GetCountBySearch(SearchField, SearchQuery);
            notes = repository.GetSeachAll(PageIndex, SearchField, SearchQuery);
        }

        // 주요 정보를 뷰 페이지로 전송
        ViewBag.TotalRecord = TotalRecordCount;
        ViewBag.SearchMode = SearchMode;
        ViewBag.SearchField = SearchField;
        ViewBag.SearchQuery = SearchQuery;

        return View(notes);
    }

    /// <summary>
    /// 게시판 글쓰기 폼
    /// </summary>
    [HttpGet]
    public IActionResult Create()
    {
        logger.LogInformation("게시판 글쓰기 페이지 로딩");

        ViewBag.FormType = BoardWriteFormType.Write;
        ViewBag.TitleDescription = "글 쓰기 - 다음 필드들을 채워주세요.";
        ViewBag.SaveButtonText = "저장";

        return View();
    }

    /// <summary>
    /// 게시판 글쓰기 처리 + 파일 업로드 처리
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(Note model, ICollection<IFormFile> files)
    {
        string fileName = string.Empty;
        int fileSize = 0;

        var uploadFolder = Path.Combine(environment.WebRootPath, "files");

        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                fileSize = Convert.ToInt32(file.Length);

                var parsedFileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName?.Trim('"');
                var safeOriginalFileName = Path.GetFileName(parsedFileName ?? string.Empty);

                fileName = Dul.FileUtility.GetFileNameWithNumbering(uploadFolder, safeOriginalFileName);

                using var fileStream = new FileStream(
                    Path.Combine(uploadFolder, fileName),
                    FileMode.Create);

                await file.CopyToAsync(fileStream);
            }
        }

        var note = new Note
        {
            Name = model.Name,
            Email = Dul.HtmlUtility.Encode(model.Email),
            Homepage = model.Homepage,
            Title = Dul.HtmlUtility.Encode(model.Title),
            Content = model.Content,
            FileName = fileName,
            FileSize = fileSize,
            Password = (new Dul.Security.CryptorEngine()).EncryptPassword(model.Password),
            PostIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            Encoding = model.Encoding
        };

        repository.Add(note);

        TempData["Message"] = "데이터가 저장되었습니다.";
        return RedirectToAction("Index");
    }

    /// <summary>
    /// 게시판 파일 강제 다운로드 기능(/BoardDown/:Id)
    /// </summary>
    public IActionResult BoardDown(int id)
    {
        var fileName = repository.GetFileNameById(id);

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return NotFound();
        }

        repository.UpdateDownCountById(id);

        var filePath = Path.Combine(environment.WebRootPath, "files", fileName);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var fileBytes = System.IO.File.ReadAllBytes(filePath);
        return File(fileBytes, "application/octet-stream", fileName);
    }

    /// <summary>
    /// 게시판의 상세 보기 페이지(Details, BoardView)
    /// </summary>
    public IActionResult Details(int id)
    {
        var note = repository.GetNoteById(id);

        if (note is null)
        {
            return NotFound();
        }

        //[!] 인코딩 방식에 따른 데이터 출력
        var encodingValue = note.Encoding ?? nameof(ContentEncodingType.Html);

        if (!Enum.TryParse(typeof(ContentEncodingType), encodingValue, out var parsedEncoding))
        {
            parsedEncoding = ContentEncodingType.Html;
        }

        var encoding = (ContentEncodingType)parsedEncoding;
        string encodedContent;

        switch (encoding)
        {
            case ContentEncodingType.Text:
                encodedContent = Dul.HtmlUtility.EncodeWithTabAndSpace(note.Content ?? string.Empty);
                break;
            case ContentEncodingType.Html:
                encodedContent = note.Content ?? string.Empty;
                break;
            case ContentEncodingType.Mixed:
                encodedContent = (note.Content ?? string.Empty).Replace("\r\n", "<br />");
                break;
            default:
                encodedContent = note.Content ?? string.Empty;
                break;
        }

        ViewBag.Content = encodedContent;

        // 첨부된 파일 확인
        if (!string.IsNullOrWhiteSpace(note.FileName))
        {
            ViewBag.FileName = string.Format(
                "<a href='/DotNetNote/BoardDown?Id={0}'>{1}{2} / 전송수: {3}</a>",
                note.Id,
                "<img src=\"/images/ext/ext_zip.gif\" border=\"0\">",
                note.FileName,
                note.DownCount);

            if (Dul.BoardLibrary.IsPhoto(note.FileName))
            {
                ViewBag.ImageDown = $"<img src='/DotNetNote/ImageDown/{note.Id}'><br />";
            }
        }
        else
        {
            ViewBag.FileName = "(업로드된 파일이 없습니다.)";
        }

        var vm = new NoteCommentViewModel
        {
            NoteCommentList = commentRepository.GetNoteComments(note.Id),
            BoardId = note.Id
        };

        ViewBag.CommentListAndId = vm;

        return View(note);
    }

    /// <summary>
    /// 게시판 삭제 폼
    /// </summary>
    [HttpGet]
    public IActionResult Delete(int id)
    {
        ViewBag.Id = id;
        return View();
    }

    /// <summary>
    /// 게시판 삭제 처리
    /// </summary>
    [HttpPost]
    public IActionResult Delete(int id, string Password)
    {
        if (repository.DeleteNote(id, (new Dul.Security.CryptorEngine()).EncryptPassword(Password)) > 0)
        {
            TempData["Message"] = "데이터가 삭제되었습니다.";

            if (DateTime.Now.Second % 2 == 0)
            {
                return RedirectToAction("DeleteCompleted");
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        else
        {
            ViewBag.Message = "삭제되지 않았습니다. 비밀번호를 확인하세요.";
        }

        ViewBag.Id = id;
        return View();
    }

    /// <summary>
    /// 게시판 삭제 완료 후 추가적인 처리할 때 페이지
    /// </summary>
    public IActionResult DeleteCompleted() => View();

    /// <summary>
    /// 게시판 수정 폼
    /// </summary>
    [HttpGet]
    public IActionResult Edit(int id)
    {
        ViewBag.FormType = BoardWriteFormType.Modify;
        ViewBag.TitleDescription = "글 수정 - 아래 항목을 수정하세요.";
        ViewBag.SaveButtonText = "수정";

        var note = repository.GetNoteById(id);
        if (note is null)
        {
            return NotFound();
        }

        if (!string.IsNullOrWhiteSpace(note.FileName))
        {
            ViewBag.FileName = note.FileName;
            ViewBag.FileSize = note.FileSize;
            ViewBag.FileNamePrevious = $"기존에 업로드된 파일명: {note.FileName}";
        }
        else
        {
            ViewBag.FileName = string.Empty;
            ViewBag.FileSize = 0;
        }

        return View(note);
    }

    /// <summary>
    /// 게시판 수정 처리 + 파일 업로드
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Edit(
        Note model,
        ICollection<IFormFile> files,
        int id,
        string previousFileName = "",
        int previousFileSize = 0)
    {
        ViewBag.FormType = BoardWriteFormType.Modify;
        ViewBag.TitleDescription = "글 수정 - 아래 항목을 수정하세요.";
        ViewBag.SaveButtonText = "수정";

        var fileName = previousFileName ?? string.Empty;
        var fileSize = previousFileSize;

        var uploadFolder = Path.Combine(environment.WebRootPath, "files");

        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                fileSize = Convert.ToInt32(file.Length);

                var parsedFileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName?.Trim('"');
                var safeOriginalFileName = Path.GetFileName(parsedFileName ?? string.Empty);

                fileName = Dul.FileUtility.GetFileNameWithNumbering(uploadFolder, safeOriginalFileName);

                using var fileStream = new FileStream(
                    Path.Combine(uploadFolder, fileName),
                    FileMode.Create);

                await file.CopyToAsync(fileStream);
            }
        }

        var note = new Note
        {
            Id = id,
            Name = model.Name,
            Email = Dul.HtmlUtility.Encode(model.Email),
            Homepage = model.Homepage,
            Title = Dul.HtmlUtility.Encode(model.Title),
            Content = model.Content,
            FileName = fileName,
            FileSize = fileSize,
            Password = (new Dul.Security.CryptorEngine()).EncryptPassword(model.Password),
            ModifyIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            Encoding = model.Encoding
        };

        var result = repository.UpdateNote(note);
        if (result > 0)
        {
            TempData["Message"] = "수정되었습니다.";
            return RedirectToAction("Details", new { Id = id });
        }

        ViewBag.ErrorMessage = "업데이트가 되지 않았습니다. 암호를 확인하세요.";
        return View(note);
    }

    /// <summary>
    /// 답변 글쓰기 폼
    /// </summary>
    /// <param name="id">부모글 Id</param>
    [HttpGet]
    public IActionResult Reply(int id)
    {
        ViewBag.FormType = BoardWriteFormType.Reply;
        ViewBag.TitleDescription = "글 답변 - 다음 필드들을 채워주세요.";
        ViewBag.SaveButtonText = "답변";

        var note = repository.GetNoteById(id);
        if (note is null)
        {
            return NotFound();
        }

        var newNote = new Note
        {
            Title = $"Re : {note.Title}",
            Content =
                $"\n\nOn {note.PostDate}, '{note.Name}' wrote:\n----------\n>" +
                $"{(note.Content ?? string.Empty).Replace("\n", "\n>")}\n---------"
        };

        return View(newNote);
    }

    /// <summary>
    /// 답변 글쓰기 처리 + 파일 업로드
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Reply(Note model, ICollection<IFormFile> files, int id)
    {
        string fileName = string.Empty;
        int fileSize = 0;

        var uploadFolder = Path.Combine(environment.WebRootPath, "files");

        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                fileSize = Convert.ToInt32(file.Length);

                var parsedFileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName?.Trim('"');
                var safeOriginalFileName = Path.GetFileName(parsedFileName ?? string.Empty);

                fileName = Dul.FileUtility.GetFileNameWithNumbering(uploadFolder, safeOriginalFileName);

                using var fileStream = new FileStream(
                    Path.Combine(uploadFolder, fileName),
                    FileMode.Create);

                await file.CopyToAsync(fileStream);
            }
        }

        var note = new Note
        {
            Id = id,
            ParentNum = id,
            Name = model.Name,
            Email = Dul.HtmlUtility.Encode(model.Email),
            Homepage = model.Homepage,
            Title = Dul.HtmlUtility.Encode(model.Title),
            Content = model.Content,
            FileName = fileName,
            FileSize = fileSize,
            Password = (new Dul.Security.CryptorEngine()).EncryptPassword(model.Password),
            PostIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            Encoding = model.Encoding
        };

        repository.ReplyNote(note);

        TempData["Message"] = "데이터가 저장되었습니다.";
        return RedirectToAction("Index");
    }

    /// <summary>
    /// ImageDown : 완성형(DotNetNote) 게시판의 이미지전용다운 페이지
    /// </summary>
    public IActionResult ImageDown(int id)
    {
        var fileName = repository.GetFileNameById(id);

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return NotFound();
        }

        var fileExtension = Path.GetExtension(fileName);
        var contentType = fileExtension?.ToLowerInvariant() switch
        {
            ".gif" => "image/gif",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };

        repository.UpdateDownCount(fileName);

        var filePath = Path.Combine(environment.WebRootPath, "files", fileName);
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var fileBytes = System.IO.File.ReadAllBytes(filePath);
        return File(fileBytes, contentType, fileName);
    }

    /// <summary>
    /// 댓글 입력
    /// </summary>
    [HttpPost]
    public IActionResult CommentAdd(int BoardId, string txtName, string txtPassword, string txtOpinion)
    {
        var comment = new NoteComment
        {
            BoardId = BoardId,
            Name = txtName,
            Password = (new Dul.Security.CryptorEngine()).EncryptPassword(txtPassword),
            Opinion = txtOpinion
        };

        commentRepository.AddNoteComment(comment);

        return RedirectToAction("Details", new { Id = BoardId });
    }

    /// <summary>
    /// 댓글 삭제 폼
    /// </summary>
    [HttpGet]
    public IActionResult CommentDelete(string BoardId, string Id)
    {
        ViewBag.BoardId = BoardId;
        ViewBag.Id = Id;

        return View();
    }

    /// <summary>
    /// 댓글 삭제 처리
    /// </summary>
    [HttpPost]
    public IActionResult CommentDelete(string BoardId, string Id, string txtPassword)
    {
        txtPassword = (new Dul.Security.CryptorEngine()).EncryptPassword(txtPassword);

        if (commentRepository.GetCountBy(Convert.ToInt32(BoardId), Convert.ToInt32(Id), txtPassword) > 0)
        {
            commentRepository.DeleteNoteComment(
                Convert.ToInt32(BoardId),
                Convert.ToInt32(Id),
                txtPassword);

            return RedirectToAction("Details", new { Id = BoardId });
        }

        ViewBag.BoardId = BoardId;
        ViewBag.Id = Id;
        ViewBag.ErrorMessage = "암호가 틀립니다. 다시 입력해주세요.";

        return View();
    }

    /// <summary>
    /// 공지글로 올리기(관리자 전용)
    /// </summary>
    [HttpGet]
    [Authorize("Administrators")]
    public IActionResult Pinned(int id)
    {
        repository.Pinned(id);
        return RedirectToAction("Details", new { Id = id });
    }

    /// <summary>
    /// (참고) 최근 글 리스트 Web API 테스트 페이지
    /// </summary>
    public IActionResult NoteServiceDemo() => View();

    /// <summary>
    /// (참고) 최근 댓글 리스트 Web API 테스트 페이지
    /// </summary>
    public IActionResult NoteCommentServiceDemo() => View();
}