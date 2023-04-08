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
        
    }
}
