<h3 align="center">NetChecker</h3>

  <p align="center">
    Приложение для проверки доступности URL и базы данных PostgreSQL
    <br />
    <a href="https://github.com/APK83/Case_Beginner"><strong>Ознакомиться с документацией »</strong></a>
    <br />
  </p>
</div>


## О приложении

[![Product Name Screen Shot][product-screenshot]](https://example.com)

Приложение NetChecker - это небольшая программа для использования в консоли позволяющая осуществлять проверку доступности интернет-русурсов и сервера СУБД PostgreSQL.
Приложение не требует ручного ввода и получает данные для проверки из файла XML. Список интернет-ресурсов может быть расщирен, а строка поджключения для провекруи доступности сервера СУБД изменена. Результаты проверки сохраняютс в XML-файл отчета, который можно просмотреть в консоле через запуск с параметром или через функция "Результат последней проверки". Адрес электронной почты на который отправляется отчет так же можно изменить на необходимый. 

<p align="right">(<a href="#readme-top"в начало</a>)</p>


### Используемые технологии

Приложение создано на платформе .NET Core 5 (https://learn.microsoft.com/ru-ru/dotnet/core/whats-new/dotnet-5)

Для организации запроса к серверу PostgreSQL использована бибилиотека Npgsql (https://www.npgsql.org/)

Для реализации логирования в приложении использована библиотека NLog (https://nlog-project.org/)


<p align="right">(<a href="#readme-top"в начало</a>)</p>



## Работа с функционалом

Данное приложение предназначено для проверки доступности интернет-ресурсов и баз данных. Помимо основных функций (непосредственно проверки доступности), приложение так же имеет вспомогательные инструменты такие как внесение изменений в список проверяемых страниц и пр. Ниже мы разберем весь функциона NetChecker детально.

Основные функции:
1. Проверка доступности БД
2. Проверка доступности интернет-ресурсов

Вспомогательные функции:
1. Изменение проверяемой строки подключения к БД.
2. Добавление интернет-ресурсов в список проверки.
3. Просмотр последнего отчета проверки.
4. Отправка последнего отчета проверки посредством электронной почты.
5. Изменение адреса электронной почты для отправки отчета.
6. Основное меню программы.

Служебные инструменты:
1. Логирование работы программа с сохранением информации в файл.

Дополнительные возможности:
1. Запуск программы с параметром.

### Стартовое меню

В целях удобства использования пользователем и для простоты навигации в приложении реализовано меню с возможность перемещения по нему при помощи стрелок на клавиатуре.

Для реализации основного меню с указанным выше способом навигации реализован класс ConsoleMenu 

```sh
class ConsoleMenu
    {
        string[] menuItems;
        int counter = 0;
        public ConsoleMenu(string[] menuItems)
        {
            this.menuItems = menuItems;
        }
        public int PrintMenu()
        {
            ConsoleKeyInfo key;
            do
            {
                Console.Clear();
                for (int i = 0; i < menuItems.Length; i++)
                {
                    if (counter == i)
                    {
                        Console.BackgroundColor = ConsoleColor.Cyan;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine(menuItems[i]);
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else
                        Console.WriteLine(menuItems[i]);

                }
                key = Console.ReadKey();
                if (key.Key == ConsoleKey.UpArrow)
                {
                    counter--;
                    if (counter == -1) counter = menuItems.Length - 1;
                }
                if (key.Key == ConsoleKey.DownArrow)
                {
                    counter++;
                    if (counter == menuItems.Length) counter = 0;
                }
            }
            while (key.Key != ConsoleKey.Enter);
            return counter;
        }
  ```
  
  Запуск основного меню в программе реализован следующим образом и позволяет отобразить на дисплее удобный и интуитивно понятный пользовательский интерфейс.
```sh  
  Logger.Info("Запуск приложения");

            try
            {

                if (args.Length == 0)
                {
                    string[] items = { "\nПроверка доступности БД PostgreSQL", "\nИзменение строки подключения к БД", "\nПроверка на доступность списка URL",      "\nДобваление нового URL в список проверки", "\nИзменение адреса e-mail для отправки отчета", "\nОтправка отчета", "\nРезультат последней проверки", "\nВыход" };
                    method[] methods = new method[] { CheckToDB, EditConnStr, ChekToURL, AddToList, EditMail, Report, reportDisplay, Exit };
                    ConsoleMenu menu = new ConsoleMenu(items);
                    int menuResult;
                    do
                    {
                        Logger.Info("Инициализация основного меню.");
                        menuResult = menu.PrintMenu();
                        methods[menuResult]();
                        Console.WriteLine("\nДля возврата в основное меню, нажмите любую клавишу.");
                        Console.ReadKey();
                        Logger.Info("Возврат в основное меню.");
                    }
                    while (menuResult != items.Length - 1);
                }
                else
                {
                    try
                    {
                        reportDisplay();
                        Logger.Info("Запуск программы с параметром");
                    }
                    catch (Exception exception)
                    {
                        Logger.Error("Ошибка: запуск приложения с параметром невозможен, проверьте правильность вводимых команд", exception);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Fatal("Критическая ошибка: не удалось инициализировать запуск стартового меню.", exception);
            }
```  
### Проверка доступности сервера СУБД PostgreSQL

Функция считывает строку подключения из файла storage.xml и отправляет запрос на сервер, полученный результат сохраняется в файл отчета report.xml.

Класс PostgreCheck отвечает за подключение к серверу СУБД. Для реализации функции использована бибилиотека Npgsql.

```sh
   class PostgreCheck
    {
        public static bool Connect(string connectionString)
        {
            NpgsqlConnection conn = new NpgsqlConnection(connectionString);
            try
            {
                conn.Open();
                Console.WriteLine($"Строка подключения: {connectionString} \nСтатус: БД доступна.");
                return true;

            }
            catch
            {
                Console.WriteLine($"База данных Postgres: ({connectionString}), \nСтатус: БД недоступна.");
            }
            return false;
        }
   ```
   
   Функция CheckToDB осуществляет взаимодействие с файлами XML и обращение к классу PostgreCheck. При запуске функции, из файла storage.xml считывает строка подключения с СУБД, после чего проверяется на доступность, результат записывается в файл отчета report.xml

```sh
 static void CheckToDB()
        {
            Logger.Info("Инициализация проверки подключения к БД PostreSQL");
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(xmlrw));
                xmlrw xmlrw_val = null;
                using (StreamReader reader = new StreamReader("storage.xml"))
                {
                    xmlrw_val = (xmlrw)serializer.Deserialize(reader);
                    Logger.Info("Чтение файла storage.xml");
                }
                XmlSerializer serializer_rep = new XmlSerializer(typeof(Reprw));
                Reprw rep_rw = null;
                using (StreamReader reader = new StreamReader("report.xml"))
                {
                    rep_rw = (Reprw)serializer_rep.Deserialize(reader);
                    Logger.Info("Чтение файла report.xml");
                }
                rep_rw.PostgresList.Clear();
                Logger.Info("Очистка данных строки подключения к БД в файле report.xml");

                foreach (string constr in xmlrw_val.PostgreList)
                {
                    status_res status_Res = new status_res();
                    status_Res.ResName = constr;
                    status_Res.Status = PostgreCheck.Connect(constr);
                    status_Res.Dat = DateTime.Now;
                    rep_rw.PostgresList.Add(status_Res);
                    Logger.Info($"Проверка строки подключения {constr}.");

                }
                XmlSerializer rep_serialazer = new XmlSerializer(typeof(Reprw));
                using (FileStream file = new FileStream("report.xml", FileMode.Create))
                using (TextWriter xwriter = new StreamWriter(file, new UTF8Encoding()))
                {
                    rep_serialazer.Serialize(xwriter, rep_rw);
                    xwriter.Close();
                    Logger.Info("Сохранение результатов проверки в report.xml");
                }
                Console.WriteLine("\nПроверка строки подключения завершена. Результат проверки сохранен в файл отчета.");
                Logger.Info("Проверка подключения к БД завершена");
            }
            catch (Exception exception)
            {
                Logger.Error("Ошибка: не удалось инициализировать запуск проверки подключения к БД PostreSQL", exception);
            }
   ```
### Проверка доступности интернет-ресурсов (URL) из списка

Функция считывает адреса интернет-ресурсов из файла-хранилища storage.xml, преобразует их в список и осуществляет поочереднуц проерку на доступность через класс WebRequest, полученные результаты записываются в файл отчета report.xml и отображается в консоли.

Для проверки доступности URL реализован класс UrlCheck

```sh
class UrlCheck
    {
        public static bool Test(string URL)
        {

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            request.Timeout = 3000;
            try
            {
                WebResponse resp = request.GetResponse();
            }
            catch
            {
                return false;
            }
            return true;

        }
    }
```    


 Функция ChekToURL осуществляет взаимодействие с файлами XML и обращение к классу UrlCheck. При запуске функции, из файла storage.xml считывает адреса URL и преобразует их в список, после чего производится последовательная проверка на доступность, результат записывается в файл отчета report.xml и отображается в консоли.

```sh
 static void ChekToURL()//Проверка адресов интернет-страниц на доступность. Сохранение результата проверки в файл отчета.
        {
            Logger.Info("Инициализация проверки списка адресов URL на доступность");

            try
            {
                Console.WriteLine("\nПроверка списка URL:");
                //Вычитываем список строк из файла конфигурации (xml).
                XmlSerializer serializer = new XmlSerializer(typeof(xmlrw));
                xmlrw xmlrw_val = null;

                using (StreamReader reader = new StreamReader("storage.xml"))
                {
                    xmlrw_val = (xmlrw)serializer.Deserialize(reader);
                    Logger.Info("Чтение данных из файла storage.xml");
                }
                XmlSerializer serializer_rep = new XmlSerializer(typeof(Reprw));
                Reprw rep_rw = null;
                using (StreamReader reader = new StreamReader("report.xml"))
                {
                    rep_rw = (Reprw)serializer_rep.Deserialize(reader);
                    Logger.Info("Чтение данных из файла report.xml");
                }
                rep_rw.UrlList.Clear();
                Logger.Info("Очистка файла отчета report.xml для сохранения данных актуальной провекри");
                //Преобразуем объекты в список строк и производим проверку доступности сайтов.
                foreach (var link in xmlrw_val.UrlList)
                {
                    status_res status_Res = new status_res();
                    status_Res.ResName = link;
                    status_Res.Status = UrlCheck.Test(link);
                    status_Res.Dat = DateTime.Now;
                    rep_rw.UrlList.Add(status_Res);
                    if (status_Res.Status)
                    {
                        Console.WriteLine($"Подключение к URL {link}: успешно");
                        Logger.Info($"Ресурс с адресом {link} доступен");
                    }
                    else
                    {
                        Console.WriteLine($"Подключение к URL {link}: ошибка");
                        Logger.Warn($"Ресурс с адресом {link} недоступен");
                    }
                }
                XmlSerializer rep_serialazer = new XmlSerializer(typeof(Reprw));
                using (FileStream file = new FileStream("report.xml", FileMode.Create))
                using (TextWriter xwriter = new StreamWriter(file, new UTF8Encoding()))
                {
                    rep_serialazer.Serialize(xwriter, rep_rw);
                    xwriter.Close();
                    Console.WriteLine("\nПроверка списка URL завершена. Результат проверки сохранен в файл отчета.");
                    Logger.Info("Результаты проверки успешно сохранены в файл report.xml");
                }
            }
            catch (Exception exception)
            {
                Logger.Fatal("Ошибка: не удалось инициализировать запуск проверки списка URL", exception);
            }


        }
```

## Контактная информация

Курбатов Андрей (APK83)
e-mail: ya.te2016@ya.ru
git profile: https://github.com/APK83

Project Link: [https://github.com/APK83/Case_Beginner](https://github.com/APK83/Case_Beginner)

<p align="right">(<a href="#readme-top">к началу</a>)</p>



