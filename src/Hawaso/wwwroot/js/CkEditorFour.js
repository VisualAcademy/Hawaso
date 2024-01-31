//[1] CKEditor 자바스크립트 인터롭 코드 작성
window.CkEditorFour = (() => {
    const editors = {};

    return {
        init(id, dotNetReference) {
            //var editor = CKEDITOR.replace(document.getElementById(id));
            var editor = CKEDITOR.replace(document.getElementById(id), {
                height: '200px',
                //extraPlugins: 'codesnippet',
                //codeSnippet_theme: 'vs',
                //filebrowserImageUploadUrl: '/Handlers/ImageUploadHandler?BoardName=Image&Num=1'
            });
            editors[id] = editor;
            editor.on('change', function (evt) {
                var data = evt.editor.getData();
                dotNetReference.invokeMethodAsync('EditorFourDataChanged', data);
            });
        },
        destroy(id) {
            editors[id].destroy()
                .then(() => delete editors[id])
                .catch(error => console.log(error));
        }
    };
})();
