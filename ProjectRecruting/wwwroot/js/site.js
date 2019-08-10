;;;

// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.




//------------------------------SHOW JSON--------------------------
function getHtmlCompanysList(json) {
    let data =json;//= JSON.parse(json);
    let res = "";
    res += "<div>";
    if (data) {
        for (let i = 0; i < data.length; ++i) {
            res += "<div style='border:1px solid black;'>";
            res += "<p>" + data[i].Id + "</p>";
            res += "<p>" + data[i].Name + "</p>";
            res += "<p>" + data[i].Description + "</p>";
            res += "<p>" + data[i].Number + "</p>";
            res += "<p>" + data[i].Email + "</p>";
            
            res += "</div>";
        }
    }
    res += "</div>";

    return res;
}

function getHtmlProjectsList(json) {
    let data = json;//= JSON.parse(json);
    let res = "";
    res += "<div>";
    if (data) {
        for (let i = 0; i < data.length; ++i) {
            res += "<div style='border:1px solid black;'>";
            res += "<p>" + data[i].ProjectId + "</p>";
            res += "<p>" + data[i].Name + "</p>";
            res += "<p>" + data[i].Status + "</p>";
           
            res += "</div>";
        }
    }
    res += "</div>";

    return res;
}

function getHtmlCompetenceList(json) {
    let data = json;//= JSON.parse(json);
    let res = "";
    res += "<div>";
    if (data) {
        for (let i = 0; i < data.length; ++i) {
            res += "<div style='border:1px solid black;'>";
            res += "<p>" + data[i].Id + "</p>";
            res += "<p>" + data[i].Name + "</p>";
            
            res += "</div>";
        }
    }
    res += "</div>";

    return res;
}




//------------------------------END SHOW JSON--------------------------






function setMainContent(content) {
    document.getElementById('mainBody').innerHTML = content;
}



function SetMainPage() {
    goAjaxRequest({
        type: 'GET', data: {
        }, url: '/home/main-page/',
        funcSuccess: function (xhr, status, jqXHR) {
            setMainContent(xhr);
            //var ggg = 10;
        },
        funcError: function (xhr, status, jqXHR) {
            alert('ошибка');
        }
    });
}



///---------------------------SHOWS PAGES---------------


//функция отрисовки страницы логина
function showLoginPage() {
    //return;
    //alert('showLoginPage');
    goAjaxRequest({
        type: 'GET', data: {}, url: '/account/login',
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
        type: 'GET', data: {}, url: '/account/register',
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




function showCreateCompanyForm() {
    let div = document.getElementById('createCompany');
    if (div.innerHTML.trim())
        return;
    goAjaxRequest({
        type: 'GET', data: {
        }, url: '/home/create-company',
        funcSuccess: function (xhr, status, jqXHR) {
            div.innerHTML = xhr;
        },
        funcError: function (xhr, status, jqXHR) {
            alert('ошибка');
        }
    });


}

function showCompanysPage() {
    goAjaxRequest({
        type: 'GET', data: {
        }, url: '/home/companys-page',
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

    goAjaxRequest({
        type: 'GET', data: {
        }, url: '/api/user/get-actual-project',
        funcSuccess: function (xhr, status, jqXHR) {
            let html_ = getHtmlProjectsList(xhr);
            setMainContent(html_);
            //var ggg = 10;
        },
        funcError: function (xhr, status, jqXHR) {
            alert('ошибка');
        }
    });


}
function showCompetencePage() {
    goAjaxRequest({
        type: 'GET', data: {
        }, url: '/api/user/get-actual-competences',
        funcSuccess: function (xhr, status, jqXHR) {
            let html_ = getHtmlCompetenceList(xhr);
            setMainContent(html_);
            //var ggg = 10;
        },
        funcError: function (xhr, status, jqXHR) {
            alert('ошибка');
        }
    });
}




///---------------------------END SHOWS PAGES---------------




function tryRegister() {

    var pass = document.getElementById('Password').value;
    var email = document.getElementById('Email').value;
    var confpass = document.getElementById('ConfirmPassword').value;

    goAjaxRequest({
        type: 'POST', data: {
            Email: email,
            Password: pass,
            ConfirmPassword: confpass
        }, url: '/api/account/register',
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
        }, url: '/api/account/login',
        funcSuccess: function (xhr, status, jqXHR) {

            localStorage.setItem('userId', xhr.username);
            localStorage.setItem('mainToken', xhr.access_token);
            localStorage.setItem('refreshToken', xhr.refresh_token);

            SetMainPage();
            //var ggg = 10;
        },
        funcError: function (xhr, status, jqXHR) {
            alert('ошибка');
        }
    });

}


function signOut() {
    goAjaxRequest({
        type: 'POST', data: {
        }, url: '/account/logout/',
        data: {
            userId: localStorage.getItem('userId'),
            refreshToken: localStorage.getItem('refreshToken')
        },
        funcComplete: function (xhr, status, jqXHR) {
            localStorage.removeItem('userId');
            localStorage.removeItem('mainToken');
            localStorage.removeItem('refreshToken');
            showLoginPage();
        },
        funcError: function (xhr, status, jqXHR) {
            alert('ошибка');
        }
    });
    
}



function loadCompany() {
    
    //goAjaxRequest({
    //    type: 'GET', data: {
    //    }, url: '/api/user/get-user-companys',
    //    funcSuccess: function (xhr, status, jqXHR) {
    //        let html_ = getHtmlCompanysList(xhr);
    //        div.innerHTML = html_;
    //        //var ggg = 10;
    //    },
    //    funcError: function (xhr, status, jqXHR) {
    //        alert('ошибка');
    //    }
    //});


}





function loadCompanysList() {
    let div = document.getElementById('allCompanys');
    if (div.innerHTML.trim())
        return;
    goAjaxRequest({
        type: 'GET', data: {
        }, url: '/api/user/get-actual-companys',
        funcSuccess: function (xhr, status, jqXHR) {
            let html_ = getHtmlCompanysList(xhr);
            div.innerHTML = html_;
            //var ggg = 10;
        },
        funcError: function (xhr, status, jqXHR) {
            alert('ошибка');
        }
    });

}

function loadMyCompanysList() {
    let div = document.getElementById('myCompanys');
    if (div.innerHTML.trim())
        return;
    goAjaxRequest({
        type: 'GET', data: {
        }, url: '/api/user/get-user-companys',
        funcSuccess: function (xhr, status, jqXHR) {
            let html_ = getHtmlCompanysList(xhr);
            div.innerHTML = html_;
            //var ggg = 10;
        },
        funcError: function (xhr, status, jqXHR) {
            alert('ошибка');
        }
    });


}



function saveNewCompany() {
    
    var data = new FormData();
    var massFiles1 = document.getElementById('newCompanyImage').files;

    $.each(massFiles1, function (key, value) {
        data.append('uploadedFile', value);//name обязательно такое же как и в upload.single('file') 
    });
    data.append('Name',document.getElementById('newCompanyName').value);
    data.append('Description', document.getElementById('newCompanyDescription').value);
    data.append('Number', document.getElementById('newCompanyNumber').value);
    data.append('Email', document.getElementById('newCompanyEmail').value);
    

    goAjaxRequest({
        type: 'POST', data: data, url: '/api/company/create-company',
        funcSuccess: function (xhr, status, jqXHR) {
            document.getElementById('createdCompany').innerHTML='id созданной компании--'+xhr.Id;//#TOdo тут должна быть ссылка
            
            //var ggg = 10;
        }
    }, true);


}


;;;