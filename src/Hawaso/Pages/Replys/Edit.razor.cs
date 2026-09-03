using BlazorInputFile;
using Microsoft.AspNetCore.Components;
using VisualAcademy.Models.Replys;

namespace Hawaso.Pages.Replys;

public partial class Edit
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

    [Inject]
    public IReplyRepository RepositoryReference { get; set; } = default!;

    [Inject]
    public NavigationManager Nav { get; set; } = default!;

    [Inject]
    public IFileStorageManager FileStorageManagerInjector { get; set; } = default!;

    #endregion

    #region Properties

    public Reply Model { get; set; } = new();

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

        Content = Dul.HtmlUtility.EncodeWithTabAndSpace(Model.Content);
        ParentId = Model.ParentId?.ToString() ?? string.Empty;
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// 수정 버튼 이벤트 처리기
    /// </summary>
    protected async Task FormSubmit()
    {
        int.TryParse(ParentId, out int parentId);
        Model.ParentId = parentId;

        #region 파일 업로드 관련 추가 코드 영역

        if (selectedFiles.Length > 0)
        {
            var file = selectedFiles.FirstOrDefault();

            if (file != null)
            {
                var fileName = file.Name;
                var fileSize = Convert.ToInt32(file.Size);

                // 기존 첨부 파일 삭제
                if (!string.IsNullOrWhiteSpace(Model.FileName))
                {
                    await FileStorageManagerInjector.DeleteAsync(
                        Model.FileName,
                        string.Empty);
                }

                // 새 파일 업로드
                fileName = await FileStorageManagerInjector.UploadAsync(
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

        Nav.NavigateTo("/Replys");
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