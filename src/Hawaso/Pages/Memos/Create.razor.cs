using BlazorInputFile;
using Microsoft.AspNetCore.Components;

namespace VisualAcademy.Pages.Memos;

public partial class Create
{
    #region Fields

    /// <summary>
    /// 첨부 파일 리스트 보관
    /// </summary>
    private IFileListEntry[] selectedFiles = Array.Empty<IFileListEntry>();

    /// <summary>
    /// 부모(카테고리) 리스트가 저장될 임시 변수
    /// </summary>
    protected int[] parentIds = { 1, 2, 3 };

    #endregion

    #region Parameters

    [Parameter]
    public int Id { get; set; }

    #endregion

    #region Injectors

    // Reference 접미사 사용해 봄
    [Inject]
    public IMemoRepository RepositoryReference { get; set; } = default!;

    // Injector 접미사 사용해 봄
    [Inject]
    public NavigationManager Nav { get; set; } = default!;

    [Inject]
    public IMemoFileStorageManager FileStorageManagerInjector { get; set; } = default!;

    #endregion

    #region Properties

    private Memo Model { get; set; } = new Memo();

    public string Name { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string ParentId { get; set; } = string.Empty;

    // 부모 글의 답변형 게시판 계층 정보를 임시 보관
    public int ParentRef { get; set; }

    public int ParentStep { get; set; }

    public int ParentRefOrder { get; set; }

    #endregion

    #region Lifecycle Methods

    /// <summary>
    /// 페이지 초기화 이벤트 처리기
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        // 답변 글쓰기 페이지라면 기존 데이터 읽어오기
        if (Id != 0)
        {
            var parent = await RepositoryReference.GetByIdAsync(Id);

            if (parent is not null)
            {
                // 답변 페이지는 새로운 글로 초기화
                Model.Name = string.Empty;
                Model.Title = $"Re: {parent.Title}";
                Model.Content = $"\r\n====\r\n{parent.Content}";

                ParentRef = (int)parent.Ref;
                ParentStep = (int)parent.Step;
                ParentRefOrder = (int)parent.RefOrder;
            }
        }
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// 저장하기 버튼 클릭 이벤트 처리기
    /// </summary>
    protected async Task FormSubmit()
    {
        int.TryParse(ParentId, out int parentId);

        // 선택한 ParentId 값 가져오기
        Model.ParentId = parentId;

        #region 파일 업로드 관련 추가 코드 영역

        if (selectedFiles.Length > 0)
        {
            var file = selectedFiles.FirstOrDefault();

            if (file is not null)
            {
                string fileName = file.Name;
                int fileSize = Convert.ToInt32(file.Size);

                fileName = await FileStorageManagerInjector.UploadAsync(
                    file.Data,
                    file.Name,
                    "Memos",
                    true);

                Model.FileName = fileName;
                Model.FileSize = fileSize;
            }
        }

        #endregion

        var m = new Memo
        {
            Name = Model.Name,
            Title = Model.Title,
            Content = Model.Content,
            Password = Model.Password,
            Email = Model.Email,
            FileName = Model.FileName,
            FileSize = Model.FileSize,

            PostDate = DateTime.Now,
            ParentNum = 0,
            AnswerNum = 0,
            CommentCount = 0,

            Created = DateTime.UtcNow,
            CreatedBy = string.Empty,

            Category = "Free",
            Encoding = "Text",

            IsPinned = false,

            Modified = DateTime.Now,
            ModifyIp = string.Empty,
            PostIp = "127.0.0.1",

            Step = 0,
            RefOrder = 0
        };

        if (Id != 0)
        {
            // Memo: 답변 글이라면
            await RepositoryReference.AddAsync(m, Id);
        }
        else
        {
            // Create: 일반 작성 글이라면
            await RepositoryReference.AddAsync(m);
        }

        // Manage 컴포넌트로 이동
        Nav.NavigateTo("/Memos");
    }

    /// <summary>
    /// 파일 첨부 이벤트 처리기
    /// </summary>
    protected void HandleSelection(IFileListEntry[] files)
    {
        selectedFiles = files ?? Array.Empty<IFileListEntry>();
    }

    #endregion
}