using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Reflection;
using System.Threading;

namespace BlazorInputFile
{
    // This is used on WebAssembly
    internal sealed class SharedMemoryFileListEntryStream : FileListEntryStream
    {
        private static readonly Type? MonoWebAssemblyJSRuntimeType =
            Type.GetType("Mono.WebAssembly.Interop.MonoWebAssemblyJSRuntime, Mono.WebAssembly.Interop");

        private static MethodInfo? _cachedInvokeUnmarshalledMethodInfo;

        public SharedMemoryFileListEntryStream(
            IJSRuntime jsRuntime,
            ElementReference inputFileElement,
            FileListEntryImpl file)
            : base(jsRuntime, inputFileElement, file)
        {
        }

        public static bool IsSupported(IJSRuntime jsRuntime)
        {
            return MonoWebAssemblyJSRuntimeType is not null
                && MonoWebAssemblyJSRuntimeType.IsAssignableFrom(jsRuntime.GetType());
        }

        protected override async Task<int> CopyFileDataIntoBuffer(
            long sourceOffset,
            byte[] destination,
            int destinationOffset,
            int maxBytes,
            CancellationToken cancellationToken)
        {
            await _jsRuntime.InvokeAsync<string>(
                "BlazorInputFile.ensureArrayBufferReadyForSharedMemoryInterop",
                cancellationToken,
                _inputFileElement,
                _file.Id);

            var methodInfo = GetCachedInvokeUnmarshalledMethodInfo();

            var result = methodInfo.Invoke(_jsRuntime, new object[]
            {
                "BlazorInputFile.readFileDataSharedMemory",
                new ReadRequest
                {
                    InputFileElementReferenceId = _inputFileElement.Id ?? string.Empty,
                    FileId = _file.Id,
                    SourceOffset = sourceOffset,
                    Destination = destination,
                    DestinationOffset = destinationOffset,
                    MaxBytes = maxBytes,
                }
            });

            return result is int bytesRead
                ? bytesRead
                : throw new InvalidOperationException("InvokeUnmarshalled did not return an Int32 result.");
        }

        private static MethodInfo GetCachedInvokeUnmarshalledMethodInfo()
        {
            if (_cachedInvokeUnmarshalledMethodInfo is not null)
            {
                return _cachedInvokeUnmarshalledMethodInfo;
            }

            var runtimeType = MonoWebAssemblyJSRuntimeType
                ?? throw new InvalidOperationException("MonoWebAssemblyJSRuntime is not available.");

            foreach (var possibleMethodInfo in runtimeType.GetMethods())
            {
                if (possibleMethodInfo.Name == "InvokeUnmarshalled" &&
                    possibleMethodInfo.GetParameters().Length == 2)
                {
                    _cachedInvokeUnmarshalledMethodInfo =
                        possibleMethodInfo.MakeGenericMethod(typeof(ReadRequest), typeof(int));
                    break;
                }
            }

            return _cachedInvokeUnmarshalledMethodInfo
                ?? throw new InvalidOperationException("Could not find the 2-param overload of InvokeUnmarshalled.");
        }

        private struct ReadRequest
        {
            public string InputFileElementReferenceId;
            public int FileId;
            public long SourceOffset;
            public byte[] Destination;
            public int DestinationOffset;
            public int MaxBytes;
        }
    }
}