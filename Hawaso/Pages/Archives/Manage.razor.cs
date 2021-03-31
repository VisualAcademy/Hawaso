using BlazorUtils;
using Hawaso.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Hawaso.Pages.Archives
{
    public partial class Manage
    {
        #region Parameters
        [Parameter]
        public int ParentId { get; set; } = 0;

        [Parameter]
        public string ParentKey { get; set; } = "";
        #endregion

        #region Injectors
        [Inject]
        public NavigationManager NavigationManagerInjector { get; set; }

        [Inject]
        public IJSRuntime JSRuntimeInjector { get; set; }

        [Inject]
        public IArchiveRepository RepositoryReference { get; set; }

        [Inject]
        public IArchiveFileStorageManager FileStorageManagerReference { get; set; }
        #endregion

        #region Properties
        /// <summary>
        /// 글쓰기 또는 수정하기 폼의 제목에 전달할 문자열(태그 포함 가능)
        /// </summary>
        public string EditorFormTitle { get; set; } = "CREATE";
        #endregion

        /// <summary>
        /// EditorForm에 대한 참조: 모달로 글쓰기 또는 수정하기
        /// </summary>
        //public Components.EditorForm EditorFormReference { get; set; }
        public Components.ModalForm EditorFormReference { get; set; }

        /// <summary>
        /// DeleteDialog에 대한 참조: 모달로 항목 삭제하기 
        /// </summary>
        public Components.DeleteDialog DeleteDialogReference { get; set; }

        /// <summary>
        /// 현재 페이지에서 리스트로 사용되는 모델 리스트 
        /// </summary>
        protected List<Archive> models = new List<Archive>();

        /// <summary>
        /// 현재 페이지에서 선택된 단일 데이터를 나타내는 모델 클래스 
        /// </summary>
        protected Archive model = new Archive();

        /// <summary>
        /// 페이저 설정
        /// </summary>
        protected DulPager.DulPagerBase pager = new DulPager.DulPagerBase()
        {
            PageNumber = 1,
            PageIndex = 0,
            PageSize = 10,
            PagerButtonCount = 5
        };

        #region Lifecycle Methods
        /// <summary>
        /// 페이지 초기화 이벤트 처리기
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await GetUserIdAndUserName();

            await DisplayData();
        }
        #endregion

        private async Task DisplayData()
        {
            // ParentKey와 ParentId를 사용하는 목적은 특정 부모의 Details 페이지에서 리스트로 표현하기 위함
            if (ParentKey != "")
            {
                var articleSet = await RepositoryReference.GetArticlesAsync<string>(pager.PageIndex, pager.PageSize, searchField: "", this.searchQuery, this.sortOrder, ParentKey);
                pager.RecordCount = articleSet.TotalCount;
                models = articleSet.Items.ToList();
            }
            else if (ParentId != 0)
            {
                var articleSet = await RepositoryReference.GetArticlesAsync<int>(pager.PageIndex, pager.PageSize, searchField: "", this.searchQuery, this.sortOrder, ParentId);
                pager.RecordCount = articleSet.TotalCount;
                models = articleSet.Items.ToList();
            }
            else
            {
                var articleSet = await RepositoryReference.GetArticlesAsync<int>(pager.PageIndex, pager.PageSize, searchField: "", this.searchQuery, this.sortOrder, parentIdentifier: 0);
                pager.RecordCount = articleSet.TotalCount;
                models = articleSet.Items.ToList();
            }

            StateHasChanged(); // Refresh
        }

        protected void NameClick(long id)
        {
            NavigationManagerInjector.NavigateTo($"/Archives/Details/{id}");
        }

        protected async void PageIndexChanged(int pageIndex)
        {
            pager.PageIndex = pageIndex;
            pager.PageNumber = pageIndex + 1;

            await DisplayData();

            StateHasChanged();
        }

        #region Event Handlers
        /// <summary>
        /// 글쓰기 모달 폼 띄우기 
        /// </summary>
        protected void ShowEditorForm()
        {
            EditorFormTitle = "CREATE";
            this.model = new Archive(); // 모델 초기화
            this.model.ParentId = ParentId; 
            this.model.ParentKey = ParentKey; // 

            model.Name = UserName; // 로그인 사용자 이름을 기본으로 제공

            EditorFormReference.Show();
        }

        /// <summary>
        /// 관리자 전용: 모달 폼으로 선택 항목 수정
        /// </summary>
        protected void EditBy(Archive model)
        {
            EditorFormTitle = "EDIT";
            this.model = new Archive(); // 모델 초기화
            this.model = model;
            this.model.ParentId = ParentId;
            this.model.ParentKey = ParentKey; // 
            EditorFormReference.Show();
        }

        /// <summary>
        /// 관리자 전용: 모달 폼으로 선택 항목 삭제
        /// </summary>
        protected void DeleteBy(Archive model)
        {
            this.model = model;
            DeleteDialogReference.Show();
        }
        #endregion

        protected async void DownloadBy(Archive model)
        {
            if (!string.IsNullOrEmpty(model.FileName))
            {
                byte[] fileBytes = await FileStorageManagerReference.DownloadAsync(model.FileName, "Archives");
                if (fileBytes != null)
                {
                    model.DownCount = model.DownCount + 1;
                    await RepositoryReference.EditAsync(model);

                    await FileUtil.SaveAs(JSRuntimeInjector, model.FileName, fileBytes);
                }
            }
        }

        /// <summary>
        /// 모델 초기화 및 모달 폼 닫기
        /// </summary>
        protected async void CreateOrEdit()
        {
            EditorFormReference.Hide();
            this.model = new Archive();
            await DisplayData();
        }

        /// <summary>
        /// 삭제 모달 폼에서 현재 선택한 항목 삭제
        /// </summary>
        protected async void DeleteClick()
        {
            if (!string.IsNullOrEmpty(model?.FileName))
            {
                // 첨부 파일 삭제 
                await FileStorageManagerReference.DeleteAsync(model.FileName, "Archives");
            }

            await RepositoryReference.DeleteAsync(this.model.Id);
            DeleteDialogReference.Hide();
            this.model = new Archive(); // 선택했던 모델 초기화
            await DisplayData(); // 다시 로드
        }

        #region Toggle with Inline Dialog
        /// <summary>
        /// 인라인 폼을 띄울건지 여부 
        /// </summary>
        public bool IsInlineDialogShow { get; set; } = false;

        protected void ToggleClose()
        {
            IsInlineDialogShow = false;
            this.model = new Archive();
        }

        /// <summary>
        /// 토글: Pinned
        /// </summary>
        protected async void ToggleClick()
        {
            this.model.IsPinned = (this.model?.IsPinned == true) ? false : true;

            // 변경된 내용 업데이트
            await RepositoryReference.UpdateAsync(this.model);

            IsInlineDialogShow = false; // 표시 속성 초기화
            this.model = new Archive(); // 선택한 모델 초기화 

            await DisplayData(); // 다시 로드
        }

        /// <summary>
        /// ToggleBy(PinnedBy)
        /// </summary>
        protected void ToggleBy(Archive model)
        {
            this.model = model;
            IsInlineDialogShow = true;
        }
        #endregion

        #region Search
        private string searchQuery = "";

        protected async void Search(string query)
        {
            pager.PageIndex = 0;

            this.searchQuery = query;

            await DisplayData();
        }
        #endregion

        #region Excel
        protected void DownloadExcelWithWebApi()
        {
            FileUtil.SaveAsExcel(JSRuntimeInjector, "/ArchiveDownload/ExcelDown");

            NavigationManagerInjector.NavigateTo($"/Archives"); // 다운로드 후 현재 페이지 다시 로드
        }

        protected void DownloadExcel()
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Archives");

                var tableBody = worksheet.Cells["B2:B2"].LoadFromCollection(
                    (from m in models select new { m.Created, m.Name, m.Title, m.DownCount, m.FileName })
                    , true);

                var uploadCol = tableBody.Offset(1, 1, models.Count, 1);

                // 그라데이션 효과 부여
                var rule = uploadCol.ConditionalFormatting.AddThreeColorScale();
                rule.LowValue.Color = Color.SkyBlue;
                rule.MiddleValue.Color = Color.White;
                rule.HighValue.Color = Color.Red;

                var header = worksheet.Cells["B2:F2"];
                worksheet.DefaultColWidth = 25;
                worksheet.Cells[3, 2, models.Count + 2, 2].Style.Numberformat.Format = "yyyy MMM d DDD";
                tableBody.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                tableBody.Style.Fill.PatternType = ExcelFillStyle.Solid;
                tableBody.Style.Fill.BackgroundColor.SetColor(Color.WhiteSmoke);
                tableBody.Style.Border.BorderAround(ExcelBorderStyle.Medium);
                header.Style.Font.Bold = true;
                header.Style.Font.Color.SetColor(Color.White);
                header.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);

                FileUtil.SaveAs(JSRuntimeInjector, $"{DateTime.Now.ToString("yyyyMMddhhmmss")}_Archives.xlsx", package.GetAsByteArray());
            }
        }
        #endregion

        #region Sorting
        private string sortOrder = "";

        protected async void SortByName()
        {
            if (sortOrder == "")
            {
                sortOrder = "Name";
            }
            else if (sortOrder == "Name")
            {
                sortOrder = "NameDesc";
            }
            else
            {
                sortOrder = "";
            }

            await DisplayData();
        }

        protected async void SortByTitle()
        {
            if (sortOrder == "")
            {
                sortOrder = "Title";
            }
            else if (sortOrder == "Title")
            {
                sortOrder = "TitleDesc";
            }
            else
            {
                sortOrder = "";
            }

            await DisplayData();
        }
        #endregion

        #region Get UserId and UserName: Blazor에서 현재 로그인 사용자 이름 획득하기
        [Parameter]
        public string UserId { get; set; } = "";

        [Parameter]
        public string UserName { get; set; } = "";

        [Inject] public UserManager<ApplicationUser> UserManagerRef { get; set; }

        [Inject] public AuthenticationStateProvider AuthenticationStateProviderRef { get; set; }

        private async Task GetUserIdAndUserName()
        {
            var authState = await AuthenticationStateProviderRef.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity.IsAuthenticated)
            {
                var currentUser = await UserManagerRef.GetUserAsync(user);
                UserId = currentUser.Id;
                UserName = user.Identity.Name;
            }
            else
            {
                UserId = "";
                UserName = "Anonymous";
            }
        }
        #endregion
    }
}
