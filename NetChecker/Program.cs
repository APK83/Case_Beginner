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
using System.Security.Policy;

namespace NetChecker
{
    class Program
    {
        delegate void method();
        static void Main(string[] args)//Основное меню: реализовано перемещение при помощи стрелок на клавиатуре. Возможность добавлять необходимое количество пунктов меню.
        {
            string[] items = { "\nПроверка доступности БД Postgres", "\nИзменение строки подключения к БД", "\nПроверка на доступность списока URL", "\nДобваление нового URL в список проверки", "\nИзменение адреса e-mail для отправки отчета", "\nОтправка отчета", "\nРезультат последней проверки", "\nВыход" };
            method[] methods = new method[] { CheckToDB, EditConnStr, ChekToURL, AddToList, EditMail, Report, reportDisplay, Exit };
            ConsoleMenu menu = new ConsoleMenu(items);
            int menuResult;
            do
            {
                menuResult = menu.PrintMenu();
                methods[menuResult]();
                Console.WriteLine("\nДля возврата в основное меню, нажмите любую клавишу.");
                Console.ReadKey();
            }
            while (menuResult != items.Length - 1);
        }
        static void CheckToDB()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(xmlrw));
            xmlrw xmlrw_val = null;
            using (StreamReader reader = new StreamReader("storage.xml"))
            {
                xmlrw_val = (xmlrw)serializer.Deserialize(reader);
            }
            XmlSerializer serializer_rep = new XmlSerializer(typeof(Reprw));
            Reprw rep_rw = null;
            using (StreamReader reader = new StreamReader("report.xml"))
            {
                rep_rw = (Reprw)serializer_rep.Deserialize(reader);
            }
            rep_rw.PostgresList.Clear();

            foreach (string constr in xmlrw_val.PostgresList)
            {
                status_res status_Res = new status_res();
                status_Res.ResName = constr;
                status_Res.Status = PostgresCheck.Connect(constr);
                status_Res.Dat = DateTime.Now;
                rep_rw.PostgresList.Add(status_Res);

            }
            XmlSerializer rep_serialazer = new XmlSerializer(typeof(Reprw));
            using (FileStream file = new FileStream("report.xml", FileMode.Create))
            using (TextWriter xwriter = new StreamWriter(file, new UTF8Encoding()))
            {
                rep_serialazer.Serialize(xwriter, rep_rw);
                xwriter.Close();
            }
            Console.WriteLine("\nПроверка строки подключения завершена. Результат проверки сохранен в файл отчета.");


        }
        static void EditConnStr()//Требуется замена строки подключения в файле storage.xml на созданную.
        {

            XmlSerializer serializer = new XmlSerializer(typeof(xmlrw));
            xmlrw xmlrw_val = null;
            //Считываем имеющиеся в конфигурационном файле строки (List)
            using (FileStream file = new FileStream("storage.xml", FileMode.Open))
                //Создаем объект класса xmlrw.
                xmlrw_val = (xmlrw)serializer.Deserialize(file);
            //Очищаем старую строку подключения, перед добавлением новой строки.
            xmlrw_val.PostgresList.Clear();

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
            //Добавляем новую строку подключения в список объектов.
            xmlrw_val.PostgresList.Add(new_connectionstring);
            using (StringWriter textWriter = new StringWriter())
            {
                serializer.Serialize(textWriter, xmlrw_val);

            }
            //Записываем новую строку подключения в файл XML.
            using (FileStream file = new FileStream("storage.xml", FileMode.Create))
            using (TextWriter xwriter = new StreamWriter(file, new UTF8Encoding()))
            {
                serializer.Serialize(xwriter, xmlrw_val);
                xwriter.Close();
            }
            Console.WriteLine("Строка подключения к серверу Postgres успешно изменена.");

        }
        static void ChekToURL()//Проверка адресов интернет-страниц на доступность. Сохранение результата проверки в файл отчета.
        {
            Console.WriteLine("\nПроверка списка URL:");
            //Вычитываем список строк из файла конфигурации (xml).
            XmlSerializer serializer = new XmlSerializer(typeof(xmlrw));
            xmlrw xmlrw_val = null;

            using (StreamReader reader = new StreamReader("storage.xml"))
            {
                xmlrw_val = (xmlrw)serializer.Deserialize(reader);
            }
            XmlSerializer serializer_rep = new XmlSerializer(typeof(Reprw));
            Reprw rep_rw = null;
            using (StreamReader reader = new StreamReader("report.xml"))
            {
                rep_rw = (Reprw)serializer_rep.Deserialize(reader);
            }
            rep_rw.UrlList.Clear();
            //Преобразуем объекты в список строк и производим проверку доступности сайтов.
            foreach (var link in xmlrw_val.UrlList)
            {
                status_res status_Res = new status_res();
                status_Res.ResName = link;
                status_Res.Status = SiteCheck.Test(link);
                status_Res.Dat = DateTime.Now;
                rep_rw.UrlList.Add(status_Res);
                if (status_Res.Status)
                {
                    Console.WriteLine($"Подключение к URL {link}: успешно");
                }
                else
                {
                    Console.WriteLine($"Подключение к URL {link}: ошибка");
                }
            }
            XmlSerializer rep_serialazer = new XmlSerializer(typeof(Reprw));
            using (FileStream file = new FileStream("report.xml", FileMode.Create))
            using (TextWriter xwriter = new StreamWriter(file, new UTF8Encoding()))
            {
                rep_serialazer.Serialize(xwriter, rep_rw);
                xwriter.Close();
            }

        }
        static void AddToList()//Добавление новых адресов сайтов в список провеки (функция работает исправно, не реализована проверка правильности ввода. Поправить ветвление).
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
                if (yn == "Y" || yn == "y")
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
                    if (yn == "N" || yn == "n")
                {
                    Console.WriteLine("Добавление нового URL в список отменено.");
                    break;
                }
                else
                {
                    Console.WriteLine("Ошибка ввода! Введите Y если желаете добваить в список проверки новый адрес или N если не желаете продолжать добавление адресов в список.\n");

                }
            }

        }
        static void EditMail()//Добавление адреса электронной почты в файл конфигурации. Не реализована проверка ввода (метод создан, требуется применить в коде).
        {
            Console.WriteLine("\nВведите адрес электронной почты для отправки отчета:\n");
            string new_mail = Console.ReadLine();
            
                
                if (isValid(new_mail) == true)
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(xmlrw));
                    xmlrw xmlrw_val = null;
                    //Считываем имеющиеся в конфигурационном файле строки (List)
                    using (FileStream file = new FileStream("storage.xml", FileMode.Open))

                        xmlrw_val = (xmlrw)serializer.Deserialize(file);
                    xmlrw_val.Email.Clear();

                    xmlrw_val.Email.Add(new_mail);
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
                    Console.WriteLine("\nНовый электронный адрес получателя отчета сохранен.");
                }
                else
                {
                    Console.WriteLine("\nАдрес электронной почты введен с ошибкой.\n");
                    Console.WriteLine("\nЕсли хотите попроьовать снова нажмите Y, если хотите вернуться в основное меню нажмите N.\n");
                    var yn = Console.ReadLine();
                    if (yn == "Y" || yn == "y")
                {
                    EditMail();
                }
                    if (yn == "N" || yn == "n")
                {
                    Console.WriteLine("ВНИМАНИЕ!!! Адрес электронной почты не изменен!");
                }
                    else
                {
                    Console.WriteLine("Вы ввели неверное значение.");
                    Console.WriteLine("\nЕсли хотите попроьовать снова нажмите Y, если хотите вернуться в основное меню нажмите N.\n");
                }


                }
            


        }
        static void Report()//Отправка отчета работает корректно.
        {
            
            XmlSerializer serializer = new XmlSerializer(typeof(xmlrw));
            xmlrw xmlrw_val = null;
            using (StreamReader reader = new StreamReader("storage.xml"))
            {
                xmlrw_val = (xmlrw)serializer.Deserialize(reader);
            }
            foreach (string to in xmlrw_val.Email)
            {
                var dat = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                string subject = "Отчет NetChecker " + dat;
                string body = "Данный отчет отправлен программой NetChecker и содержит XML-файл с результатами проверки интернет-ресурсов.\nДля просомтра файла отчета откройте его в любом браузере.\nБлагодарим за использование нашего приложения.\n\nС уважением, команда NetChecker.";

                using (MailMessage mm = new MailMessage(ConfigurationManager.AppSettings["FromEmail"], to))
                {
                    mm.Subject = subject;
                    mm.Body = body;
                    mm.IsBodyHtml = false;
                    //Прикрепляем файл во вложении.
                    var attachment = new Attachment("report.xml");
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
                    Console.WriteLine($"\nОтчет отправлен на электронную почту {to}.");
                    System.Threading.Thread.Sleep(3000);

                }

            }
            
        }
        static void Exit()//Завершение работы приложеня. 1. Нужно сделать красиво оформленный логотип на прощальном экране (сделать по центру).
        {
            string centerText = "\n      ::::    ::: :::::::::: ::::::::::: ::::::::  :::    ::: :::::::::: ::::::::  :::    ::: :::::::::: ::::::::: \n     :+:+:   :+: :+:            :+:    :+:    :+: :+:    :+: :+:       :+:    :+: :+:   :+:  :+:        :+:    :+: \n    :+:+:+  +:+ +:+            +:+    +:+        +:+    +:+ +:+       +:+        +:+  +:+   +:+        +:+    +:+  \n   +#+ +:+ +#+ +#++:++#       +#+    +#+        +#++:++#++ +#++:++#  +#+        +#++:++    +#++:++#   +#++:++#:    \n  +#+  +#+#+# +#+            +#+    +#+        +#+    +#+ +#+       +#+        +#+  +#+   +#+        +#+    +#+    \n #+#   #+#+# #+#            #+#    #+#    #+# #+#    #+# #+#       #+#    #+# #+#   #+#  #+#        #+#    #+#    \n###    #### ##########     ###     ########  ###    ### ########## ########  ###    ### ########## ###    ###      ";
            Console.Clear();
            //int centerX = (Console.WindowWidth / 2) - (centerText.Length / 2);
            //int centerY = (Console.WindowHeight / 2) - 1;
            //Console.SetCursorPosition(centerX, centerY);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(centerText);
            Environment.Exit(3);
        }

        public static bool isValid(string email)//Проверка правильности вводе E-MAIL.
        {
            string pattern = "[.\\-_a-z0-9]+@([a-z0-9][\\-a-z0-9]+\\.)+[a-z]{2,6}";
            Match isMatch = Regex.Match(email, pattern, RegexOptions.IgnoreCase);
            return isMatch.Success;
        }

        static void reportDisplay()//Требуется удалить пробелы и привести отображение в читабельный вид.
        {
            XmlSerializer serializer_rep = new XmlSerializer(typeof(Reprw));
            Reprw rep_rw = null;
            using (StreamReader reader = new StreamReader("report.xml"))
            {
                rep_rw = (Reprw)serializer_rep.Deserialize(reader);
            }

            //Преобразуем объекты в список строк и производим проверку доступности сайтов.
            Console.WriteLine("\nРЕЗУЛЬТАТЫ ПРОВЕРКИ URL:");
            foreach (var link in rep_rw.UrlList)
            {
                Console.WriteLine($"\nРесурс:\n" + link.ResName + "\nСтатус:\n" + link.Status + "\nВремя проверки:\n" + link.Dat);
            }
            Console.WriteLine("\nРЕЗУЛЬТАТЫ ПРОВЕРКИ POSTGRES:");
            foreach (var pg in rep_rw.PostgresList)
            {
                Console.WriteLine($"\nСтрока подключения Postgres:\n" + pg.ResName + "\nСтатус:\n" + pg.Status + "\nВремя проверки:\n" + pg.Dat);
            }

        }

    }

}
