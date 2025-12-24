using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Settings
{
    public class MessageSettings
    {
        //   private const string EventTypeExecuteNonQueryAsync = "950";
        //private const string EventTypeExecuteScalarAsync = "951";
        // private const string EventTypeGetDataReader = "952";
        // private const string EventTypeGetDataTable = "953";

        /// 901: Start Generation Call
        /// 902: End Generation Call
        /// 911: Start Generation For Company
        /// 912: End Generation For Company
        /// 921: Start Generation All
        /// 922: End Generation All
        /// 961: Issue with a tasks within Generation All
        /// 962: Issue with a task with OneTimeOnly
        /// 963: Issue with a task with Weekly
        /// 964: Issue with a task with Monthly
        /// 965: Issue with a task with Shifts
        /// 999: Exception
        /// 
        /// 801: Succesfull login
        /// 802: Unseccessfull login
        /// 803: Unsuccessfull login incorrect username
        /// 804: Unsuccessfull login incorrect password
        /// 805: Unsuccessfull login too many attempts
        /// 806: Unsuccessfull login user invalid ip

    }
}
