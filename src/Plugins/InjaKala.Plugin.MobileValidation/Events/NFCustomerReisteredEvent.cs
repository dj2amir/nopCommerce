using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Events;
using InjaKalaIR.Plugin.MobileValidation.Models;
using InjaKalaIR.Plugin.MobileValidation.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjaKalaIR.Plugin.MobileValidation.Events
{
    internal class NFCustomerReisteredEvent : IConsumer<CustomerRegisteredEvent>
    {
        private readonly ISettingService isettingService_0;
        private readonly IStoreContext istoreContext_0;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerService _CustomerService;

        public NFCustomerReisteredEvent(ISettingService settingService,
            IGenericAttributeService genericAttributeService,
            ICustomerService iCustomerService,
            IStoreContext istoreContext)
        {
            this._CustomerService = iCustomerService;
            this.isettingService_0 = settingService;
            this._genericAttributeService = genericAttributeService;
            this.istoreContext_0 = istoreContext;
        }

        public Task HandleEventAsync(CustomerRegisteredEvent eventMessage)
        {
            var currentStore = this.istoreContext_0.GetCurrentStore();
            MobileValidationSettings nopSmsSettings2 = this.isettingService_0.LoadSettingAsync<MobileValidationSettings>(currentStore.Id).Result;
            if (nopSmsSettings2.SmsIsSendValidation)
            {
                eventMessage.Customer.Active = false;
                this._CustomerService.UpdateCustomerAsync(eventMessage.Customer).Wait();
                //Random rnd = new Random();
                //string randomCode = rnd.Next(9529).ToString();
                //_genericAttributeService.InsertAttribute(new Nop.Core.Domain.Common.GenericAttribute()
                //{
                //    EntityId = eventMessage.Customer.Id,
                //    Key = "NFRCustomer",
                //    KeyGroup = "NFRInjaKala",
                //    StoreId = currentStore.Id,
                //    Value = randomCode
                //});

                //var msg = new Envelope() {
                //    Message = //"کد فعالسازی:" +
                //        randomCode,
                //    To = eventMessage.Customer.Username
                //};
                //CurrentSmsService.SmsService.SendSms(nopSmsSettings2, new System.Collections.Generic.List<Envelope>() { msg });
            }
            return Task.CompletedTask;
        }
    }
}