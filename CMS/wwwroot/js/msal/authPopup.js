// Create the main myMSALObj instance
// configuration parameters are located at authConfig.js
const myMSALObj = new msal.PublicClientApplication(msalConfig);

let username = "";

function handleResponse(response) {

    /**
     * To see the full list of response object properties, visit:
     * https://github.com/AzureAD/microsoft-authentication-library-for-js/blob/dev/lib/msal-browser/docs/request-response-object.md#response
     */

    if (response !== null) {
        username = response.account.username;

        showWelcomeMessage(username);
        if (window['UseRedirect'] == true) {
            redirectLogin(response.account.username, response.accessToken, response.idToken, response.tenantId, response.uniqueId);
        }
    } else {
        selectAccount();
    }
}

function redirectLogin(username, token, idtoken, id1, id2) {

    var form = document.createElement("form");
    form.setAttribute("method", 'post');
    form.setAttribute("action", '/login/external');

    var u = document.createElement("input"); //input element, text
    u.setAttribute('type', "text");
    u.setAttribute('name', "UserName");
    u.setAttribute('value', username);

    var t = document.createElement("input"); //input element, text
    t.setAttribute('type', "text");
    t.setAttribute('name', "AccessToken");
    t.setAttribute('value', token);

    var t2 = document.createElement("input"); //input element, text
    t2.setAttribute('type', "text");
    t2.setAttribute('name', "IdToken");
    t2.setAttribute('value', idtoken);

    var i1 = document.createElement("input"); //input element, text
    i1.setAttribute('type', "text");
    i1.setAttribute('name', "ExternalIdentifier1");
    i1.setAttribute('value', id1);

    var i2 = document.createElement("input"); //input element, text
    i2.setAttribute('type', "text");
    i2.setAttribute('name', "ExternalIdentifier2");
    i2.setAttribute('value', id2);

    form.appendChild(u);
    form.appendChild(t);
    form.appendChild(t2);

    form.appendChild(i1);
    form.appendChild(i2);

    document.body.appendChild(form);
    form.submit(); //auto submit
}

function signIn() {

    /**
     * You can pass a custom request object below. This will override the initial configuration. For more information, visit:
     * https://github.com/AzureAD/microsoft-authentication-library-for-js/blob/dev/lib/msal-browser/docs/request-response-object.md#request
     */

    myMSALObj.loginPopup(loginRequest)
        .then(handleResponse)
        .catch(error => {
            console.error(error);
        });
}

function signOut() {

    /**
     * You can pass a custom request object below. This will override the initial configuration. For more information, visit:
     * https://github.com/AzureAD/microsoft-authentication-library-for-js/blob/dev/lib/msal-browser/docs/request-response-object.md#request
     */

    const logoutRequest = {
        account: myMSALObj.getAccountByUsername(username),
        postLogoutRedirectUri: msalConfig.auth.redirectUri,
        mainWindowRedirectUri: msalConfig.auth.redirectUri
    };

    myMSALObj.logoutPopup(logoutRequest);
}

function getTokenPopup(request) {

    /**
     * See here for more info on account retrieval:
     * https://github.com/AzureAD/microsoft-authentication-library-for-js/blob/dev/lib/msal-common/docs/Accounts.md
     */
    request.account = myMSALObj.getAccountByUsername(username);

    return myMSALObj.acquireTokenSilent(request)
        .catch(error => {
            console.warn("silent token acquisition fails. acquiring token using popup");
            if (error instanceof msal.InteractionRequiredAuthError) {
                // fallback to interaction when silent call fails
                return myMSALObj.acquireTokenPopup(request)
                    .then(tokenResponse => {
                        return tokenResponse;
                    }).catch(error => {
                        console.error(error);
                    });
            } else {
                console.warn(error);
            }
    });
}

