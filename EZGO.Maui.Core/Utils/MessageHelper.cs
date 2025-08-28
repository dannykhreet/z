using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Message;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Utils
{
    public class MessageHelper
    {
        public static async Task<bool> ErrorMessageIsNotSent(IMessageService messageService)
        {
            if (!await InternetHelper.HasInternetConnection())
            {
                string result = TranslateExtension.GetValueFromDictionary(LanguageConstants.onlyOnlineAction);
                messageService.SendMessage(result, Colors.Red, MessageIconTypeEnum.Warning, true, true, MessageTypeEnum.Connection);
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
