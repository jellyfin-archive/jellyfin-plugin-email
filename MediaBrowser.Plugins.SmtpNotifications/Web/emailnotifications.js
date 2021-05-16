const SmtpConfigurationPageVar = {
    pluginId: 'cfa0f7f4-4155-4d71-849b-d6598dc4c5bb'
};

function loadUserConfig(page, userId) {
    Dashboard.showLoadingMsg();
    ApiClient.getPluginConfiguration(SmtpConfigurationPageVar.pluginId).then(function (config) {
        const smtpConfig = config.Options.filter(function (c) {
            return userId === c.UserId;
        })[0] || { Enabled: false };

        page.querySelector('#chkEnableSMTP').checked = smtpConfig.Enabled;
        page.querySelector('#txtEmailFrom').value = smtpConfig.EmailFrom || '';
        page.querySelector('#txtEmailTo').value = smtpConfig.EmailTo || '';
        page.querySelector('#txtServer').value = smtpConfig.Server || '';
        page.querySelector('#txtPort').value = smtpConfig.Port || '';
        page.querySelector('#chkEnableSSL').checked = smtpConfig.SSL || false;
        page.querySelector('#chkEnableAuth').checked = smtpConfig.UseCredentials || false;
        page.querySelector('#txtUsername').value = smtpConfig.Username || '';
        page.querySelector('#txtPassword').value = smtpConfig.Password || '';

        Dashboard.hideLoadingMsg();
    });
}

export default function (view) {

    view.querySelector('#selectUser').addEventListener('change', function () {
        loadUserConfig(view, this.value);
    });

    view.querySelector('#testNotification').addEventListener('click', function (event) {
        Dashboard.showLoadingMsg();
        const onError = function () {
            Dashboard.alert('There was an error sending the test email. Please check your email settings and try again.');
        };

        ApiClient.getPluginConfiguration(SmtpConfigurationPageVar.pluginId).then(function (config) {
            if (!config.Options.length) {
                Dashboard.hideLoadingMsg();
                Dashboard.alert('Please configure and save at least one email account.');
            }

            config.Options.map(function (c) {
                ApiClient.ajax({
                    type: 'POST',
                    url: ApiClient.getUrl('Notification/SMTP/Test/' + c.UserId)
                }).catch(onError);
            });

            Dashboard.hideLoadingMsg();
        }, onError);

        event.preventDefault();
    });

    view.querySelector('.smtpConfigurationForm').addEventListener('submit', function (e) {
        console.info('.smtpConfigurationForm.submit');
        Dashboard.showLoadingMsg();
        const form = this;

        ApiClient.getPluginConfiguration(SmtpConfigurationPageVar.pluginId).then(function (config) {
            const userId = form.querySelector('#selectUser').value;
            let smtpConfig = config.Options.filter(function (c) {
                return userId === c.UserId;
            })[0];

            if (!smtpConfig) {
                smtpConfig = {};
                config.Options.push(smtpConfig);
            }

            smtpConfig.UserId = userId;
            smtpConfig.Enabled = form.querySelector('#chkEnableSMTP').checked;
            smtpConfig.EmailFrom = form.querySelector('#txtEmailFrom').value;
            smtpConfig.EmailTo = form.querySelector('#txtEmailTo').value;
            smtpConfig.Server = form.querySelector('#txtServer').value;
            smtpConfig.Port = form.querySelector('#txtPort').value;
            smtpConfig.useCredentials = form.querySelector('#chkEnableAuth').checked;
            smtpConfig.SSL = form.querySelector('#chkEnableSSL').checked;
            smtpConfig.Username = form.querySelector('#txtUsername').value;
            smtpConfig.Password = form.querySelector('#txtPassword').value;

            ApiClient.updatePluginConfiguration(SmtpConfigurationPageVar.pluginId, config).then(function (result) {
                Dashboard.processPluginConfigurationUpdateResult(result);
            });
        });

        e.preventDefault();
        return false;
    });

    view.addEventListener('viewshow', function() {
        Dashboard.showLoadingMsg();
        const page = this;

        ApiClient.getUsers().then(function (users) {
            const selUser = page.querySelector('#selectUser');
            selUser.innerHTML = users.map(function (user) {
                return '<option value="' + user.Id + '">' + user.Name + '</option>';
            });
            selUser.dispatchEvent(new Event('change', {
                bubbles: true,
                cancelable: false
            }));
        });

        Dashboard.hideLoadingMsg();
    });
}
