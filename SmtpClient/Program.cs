using SmtpClient.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace SmtpClientt
{
    
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Client";
            Console.WriteLine("destination e-mail:");
            string destinationEMail = Console.ReadLine();
            // отправитель - устанавливаем адрес и отображаемое в письме имя
            MailAddress from = new MailAddress("user1@localhost.com", "Tom");
            // кому отправляем
            MailAddress to = new MailAddress(destinationEMail);
            // создаем объект сообщения
            MailMessage m = new MailMessage(from, to);
            // тема письма
            m.Subject = "Тест";
            // текст письма
            
            Console.WriteLine("your message:");
            string message = Console.ReadLine();
            while (message.Length % 16 != 0)
            {
                message += '0';
            }
            string message_hashed = DigitalSign.Hash(message);
            byte[] hashed = Convert.FromBase64String(message_hashed);
            //Console.WriteLine(message_hashed);
            RijndaelManaged myRijndael = new RijndaelManaged();
            FileStream fileKey = new FileStream("Key.txt", FileMode.OpenOrCreate);
            FileStream fileIV = new FileStream("IV.txt", FileMode.OpenOrCreate);

            //myRijndael.GenerateKey();
            //string keyyy = Convert.ToBase64String(myRijndael.Key);
            //myRijndael.GenerateIV();
            //string IVVVV =Convert.ToBase64String(myRijndael.IV);

            //Console.WriteLine(keyyy);
            //Console.WriteLine(IVVVV);
            
            byte[] key=new byte[16];
            byte[] IV= new byte[16];
            fileKey.Read(key, 0,16);
            fileIV.Read(IV, 0, 16);
            fileKey.Close();
            fileIV.Close();
            
            string encrtpted_message = Convert.ToBase64String(RijndaelExample.Encrypt(message, key, IV), 0, message.Length);
            
            Console.WriteLine(encrtpted_message);
            Console.WriteLine(message_hashed);
            //Console.WriteLine(encrtpted_message+message_hashed);
            //m.Body = encrtpted_message + message_hashed;
            //Console.WriteLine(message_hashed.Length);
            m.Body = encrtpted_message+ message_hashed;
            // письмо представляет код html
            m.IsBodyHtml = false;
            // адрес smtp-сервера и порт, с которого будем отправлять письмо
            System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("localhost", 25);
            // логин и пароль
            smtp.Credentials = new NetworkCredential("somemail@gmail.com", "mypassword");
            smtp.EnableSsl = false;
            smtp.Send(m);
            smtp.Dispose();
            Console.Read();
        }
    }
}
