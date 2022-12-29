<h3 align="center">NetChecker</h3>

  <p align="center">
    Приложение для проверки доступности URL и базы данных PostgreSQL
    <br />
    <a href="https://github.com/APK83/Case_Beginner"><strong>Ознакомиться с документацией »</strong></a>
    <br />
  </p>
</div>


## О приложении

Приложение NetChecker - это небольшая программа для использования в консоли позволяющая осуществлять проверку доступности интернет-русурсов и сервера СУБД PostgreSQL.
Приложение не требует ручного ввода и получает данные для проверки из файла XML. Список интернет-ресурсов может быть расщирен, а строка поджключения для провекруи доступности сервера СУБД изменена. Результаты проверки сохраняютс в XML-файл отчета, который можно просмотреть в консоле через запуск с параметром или через функция "Результат последней проверки". Адрес электронной почты на который отправляется отчет так же можно изменить на необходимый. 

### Используемые технологии

Приложение создано на платформе .NET Core 5 (https://learn.microsoft.com/ru-ru/dotnet/core/whats-new/dotnet-5)

Для организации запроса к серверу PostgreSQL использована бибилиотека Npgsql (https://www.npgsql.org/)

Для реализации логирования в приложении использована библиотека NLog (https://nlog-project.org/)

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

## Описание кода программы

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

### Работа программы с XML

Для работы c файлами XML в программе используются два класса, xmlrw для работы с файлом storage.xml и Reprw для работы с файлом report.xml. Обращение и работа с файлами xml производится при помощи метода XmlSerializer.Serialize (https://learn.microsoft.com/ru-ru/dotnet/api/system.xml.serialization.xmlserializer.serialize?view=net-7.0)

Класс xmlrw

```sh
public class xmlrw
    {
        public List<string> UrlList { get; set; }
        public List<string> PostgreList { get; set; }
        public List<string> Email { get; set; }

    }
```    
Класс Reprw

```sh
 public class Reprw
    {
        public List<status_res> UrlList { get; set; } = new List<status_res>();
        public List<status_res> PostgresList { get; set; } = new List<status_res>();

    }
``` 
Файл storage.xml имеет следующий вид и содержит спимок адресов интернет-ресурсов, строкуподключения к СУБД и адрес электронной почты для отправки отчета.

```sh
<?xml version="1.0" encoding="utf-8"?>
<xmlrw xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <UrlList>
	  <string>https://vk.com</string>
	  <string>https://git-scm.com</string>
	  <string>https://keyrand.forum2x2.ru/</string>
	  <string>https://keyrand.forum2x2.ru</string>
	  <string>https://www.cyberforum.ru</string>
	  <string>https://www.cyberf3873orum.ru</string>
  </UrlList>
	<PostgreList>
		<string>Server=localhost; Port=5432; Database=Test_base_001; UserId=postgres; Password=1234; commandTimeout=120;</string>
	</PostgreList>
	<Email>
		<string>technoservice@nxt.ru</string>
	</Email>
</xmlrw>
```
Файл report.xml служит для записи в него результатов последней проверки. Элементы отчета имеют атрибуты, такие как Status и Dat, в них сохраняется результат проверки и время проверки каждого элемента.

```sh
<Reprw xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
<UrlList>
<status_res>
<ResName>https://vk.com</ResName>
<Status>true</Status>
<Dat>2022-12-29T21:35:14.0552628+05:00</Dat>
</status_res>
<status_res>
<ResName>https://git-scm.com</ResName>
<Status>true</Status>
<Dat>2022-12-29T21:35:14.2460202+05:00</Dat>
</status_res>
<status_res>
<ResName>https://keyrand.forum2x2.ru/</ResName>
<Status>true</Status>
<Dat>2022-12-29T21:35:14.7913293+05:00</Dat>
</status_res>
<status_res>
<ResName>https://keyrand.forum2x2.ru</ResName>
<Status>true</Status>
<Dat>2022-12-29T21:35:15.1833427+05:00</Dat>
</status_res>
<status_res>
<ResName>https://www.cyberforum.ru</ResName>
<Status>true</Status>
<Dat>2022-12-29T21:35:15.5447907+05:00</Dat>
</status_res>
<status_res>
<ResName>https://www.cyberf3873orum.ru</ResName>
<Status>false</Status>
<Dat>2022-12-29T21:35:15.5978057+05:00</Dat>
</status_res>
</UrlList>
<PostgresList>
<status_res>
<ResName>Server=localhost; Port=5432; Database=Test_base_001; UserId=postgres; Password=1234; commandTimeout=120;</ResName>
<Status>true</Status>
<Dat>2022-12-29T21:35:11.6439997+05:00</Dat>
</status_res>
</PostgresList>
</Reprw>
```
Для формированя атребутов элементов отчета создан класс status_res

```sh
public class status_res
    {
        public string ResName { get; set; }
        public bool Status { get; set; }
        public DateTime Dat { get; set; }
    }
```
### Реализация логгирования в программе

Для отладки работы программы и отслеживания хода работы приложения реализована служба логгирования на основе библиотеки NLog (https://nlog-project.org/)/
Конфигурация логгирования реализована без использования файла и прописана непосредственно в Main, такой способ выбран в связи с небольшим размером приложения.

```sh
 var config = new NLog.Config.LoggingConfiguration();
            LogManager.Configuration = new LoggingConfiguration();
            const string LayoutFile = @"[${date:format=yyyy-MM-dd HH\:mm\:ss}] [${logger}/${uppercase: ${level}}] [THREAD: ${threadid}] >> ${message} ${exception: format=ToString}";
            var logfile = new FileTarget();
            if (!Directory.Exists("Logs"))
                Directory.CreateDirectory("Logs");
            logfile.CreateDirs = true;
            logfile.FileName = $"Logs{Path.DirectorySeparatorChar}log {DateTime.Now}.log";
            logfile.AutoFlush = true;
            logfile.LineEnding = LineEndingMode.CRLF;
            logfile.Layout = LayoutFile;
            logfile.FileNameKind = FilePathKind.Absolute;
            logfile.ConcurrentWrites = false;
            logfile.KeepFileOpen = true;
            //Настраиваем уровни логирования
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);
            //Применяем конфишурация логирования
            NLog.LogManager.Configuration = config;
```
Объявление метода в Programm

```sh
public static NLog.Logger Logger = NLog.LogManager.GetLogger("NetChecker");
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

### Изменение строки подключения к СУБД

В программе предусмотрена возможность изменения строки подключения к серверу СУБД. В результате использования функции новая строка подключения записывается в файл storage.xml и при последующей проверки доступности программа будет отправлять запросу в соответсвии с новыми параметрами. За изменение строки подключения отвечает функция EditConnStr. При запуске программа запрашивает у пользователя поочередно ввод всех параметров строки подключения, после того как все необходимые данные получены, формируется строка подключения и записывается в файл storage.xml.

```sh
static void EditConnStr()//Функция работает исправно (предыдущая строка очищается, после чего прописывается новая).
        {
            try
            {
                Logger.Info("Инициализация редактирования строки подключения к БД PostgreSQL.");

                XmlSerializer serializer = new XmlSerializer(typeof(xmlrw));
                xmlrw xmlrw_val = null;
                //Считываем имеющиеся в конфигурационном файле строки (List)
                using (FileStream file = new FileStream("storage.xml", FileMode.Open))
                {
                    //Создаем объект класса xmlrw.
                    xmlrw_val = (xmlrw)serializer.Deserialize(file);
                    //Очищаем старую строку подключения, перед добавлением новой строки.
                    xmlrw_val.PostgreList.Clear();
                    Logger.Info("Очистка данных строки подключения к БД в файле starage.xml");
                }

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
                xmlrw_val.PostgreList.Add(new_connectionstring);
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
                    Logger.Info("Сохранение изменений в файл storage.xml");
                }
                Console.WriteLine("Строка подключения к серверу Postgres успешно изменена.");
                Logger.Info("Изменение строки подключения к БД завершено успешно");
            }
            catch (Exception exception)
            {
                Logger.Fatal("Ошибка: не удалось инициализировать запуск изменения строки подключения к БД", exception);
            }
        }

```

### Добавление адреса URL в список проверки

В программе имеется возможность добавлять новые адреса интернет-ресурсов в список проверки. Функция AddToList позволяет добавить необходимое количество URL.
Адреса вводятся по одному. после сохранения первого адреса, функция предлагает ввести еще адрес либо прервать добавление и выйти в основное меню. Новые адреса добавляются к уже сохраненному списку в файл storage.xml и при запуске последующей проверки опрашивается уже список с учетом добавленных URL.

```sh
static void AddToList()//Добавление новых адресов сайтов в список провеки (функция работает исправно, реализована проверка правильности ввода через регулярное выражение.).
        {
            Logger.Info("Инициализация редактирования списка адресов URL на доступность");
            try
            
                XmlSerializer serializer = new XmlSerializer(typeof(xmlrw));
                xmlrw xmlrw_val = null;
                //Считываем имеющиеся в конфигурационном файле строки (List)
                using (FileStream file = new FileStream("storage.xml", FileMode.Open))
                {
                    xmlrw_val = (xmlrw)serializer.Deserialize(file);
                    Logger.Info("Чтение данных из файла storage.xml");
                }
                //Добавляем новые строки в конфигурационный файл.
                string yn = null;
                while (yn != "N")
                {
                    Console.WriteLine("Хотите добавить в список новый URL? YES/NO (Y/N)");
                    yn = Console.ReadLine();
                    if (yn == "Y" || yn == "y")
                    {
                        Console.WriteLine("Введите новый URL:\n");
                        string new_url = Console.ReadLine();
                        if (isValidUrl(new_url) == true)
                        {
                            xmlrw_val.UrlList.Add(new_url);
                            using (StringWriter textWriter = new StringWriter())
                            {
                                serializer.Serialize(textWriter, xmlrw_val);
                                textWriter.Close();
                                Logger.Info("Добавление адреса URL в список проверки");

                            }
                            //Сохранение данных в файл xml с новыми строками.
                            using (FileStream file = new FileStream("storage.xml", FileMode.Create))
                            using (TextWriter xwriter = new StreamWriter(file, new UTF8Encoding()))
                            {
                                serializer.Serialize(xwriter, xmlrw_val);
                                xwriter.Close();
                                Logger.Info("Сохранение адреса URL в список провекри файла storage.xml");
                            }
                            Console.WriteLine("\nURL добавлен в список проверки.\n");
                            Logger.Info("Новый адрес URL успешно добавлен в список проверки");
                        }
                        else
                        {
                            Console.WriteLine("\nТакой URL не существует. Введите правильный URL.\n");
                            Logger.Warn("Неправильный формат введеного адреса URL");

                        }

                    }

                    else
                        if (yn == "N" || yn == "n")
                    {
                        Console.WriteLine("\nДобавление нового URL в список отменено.\n");
                        Logger.Warn("Добавление нового адреса URL в список проверки отменено пользователем");
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Ошибка ввода! Введите Y если желаете добаваить в список проверки новый адрес или N если не желаете продолжать добавление адресов в список.\n");
                        Logger.Warn("Неправильный формат введеного адреса URL");
                    }

                }
            }
            catch (Exception exception)
            {
                Logger.Fatal("Ошибка: не удалось инициализировать запуск редактирования списка URL", exception);
            }


        }
```        
Примечание: Расширение списка сформировано исходя из проектного задания. Соответсвенно на данном этапе возможно только добавление URL в список проверки. Удаление или изменение уже имеющихся в списке адресов не предусмотрено. Данная функция будет реализована после доработки программы в первой половине 2023 года. 

### Отправка отчета посредством электронной почты

В программе реализована функция Report, которая позаоляет отправить файл отчета на указанный адрес электронной почты. При реализации функции использовано пространство имен System.Net.Mail  (https://learn.microsoft.com/ru-ru/dotnet/api/system.net.mail?view=net-7.0). Функция полностью автономна и отправляет файл с результатами проверок report.xml на заранее указанный адрес электронной почты. Адрес электронной почты получателя храниться в файле storage.xml, параметры учетной записи отправителя содержаться в конфигурациооном файле App.config

Конфигурация учетной записи электронной почты в App.config

```sh
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
	<appSettings>
		<add key ="Host" value="smtp.yandex.ru"/>
		<add key ="Port" value="25"/>
		<add key ="SSL" value="true"/>
		<add key ="FromEmail" value="from_mail@yandex.ru"/>
		<add key ="Username" value="from_mail@yandex.ru"/>
		<add key ="Password" value="password"/>
	</appSettings>
</configuration>

```
Адрес получателя в файле storage.xml

```sh
<Email>
		<string>technoservice@nxt.ru</string>
</Email>
```
Функция Report

```sh
static void Report()//Отправка отчета работает корректно.
        {
            Logger.Info("Инициализация отправки отчета на электронную почту.");
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(xmlrw));
                xmlrw xmlrw_val = null;
                using (StreamReader reader = new StreamReader("storage.xml"))
                {
                    xmlrw_val = (xmlrw)serializer.Deserialize(reader);
                    reader.Close();
                    Logger.Info("Чтение актуального адреса эл.посты для отправки отчета из файла storage.xml");
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
                        try
                        {
                            mm.Attachments.Add(attachment);
                            Logger.Info("Файл отчета report.xml прикреплен к сообщению");
                        }
                        catch (Exception exception)
                        {
                            Logger.Error("Ошибка: файл report.xml не найден", exception);
                        }
                        SmtpClient smtp = new SmtpClient();
                        smtp.Host = ConfigurationManager.AppSettings["Host"];
                        smtp.EnableSsl = true;
                        NetworkCredential NetworkCred = new NetworkCredential(ConfigurationManager.AppSettings["Username"], ConfigurationManager.AppSettings["Password"]);
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = NetworkCred;
                        smtp.Port = int.Parse(ConfigurationManager.AppSettings["Port"]);
                        Console.WriteLine("Отправка отчета......");
                        try
                        {
                            smtp.Send(mm);
                            Console.WriteLine($"\nОтчет отправлен на электронную почту {to}.");
                            System.Threading.Thread.Sleep(3000);
                            Logger.Info("Отчет успешно отправлен на указанный адрес электронной почты");
                        }
                        catch (Exception)
                        {
                            Logger.Error("Ошибка: отчет не отправлен, проверьте правильность настроек электронной почты");
                        }

                    }

                }
            }
            catch (Exception exception)
            {
                Logger.Fatal("Ошибка: не удалось инициализировать отправку отчета на электронную почту", exception);
            }
        }
 ```       

### Изменение адреса электронной почты получателя отчета

Программа предусматривает возможность изменить адрес электронной почты получателя отчета проверок. За данный функционал отвечает функция EditMail. При запуске функции программа просит пользователя указать новый адрес электронной поты, после того как данные получены, элемент Email файла storage.xml очишается и прописывается новый адрес. При последующей отправке отчета, он будет отправлен на новый адрес электронной почты. Во избежания ошибок ввода в функции реализована проверка правильности ввода с помощью регулярного выражения прописанного в функции isValid.

Функция EditMail

```sh
static void EditMail()//Добавление адреса электронной почты в файл конфигурации. Реализована проверка правильности ввода через регулярное выражение.
        {
            Logger.Info("Инициализация редактирования адреса электронной почты для отправки отчета");
            try
            {
                Console.WriteLine("\nВведите адрес электронной почты для отправки отчета:\n");
                string new_mail = Console.ReadLine();


                if (isValid(new_mail) == true)
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(xmlrw));
                    xmlrw xmlrw_val = null;
                    //Считываем имеющиеся в конфигурационном файле строки.
                    using (FileStream file = new FileStream("storage.xml", FileMode.Open))
                    {
                        xmlrw_val = (xmlrw)serializer.Deserialize(file);
                        Logger.Info("Чтение данных об адресе электронной почты из файла storage.xml");
                        xmlrw_val.Email.Clear();
                        Logger.Info("Удаление неактуального адреса электронной почты из файла storage.xml");
                        xmlrw_val.Email.Add(new_mail);
                    }

                    using (StringWriter textWriter = new StringWriter())
                    {
                        serializer.Serialize(textWriter, xmlrw_val);
                        Logger.Info("Добавление данных об актуальном адресе электронной почты в файл storage.xml");
                    }
                    //Сохранение данных в файл xml с новыми строками.
                    using (FileStream file = new FileStream("storage.xml", FileMode.Create))
                    using (TextWriter xwriter = new StreamWriter(file, new UTF8Encoding()))
                    {
                        serializer.Serialize(xwriter, xmlrw_val);
                        xwriter.Close();
                        Logger.Info("Сохранение нового адреса электронной почты в файл storage.xml");
                    }
                    Console.WriteLine("\nНовый электронный адрес получателя отчета сохранен.");
                }
                else
                {
                    Console.WriteLine("\nАдрес электронной почты введен с ошибкой.\n");
                    Console.WriteLine("\nЕсли хотите попроьовать снова нажмите Y, если хотите вернуться в основное меню нажмите N.\n");
                    Logger.Warn("Адрес электронной почты введен с ошибкой");

                    var yn = Console.ReadLine();
                    if (yn == "Y" || yn == "y")
                    {
                        EditMail();
                        Logger.Info("Адрес электронной почты для отправки отчета успешно изменен");

                    }
                    if (yn == "N" || yn == "n")
                    {
                        Console.WriteLine("ВНИМАНИЕ!!! Адрес электронной почты не изменен!");
                        Logger.Warn("Добавление нового адреса электронной почты отменено пользователем");
                    }
                    
                }
            }
            catch (Exception exception)
            {
                Logger.Fatal("Ошибка: не удалось инициализировать запуск редактирования электронной почты", exception);
            }
        }
  ``` 
  
Функция для проверки корректности вводимого адреса электронной почты isValid

  ```sh
  public static bool isValid(string email)//Проверка правильности вводе E-MAIL.
        {
            string pattern = "[.\\-_a-z0-9]+@([a-z0-9][\\-a-z0-9]+\\.)+[a-z]{2,6}";
            Match isMatch = Regex.Match(email, pattern, RegexOptions.IgnoreCase);
            return isMatch.Success;
        }
   ``` 
   
### Вывод результатов последней проверки на консоль

Программа предусмартивает просмотр результатов проверки непосредственно в приложении. для этого служит функция reportDisplay. Данная функция считывает данные из файла report.xml и отображает на экране в окне программы. При запуске приложения с любым параметром функция будет запущена автоматически и отобразит результаты последней проверки.

```sh
static void reportDisplay()//Вывод результатов последней проверки на дисплей. Считывается из файла отчета.
        {
            XmlSerializer serializer_rep = new XmlSerializer(typeof(Reprw));
            Reprw rep_rw = null;
            using (StreamReader reader = new StreamReader("report.xml"))
            {
                rep_rw = (Reprw)serializer_rep.Deserialize(reader);
                reader.Close();
            }

            Console.WriteLine("\nРЕЗУЛЬТАТЫ ПРОВЕРКИ URL:");
            foreach (var link in rep_rw.UrlList)
            {
                Console.WriteLine($"\nРесурс: " + link.ResName + "\nСтатус: " + link.Status + "\nВремя проверки: " + link.Dat);
            }
            Console.WriteLine("\nРЕЗУЛЬТАТЫ ПРОВЕРКИ PostgreSQL:");
            foreach (var pg in rep_rw.PostgresList)
            {
                Console.WriteLine($"\nСтрока подключения к БД: " + pg.ResName + "\nСтатус: " + pg.Status + "\nВремя проверки: " + pg.Dat);
            }
        }
```
### Завершение работы программы

Для завершения работы программы используется функция Exit.
Данная функция после того как пользователь нажимает Выход, очищает консоль и отображает стилизованное название программы. После нажатия любой клавиши окно закрывается. В эстетических целях оформлен вывод названия программы в центре экрана нарисованного символами кодировки ASCII.

```sh
static void Exit()//Завершение работы приложеня.
        {
            string centerText1 = "      ::::    ::: :::::::::: ::::::::::: ::::::::  :::    ::: :::::::::: ::::::::  :::    ::: :::::::::: :::::::::\n";
            string centerText2 = "     :+:+:   :+: :+:            :+:    :+:    :+: :+:    :+: :+:       :+:    :+: :+:   :+:  :+:        :+:    :+:\n";
            string centerText3 = "    :+:+:+  +:+ +:+            +:+    +:+        +:+    +:+ +:+       +:+        +:+  +:+   +:+        +:+    +:+ \n";
            string centerText4 = "   +#+ +:+ +#+ +#++:++#       +#+    +#+        +#++:++#++ +#++:++#  +#+        +#++:++    +#++:++#   +#++:++#:   \n";
            string centerText5 = "  +#+  +#+#+# +#+            +#+    +#+        +#+    +#+ +#+       +#+        +#+  +#+   +#+        +#+    +#+   \n";
            string centerText6 = " #+#   #+#+# #+#            #+#    #+#    #+# #+#    #+# #+#       #+#    #+# #+#   #+#  #+#        #+#    #+#    \n";
            string centerText7 = "###    #### ##########     ###     ########  ###    ### ########## ########  ###    ### ########## ###    ###     ";
            string Copyraght = "programming by APK83";

            int centerX1 = (Console.WindowWidth / 2) - (centerText1.Length / 2);
            int centerY1 = (Console.WindowHeight / 2) - 2;
            int centerX2 = (Console.WindowWidth / 2) - (centerText1.Length / 2);
            int centerY2 = (Console.WindowHeight / 2) - 1;
            int centerX3 = (Console.WindowWidth / 2) - (centerText1.Length / 2);
            int centerY3 = Console.WindowHeight / 2;
            int centerX4 = (Console.WindowWidth / 2) - (centerText1.Length / 2);
            int centerY4 = (Console.WindowHeight / 2) + 1;
            int centerX5 = (Console.WindowWidth / 2) - (centerText1.Length / 2);
            int centerY5 = (Console.WindowHeight / 2) + 2;
            int centerX6 = (Console.WindowWidth / 2) - (centerText1.Length / 2);
            int centerY6 = (Console.WindowHeight / 2) + 3;
            int centerX7 = (Console.WindowWidth / 2) - (centerText1.Length / 2);
            int centerY7 = (Console.WindowHeight / 2) + 4;
            int centerXc = (Console.WindowWidth / 2) - (Copyraght.Length / 2);
            int centerYc = (Console.WindowHeight / 2) + 22;

            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.SetCursorPosition(centerX1, centerY1);
            Console.Write(centerText1);
            Console.SetCursorPosition(centerX2, centerY2);
            Console.Write(centerText2);
            Console.SetCursorPosition(centerX3, centerY3);
            Console.Write(centerText3);
            Console.SetCursorPosition(centerX4, centerY4);
            Console.Write(centerText4);
            Console.SetCursorPosition(centerX5, centerY5);
            Console.Write(centerText5);
            Console.SetCursorPosition(centerX6, centerY6);
            Console.Write(centerText6);
            Console.SetCursorPosition(centerX7, centerY7);
            Console.Write(centerText7);
            Console.SetCursorPosition(centerXc, centerYc);
            Console.Write(Copyraght);
            Environment.Exit(3);
            Logger.Info("Завершение работы приложения.");
        }
```

  
## Контактная информация

Курбатов Андрей (APK83)
e-mail: ya.te2016@ya.ru
git profile: https://github.com/APK83

Project Link: [https://github.com/APK83/Case_Beginner](https://github.com/APK83/Case_Beginner)
