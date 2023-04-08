using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Washouse.Common.Helpers
{
    public class DataValidation
    {
        public static bool CheckPhoneNumber(string phoneNumber)
        {
            bool isPhoneNumber = Regex.IsMatch(phoneNumber, @"^0\d{9}$");

            if (isPhoneNumber)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool CheckValidOrderStatus(string status)
        {
            if (status.Trim().ToLower() == "pending" || status.Trim().ToLower() == "confirmed" 
                || status.Trim().ToLower() == "processing" || status.Trim().ToLower() == "ready" 
                || status.Trim().ToLower() == "completed" || status.Trim().ToLower() == "cancelled")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public static bool CheckValidUpdateOrderStatus(string status)
        {
            if (status.Trim().ToLower() == "confirmed" 
                || status.Trim().ToLower() == "processing"
                || status.Trim().ToLower() == "completed" 
                || status.Trim().ToLower() == "cancelled")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool CheckValidUpdateOrderDetailStatus(string status)
        {
            if (status.Trim().ToLower() == "processing"
                || status.Trim().ToLower() == "completed")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
