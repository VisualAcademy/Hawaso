using Azunt.ResourceManagement;
using Microsoft.AspNetCore.Components;

namespace Azunt.Web.Components.Pages.Resources.Components;

public partial class ModalForm : ComponentBase
{
    #region Properties
    public bool IsShow { get; set; } = false;
    #endregion

    #region Public Methods
    public void Show() => IsShow = true;
    public void Hide()
    {
        IsShow = false;
        StateHasChanged();
    }
    #endregion

    #region Parameters
    [Parameter]
    public string UserName { get; set; } = "";

    [Parameter]
    public RenderFragment EditorFormTitle { get; set; } = null!;

    [Parameter]
    public Resource ModelSender { get; set; } = null!;

    public Resource ModelEdit { get; set; } = null!;

    protected override void OnParametersSet()
    {
        if (ModelSender != null)
        {
            ModelEdit = new Resource
            {
                Id = ModelSender.Id,
                Alias = ModelSender.Alias,
                Title = ModelSender.Title,
                Route = ModelSender.Route,
                Description = ModelSender.Description,
                GroupOrder = ModelSender.GroupOrder,
                AppName = ModelSender.AppName,
                Step = ModelSender.Step
            };
        }
        else
        {
            ModelEdit = new Resource();
        }
    }

    [Parameter]
    public Action CreateCallback { get; set; } = null!;

    [Parameter]
    public EventCallback<bool> EditCallback { get; set; }

    [Parameter]
    public string ParentKey { get; set; } = "";
    #endregion

    #region Injectors
    [Inject]
    public IResourceRepository RepositoryReference { get; set; } = null!;
    #endregion

    #region Event Handlers
    protected async void CreateOrEditClick()
    {
        ModelSender.Alias = ModelEdit.Alias;
        ModelSender.Title = ModelEdit.Title;
        ModelSender.Route = ModelEdit.Route;
        ModelSender.Description = ModelEdit.Description;
        ModelSender.GroupOrder = ModelEdit.GroupOrder;
        ModelSender.AppName = ModelEdit.AppName;
        ModelSender.Step = ModelEdit.Step;

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
    #endregion
}
