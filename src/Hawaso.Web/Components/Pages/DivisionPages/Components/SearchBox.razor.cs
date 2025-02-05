using Microsoft.AspNetCore.Components;
using System.Timers;

namespace Hawaso.Web.Components;

public partial class SearchBox : ComponentBase, IDisposable
{
    #region Fields
    private string searchQuery;
    private System.Timers.Timer debounceTimer;
    #endregion

    #region Parameters
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object> AdditionalAttributes { get; set; }

    // �ڽ� ������Ʈ���� �߻��� ������ �θ� ������Ʈ���� ����
    [Parameter]
    public EventCallback<string> SearchQueryChanged { get; set; }

    [Parameter]
    public int Debounce { get; set; } = 300;
    #endregion

    #region Properties
    public string SearchQuery
    {
        get => searchQuery;
        set
        {
            searchQuery = value;
            debounceTimer.Stop(); // �ؽ�Ʈ�ڽ��� ���� �Է��ϴ� ���� Ÿ�̸� ����
            debounceTimer.Start(); // Ÿ�̸� ����(300�и��� �Ŀ� �� �� �� ����)
        }
    }
    #endregion

    #region Lifecycle Methods
    /// <summary>
    /// ������ �ʱ�ȭ �̺�Ʈ ó����
    /// </summary>
    protected override void OnInitialized()
    {
        debounceTimer = new System.Timers.Timer();
        debounceTimer.Interval = Debounce;
        debounceTimer.AutoReset = false; // �� �ѹ� ���� 
        debounceTimer.Elapsed += SearchHandler;
    }
    #endregion

    #region Event Handlers
    protected void Search() => SearchQueryChanged.InvokeAsync(SearchQuery); // �θ��� �޼��忡 �˻��� ����

    protected async void SearchHandler(object source, ElapsedEventArgs e) => await InvokeAsync(() => SearchQueryChanged.InvokeAsync(SearchQuery)); // �θ��� �޼��忡 �˻��� ����
    #endregion

    #region Public Methods
    public void Dispose() => debounceTimer.Dispose();
    #endregion
}