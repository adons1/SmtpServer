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
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace SmtpClientt
{
    
    class Program
    {
        static void Main(string[] args)
        {
            string message;
            Console.Title = "Client";
            Console.WriteLine("destination e-mail:");
            string destinationEMail = "user2@localhost.com";
            // отправитель - устанавливаем адрес и отображаемое в письме имя
            MailAddress from = new MailAddress("user1@localhost.com", "Tom");
            // кому отправляем
            MailAddress to = new MailAddress(destinationEMail);
            // создаем объект сообщения
            MailMessage m = new MailMessage(from, to);
            m.Subject = "Themawqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqrwr";
            // тема письма


            // текст письма
            while (true)
            {
                Console.WriteLine("источник:");
                string typeofmes = Console.ReadLine();
                if (typeofmes == "file")
                {
                    Console.WriteLine("путь к файлу:");
                    string filepath = Console.ReadLine();
                    string textFromFile;
                    using (FileStream fstream = File.OpenRead("text.txt"))
                    {
                        // преобразуем строку в байты
                        byte[] array = new byte[fstream.Length];
                        // считываем данные
                        fstream.Read(array, 0, array.Length);
                        // декодируем байты в строку
                        textFromFile = Encoding.UTF8.GetString(array);
                        //textFromFile = Convert.ToBase64String(array);
                    }
                    message = textFromFile;
                    break;
                }
                if (typeofmes == "console")
                {
                    Console.WriteLine("текст письма:");
                    message = Console.ReadLine(); //textFromFile;
                    break;
                }
                else
                {
                    Console.WriteLine("неверный формат:");
                    
                }
            }
            
            //while (message.Length % 16 != 0)
            //{
            //    message += '0';
            //}
            
            Console.WriteLine(message.Length);
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

            byte[] key = new byte[16];
            byte[] IV = new byte[16];
            fileKey.Read(key, 0, 16);
            fileIV.Read(IV, 0, 16);
            fileKey.Close();
            fileIV.Close();
            byte[] res = RijndaelExample.Encrypt(message, key, IV);
            string encrtpted_message = Convert.ToBase64String(res);
            byte[] arr = Convert.FromBase64String(encrtpted_message);

            foreach (byte b in arr)
            {
                Console.Write(b);
                Console.Write("\t");
            }
            Console.WriteLine("\n\n");
            Console.WriteLine("arr.Length:");
            Console.WriteLine(arr.Length);
            Console.WriteLine("encrtpted_message:");
            Console.WriteLine(encrtpted_message);
            Console.WriteLine("encrtpted_message.Length:");
            Console.WriteLine(encrtpted_message.Length);
            Console.WriteLine("message_hashed:");
            Console.WriteLine(message_hashed);
            Console.WriteLine("message_hashed.Length:");
            Console.WriteLine(message_hashed.Length);
            //Console.WriteLine(encrtpted_message+message_hashed);
            //m.Body = encrtpted_message + message_hashed;
            
            m.Body = encrtpted_message+message_hashed;
            // письмо представляет код html
            m.IsBodyHtml = false;
            // адрес smtp-сервера и порт, с которого будем отправлять письмо
            System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("192.168.1.44", 587);
            //smtp.UseDefaultCredentials = true;
            // логин и пароль
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("user1@localhost.com", "mypassword");
            
            smtp.EnableSsl = false;

            try
            {
                smtp.Send(m);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


            Thread.Sleep(10000);
            smtp.Dispose();
            
            Console.Read();
        }
    }
}
