using DulPager;
using Hawaso.Models;
using Hawaso.Pages.Logins.Components;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hawaso.Pages.Logins
{
    public partial class Manage
    {
        [Inject]
        public ILoginRepositoryAsync LoginRepositoryAsync { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        private DulPagerBase pager = new DulPagerBase()
        {
            PageNumber = 1,
            PageIndex = 0,
            PageSize = 10,
            PagerButtonCount = 5
        };

        private List<Login> logins;

        public string EditorFormTitle { get; set; } = "ADD";

        public Login Login { get; set; } = new Login();

        public LoginEditorForm LoginEditorForm { get; set; }

        public LoginDeleteDialog LoginDeleteDialog { get; set; }

        public bool IsInlineDialogShow { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await DisplayData();
        }

        private async Task DisplayData()
        {
            var articleSet = await LoginRepositoryAsync.GetAllAsync(pager.PageIndex, pager.PageSize);
            pager.RecordCount = articleSet.TotalRecords;
            logins = articleSet.Records.ToList();
        }

        private async void PageIndexChanged(int pageIndex)
        {
            pager.PageIndex = pageIndex;
            pager.PageNumber = pageIndex + 1;

            await DisplayData();

            StateHasChanged();
        }

        protected void btnCreate_Click()
        {
            EditorFormTitle = "ADD";
            Login = new Login();

            LoginEditorForm.Show();
        }

        protected async void SaveOrUpdated()
        {
            LoginEditorForm.Close();

            await DisplayData();

            StateHasChanged();
        }

        protected void EditBy(Login customer)
        {
            EditorFormTitle = "EDIT";
            Login = customer;

            LoginEditorForm.Show();
        }

        protected void DeleteBy(Login customer)
        {
            Login = customer;
            LoginDeleteDialog.Show();
        }

        protected async void btnDelete_Click()
        {
            await LoginRepositoryAsync.DeleteAsync(Login.LoginId);

            LoginDeleteDialog.Close();

            Login = new Login();

            await DisplayData();

            StateHasChanged();
        }

        protected void btnClose_Click()
        {
            IsInlineDialogShow = false;
            Login = new Login();
        }
    }
}
