using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace SmtpServer
{
    public class Message
    {
        public string message="";
        public string DigitalSign="";

        
    }
    class RijndaelExample
    {
        public static string Decrypt(byte[] cipherText, byte[] Key, byte[] IV)
        {
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            string plaintext = null;
            // Create AesManaged    
            using (AesManaged aes = new AesManaged())
            {
                aes.Key = Key;
                aes.IV = IV;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.Zeros;
                //Console.WriteLine("---"+Convert.ToBase64String(Key)+ "---");
                //Console.WriteLine(cipherText.Length);
                // Create a decryptor    
                ICryptoTransform decryptor = aes.CreateDecryptor(Key, IV);
                // Create the streams used for decryption.    
                using (MemoryStream ms = new MemoryStream(cipherText))
                {
                    // Create crypto stream    
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(cs))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                        //Console.WriteLine(plaintext);
                    }
                }
            }
            return plaintext;
        }
    }
    class SMTPServer
    {
        TcpClient client;
        public static string HashCheck(string input)
        {
            SHA1Managed hashed_managed = new SHA1Managed();

            var hash = hashed_managed.ComputeHash(Encoding.UTF8.GetBytes(input));
            string return_string = Convert.ToBase64String(hash);
            return return_string;
        }
        public void Init(TcpClient client)
        {
            this.client = client;
        }
        public void Run()
        {
            Write("220 localhost -- Fake proxy server");
            string strMessage = String.Empty;
            Message message = new Message();
            bool bol = false;
            while (true)
            {
                try
                {
                    strMessage = Read();
                    Console.WriteLine(strMessage);
                    if (bol == true)
                    {
                        while (strMessage.IndexOf("3D")>=0)
                        {
                            strMessage = strMessage.Remove(strMessage.IndexOf("3D"), 2);
                        }
                        while (strMessage.IndexOf("=\r\n") >= 0)
                        {
                            strMessage = strMessage.Remove(strMessage.IndexOf("=\r\n"), 3);
                        }
                        //Console.WriteLine(strMessage);
                        for(int i=0; i < strMessage.Length; i++)
                        {
                            if (i < strMessage.Length - 28)
                            {
                                message.message += strMessage[i];
                            }
                            else
                            {
                                message.DigitalSign += strMessage[i];
                            }
                        }
                        //Console.WriteLine(message.message);
                        //Console.WriteLine(message.DigitalSign);

                        FileStream fileKey = new FileStream("Key.txt", FileMode.OpenOrCreate);
                        FileStream fileIV = new FileStream("IV.txt", FileMode.OpenOrCreate);

                        byte[] key = new byte[16];
                        byte[] IV = new byte[16];
                        fileKey.Read(key, 0, 16);
                        fileIV.Read(IV, 0, 16);
                        fileKey.Close();
                        fileIV.Close();
                        byte[] helpmepls = Convert.FromBase64String(message.message);
                        string decrypted_body = RijndaelExample.Decrypt(helpmepls, key, IV);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(decrypted_body);
                        Console.ForegroundColor = ConsoleColor.White;

                        Console.WriteLine(message.DigitalSign);
                        Console.WriteLine(HashCheck(decrypted_body));
                        
                        if (HashCheck(decrypted_body) == message.DigitalSign)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine(true);
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(false);
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        bol = false;
                    }
                    
                }
                catch (Exception e)
                {
                    //a socket error has occured
                    Console.WriteLine(e.Message);
                    break;
                }

                if (strMessage.Length > 0)
                {
                    if (strMessage.StartsWith("QUIT"))
                    {
                        client.Close();
                        break;//exit while
                    }
                    //message has successfully been received
                    if (strMessage.StartsWith("EHLO"))
                    {
                        Write("250 OK");
                    }

                    if (strMessage.StartsWith("RCPT TO"))
                    {
                        Write("250 OK");
                    }

                    if (strMessage.StartsWith("MAIL FROM"))
                    {

                        Write("250 OK");
                    }

                    if (strMessage.StartsWith("DATA"))
                    {
                        Write("354 Start mail input; end with");
                        strMessage = Read();
                        Write("250 OK");
                        bol = true;
                    }
                   
                }
            }
        }

        private void Write(string strMessage)
        {
            NetworkStream clientStream = client.GetStream();
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] buffer = encoder.GetBytes(strMessage + "\r\n");

            clientStream.Write(buffer, 0, buffer.Length);
            clientStream.Flush();
        }

        private string Read()
        {
            byte[] messageBytes = new byte[8192];
            int bytesRead = 0;
            NetworkStream clientStream = client.GetStream();
            ASCIIEncoding encoder = new ASCIIEncoding();
            bytesRead = clientStream.Read(messageBytes, 0, 8192);
            //Console.WriteLine();
            //for (int i=0 ;i<100; ++i)
            //{
            //    Console.Write(messageBytes[i].ToString()+"  ");
            //}
            //Console.WriteLine();
            string strMessage = encoder.GetString(messageBytes, 0, bytesRead);
            return strMessage;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Server";
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 25);
            TcpListener listener = new TcpListener(endPoint);
            listener.Start();

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                SMTPServer handler = new SMTPServer();
                handler.Init(client);
                Thread thread = new System.Threading.Thread(new ThreadStart(handler.Run));
                thread.Start();
            }
        }
        
    }
}
