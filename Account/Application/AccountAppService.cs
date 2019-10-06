using System;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Net.Mail;
using Abp.UI;
using Abp.Zero.Configuration;
using Hangfire;
using Generals.Application.Account.Dto;
using Generals.Authorization.Accounts.Dto;
using Generals.Authorization.Users;
using Generals.Configuration;
using Generals.Users.Dto;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using Generals.Application.Accounts.Dto;

namespace Generals.Authorization.Accounts
{
    public class AccountAppService : GeneralsAppServiceBase, IAccountAppService
    {
        // from: http://regexlib.com/REDetails.aspx?regexp_id=1923
        public const string PasswordRegex = "[(A-Za-z\\d)(@|#|$|&|*|.)]{6,32}";

        private readonly UserRegistrationManager _userRegistrationManager;
        private readonly SmsService smsService;
        private readonly IEmailSender emailSender;
        private readonly SmsService smsService1;
        private readonly IPasswordHasher<User> passwordHasher;
        private readonly Abp.Events.Bus.IEventBus eventBus;
        private readonly IRepository<User, long> userRepo;

        public AccountAppService(
            UserRegistrationManager userRegistrationManager,
            SmsService smsService,
            IEmailSender emailSender,
            IPasswordHasher<User> passwordHasher,
            Abp.Events.Bus.IEventBus eventBus,
            IRepository<User,long> userRepo)
        {
            _userRegistrationManager = userRegistrationManager;
            this.emailSender = emailSender;
            smsService1 = smsService;
            this.passwordHasher = passwordHasher;
            this.eventBus = eventBus;
            this.userRepo = userRepo;
            this.smsService = smsService;
        }
        public AccountAppService(
            UserRegistrationManager userRegistrationManager)
        {
            _userRegistrationManager = userRegistrationManager;
        }

        public async Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input)
        {
            var tenant = await TenantManager.FindByTenancyNameAsync(input.TenancyName);
            if (tenant == null)
            {
                return new IsTenantAvailableOutput(TenantAvailabilityState.NotFound);
            }

            if (!tenant.IsActive)
            {
                return new IsTenantAvailableOutput(TenantAvailabilityState.InActive);
            }

            return new IsTenantAvailableOutput(TenantAvailabilityState.Available, tenant.Id);
        }
        public async Task<RegisterOutput> Register(RegisterInput input)
        {
            bool isCaptchaValid = (ReCaptchaClass.Validate(input.CaptchaResponse) == "true" ? true : false);
            if (!isCaptchaValid)
            {
                throw new UserFriendlyException("تیک من ربات نیستم را کلیک نمایید.");
            }
            var userExists = userRepo.GetAll().Any(ff => ff.NormalizedEmailAddress == input.EmailAddress.ToUpper());
            if (userExists)
            {
                throw new UserFriendlyException("آدرس ایمیل وارد شده تکراری است.");

            }

            var user = await _userRegistrationManager.RegisterAsync(
                    input.Name,
                    input.Surname,
                    input.EmailAddress,
                    input.EmailAddress,
                    input.Password,
                    input.KodeMelli,
                    input.PhoneNumber,
                    false
                );

            user.SetNewEmailConfirmationCode();

            BackgroundJobClient.Enqueue<AccountAppService>(instance => instance.SendConfirmEmailAsync(user));

            var verifyCode = RandomService.GetNDigitsRandomNumber(4).ToString();
            user.MobileVerificationCode = verifyCode;
            user.MobileVerificationCodeTime = DateTime.Now;
            user.IsPhoneNumberConfirmed = false;
            await UserManager.UpdateAsync(user);
            var message = $@"مالکینو
کد تایید شما: {verifyCode}
";
            BackgroundJobClient.Enqueue<AccountAppService>(instance => instance.SendSms(message, user.PhoneNumber));

            return new RegisterOutput
            {
                RegisterResult = true
            };

        }

        public void SendSms(string v, string phoneNumber)
        {
            var res = smsService.Send(v, phoneNumber).Result;
        }

        public void SendConfirmEmailAsync(User user)
        {
            var emailBody = new ConfirmEmailBodyProvider().GetBody(user.Id, user.EmailAddress, user.EmailAddress, user.EmailConfirmationCode).Result;

            var message = new MailMessage(SettingManager.GetSettingValue(EmailSettingNames.DefaultFromAddress), user.EmailAddress, "عضویت در مالکینو، تایید آدرس ایمیل", emailBody)
            {
                IsBodyHtml = true
            };

            emailSender.Send(message);
        }
        
        [AbpAuthorize]
        public async Task<bool> ChangePassword(ChangePasswordDto input)
        {
            bool IsCaptchaValid = (ReCaptchaClass.Validate(input.CaptchaResponse) == "true" ? true : false);

            if (!IsCaptchaValid)
            {
                throw new UserFriendlyException("تیک من ربات نیستم را کلیک کنید.");
            }


            if (AbpSession.UserId == null)
            {
                throw new UserFriendlyException("شماره هنوز وارد سیستم نشده اید.");
            }
            long userId = AbpSession.UserId.Value;
            var user = await UserManager.GetUserByIdAsync(userId);
            var passwordCombination = passwordHasher.VerifyHashedPassword(user, user.Password, input.CurrentPassword);
            if (passwordCombination != PasswordVerificationResult.Success)
            {
                throw new UserFriendlyException("کلمه عبور ثبت شده برای شما با کلمه عبور وارد شده مطابقت ندارد.");
            }
            if (!new Regex(PasswordRegex).IsMatch(input.NewPassword))
            {
                throw new UserFriendlyException("کاراکترهای مجاز برای رمز عبور: حروف انگلیسی بزرگ و کوچک، اعداد و @#.$&* می‌باشند. طول رمز عبور باید حداقل 6 باشد");
            }
            user.Password = passwordHasher.HashPassword(user, input.NewPassword);
            user.IsPasswordReset = false;
            CurrentUnitOfWork.SaveChanges();
            return true;
        }
        
        public async Task<ResetPassOutput> ResetPass(ResetPassInput model)
        {
            bool IsCaptchaValid = (ReCaptchaClass.Validate(model.CaptchaResponse) == "true" ? true : false);

            if (!IsCaptchaValid)
            {
                return new ResetPassOutput
                {
                    CaptchaInvalid = true
                };
            }

            try
            {
                var randomService = new RandomService();
                var user = await UserManager.FindByEmailAsync(model.Email);

                var code = await UserManager.GeneratePasswordResetTokenAsync(user);
                user.IsPasswordReset = true;
                CurrentUnitOfWork.SaveChanges();
                var password = $"{randomService.RandomPassword()}";
                var resetRes = await UserManager.ResetPasswordAsync(user, code, password);
                if (!resetRes.Succeeded)
                {
                    throw new UserFriendlyException("خطایی در تغییر رمز عبور اتفاق افتاده است.");
                }
                BackgroundJobClient.Enqueue<AccountAppService>(instance => instance.SendResetPassEmailAsync(user, password));

                eventBus.Trigger(new PasswordResetEventData { UserId = user.Id, NewPassword = password });

                return new ResetPassOutput
                {
                    Success = true

                };
            }
            catch (Exception ex)
            {
                Logger.Error("Error in reset password", ex);
                return new ResetPassOutput
                {
                    Success = true

                };
            }
        }
        public void SendResetPassEmailAsync(User user, string pass)
        {
            var emailBody = new ResetPasswordEmailBodyProvider().GetBody(user.Id, user.EmailAddress, pass).Result;

            var message = new MailMessage(SettingManager.GetSettingValue(EmailSettingNames.DefaultFromAddress), user.EmailAddress, "بازیابی رمز عبور در مالکینو", emailBody)
            {
                IsBodyHtml = true
            };

            emailSender.Send(message);
        }

    }
}
