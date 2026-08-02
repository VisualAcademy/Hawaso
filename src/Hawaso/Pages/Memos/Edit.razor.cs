using BlazorInputFile;
using Microsoft.AspNetCore.Components;

namespace VisualAcademy.Pages.Memos;

public partial class Edit
{
    #region Fields

    /// <summary>
    /// 첨부 파일 리스트 보관
    /// </summary>
    private IFileListEntry[] selectedFiles =
        Array.Empty<IFileListEntry>();

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

    [Inject]
    public IMemoRepository RepositoryReference { get; set; }
        = default!;

    [Inject]
    public NavigationManager Nav { get; set; }
        = default!;

    [Inject]
    public IMemoFileStorageManager FileStorageManagerInjector { get; set; }
        = default!;

    #endregion

    #region Properties

    public Memo Model { get; set; } = new();

    public string ParentId { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    #endregion

    #region Lifecycle Methods

    /// <summary>
    /// 페이지 초기화 이벤트 처리기
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        Model = await RepositoryReference.GetByIdAsync(Id);

        Content = Dul.HtmlUtility.EncodeWithTabAndSpace(
            Model.Content);

        ParentId = Model.ParentId.ToString();
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// 수정 버튼 이벤트 처리기
    /// </summary>
    protected async Task FormSubmit()
    {
        int.TryParse(
            ParentId,
            out var parentId);

        Model.ParentId = parentId;

        #region 파일 업로드 관련 추가 코드 영역

        if (selectedFiles.Length > 0)
        {
            var file = selectedFiles.FirstOrDefault();

            if (file is not null)
            {
                var fileSize = Convert.ToInt32(file.Size);

                // 기존 첨부 파일 삭제
                await FileStorageManagerInjector.DeleteAsync(
                    Model.FileName,
                    "Memos");

                // 새 첨부 파일 업로드
                var fileName =
                    await FileStorageManagerInjector.UploadAsync(
                        file.Data,
                        file.Name,
                        string.Empty,
                        true);

                Model.FileName = fileName;
                Model.FileSize = fileSize;
            }
        }

        #endregion

        await RepositoryReference.EditAsync(Model);

        Nav.NavigateTo("/Memos");
    }

    /// <summary>
    /// 파일 선택 이벤트 처리기
    /// </summary>
    protected void HandleSelection(IFileListEntry[] files)
    {
        selectedFiles = files ?? Array.Empty<IFileListEntry>();
    }

    #endregion
}