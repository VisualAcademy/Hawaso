﻿using BlazorInputFile;
using Microsoft.AspNetCore.Components;

namespace VisualAcademy.Pages.Memos.Components;

public partial class ModalForm
{
    private string titleErrorMessage = string.Empty;
    private string passwordErrorMessage = string.Empty;

    #region Fields
    private string parentId = "";

    private bool isSubmitting = false; // 업로드 중인지 여부를 나타내는 플래그 추가
    private string submitButtonText = "Submit"; // 버튼 텍스트 속성 추가

    /// <summary>
    /// 첨부 파일 리스트 보관
    /// </summary>
    private IFileListEntry[] selectedFiles;
    #endregion

    #region Properties
    /// <summary>
    /// (글쓰기/글수정)모달 다이얼로그를 표시할건지 여부 
    /// </summary>
    public bool IsShow { get; set; } = false;
    #endregion

    #region Public Methods
    /// <summary>
    /// 폼 보이기 
    /// </summary>
    public void Show()
    {
        IsShow = true; // 현재 인라인 모달 폼 보이기
        submitButtonText = "Submit"; // 폼이 열릴 때 버튼 텍스트를 기본 값으로 재설정
    }

    /// <summary>
    /// 폼 닫기
    /// </summary>
    public void Hide() => IsShow = false; // 현재 인라인 모달 폼 숨기기
    #endregion

    #region Parameters
    /// <summary>
    /// 폼의 제목 영역
    /// </summary>
    [Parameter]
    public RenderFragment EditorFormTitle { get; set; }

    /// <summary>
    /// 넘어온 모델 개체 
    /// </summary>
    [Parameter]
    public Memo ModelSender { get; set; }

    /// <summary>
    /// 전체 넘어온 개체 중에서 폼에서 변경되는 내용만 따로 관리: ModelEdit => MemoEdit, MemoViewModel, ...
    /// </summary>
    public Memo ModelEdit { get; set; }

    public string[] Encodings { get; set; } = { "Plain-Text", "Text/HTML", "Mixed-Text" };

    #region Lifecycle Methods
    // 넘어온 Model 값을 수정 전용 ModelEdit에 담기 
    protected override void OnParametersSet()
    {
        ModelEdit = new Memo();
        ModelEdit.Id = ModelSender.Id;
        ModelEdit.Name = ModelSender.Name;
        ModelEdit.Title = ModelSender.Title;
        ModelEdit.Content = ModelSender.Content;
        ModelEdit.Password = ModelSender.Password;

        if (ModelEdit.Encoding != null)
        {
            ModelEdit.Encoding = ModelSender.Encoding;
        }
        else
        {
            ModelEdit.Encoding = "Plain-Text"; // Plain-Text, Text/HTML, Mixed-Text
        }

        // 더 많은 정보는 여기에서...

        // ParentId가 넘어온 값이 있으면... 즉, 0이 아니면 ParentId 드롭다운 리스트 기본값 선택
        parentId = ModelSender.ParentId.ToString();
        if (parentId == "0")
        {
            parentId = "";
        }
    }
    #endregion

    /// <summary>
    /// 부모 컴포넌트에게 생성(Create)이 완료되었다고 보고하는 목적으로 부모 컴포넌트에게 알림
    /// 학습 목적으로 Action 대리자 사용
    /// </summary>
    [Parameter]
    public Action CreateCallback { get; set; }

    /// <summary>
    /// 부모 컴포넌트에게 수정(Edit)이 완료되었다고 보고하는 목적으로 부모 컴포넌트에게 알림
    /// 학습 목적으로 EventCallback 구조체 사용
    /// </summary>
    [Parameter]
    public EventCallback<bool> EditCallback { get; set; }

    [Parameter]
    public string ParentKey { get; set; } = "";
    #endregion

    #region Injectors
    /// <summary>
    /// 리포지토리 클래스에 대한 참조 
    /// </summary>
    [Inject]
    public IMemoRepository RepositoryReference { get; set; }

    [Inject]
    public IMemoFileStorageManager FileStorageManagerReference { get; set; }
    #endregion

    #region Event Handlers
    protected async void CreateOrEditClick()
    {
        // 초기화
        titleErrorMessage = string.Empty;
        passwordErrorMessage = string.Empty;

        // 유효성 검사
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
            StateHasChanged(); // 오류 메시지 표시를 위해 UI 갱신
            return; // 저장 중단
        }

        if (isSubmitting) return; // 이미 제출 중이면 반환

        isSubmitting = true; // 제출 시작 플래그 설정
        submitButtonText = "Uploading..."; // 버튼 텍스트를 'Uploading...'으로 변경

        try
        {
            // 변경 내용 저장
            ModelSender.Name = ModelEdit.Name;
            ModelSender.Title = ModelEdit.Title;
            ModelSender.Content = ModelEdit.Content;
            ModelSender.Password = ModelEdit.Password;
            ModelSender.Encoding = ModelEdit.Encoding;

            #region 파일 업로드 관련 추가 코드 영역
            if (selectedFiles != null && selectedFiles.Length > 0)
            {
                // 파일 업로드
                var file = selectedFiles.FirstOrDefault();
                if (file != null)
                {
                    string fileName = file.Name;

                    // 파일명이 30자를 넘으면 앞의 30자까지만 사용
                    if (fileName.Length > 30)
                    {
                        fileName = fileName.Substring(0, 30);
                    }

                    int fileSize = Convert.ToInt32(file.Size);

                    await FileStorageManagerReference.UploadAsync(file.Data, fileName, "Memos", true);

                    ModelSender.FileName = fileName;
                    ModelSender.FileSize = fileSize;
                }
            }
            #endregion

            if (!int.TryParse(parentId, out int newParentId))
            {
                newParentId = 0;
            }
            ModelSender.ParentId = newParentId;
            ModelSender.ParentKey = ModelSender.ParentKey;

            if (ModelSender.Id == 0)
            {
                // Create
                await RepositoryReference.AddAsync(ModelSender);
                CreateCallback?.Invoke();
            }
            else
            {
                // Edit
                await RepositoryReference.UpdateAsync(ModelSender);
                await EditCallback.InvokeAsync(true);
            }
        }
        catch (Exception ex)
        {
            // 오류 처리 로직 추가 (필요시)
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            isSubmitting = false; // 작업 완료 후 플래그 재설정
            submitButtonText = "Submit"; // 작업 완료 후 버튼 텍스트를 'Submit'으로 재설정
        }
    }

    protected void HandleSelection(IFileListEntry[] files) => this.selectedFiles = files;
    #endregion
}
