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
            res += getHtmlCompany(data[i]);
        }
    }
    res += "</div>";

    return res;
}

function getHtmlCompany(json) {
    let data = json;//= JSON.parse(json);
    let res = "";
    res += "<div style='border:1px solid black;'>";
    res += "<p>" + "<button onclick='loadCompanyPage(\"" + data.Id +"\")'>" + data.Id +"</button>" + "</p>";
    res += "<p>" + data.Name + "</p>";
    res += "<p>" + data.Description + "</p>";
    res += "<p>" + data.Number + "</p>";
    res += "<p>" + data.Email + "</p>";

    res += "</div>";


    return res;
}

function getHtmlProjectsList(json) {
    let data = json;//= JSON.parse(json);
    let res = "";
    res += "<div>";
    if (data) {
        for (let i = 0; i < data.length; ++i) {
            res += getHtmlProject(data[i]);
        }
    }
    res += "</div>";

    return res;
}

function getHtmlProject(json) {
    let data = json;//= JSON.parse(json);
    let res = "";
    res += "<div style='border:1px solid black;'>";
    res += "<p>" + "<button onclick='loadProjectPage(\"" + data.Id + "\")'>" + data.Id + "</button>" + "</p>";
    res += "<p>" + data.Name + "</p>";
    res += "<p>" + data.Status + "</p>";

    res += "</div>";

    return res;
}


function getHtmlCompetenceList(json) {
    let data = json;//= JSON.parse(json);
    let res = "";
    res += "<div>";
    if (data) {
        for (let i = 0; i < data.length; ++i) {
            getHtmlCompetence(data[i]);
        }
    }
    res += "</div>";

    return res;
}
function getHtmlCompetence(json) {
    let data = json;//= JSON.parse(json);
    let res = "";
    res += "<div style='border:1px solid black;'>";
    res += "<p>" + data.Id + "</p>";
    res += "<p>" + data.Name + "</p>";

    res += "</div>";

    return res;
}

function getHtmlTowns(json) {
    let data = json;//= JSON.parse(json);
    let res = "";
    res += "<div>";
    if (data) {
        for (let i = 0; i < data.length; ++i) {
            res += "<p>" + data.Id + "</p>";
            res += "<p>" + data.Name + "</p>";
        }
    }
    res += "</div>";

    return res;
}

function getHtmlCompanyPage(json) {
    let data = json;
    let res = "";
    res += "<div style='border:1px solid black;'>";
    res += "<p>" + data.Id + "</p>";
    res += "<p>" + data.Name + "</p>";
    res += "<p>" + data.Description + "</p>";
    res += "<p>" + data.Number + "</p>";
    res += "<p>" + data.Email + "</p>";
    res += "<p>" + data.CanEdit + "</p>";
    
    res += "</div>";


    return res;
}


function getHtmlProjectPage(json) {
    let data = json;
    let res = "";
    res += "<div style='border:1px solid black;'>";
    res += "<p>" + data.Id + "</p>";//
    res += "<p>" + data.Name + "</p>";
    res += "<p>" + data.Description + "</p>";
    res += "<p>" + data.Payment + "</p>";
    res += "<p>" + data.CompanyId + "</p>";
    res += "<p>" + data.CompanyName + "</p>";
    res += "<p>" + data.CanEdit + "</p>";
    res += "<p>" + data.Status + "</p>";
    if (data.Competences)
        for (let i = 0; i < data.Competences.length; ++i) {
            getHtmlCompetence(data.Competences[i]);
        }
    if (data.Towns)
        for (let i = 0; i < data.Towns.length; ++i) {
            getHtmlTowns(data.Towns[i]);
        }
    if (data.ImagesId)
        for (let i = 0; i < data.Images.length; ++i) {
            res += "<img src='" + data.Images[i].Path + "'/>";//#TODO будет относительный
        }
    res += "</div>";
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

function showCreateProjectForm() {
    let div = document.getElementById('createProject');
    if (div.innerHTML.trim())
        return;
    goAjaxRequest({
        type: 'GET', data: {
        }, url: '/home/create-project',
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
        }, url: '/home/projects-page',
        funcSuccess: function (xhr, status, jqXHR) {
            //let html_ = getHtmlProjectsList(xhr);
            setMainContent(xhr);
            //var ggg = 10;
            loadProjectsList();
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

function loadCompanyPage(id) {
    goAjaxRequest({//
        type: 'GET', data: { id: id}, url: '/api/user/get-company',
        funcSuccess: function (xhr, status, jqXHR) {
            let html_ = getHtmlCompanyPage(xhr);
            setMainContent(html_);
            //var ggg = 10;
        },
        funcError: function (xhr, status, jqXHR) {
            alert('ошибка');
        }
    });

}


function loadProjectPage(id) {
    goAjaxRequest({
        type: 'GET', data: {}, url: '/api/user/get-project/' + id,
        funcSuccess: function (xhr, status, jqXHR) {
            let html_ = getHtmlProjectPage(xhr);
            setMainContent(html_);
            //var ggg = 10;
        },
        funcError: function (xhr, status, jqXHR) {
            alert('ошибка');
        }
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

function loadProjectsList() {
    let div = document.getElementById('allProjects');
    if (div.innerHTML.trim())
        return;
    goAjaxRequest({
        type: 'GET', data: {
        }, url: '/api/user/get-actual-project',
        funcSuccess: function (xhr, status, jqXHR) {
            let html_ = getHtmlProjectsList(xhr);
            div.innerHTML = html_;
            //var ggg = 10;
        },
        funcError: function (xhr, status, jqXHR) {
            alert('ошибка');
        }
    });

}

function loadMyProjectsList() {
    let div = document.getElementById('myProjects');
    if (div.innerHTML.trim())
        return;
    goAjaxRequest({
        type: 'GET', data: {
        }, url: '/api/user/get-user-responsibility-projects',
        funcSuccess: function (xhr, status, jqXHR) {
            let html_ = getHtmlProjectsList(xhr);
            div.innerHTML = html_;
            //var ggg = 10;
        },
        funcError: function (xhr, status, jqXHR) {
            alert('ошибка');
        }
    });


}


function saveNewProject() {

    var data = new FormData();
    var massFiles1 = document.getElementById('newProjectImage').files;

    $.each(massFiles1, function (key, value) {
        data.append('uploadedFile', value);//name обязательно такое же как и в upload.single('file') 
    });
    data.append('Name', document.getElementById('newProjectName').value);
    data.append('Description', document.getElementById('newProjectDescription').value);
    data.append('Payment', document.getElementById('newProjectPayment').value);
    data.append('CompanyId', document.getElementById('newProjectCompanyId').value);
    data.append('Status', document.getElementById('newProjectStatus').value);
    
    goAjaxRequest({
        type: 'POST', data: data, url: '/api/company/create-project',
        funcSuccess: function (xhr, status, jqXHR) {
            document.getElementById('createdProject').innerHTML = 'id созданного проекта--' + xhr.Id;//#TOdo тут должна быть ссылка

            //var ggg = 10;
        }
    }, true);


}



;;;