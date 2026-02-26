using Microsoft.AspNetCore.Components;

namespace Hawaso.Pages.Logins.Components;

public partial class LoginEditorForm
{
    [Parameter]
    public RenderFragment EditorFormTitle { get; set; } = default!;

    [Parameter]
    public Login Model { get; set; } = default!;

    [Parameter]
    public Action? SaveOrUpdated { get; set; }

    [Parameter]
    public EventCallback<bool> ChangeCallback { get; set; }

    [Inject]
    public ILoginRepositoryAsync LoginRepositoryAsync { get; set; } = default!;

    public bool IsShow { get; set; }

    public void Show() => IsShow = true;

    public void Close() => IsShow = false;

    protected async Task btnSaveOrUpdate_Click()
    {
        if (Model.LoginId == 0)
        {
            await LoginRepositoryAsync.AddAsync(Model);
            SaveOrUpdated?.Invoke();
        }
        else
        {
            await LoginRepositoryAsync.EditAsync(Model);
            await ChangeCallback.InvokeAsync(true);
        }

        IsShow = false;
    }
}