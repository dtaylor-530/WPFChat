using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleTcp
{
    public class IPHelper
    {
        public static string ExternalIp()
        {
            try
            {
                string externalIP;
                externalIP = new WebClient().DownloadString("http://checkip.dyndns.org/");
                externalIP = new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}")
                             .Matches(externalIP)[0].ToString();

                return externalIP;
            }
            catch
            {
                return "Error receiving IP address.";
            }
        }
        public static string InternalIp()
        {

            return "127.0.0.1";


        }
    }
}
