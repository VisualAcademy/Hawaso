using ChartJs.Blazor.ChartJS.BarChart;
using ChartJs.Blazor.ChartJS.BarChart.Axes;
using ChartJs.Blazor.ChartJS.Common.Axes;
using ChartJs.Blazor.ChartJS.Common.Axes.Ticks;
using ChartJs.Blazor.ChartJS.Common.Properties;
using ChartJs.Blazor.ChartJS.Common.Wrappers;
using ChartJs.Blazor.Util;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zero.Models;

namespace Hawaso.Pages.BriefingLogs
{
    public partial class Report
    {
        [Inject]
        public IBriefingLogRepository UploadRepositoryReference { get; set; }

        private BarConfig _barChartConfig;
        private BarDataset<DoubleWrapper> _barDataSet;

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

            List<string> backgroundColors = new List<string>(); // 배경색: 랜덤
            List<string> labels = new List<string>(); // 1월부터 12월까지
            List<double> values = new List<double>(); // 1월부터 12월까지의 데이터

            for (int i = 1; i <= 12; i++)
            {
                labels.Add($"{i}");
                backgroundColors.Add(ColorUtil.RandomColorString());
            }

            var sortedList = await UploadRepositoryReference.GetMonthlyCreateCountAsync();
            for (int i = 1; i <= 12; i++)
            {
                values.Add(sortedList[i]);
            }

            _barChartConfig.Data.Labels.AddRange(labels.ToArray());

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
}
