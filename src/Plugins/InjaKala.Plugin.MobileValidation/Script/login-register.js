/*!
   InjaKala.ir
*/
$(document).on('click', '.nf-form-style.nf-theme-1.nf-ver-2 .nf-row .nf-password-icon', (e) => {
    let obj = $(e.currentTarget);
    let objInput = obj.parent().find('input');
    if (obj.hasClass('nf-active')) {
        obj.addClass('fa-eye');
        obj.removeClass('nf-active').removeClass('fa-eye-slash');

        objInput.attr('type', 'password');
    } else {
        obj.removeClass('fa-eye');
        obj.addClass('nf-active fa-eye-slash');
        objInput.attr('type', 'text');
    }
});

function _otpConfirmationFormInit() {
    this.config_InjaKalair.otpActivationLength = parseInt(this.form.find('input[name="username"]').attr('nf-otp-max-length'));
    $('body').on('input', 'input[name="otp"]', (e) => {
        let obj = $(e.currentTarget);
        if (obj.val().length === this.config_InjaKalair.otpActivationLength) {
            this.form.trigger('submit');
        }
    });
}

function _changeMobileNumberBtnInit() {
    $('body').on('click', '.nf-row-change-mobile-number', (e) => {
        e.preventDefault();
        this.changeMobileNumber(this.form)
    });
}

//function getFormData($form) {
//    var unindexed_array = $form.serializeArray();
//    var indexed_array = {};

//    $.map(unindexed_array, function (n, i) {
//        indexed_array[n['name']] = n['value'];
//    });

//    return indexed_array;
//}

function _changeOtpNumberBtnInit() {

    $('body').on('click', '.nf-row-otp-number', (e) => {
        e.preventDefault();
        let data = getFormData2($('#nf-form-login'));// JSON.stringify($('#nf-form-login').serializeArray());
        fetch("/Otp/OtpSendCode", {
            method: 'POST',
            body: getFormData2(),
        }).then((resp) => {
            return resp.json()
        }).then(response => {
            let r = response;
            let success = r.hasOwnProperty("Success") && r.Success;

            if (r.hasOwnProperty('ErrorList') && r.ErrorList.length) {
                r.ErrorList.forEach((message, index) => {
                    if (success) {
                        alert(message);
                    } else {
                        alert(message);
                    }
                });
            }
            if (success) {
                this.createOtpField(this.form);
                $('.nf-row-password').remove();
                $('.nf-row-otp-number').remove();
                this.config_InjaKalair.levelNo = 3;
            }
        }).catch(error => {
            console.log(error);
        }).finally(() => {
            //btnSubmit.prop('disabled', false);
        });

    });
}

function _loginRegisterTabChangeListener() {
    $(document).on("click", '[nf-app="login_register"][nf-controller="tab"][nf-action="change"]', (event) => {
        event.preventDefault();
        let thisItem = $(event.currentTarget);
        this._loginRegisterTabTabSwitch(thisItem.attr('nf-id'));
    });
}

var config_InjaKalair = {
    levelNo: 1,
    timer: 70,
    otpActivationLength: 4,
};

function getFormData2() {
    let loginForm = this.form = $('#nf-form-login');
    let data = loginForm.serializeArray();


    let formData = new FormData();
    data.forEach((item, index) => {
        if (item.name.trim() === 'username') {
            let emailPattern = /^([a-zA-Z0-9_.+-])+\@(([a-zA-Z0-9-])+\.)+([a-zA-Z0-9]{2,4})+$/;
            if (emailPattern.test(item.value)) {
                formData.append("email", item.value);
                if (this.config_InjaKalair.levelNo === 1) {
                    if (!loginForm.find('.nf-row-password').length) {
                        this.createPasswordField(loginForm);
                        this.config_InjaKalair.levelNo = 2;
                        /*debugger*/;
                        formData.append('show_pass', true);
                    } else {
                        this.config_InjaKalair.levelNo = 3;
                    }
                }
            } else {
                formData.append("mobile", item.value);
            }
        } else {
            formData.append(item.name, item.value);
        }
    });
    formData.append('levelNo', this.config_InjaKalair.levelNo);
    return formData;
}

function _loginRegisterFormInit() {
    let loginForm = this.form = $('#nf-form-login');

    loginForm.validate({
        lang: 'fa',
    });
    loginForm.on('submit', (e) => {
        e.preventDefault();
        let obj = $(e.currentTarget);
        if (!obj.valid()) {
            alert("لطفا فرم را صحیح تكميل نمایید ...");
        } else {
            let btnSubmit = loginForm.find('button[type="submit"]');
            let canLogin = true;

            let formData = getFormData2();
            console.log(formData)
            /*debugger*/;
            if (canLogin && !formData.get('show_pass')) {
                btnSubmit.prop('disabled', true);

                fetch("/Otp/OtpLoginFull", {
                    method: 'POST',
                    body: formData,
                }).then((resp) => {
                    return resp.json()
                }).then(response => {
                    let r = response;
                    let success = r.hasOwnProperty("Success") && r.Success;

                    if (r.hasOwnProperty('ErrorList') && r.ErrorList.length) {
                        r.ErrorList.forEach((message, index) => {
                            if (success) {
                                alert(message);
                            } else {
                                alert(message);
                            }
                        });
                    }
                    if (r.step == 1) {

                    }

                    if (r.hasOwnProperty('Data')) {
                        let Result = r.Data.Result;

                        if (Result.hasOwnProperty('number_levelNo') && Result.number_levelNo) {
                            this.config_InjaKalair.levelNo = parseInt(Result.number_levelNo);
                            if (parseInt(Result.number_levelNo) === 2) {
                                this.disableStep1(loginForm);
                            }
                        }

                        if (Result.hasOwnProperty('redirect') && Result.redirect.length) {
                            btnSubmit.prop('disabled', true);
                            document.location = Result.redirect;
                        }

                        if (Result.hasOwnProperty('need_otp') && Result.need_otp) {
                            this.createOtpField(loginForm);
                            this.createChangeMobileNumberBtn(loginForm);
                            this._changeMobileNumberBtnInit();
                        }

                        if (Result.hasOwnProperty('need_password') && Result.need_password) {
                            //debugger;
                            this.createPasswordField(loginForm);
                            this.createChangeMobileNumberBtn(loginForm);
                            this._changeMobileNumberBtnInit();
                            this.createChangeOtpNumberBtn(loginForm);
                            this.createRecoveryPassNumberBtn(loginForm);
                            this._changeOtpNumberBtnInit();
                        }

                        if (Result.hasOwnProperty('need_email_confirmation') && Result.need_email_confirmation) {
                            alert("لطفا ایمیل خود را تایید نماید.")
                        }
                    }
                }).catch(error => {
                    console.log(error);
                }).finally(() => {
                    btnSubmit.prop('disabled', false);
                });
            }
        }
    });
    $.validator.messages.required = 'ورود این فیلد اجباری است.';
    $.validator.messages.maxlength = $.validator.format("لطفا بیشتر از {0} کارکتر وارد نکنید.");
}

function _loginRegisterFocusFieldInit() {
    $(document).on('focus', '.nf-on-focus-make-top', (e) => { })
}

function _resendCodeInit() {
    $('body').on('click', '.nf-resend-login-code', (e) => {
        e.preventDefault();
        let obj = $(e.currentTarget);
        let form = obj.find('closest');
        if (!obj.hasClass('nf-disabled')) {
            resetForm(this.form, this.form.find('input[name="username"]').val());
        }
    })
}

function disableStep1(form) {
    form.find('input[name="username"]').prop('readonly', true);
    form.find('.nf-row-username').slideUp();
}

function disableStep2(form) {
    form.find('.nf-row-step-2').remove();
}

function enableStep1(form) {
    this.config_InjaKalair.levelNo = 1;
    form.find('input[name="username"]').prop('readonly', false);
    form.find('.nf-row-username').slideDown();
}

function createPasswordField(form) {
    if (!form.find('.nf-row-password').length) {
        let passwordRow = `
                        <div class="nf-row nf-row-step-2 nf-row-password">
                            <label class="nf-label" for="txtLoginPassword">
                                رمز عبور
                                <span class="nf-required">*</span>
                            </label>
                            <input type="password" name="password" class="nf-on-focus-make-top" minlength="6" id="txtLoginPassword" dir="ltr" required style="padding-left:50px;" />
                        </div>
                        `;

        form.find('.nf-row-username').after(passwordRow);
        form.find('#txtLoginPassword').trigger('focus');
    }
}

function createOtpField(form) {
    if (!form.find('.nf-row-otp').length) {
        clearInterval(this.loginCodeTimer);

        let _mobileNumber = form.find('input[name="username"]').val();
        let mobileNumber = _mobileNumber;
        let otpRow = `
                        <div class="nf-row nf-row-step-2 nf-row-otp" style="text-align: center;">
                            <label class="nf-label" for="txtLoginOtp">
                                 کد ارسال شده به شماره (
                                  <span dir="ltr">${mobileNumber}</span>
                                  ) را وارد کنید
                                <span class="nf-required">*</span>
                            </label>
                            <input type="number" name="otp" id="txtLoginOtp" class="nf-no-style-browser" dir="ltr" required style="text-align: center;margin-bottom:25px;height: 40px;" maxlength="${this.config_InjaKalair.otpActivationLength}" />
<br />
                            <a href="#" class="nf-resend-login-code  ">
                                ارسال مجدد کد بعد
                                <span class="nf-resend-login-code-timer"></span> ثانيه
                            </a>
                        </div>
                        `;
        form.find('.nf-row-username').after(otpRow);
        form.find('#txtLoginOtp').trigger('focus');

        this.resetTimer(form);
    }
}

function createChangeMobileNumberBtn(form) {
    if (!form.find('.nf-row-change-mobile-number').length) {
        let otpRow = `
                        <div class="nf-row nf-row-step-2 nf-row-change-mobile-number">
                            تغییر شماره موبایل >
                        </div>
                        `;

        $('#nf-form-login .btns').before(otpRow);
    }
}

function createChangeOtpNumberBtn(form) {
    if (!form.find('.nf-row-otp-number').length) {
        let otpRow = `
                        <div class="nf-row nf-row-step-2 nf-row-otp-number">
                            ورود با رمز یک‌بار‌مصرف >
                        </div>
                        `;

        $('#nf-form-login .btns').before(otpRow);
    }
}

function createRecoveryPassNumberBtn(form) {
    if (!form.find('.nf-row-rec-number').length) {
        let otpRow = `
                        <div class="nf-row nf-row-step-2 nf-row-rec-number">
                            <a href="/passwordrecovery">تعریف رمز ثابت ></a>
                        </div>
                        `;

        $('#nf-form-login .btns').before(otpRow);
    }
}

function changeMobileNumber(form) {
    resetForm(form);
}

function resetTimer(form) {
    let objTimer = form.find('.nf-resend-login-code-timer').html(`(${this.config_InjaKalair.timer})`);
    let sendBtn = objTimer.closest('.nf-resend-login-code');
    sendBtn.addClass('nf-disabled');
    this.loginCodeTimer = setInterval(() => {
        let oTimer = parseInt(objTimer.text().replace(/\D/g, ''));
        if (oTimer > 0) {
            oTimer -= 1;
        } else {
            clearInterval(this.loginCodeTimer);
            sendBtn.removeClass("nf-disabled");
        }
        objTimer.html(oTimer ? `(${oTimer})` : "")
    }, 1000);
}

function resetForm(form, reLogin = "") {
    clearInterval(this.loginCodeTimer);
    this.disableStep2(form);
    this.enableStep1(form);

    form.find('input[name="username"]').val(reLogin || "");
    form.find('button[type="submit"]').prop('disabled', false);

    if (reLogin.length) {
        form.trigger('submit');
    }
}

_loginRegisterFormInit();
_resendCodeInit();