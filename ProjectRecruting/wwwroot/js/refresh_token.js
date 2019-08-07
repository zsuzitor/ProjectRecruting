;;

function refreshToken(func) {
    //var userId = localStorage.getItem('userId');
    var mainToken = localStorage.getItem('mainToken');
    var refreshToken = localStorage.getItem('refreshToken');
    //TODO установить главный токен
    
    goAjaxRequest({
        type: 'POST', data: { refreshToken: refreshToken }, url: '/api/Account/RefreshToken',
        funcSuccess: function (xhr, status, jqXHR) {
            //localStorage.setItem('userId', xhr.access_token)
            localStorage.setItem('mainToken', xhr.access_token)
            localStorage.setItem('refreshToken', xhr.refresh_token)
            func();
        },
        funcComplete: function (xhr, status, jqXHR) {
            tokenRequested = false;//1строкой
            if (xhr.status == 401) {
                showLoginPage();
            }
            
        },
        funcBeforeSend: function () {
            tokenRequested = true;
        }
    });
    
}




;;;