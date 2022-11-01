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
using System.Configuration;
using System.Text.RegularExpressions;

namespace NetChecker
{
    class Program
    {
        delegate void method();
        static void Main(string[] args)
        {
            string[] items = { "Проверка доступности БД", "Изменение строки подключения к БД", "Проверка на доступность списока URL", "Добваление нового URL в список", "Изменение E-MAIL", "Отправка отчета", "Выход" };
            method[] methods = new method[] { CheckToDB, EditConnStr, ChekToURL, AddToList, EditMail, Report, Exit };
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
        static void ChekToURL()//Проверка адресов интернет-страниц на доступность (функция работает исправно. Не реализовано сохранение в файл отчета. Не реализована отправка по электронной почте).
        {
            Console.WriteLine("\nПроверка списка URL:");
            //Вычитываем список строк из файла конфигурации (xml).
            XmlSerializer serializer = new XmlSerializer(typeof(xmlrw));
            xmlrw xmlrw_val = null;

            //using (FileStream file = new FileStream("storage.xml", FileMode.Open))

            //    xmlrw_val = (xmlrw)serializer.Deserialize(file);
            using (StreamReader reader = new StreamReader("storage.xml"))
            {
                xmlrw_val = (xmlrw)serializer.Deserialize(reader);
            }
            //Преобразуем объекты в список строк и производим проверку доступности сайтов.
            foreach (var link in xmlrw_val.UrlList)
            {
                if (SiteCheck.Test(link) == true)
                {
                    Console.WriteLine($"Conneting to URL {link}: Success");
                }
                else
                {
                    Console.WriteLine($"Conneting to URL {link}: Fail");
                }
            }
            
        }
        static void AddToList()//Добавление новых адресов сайтов в список провеки (функция работает исправно, не реализована проверка правильности ввода).
        {
            XmlSerializer serializer = new XmlSerializer(typeof(xmlrw));
            xmlrw xmlrw_val = null;
            //Считываем имеющиеся в конфигурационном файле строки (List)
            using (FileStream file = new FileStream("storage.xml", FileMode.Open))

            xmlrw_val = (xmlrw)serializer.Deserialize(file);
            //Добавляем новые строки в конфигурационный файл.
            string yn = null;
            while (yn != "N")
            {
                Console.WriteLine("Хотите добавить в список новый URL? YES/NO (Y/N)");
                yn = Console.ReadLine();
                if (yn == "Y")
                { 
                Console.WriteLine("Введите новый URL:");
                xmlrw_val.UrlList.Add(Console.ReadLine());
                using (StringWriter textWriter = new StringWriter())
                {
                    serializer.Serialize(textWriter, xmlrw_val);

                }
                //Сохранение данных в файл xml с новыми строками.
                using (FileStream file = new FileStream("storage.xml", FileMode.Create))
                using (TextWriter xwriter = new StreamWriter(file, new UTF8Encoding()))
                {
                    serializer.Serialize(xwriter, xmlrw_val);
                    xwriter.Close();
                }
                Console.WriteLine("URL добавлен в список проверки.");
                }
                else
                    if (yn == "N")
                {
                    Console.WriteLine("Добавление нового URL в список отменено.");
                }
            }

        }
        static void EditMail()//Добавление адреса электронной почты в файл конфигурации. Не реализована замена имеющегося адреса. Не реализована проверка ввода.
        {
            XmlSerializer serializer = new XmlSerializer(typeof(xmlrw));
            xmlrw xmlrw_val = null;
            //Считываем имеющиеся в конфигурационном файле строки (List)
            using (FileStream file = new FileStream("storage.xml", FileMode.Open))

                xmlrw_val = (xmlrw)serializer.Deserialize(file);
            Console.WriteLine("Введите адрес электронной почты для отправки отчета:");
            xmlrw_val.Email.Add(Console.ReadLine());
            using (StringWriter textWriter = new StringWriter())
            {
                serializer.Serialize(textWriter, xmlrw_val);

            }
            //Сохранение данных в файл xml с новыми строками.
            using (FileStream file = new FileStream("storage.xml", FileMode.Create))
            using (TextWriter xwriter = new StreamWriter(file, new UTF8Encoding()))
            {
                serializer.Serialize(xwriter, xmlrw_val);
                xwriter.Close();
            }
            Console.WriteLine("Новый электронный адрес получателя отчета добавлен.");


        }
        static void Report()//Отправка отчета в общем работает корректно. Требуются следующие корректировки: 1. Нужно сдаелать подтягивание адреса отправки не из конфига, а из xml 2.  Изменить вложение, должен отправлять файл с результатами проверки, а не исходный xml.
        {
            //Console.WriteLine("Enter To Address:");
            string to = ConfigurationManager.AppSettings["ToEmail"];

            //Console.WriteLine("Enter Subject:");
            //string subject = Console.ReadLine().Trim();
            var dat = DateTime.Now.ToString("dd/ MM/yyyy HH:mm");
            string subject = "Отчет NetChecker" + dat;

            //Console.WriteLine("Enter Body:");
            //string body = Console.ReadLine().Trim();
            string body = "Данный отчет отправлен программой NetChecke и содержит XML-файл с результатами проверки интернет-ресурсов.n/Для просомтра файла отчета откройте кго в любом браузере.n/Благодарим за использование нашего приложенгия.n/С уважением, командв NetChecker.";

            using (MailMessage mm = new MailMessage(ConfigurationManager.AppSettings["FromEmail"], to))
            {
                mm.Subject = subject;
                mm.Body = body;
                mm.IsBodyHtml = false;
                //Прикрепляем файл во вложении.
                var attachment = new Attachment("storage.xml");
                mm.Attachments.Add(attachment);
                SmtpClient smtp = new SmtpClient();
                smtp.Host = ConfigurationManager.AppSettings["Host"];
                smtp.EnableSsl = true;
                NetworkCredential NetworkCred = new NetworkCredential(ConfigurationManager.AppSettings["Username"], ConfigurationManager.AppSettings["Password"]);
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = NetworkCred;
                smtp.Port = int.Parse(ConfigurationManager.AppSettings["Port"]);
                Console.WriteLine("Отправка отчета......");
                smtp.Send(mm);
                Console.WriteLine($"Отчет отправлен на электронную почту {to}.");
                System.Threading.Thread.Sleep(3000);
                
            }

        }
        static void Exit()//Завершение работы приложеня. 1. Нужно сделать красиво оформленный логотип на прощальном экране.
        {
            string centerText = "*** NetChecker ***";
            Console.Clear();
            int centerX = (Console.WindowWidth / 2) - (centerText.Length / 2);
            int centerY = (Console.WindowHeight / 2) - 1;
            Console.SetCursorPosition(centerX, centerY);
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(centerText);
            Environment.Exit(3);
        }
       
    }

}
