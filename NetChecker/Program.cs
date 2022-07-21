using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Xml.Linq;
using System.Xml;
using System.Text;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Net;

namespace NetChecker
{
    class Program
    {
        delegate void method();
        static void Main(string[] args)
        {
            //XmlSerializer serializer = new XmlSerializer(typeof(xmlrw));

            //var xmlrw_val = new xmlrw()
            //{
            //    UrlList = new List<string>()
            //};

            //Console.WriteLine("Введите URL:");
            //xmlrw_val.UrlList.Add(Console.ReadLine());
            //xmlrw_val.UrlList.Add("https://git-scm.com");
            //xmlrw_val.UrlList.Add("https://keyrand.forum2x2.ru/");
            //xmlrw_val.UrlList.Add("https://www.cyberforum.ru");


            //using (StringWriter textWriter = new StringWriter())
            //{
            //    serializer.Serialize(textWriter, xmlrw_val);
            //    Console.WriteLine(textWriter.ToString());

            //}
            ////Сохранение данных в файл xml
            //FileStream fs = new FileStream("storage.xml", FileMode.Create);
            //TextWriter writer = new StreamWriter(fs, new UTF8Encoding());
            //serializer.Serialize(writer, xmlrw_val);
            //writer.Close();

            //SendEmailAsync().GetAwaiter();
            //Console.Read();

            //XmlTextWriter textWritter = new XmlTextWriter("storage.xml", null);
            //textWritter.Close();
            //textWritter.Dispose();
            //XmlDocument xml_document = new XmlDocument();
            //XmlDeclaration xml_declaration = xml_document.CreateXmlDeclaration("1.0", "UTF-8", String.Empty);
            //XmlElement UrlList = xml_document.CreateElement("UrlList");
            //xml_document.AppendChild(UrlList);
            //xml_document.InsertBefore(xml_declaration, UrlList);
            //xml_document.Save("storage.xml");


            //var adress = new Urls()._adress;
            //Console.WriteLine("Введите URL:");
            //string adr_1 = Console.ReadLine();
            //adress.Add(new Urls() { Adress = adr_1});
            //adress.Add(new Urls() { Adress = "https://vk.com"});
            //foreach (var st in adress)
            //    Console.WriteLine(st.ToString());

            string[] items = { "Проверка доступности БД", "Изменение строки подключения к БД", "Проверка на доступность списока URL", "Добваление нового URL в список", "Изменить E-MAIL", "Выход" };
            method[] methods = new method[] { CheckToDB, EditConnStr, ChekToURL, AddToList, EditMail, Exit };
            ConsoleMenu menu = new ConsoleMenu(items);
            int menuResult;
            do
            {
                menuResult = menu.PrintMenu();
                methods[menuResult]();
                Console.WriteLine("Для продолжения нажмите любую клавишу");
                Console.ReadKey();
            }
            while (menuResult != items.Length - 1);
        }
        static void CheckToDB()
        {
            string conn_str = "Server=localhost; Port=5432; Database=Test_base_001; UserId=postgres; Password=1234; commandTimeout=120;";
            Console.WriteLine("Подключение к базе данных:");
            PostgresCheck.Connect(conn_str);
        }
        static void EditConnStr()
        {
            Console.WriteLine("Изменение строки подключения для проверки доступности БД:");
            Console.WriteLine("Server:");
            string server = Console.ReadLine();
            Console.WriteLine("Port:");
            string port = Console.ReadLine();
            Console.WriteLine("Database:");
            string database = Console.ReadLine();
            Console.WriteLine("UserId:");
            string userid = Console.ReadLine();
            Console.WriteLine("Password:");
            string password = Console.ReadLine();
            Console.WriteLine("commandTimeout:");
            string commandtimeout = Console.ReadLine();
            string new_connectionstring = "Server=" + server + "; " + "Port=" + port + "; " + "Database=" + database + "; " + "UserId=" + userid + "; " +
                "Password=" + password + "; " + "commandTimeout=" + commandtimeout + ";";
            PostgresCheck.Connect(new_connectionstring);
        }
        static void ChekToURL()
        {
            Console.WriteLine("\nПроверка списка URL:");
            string[] Links = new string[5] { "https://vk.com", "https://git-scm.com", "https://keyrand.forum2x2.ru/", "https://www.cyberforum.ru", "https://www.cyberf3873orum.ru" };
            for (int i = 0; i < Links.Length; i++)
            {
                if (SiteCheck.Test(Links[i]) == true)
                {
                    Console.WriteLine($"Conneting to URL {Links[i]}: Success");
                }
                else
                {
                    Console.WriteLine($"Conneting to URL {Links[i]}: Fail");
                }
            }
        }
        static void AddToList()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(xmlrw));
            var xmlrw_val = new xmlrw()
            {
                UrlList = new List<string>()
            };

            Console.WriteLine("Хотите добавить в список новый URL? (Y/N)");
            string yn = Console.ReadLine();
            if (yn == "Y")
            {
                Console.WriteLine("Введите новый URL:");
                xmlrw_val.UrlList.Add(Console.ReadLine());
                using (StringWriter textWriter = new StringWriter())
                {
                    serializer.Serialize(textWriter, xmlrw_val);
                    
                }
                //Сохранение данных в файл xml
                FileStream file = new FileStream("storage.xml", FileMode.Create);
                TextWriter xwriter = new StreamWriter(file, new UTF8Encoding());
                serializer.Serialize(xwriter, xmlrw_val);
                xwriter.Close();
                Console.WriteLine("URL добавлен в список проверки.");
            }
            else
               if (yn == "N")
            {
                
            }
                
            

            using (StringWriter textWriter = new StringWriter())
            {
                serializer.Serialize(textWriter, xmlrw_val);
                Console.WriteLine(textWriter.ToString());

            }
            //Сохранение данных в файл xml
            FileStream fs = new FileStream("storage.xml", FileMode.Create);
            TextWriter writer = new StreamWriter(fs, new UTF8Encoding());
            serializer.Serialize(writer, xmlrw_val);
            writer.Close();

        }
        static void EditMail()
        {
            Console.WriteLine("Введите адрес электронной почты для отправки отчета:");
            MailAddress new_to = new MailAddress(Console.ReadLine());
            
        }
        static void Exit()
        {
            Console.WriteLine("Приложение заканчивает работу!");
            Environment.Exit(0);
        }
        private static async Task SendEmailAsync()
        {
            MailAddress from = new MailAddress("ya.te2016@ya.ru", "NetChecker");
            MailAddress to = new MailAddress("technoservice@nxt.ru");
            MailMessage m = new MailMessage(from, to);
            m.Attachments.Add(new Attachment("E://Учеба_ВШЭ2021/NetCh/NetChecker/NetChecker/bin/Debug/net5.0/storage.xml"));
            m.Subject = "Отчет от проверке URL/POSTGRES";
            m.Body = "Письмо-тест реботы программы NetChecker";
            SmtpClient smtp = new SmtpClient("smtp.yandex.ru", 465);
            smtp.Credentials = new NetworkCredential("ya.te2016@ya.ru", "Acsi300883101289!");
            smtp.EnableSsl = true;
            await smtp.SendMailAsync(m);
            Console.WriteLine("Письмо отправлено");
        }
    }

}
