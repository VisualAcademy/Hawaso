using BlazorInputFile;
using Microsoft.AspNetCore.Components;
using VisualAcademy.Models.Replys;

namespace Hawaso.Pages.Replys;

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
    protected int[] ParentIds { get; } = { 1, 2, 3 };

    #endregion

    #region Parameters

    [Parameter]
    public int Id { get; set; }

    #endregion

    #region Injectors

    [Inject]
    public IReplyRepository RepositoryReference { get; set; } = default!;

    [Inject]
    public NavigationManager Nav { get; set; } = default!;

    [Inject]
    public IFileStorageManager FileStorageManagerInjector { get; set; } = default!;

    #endregion

    #region Properties

    public Reply Model { get; set; } = new();

    public string ParentId { get; set; } = "";

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
        // 답변 글쓰기 페이지라면, 기존 데이터 읽어오기
        if (Id == 0)
        {
            return;
        }

        // 기존 글의 데이터를 읽어오기
        Model = await RepositoryReference.GetByIdAsync(Id);

        Model.Id = 0; // 답변 페이지는 새로운 글로 초기화
        Model.Name = "";
        Model.Title = $"Re: {Model.Title}";
        Model.Content = $"\r\n====\r\n{Model.Content}";

        ParentRef = Convert.ToInt32(Model.Ref);
        ParentStep = Convert.ToInt32(Model.Step);
        ParentRefOrder = Convert.ToInt32(Model.RefOrder);
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// 저장하기 버튼 클릭 이벤트 처리기
    /// </summary>
    protected async Task FormSubmit()
    {
        int.TryParse(ParentId, out int parentId);
        Model.ParentId = parentId;

        #region 파일 업로드 관련 추가 코드 영역

        if (selectedFiles.Length > 0)
        {
            var file = selectedFiles.FirstOrDefault();

            if (file is not null)
            {
                var fileSize = Convert.ToInt32(file.Size);

                var fileName = await FileStorageManagerInjector.UploadAsync(
                    file.Data,
                    file.Name,
                    "",
                    true);

                Model.FileName = fileName;
                Model.FileSize = fileSize;
            }
        }

        #endregion

        if (Id != 0)
        {
            // Reply: 답변 글이라면
            await RepositoryReference.AddAsync(Model, ParentRef, ParentStep, ParentRefOrder);
        }
        else
        {
            // Create: 일반 작성 글이라면
            await RepositoryReference.AddAsync(Model);
        }

        Nav.NavigateTo("/Replys");
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