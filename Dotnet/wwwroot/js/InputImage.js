function CreateImagePicker(csharp, uid){
    let preview = document.getElementById(uid + '-preview');
    let noImage = document.getElementById(uid + '-no-image');
    let chooser = document.getElementById(uid + '-file-chooser');
    
    let getImageBase64 = (file, callback) => {
        var reader = new FileReader();
        reader.readAsDataURL(file);
        reader.onload = function () {
            callback(reader.result);
        };
        reader.onerror = function (error) {
            console.log('Error: ', error);
        };
    }
    
    chooser.addEventListener('change', (event) => {
        let file = event.target.files[0];
        if(!file)
            return;
        getImageBase64(file, (base64) => {
            noImage.style.display = 'none';
            preview.style.display = '';
            preview.setAttribute('src', base64);
            csharp.invokeMethodAsync("updateValue", base64);
        });
    })
}