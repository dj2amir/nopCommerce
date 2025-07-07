$(document).ready(function () {
  // Phone number validation
  $('#TestPhoneNumber').on('input', function () {
    var phoneNumber = $(this).val();
    var phoneRegex = /^09\d{9}$/;
    var feedbackElement = $(this).siblings('.invalid-feedback');

    if (phoneNumber && !phoneRegex.test(phoneNumber)) {
      $(this).addClass('is-invalid');
      if (feedbackElement.length === 0) {
        $(this).after('<div class="invalid-feedback">شماره تلفن باید با فرمت 09xxxxxxxxx وارد شود</div>');
      }
    } else {
      $(this).removeClass('is-invalid');
      feedbackElement.remove();
    }
  });

  // URL validation
  $('#SmsProviderBaseUrl').on('input', function () {
    var url = $(this).val();
    var urlRegex = /^https?:\/\/.+/;
    var feedbackElement = $(this).siblings('.invalid-feedback');

    if (url && !urlRegex.test(url)) {
      $(this).addClass('is-invalid');
      if (feedbackElement.length === 0) {
        $(this).after('<div class="invalid-feedback">آدرس باید با http:// یا https:// شروع شود</div>');
      }
    } else {
      $(this).removeClass('is-invalid');
      feedbackElement.remove();
    }
  });

  // Numeric validation for OtpCodeLength
  $('#OtpCodeLength').on('input', function () {
    var value = parseInt($(this).val());
    var feedbackElement = $(this).siblings('.invalid-feedback');

    if (isNaN(value) || value < 4 || value > 8) {
      $(this).addClass('is-invalid');
      if (feedbackElement.length === 0) {
        $(this).after('<div class="invalid-feedback">طول کد OTP باید بین 4 تا 8 رقم باشد</div>');
      }
    } else {
      $(this).removeClass('is-invalid');
      feedbackElement.remove();
    }
  });

  // Numeric validation for OtpExpirationMinutes
  $('#OtpExpirationMinutes').on('input', function () {
    var value = parseInt($(this).val());
    var feedbackElement = $(this).siblings('.invalid-feedback');

    if (isNaN(value) || value < 1 || value > 30) {
      $(this).addClass('is-invalid');
      if (feedbackElement.length === 0) {
        $(this).after('<div class="invalid-feedback">مدت انقضا باید بین 1 تا 30 دقیقه باشد</div>');
      }
    } else {
      $(this).removeClass('is-invalid');
      feedbackElement.remove();
    }
  });

  // Numeric validation for MaxOtpAttempts
  $('#MaxOtpAttempts').on('input', function () {
    var value = parseInt($(this).val());
    var feedbackElement = $(this).siblings('.invalid-feedback');

    if (isNaN(value) || value < 1 || value > 10) {
      $(this).addClass('is-invalid');
      if (feedbackElement.length === 0) {
        $(this).after('<div class="invalid-feedback">حداکثر تلاش باید بین 1 تا 10 باشد</div>');
      }
    } else {
      $(this).removeClass('is-invalid');
      feedbackElement.remove();
    }
  });

  // Template ID validation
  $('#SmsTemplateId').on('input', function () {
    var value = parseInt($(this).val());
    var feedbackElement = $(this).siblings('.invalid-feedback');

    if (isNaN(value) || value <= 0) {
      $(this).addClass('is-invalid');
      if (feedbackElement.length === 0) {
        $(this).after('<div class="invalid-feedback">شناسه قالب پیامک باید بزرگتر از 0 باشد</div>');
      }
    } else {
      $(this).removeClass('is-invalid');
      feedbackElement.remove();
    }
  });
});