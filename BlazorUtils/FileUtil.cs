using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace BlazorUtils
{
    public static class FileUtil
    {
        public static ValueTask SaveAs(this IJSRuntime js, string filename, byte[] data)
            => js.InvokeVoidAsync(
                "saveAsFile",
                filename,
                Convert.ToBase64String(data));

        public static ValueTask SaveAsExcel(this IJSRuntime js, string url)
            => js.InvokeVoidAsync("saveAsExcel", url);
    }
}
