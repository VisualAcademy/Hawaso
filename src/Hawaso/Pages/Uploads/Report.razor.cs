using ChartJs.Blazor.ChartJS.BarChart;
using ChartJs.Blazor.ChartJS.BarChart.Axes;
using ChartJs.Blazor.ChartJS.Common.Axes;
using ChartJs.Blazor.ChartJS.Common.Axes.Ticks;
using ChartJs.Blazor.ChartJS.Common.Properties;
using ChartJs.Blazor.ChartJS.Common.Wrappers;
using ChartJs.Blazor.Util;
using Microsoft.AspNetCore.Components;

namespace Hawaso.Pages.Uploads;

public partial class Report
{
    [Inject]
    public IUploadRepository UploadRepositoryReference { get; set; } = default!;

    private BarConfig _barChartConfig = default!;
    private BarDataset<DoubleWrapper> _barDataSet = default!;

    protected override async Task OnInitializedAsync()
    {
        _barChartConfig = new BarConfig
        {
            Options = new BarOptions
            {
                Legend =
                {
                    Display = false
                },
                Title = new OptionsTitle
                {
                    Display = true,
                    Text = $"지난 1년동안의 {nameof(UploadApp)} 글 수"
                },
                Scales = new BarScales
                {
                    XAxes = new List<CartesianAxis>
                    {
                        new BarCategoryAxis
                        {
                            BarPercentage = 0.5,
                            BarThickness = BarThickness.Flex
                        }
                    },
                    YAxes = new List<CartesianAxis>
                    {
                        new BarLinearCartesianAxis
                        {
                            Ticks = new LinearCartesianTicks
                            {
                                BeginAtZero = true
                            }
                        }
                    }
                },
                Responsive = true
            }
        };

        var backgroundColors = new List<string>();
        var labels = new List<string>();
        var values = new List<double>();

        for (var month = 1; month <= 12; month++)
        {
            labels.Add(month.ToString());
            backgroundColors.Add(ColorUtil.RandomColorString());
        }

        var monthlyCreateCounts = await UploadRepositoryReference.GetMonthlyCreateCountAsync();

        for (var month = 1; month <= 12; month++)
        {
            values.Add(monthlyCreateCounts.TryGetValue(month, out var count) ? count : 0);
        }

        _barChartConfig.Data.Labels.AddRange(labels);

        _barDataSet = new BarDataset<DoubleWrapper>
        {
            BackgroundColor = backgroundColors.ToArray(),
            BorderWidth = 0,
            HoverBackgroundColor = ColorUtil.RandomColorString(),
            HoverBorderColor = ColorUtil.RandomColorString(),
            HoverBorderWidth = 1,
            BorderColor = "#ffffff"
        };

        _barDataSet.AddRange(values.Wrap());
        _barChartConfig.Data.Datasets.Add(_barDataSet);
    }
}