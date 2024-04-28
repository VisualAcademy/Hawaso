// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
window.attachmentFunctions = {
    openPreview: function (attachmentId) {
        const url = '/MemoDownload/FilePreview';
        window.open(`${url}?id=${attachmentId}`, '_blank', 'width=800,height=700');
    }
};
