using Generals.Authorization.Users;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Generals
{
    public class ConfirmInput
    {
        public string UserName { get; set; }
        public string Token { get; set; }
    }
    public class ConfirmOutput
    {
        public bool Success { get; set; }
    }
    public class ConfirmAppService : GeneralsAppServiceBase
    {
        private readonly UserManager userManager;

        public ConfirmAppService(UserManager userManager)
        {
            this.userManager = userManager;
        }
        public async Task<ConfirmOutput> PhoneConfirm(ConfirmInput model)
        {
            CurrentUnitOfWork.SetTenantId(1);
            var user = await userManager.FindByNameAsync(model.UserName);
            if (user.MobileVerificationCode == model.Token)
            {
                user.IsActive = true;
                user.MobileVerificationCode = "";
                user.MobileVerificationCodeTime = DateTime.Now;
                user.IsPhoneNumberConfirmed = true;
                CurrentUnitOfWork.SaveChanges();
                return new ConfirmOutput
                {
                    Success = true
                };
            }
            return new ConfirmOutput
            {
                Success = false
            };
        }

    }

}
