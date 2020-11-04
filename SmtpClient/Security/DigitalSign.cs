using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace SmtpClient.Security
{
    public class Message
    {
        public string message;
        public string DigitalSign;
    }
    class DigitalSign
    {
        public static string Hash(string input)
        {
            SHA1Managed hashed_managed = new SHA1Managed();
            
            var hash = hashed_managed.ComputeHash(Encoding.UTF8.GetBytes(input));
            string return_string = Convert.ToBase64String(hash);
            return return_string;
        }
        
    }
}
