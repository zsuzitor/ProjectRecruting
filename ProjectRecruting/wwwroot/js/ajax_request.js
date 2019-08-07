;;
//-------------обычный post
//goAjaxRequest({type:'POST',data:{name:'nm1'},url:'/add-example'});

//-------FILE IMAGE
// var data = new FormData();
// var massFiles1 = document.getElementById('upLoadImagesId').files;
//
// $.each(massFiles1, function (key, value) {
//     data.append('file', value);//name обязательно такое же как и в upload.single('file')
// });
// goAjaxRequest({type:'POST',data:data,url:'/api/v1/example-upload-file'});


function goAjaxRequest(obj, fileLoad) {
    if (!obj.type)
        obj.type = 'GET';
    //if (!obj.dataType)
    //  obj.dataType = 'json';//html

    let ajaxObj = {
        type: obj.type,
        data: obj.data,
        url: obj.url,
        //processData: false, // Не обрабатываем файлы
        //contentType: false, // Так jQuery скажет серверу что это строковой запрос
        success: function (xhr, status, jqXHR) {
            //if(jqXHR.status==200){//EXAMPLE STATUS
            //DO SOMETHING
            //}
            if (obj.funcSuccess) {
                try {
                    obj.funcSuccess(xhr, status, jqXHR);
                }
                catch (e) {
                    console.log('Ошибка ' + e.name + ":" + e.message + "\n" + e.stack);
                }

            }
        },
        error: function (xhr, status, error) {
            //alert("ошибка загрузки");
            if (obj.funcError)
                obj.funcError(xhr, status, error);
        },
        // shows the loader element before sending.
        beforeSend: function () {
            if (obj.funcBeforeSend)
                obj.funcBeforeSend();
            //  PreloaderShowChange(true);
        },
        headers: { 'Authorization': localStorage.getItem('mainToken') },
        // hides the loader after completion of request, whether successfull or failor.
        complete: function (xhr, status, jqXHR) {
            if (xhr.status == 401) {
                //refreshToken();
            }
            if (obj.funcComplete) {
                try {
                    obj.funcComplete(xhr, status, jqXHR);
                }
                catch (e) {
                    console.log('Ошибка ' + e.name + ":" + e.message + "\n" + e.stack);
                }
            }

            //PreloaderShowChange(false);
        },
        dataType: obj.dataType//'html'
    };
    // if(obj.dataType){
    //     ajaxObj.dataType=obj.dataType
    // }

    if (fileLoad) {
        //processData: false, // Не обрабатываем файлы
        //contentType: false,
        ajaxObj.processData = false;
        ajaxObj.contentType = false;
    }

    trySend(ajaxObj);

    
}

function trySend(ajaxObj) {
    if (tokenRequested) {
        setTimeout(function () {
            trySend(ajaxObj);
        }, 200);
    }
    else {
        $.ajax(ajaxObj);
    }
}

;;;