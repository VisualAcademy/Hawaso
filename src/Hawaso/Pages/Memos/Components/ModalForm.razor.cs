using BlazorInputFile;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text.RegularExpressions;

namespace VisualAcademy.Pages.Memos.Components;

public partial class ModalForm
{
    private string titleErrorMessage = string.Empty;
    private string passwordErrorMessage = string.Empty;

    #region Fields

    private string parentId = string.Empty;

    private bool isSubmitting;
    private string submitButtonText = "Submit";

    /// <summary>
    /// 첨부 파일 리스트 보관
    /// </summary>
    private IFileListEntry[] selectedFiles = Array.Empty<IFileListEntry>();

    private const string PlaceholderRelativePath = "images/skip-upload-placeholder.png";

    #endregion

    #region Properties

    /// <summary>
    /// 글쓰기/글수정 모달 다이얼로그를 표시할지 여부
    /// </summary>
    public bool IsShow { get; set; }

    /// <summary>
    /// 전체 넘어온 개체 중에서 폼에서 변경되는 내용만 따로 관리
    /// </summary>
    public Memo ModelEdit { get; set; } = new();

    public string[] Encodings { get; set; } =
    [
        "Plain-Text",
        "Text/HTML",
        "Mixed-Text"
    ];

    #endregion

    #region Parameters

    /// <summary>
    /// 폼의 제목 영역
    /// </summary>
    [Parameter]
    public RenderFragment? EditorFormTitle { get; set; }

    /// <summary>
    /// 넘어온 모델 개체
    /// </summary>
    [Parameter]
    public Memo ModelSender { get; set; } = new();

    /// <summary>
    /// 부모 컴포넌트에게 생성 완료를 알림
    /// </summary>
    [Parameter]
    public Action? CreateCallback { get; set; }

    /// <summary>
    /// 부모 컴포넌트에게 수정 완료를 알림
    /// </summary>
    [Parameter]
    public EventCallback<bool> EditCallback { get; set; }

    [Parameter]
    public string ParentKey { get; set; } = string.Empty;

    #endregion

    #region Injectors

    [Inject]
    public IMemoRepository RepositoryReference { get; set; } = default!;

    [Inject]
    public IMemoFileStorageManager FileStorageManagerReference { get; set; } = default!;

    [Inject]
    public IJSRuntime JS { get; set; } = default!;

    [Inject]
    public IWebHostEnvironment WebHostEnv { get; set; } = default!;

    #endregion

    #region Public Methods

    /// <summary>
    /// 폼 보이기
    /// </summary>
    public void Show()
    {
        IsShow = true;
        submitButtonText = "Submit";
        titleErrorMessage = string.Empty;
        passwordErrorMessage = string.Empty;
    }

    /// <summary>
    /// 폼 닫기
    /// </summary>
    public void Hide() => IsShow = false;

    #endregion

    #region Lifecycle Methods

    /// <summary>
    /// 넘어온 Model 값을 수정 전용 ModelEdit에 담기
    /// </summary>
    protected override void OnParametersSet()
    {
        ModelEdit = new Memo
        {
            Id = ModelSender.Id,
            Name = ModelSender.Name ?? string.Empty,
            Title = ModelSender.Title ?? string.Empty,
            Content = ModelSender.Content ?? string.Empty,
            Password = ModelSender.Password ?? string.Empty,
            Encoding = string.IsNullOrWhiteSpace(ModelSender.Encoding)
                ? "Plain-Text"
                : ModelSender.Encoding,
            ParentId = ModelSender.ParentId,
            ParentKey = ModelSender.ParentKey ?? string.Empty
        };

        var senderParentId = ModelSender.ParentId.GetValueOrDefault();

        parentId = senderParentId == 0
            ? string.Empty
            : senderParentId.ToString();
    }

    #endregion

    #region Event Handlers

    protected async Task CreateOrEditClickAsync()
    {
        titleErrorMessage = string.Empty;
        passwordErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(ModelEdit.Title))
        {
            titleErrorMessage = "제목은 필수 항목입니다.";
        }

        if (string.IsNullOrWhiteSpace(ModelEdit.Password))
        {
            passwordErrorMessage = "비밀번호는 필수 항목입니다.";
        }

        if (!string.IsNullOrEmpty(titleErrorMessage) || !string.IsNullOrEmpty(passwordErrorMessage))
        {
            StateHasChanged();
            return;
        }

        if (isSubmitting)
        {
            return;
        }

        isSubmitting = true;
        submitButtonText = "Uploading...";

        try
        {
            ApplyFormValuesToSender();

            if (selectedFiles.Length > 0)
            {
                var file = selectedFiles.FirstOrDefault();

                if (file is not null)
                {
                    var fileName = file.Name;

                    if (fileName.Length > 30)
                    {
                        fileName = fileName[..30];
                    }

                    var fileSize = Convert.ToInt32(file.Size);

                    await FileStorageManagerReference.UploadAsync(file.Data, fileName, "Memos", true);

                    ModelSender.FileName = fileName;
                    ModelSender.FileSize = fileSize;
                }
            }

            ApplyParentValuesToSender();

            if (ModelSender.Id == 0)
            {
                await RepositoryReference.AddAsync(ModelSender);
                CreateCallback?.Invoke();
            }
            else
            {
                await RepositoryReference.UpdateAsync(ModelSender);
                await EditCallback.InvokeAsync(true);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            isSubmitting = false;
            submitButtonText = "Submit";
        }
    }

    protected void HandleSelection(IFileListEntry[] files)
    {
        selectedFiles = files ?? Array.Empty<IFileListEntry>();
    }

    #endregion

    #region Skip Upload

    protected async Task SkipUploadAsync()
    {
        if (isSubmitting)
        {
            return;
        }

        var ok = await JS.InvokeAsync<bool>(
            "confirm",
            "This will attach a placeholder image instead of a real file.\n\n" +
            "(A blank/placeholder image will be attached instead of a file upload.)\n\nContinue?");

        if (!ok)
        {
            return;
        }

        try
        {
            isSubmitting = true;
            submitButtonText = "Uploading...";

            ApplyFormValuesToSender();

            var webRoot = WebHostEnv.WebRootPath ?? "wwwroot";
            var placeholderFullPath = Path.Combine(webRoot, PlaceholderRelativePath);

            if (!System.IO.File.Exists(placeholderFullPath))
            {
                await JS.InvokeVoidAsync("alert", $"Placeholder file not found:\n{placeholderFullPath}");
                return;
            }

            await using var fs = System.IO.File.OpenRead(placeholderFullPath);

            var sanitizedTitle = Regex
                .Replace(ModelSender.Title ?? string.Empty, "[\\\\/:*?\"<>|]+", "-")
                .TrimEnd('.')
                .Replace(" ", "-");

            if (sanitizedTitle.Length > 10)
            {
                sanitizedTitle = sanitizedTitle[..10];
            }

            if (string.IsNullOrWhiteSpace(sanitizedTitle))
            {
                sanitizedTitle = "memo";
            }

            var fileName = $"{sanitizedTitle}{DateTime.Now:yyyyMMddHHmmss}-skip.png";

            await FileStorageManagerReference.UploadAsync(fs, fileName, "Memos", true);

            var fileInfo = new System.IO.FileInfo(placeholderFullPath);

            ModelSender.FileName = fileName;
            ModelSender.FileSize = (int)fileInfo.Length;

            ApplyParentValuesToSender();

            if (ModelSender.Id == 0)
            {
                await RepositoryReference.AddAsync(ModelSender);
                CreateCallback?.Invoke();
            }
            else
            {
                await RepositoryReference.UpdateAsync(ModelSender);
                await EditCallback.InvokeAsync(true);
            }
        }
        finally
        {
            isSubmitting = false;
            submitButtonText = "Submit";
        }
    }

    #endregion

    #region Helpers

    private void ApplyFormValuesToSender()
    {
        ModelSender.Name = ModelEdit.Name;
        ModelSender.Title = ModelEdit.Title;
        ModelSender.Content = ModelEdit.Content;
        ModelSender.Password = ModelEdit.Password;
        ModelSender.Encoding = string.IsNullOrWhiteSpace(ModelEdit.Encoding)
            ? "Plain-Text"
            : ModelEdit.Encoding;
    }

    private void ApplyParentValuesToSender()
    {
        if (!int.TryParse(parentId, out var newParentId))
        {
            newParentId = 0;
        }

        ModelSender.ParentId = newParentId;

        if (!string.IsNullOrWhiteSpace(ParentKey))
        {
            ModelSender.ParentKey = ParentKey;
        }
    }

    #endregion
}