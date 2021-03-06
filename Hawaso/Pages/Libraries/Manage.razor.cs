using BlazorUtils;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using ReplyApp.Managers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Zero.Models;

namespace Hawaso.Pages.Libraries
{
    public partial class Manage
    {
        [Parameter]
        public int ParentId { get; set; } = 0;

        [Parameter]
        public string ParentKey { get; set; } = "";

        [Inject]
        public ILibraryRepository UploadRepositoryAsyncReference { get; set; }

        [Inject]
        public NavigationManager NavigationManagerReference { get; set; }

        /// <summary>
        /// EditorForm에 대한 참조: 모달로 글쓰기 또는 수정하기
        /// </summary>
        public Components.EditorForm EditorFormReference { get; set; }

        /// <summary>
        /// DeleteDialog에 대한 참조: 모달로 항목 삭제하기 
        /// </summary>
        public Components.DeleteDialog DeleteDialogReference { get; set; }

        protected List<LibraryModel> models;

        protected LibraryModel model = new LibraryModel();

        /// <summary>
        /// 공지사항으로 올리기 폼을 띄울건지 여부 
        /// </summary>
        public bool IsInlineDialogShow { get; set; } = false;

        protected DulPager.DulPagerBase pager = new DulPager.DulPagerBase()
        {
            PageNumber = 1,
            PageIndex = 0,
            PageSize = 10,
            PagerButtonCount = 5
        };

        protected override async Task OnInitializedAsync()
        {
            await DisplayData();
        }

        private async Task DisplayData()
        {
            if (ParentKey != "")
            {
                var articleSet = await UploadRepositoryAsyncReference.GetArticles<string>(pager.PageIndex, pager.PageSize, "", this.searchQuery, this.sortOrder, ParentKey);
                pager.RecordCount = articleSet.TotalCount;
                models = articleSet.Items.ToList();
            }
            else if (ParentId != 0)
            {
                var articleSet = await UploadRepositoryAsyncReference.GetArticles<int>(pager.PageIndex, pager.PageSize, "", this.searchQuery, this.sortOrder, ParentId);
                pager.RecordCount = articleSet.TotalCount;
                models = articleSet.Items.ToList();
            }
            else
            {
                var articleSet = await UploadRepositoryAsyncReference.GetArticles<int>(pager.PageIndex, pager.PageSize, "", this.searchQuery, this.sortOrder, 0);
                pager.RecordCount = articleSet.TotalCount;
                models = articleSet.Items.ToList();
            }

            StateHasChanged();
        }

        protected void NameClick(int id)
        {
            NavigationManagerReference.NavigateTo($"/Libraries/Details/{id}");
        }

        protected async void PageIndexChanged(int pageIndex)
        {
            pager.PageIndex = pageIndex;
            pager.PageNumber = pageIndex + 1;

            await DisplayData();

            StateHasChanged();
        }

        public string EditorFormTitle { get; set; } = "CREATE";

        protected void ShowEditorForm()
        {
            EditorFormTitle = "CREATE";
            this.model = new LibraryModel();
            this.model.ParentKey = ParentKey; // 
            EditorFormReference.Show();
        }

        protected void EditBy(LibraryModel model)
        {
            EditorFormTitle = "EDIT";
            this.model = new LibraryModel();
            this.model = model;
            this.model.ParentKey = ParentKey; // 
            EditorFormReference.Show();
        }

        protected void DeleteBy(LibraryModel model)
        {
            this.model = model;
            DeleteDialogReference.Show();
        }

        protected void ToggleBy(LibraryModel model)
        {
            this.model = model;
            IsInlineDialogShow = true;
        }

        protected async void DownloadBy(LibraryModel model)
        {
            if (!string.IsNullOrEmpty(model.FileName))
            {
                byte[] fileBytes = await FileStorageManager.DownloadAsync(model.FileName, "");
                if (fileBytes != null)
                {
                    // DownCount
                    model.DownCount = model.DownCount + 1;
                    await UploadRepositoryAsyncReference.EditAsync(model);

                    await FileUtil.SaveAs(JSRuntime, model.FileName, fileBytes);
                }
            }
        }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        [Inject]
        public IFileStorageManager FileStorageManager { get; set; }

        protected async void CreateOrEdit()
        {
            EditorFormReference.Hide();
            this.model = new LibraryModel();
            await DisplayData();
        }

        protected async void DeleteClick()
        {
            if (!string.IsNullOrEmpty(model?.FileName))
            {
                // 첨부 파일 삭제 
                await FileStorageManager.DeleteAsync(model.FileName, "");
            }

            await UploadRepositoryAsyncReference.DeleteAsync(this.model.Id);
            DeleteDialogReference.Hide();
            this.model = new LibraryModel();
            await DisplayData();
        }

        protected void ToggleClose()
        {
            IsInlineDialogShow = false;
            this.model = new LibraryModel();
        }

        protected async void ToggleClick()
        {
            this.model.IsPinned = (this.model?.IsPinned == true) ? false : true;

            await UploadRepositoryAsyncReference.EditAsync(this.model);
            IsInlineDialogShow = false;
            this.model = new LibraryModel();
            await DisplayData();
        }

        #region Search
        private string searchQuery = "";

        protected async void Search(string query)
        {
            pager.PageIndex = 0;

            this.searchQuery = query;

            await DisplayData();

            StateHasChanged();
        }
        #endregion

        protected void DownloadExcel()
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Libraries");

                var tableBody = worksheet.Cells["B2:B2"].LoadFromCollection(
                    (from m in models select new { m.Created, m.Name, m.Title, m.DownCount, m.FileName })
                    , true);

                var uploadCol = tableBody.Offset(1, 1, models.Count, 1);
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

                FileUtil.SaveAs(JSRuntime, $"{DateTime.Now.ToString("yyyyMMddhhmmss")}_Libraries.xlsx", package.GetAsByteArray());
            }
        }

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
    }
}
