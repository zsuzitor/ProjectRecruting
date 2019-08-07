;;;

// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


function setMainContent(content) {
    document.getElementById('mainBody').innerHTML = content;
}


//функция отрисовки страницы логина
function showLoginPage() {
    //return;
    //alert('showLoginPage');
    goAjaxRequest({
        type: 'GET', data: {}, url: '/Account/Login',
        funcSuccess: function (xhr, status, jqXHR) {
            setMainContent(xhr);
            //var ggg = 10;
        },
        funcError: function (xhr, status, jqXHR) {
            alert('ошибка');
        }
        //funcComplete: function (xhr, status, jqXHR) {
        //    if (xhr.status == 401) {
        //        showLoginPage();
        //    }
        //}
    });
}

function showRegisterPage() {
    goAjaxRequest({
        type: 'GET', data: {}, url: '/Account/Register',
        funcSuccess: function (xhr, status, jqXHR) {
            setMainContent( xhr);
            //var ggg = 10;
        },
        funcError: function (xhr, status, jqXHR) {
            alert('ошибка');
        }
        //funcComplete: function (xhr, status, jqXHR) {
        //    if (xhr.status == 401) {
        //        showLoginPage();
        //    }
        //}
    });
}



function tryRegister() {

    var pass = document.getElementById('Password').value;
    var email = document.getElementById('Email').value;
    var confpass = document.getElementById('ConfirmPassword').value;

    goAjaxRequest({
        type: 'POST', data: {
            Email: email,
            Password: pass,
            ConfirmPassword: confpass
        }, url: '/api/Account/Register',
        funcSuccess: function (xhr, status, jqXHR) {

            localStorage.setItem('userId', xhr.access_token);
            localStorage.setItem('mainToken', xhr.username);
            localStorage.setItem('refreshToken', xhr.refresh_token);

            SetMainPage();
            //var ggg = 10;
        },
        funcError: function (xhr, status, jqXHR) {
            alert('ошибка');
        }
    });

}

function tryLogin() {

    var pass = document.getElementById('Password').value;
    var email = document.getElementById('Email').value;

    goAjaxRequest({
        type: 'POST', data: {
            username: email,
            password: pass
        }, url: '/api/Account/Login',
        funcSuccess: function (xhr, status, jqXHR) {

            localStorage.setItem('userId', xhr.access_token);
            localStorage.setItem('mainToken', xhr.username);
            localStorage.setItem('refreshToken', xhr.refresh_token);

            SetMainPage();
            //var ggg = 10;
        },
        funcError: function (xhr, status, jqXHR) {
            alert('ошибка');
        }
    });

}


function SetMainPage() {
    goAjaxRequest({
        type: 'GET', data: {
        }, url: '/',
        funcSuccess: function (xhr, status, jqXHR) {
            setMainContent(xhr);
            //var ggg = 10;
        },
        funcError: function (xhr, status, jqXHR) {
            alert('ошибка');
        }
    });
}

function loadCompanysList() {
    goAjaxRequest({
        type: 'GET', data: {
        }, url: '/api/User/GetActualCompanys',
        funcSuccess: function (xhr, status, jqXHR) {
            document.getElementById('allCompanys').innerHTML=xhr;
            //var ggg = 10;
        },
        funcError: function (xhr, status, jqXHR) {
            alert('ошибка');
        }
    });
    

}

function loadMyCompanysList() {
    goAjaxRequest({
        type: 'GET', data: {
        }, url: '/api/User/GetUserCompanys',
        funcSuccess: function (xhr, status, jqXHR) {
            document.getElementById('myCompanys').innerHTML = xhr;
            //var ggg = 10;
        },
        funcError: function (xhr, status, jqXHR) {
            alert('ошибка');
        }
    });


}

function showCreateCompany() {
    goAjaxRequest({
        type: 'GET', data: {
        }, url: '/Home/CreateCompany',
        funcSuccess: function (xhr, status, jqXHR) {
            document.getElementById('createCompany').innerHTML = xhr;
            //var ggg = 10;
        },
        funcError: function (xhr, status, jqXHR) {
            alert('ошибка');
        }
    });


}


function showCompanysPage() {
    goAjaxRequest({
        type: 'GET', data: {
        }, url: '/Home/CompanysPage',
        funcSuccess: function (xhr, status, jqXHR) {
            setMainContent(xhr);
            loadCompanysList();
            //var ggg = 10;
        },
        funcError: function (xhr, status, jqXHR) {
            alert('ошибка');
        }
    });
}

function showProjectsPage() {

}
function showCompetencePage() {

}

;;;