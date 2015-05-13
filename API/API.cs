//Оснастить код комментариями
//Сделать unicod в ListenerService
//Дополнить в инструкции раздел ошибок:
//Ошибка при недавней загрузке на github библиотеки и мгновенной скачки оттуда - не может скачать библиотеку
//Ошибка при добавлении новой библиотеки в gac без удаления старой версии - остается привязка к старой версии. Новая игнорируется
//Создать класс, унаследованный от APIResponse, который будет родителем всем классам кроме APIErrorRespomse и будет реализовывать свойство numberOfElements
//Поменять имена переменных на интуитивно понятные: для Meals не bigList, а Meals. И т.д.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Windows;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Threading;
using Microsoft.Win32;

namespace API
{


    public static class Api
    {
        #region Точка входа

        /// <summary>
        /// Генерирует ответ на запрос methodName. Является точкой входа в API
        /// </summary>
        /// <param name="methodName">Название функции</param>
        /// <param name="content">Тип возвращяемого значения</param>
        /// <param name="args">Аргументы</param>
        /// <returns></returns>
        public static string ApiGenerateResponse(string methodName, out string content, IList<string> args)
        {
            //тип ответа
            content = _extensions["html"];
            methodName = methodName.ToLower();
            switch (methodName)
            {
                //возращается объект реализующий интерфейс IApiResponse
                case "/getcountries":
                    content = _extensions["json"];
                    return invokeSQLFunction(getCountries);
                case "/getcurrencies":
                    content = _extensions["json"];
                    return invokeSQLFunction(getCurrencies);
                case "/getmeals":
                    content = _extensions["json"];
                    return invokeSQLFunction(getMeals);
                case "/getcategories":
                    content = _extensions["json"];
                    return invokeSQLFunction(getCategories);
                case "/getregions":
                    content = _extensions["json"];
                    //определить id страны

                    return invokeSQLFunction(getRegions);
                case "/gethotels":
                    content = _extensions["json"];
                    return invokeSQLFunction(getHotels, args);
                case "/getoperators":
                    content = _extensions["json"];
                    return invokeSQLFunction(getOperators);
                case "/getresults":
                    content = _extensions["json"];
                    //получить параметры для поискового запроса
                    return invokeSQLFunction(getResults, args);
                case "/version":
                    content = _extensions["html"];
                    return invokeFunction(getVersion);
                case "/hi":
                    content = _extensions["html"];
                    return invokeFunction(hi);
                case "/getFunctions":
                default:
                    content = _extensions["xml"];
                    return invokeXMLFunction(getFunctions, args);
            }

        }



        #endregion

        #region служебные

        #region переменные


        //строка подключения к БД
        static string _connectionString;
        public static string ConnectionString
        {
            get
            {
                if (Api._connectionString == null)
                {
                    //определить адрес XML-файла конфигурации
                    string XMLFile = ApiXmlFilePath;
                    XmlDocument doc = new XmlDocument();
                    ////открыть XML-файл и считать строку подключения
                    doc.Load(XMLFile);
                    Api._connectionString = doc.SelectSingleNode(".//connectionString").Attributes["name"].Value;
                }
                return Api._connectionString;
            }
        }

        //файл настроек
        static string _apiXmlFilePath;
        public static string ApiXmlFilePath
        {
            get
            {
                if (Api._apiXmlFilePath == null)
                {
                    //считать значение из реестра
                    RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\ListenerService");
                    if (key != null)
                    {
                        Api._apiXmlFilePath = (string)key.GetValue("servicePath") + @"API.xml";
                    }
                    else
                    {
                        throw new Exception("Файл настроек не найден");
                    }
                }
                return Api._apiXmlFilePath;
            }
        }
        //обработчик запросов, которым требуется SQL-подключение
        delegate IApiResponse SQLDelegate(SqlCommand command, IList<string> Args);
        //обработчик запросов, которым требуется XML-файл
        delegate XmlDocument XMLDelegate(IList<string> Args);
        //обработчик запросов, которые есть не просят
        delegate string noFoodDelegate(IList<string> Args);
        //виды возвращаемого контента
        static Dictionary<string, string> _extensions = new Dictionary<string, string>
        { 
            //{ "extension", "content type" }
            { "htm", "text/html"},
            { "html", "text/html" },
            { "xml", "text/xml" },
            { "txt", "text/plain" },
            { "css", "text/css" },
            { "png", "image/png" },
            { "gif", "image/gif" },
            { "jpg", "image/jpg" },
            { "jpeg", "image/jpeg" },
            { "zip", "application/zip"},
            { "json", "application/json"}
        };

        #endregion

        /// <summary>
        /// Вызывает функция переданную в качестве параметра или же сообщение о ненахождении запрашиваемой функции
        /// </summary>
        /// <param name="method">Название запрашиваемой функции</param>
        /// <returns></returns>
        static string invokeSQLFunction(SQLDelegate function, IList<string> args = null)
        {
            //результат работы функции
            IApiResponse apiResponse;
            //соединение с базой данных
            SqlConnection connection = new SqlConnection(ConnectionString);
            SqlCommand command = new SqlCommand();
            try
            {
                command.Connection = connection;
                connection.Open();
                //выполнить обработчик
                apiResponse = function(command, args);
            }
            catch (Exception ex)
            {
                //в случае ошибки создать ответ ошибки
                apiResponse = new ApiErrorResponse(ex.Message);
            }
            finally
            {
                //закрыть соединение с БД
                connection.Close();
            }
            return JsonConvert.SerializeObject(apiResponse);
        }

        static string invokeXMLFunction(XMLDelegate function, IList<string> args = null)
        {

            XmlDocument xmlDoc = function(args);
            //конвертировать XML в строку
            using (var stringWriter = new System.IO.StringWriter())
            using (var xmlTextWriter = XmlWriter.Create(stringWriter))
            {
                xmlDoc.WriteTo(xmlTextWriter);
                xmlTextWriter.Flush();
                return stringWriter.GetStringBuilder().ToString();
            }

        }

        static string invokeFunction(noFoodDelegate function, IList<string> args = null)
        {
            return function(args);
        }

        /// <summary>
        /// Преобразовывает список аргументов, в воспринимаемый функцией getFunctions(убирает "function="). А также исключает из списка одинаковые аргументы
        /// </summary>
        /// <param name="Args">Изменяемые аргументы</param>
        static List<string> parseArgs(IList<string> args, params string[] values)
        {
            List<string> result = new List<string>();
            int i = 0;
            int j = 0;
            foreach (string value in values)
            {
                //найти нужный параметр по имени
                foreach (string arg in args)
                {
                    if (arg.Contains(value))
                    {
                        //если найден, то обрезать по знак равно включительно
                        result.Add(arg.Substring(arg.IndexOf("=") + 1));
                        i++;
                        //выйти из цикла
                        break;
                    }
                }
                //если именованного параметра не найдено
                if (i == j)
                {
                    result.Add(args[i]);
                }
                j++;
            }
            return result;
        }

        #region closed

        /// <summary>
        /// Забираем значение переменнуой из xml файла проекта
        /// </summary>
        /// <param name="typeName">Тип переменной - узел xml документа</param>
        /// <param name="variableName">Имя переменной - атрибут "name" узла xml документа</param>
        /// <returns></returns>
        private static string getFromXML(string typeName, string variableName)
        {
            XmlTextReader reader = new XmlTextReader(_apiXmlFilePath);
            reader.ReadToFollowing(typeName);
            reader.MoveToAttribute("name");
            //Пока достигнем конца файла или не найдем имя нужной переменной
            while (reader.Value != variableName & !reader.EOF)
            {
                //Перемещаемся между узлами typeName и смотрим атрибут "name"
                reader.ReadToFollowing(typeName);
                reader.MoveToAttribute("name");
            }
            if (reader.EOF)
                return "Достигнут конец файла. Заданное имя переменной не найдено.";
            //Если имя нужной переменной совпало с именем, хранимым в xml файле, 
            //возвращаем содержимое узла - значение переменной
            reader.MoveToElement();
            return reader.ReadElementContentAsString();
        }

        private static void outputSqlData(List<List<object>> bigList)
        {
            foreach (List<object> big in bigList)
            {
                foreach (object small in big)
                {
                    Console.Write(small.ToString() + " ");
                }
                Console.WriteLine();
            }
        }


        public static string formRequestString(IList<string> Args)
        {
            string result = "";
            SqlConnection con = new SqlConnection();
            SqlDataReader reader;
            Console.WriteLine(Args[0]);
            string dbCommand = "Select CountryCode from Countries where CountryKey='" + Args[0] + "';";
            reader = openSqlConnectionWithCommandExecution(con, dbCommand);
            reader.Read();
            result += "`" + reader["CountryCode"] + "`|`" + Args[1] + "`|```" + Args[2] + "```|```" + Args[3] + "```";
            for (int i = 4; i < 16; i++)
            {
                result += "|`" + Args[i] + "`";
            }
            result += "|`0`" + "|`price`" + "|`0`" + "|`1`" + "|`" + Args[16] + "`";
            return result;
        }

        //public static string searchEngine(string searchStr)
        //{
        //    SqlDataReader reader = null;
        //    var cmd = new SqlCommand("", new SqlConnection(ConnectionString));
        //    try
        //    {
        //        var service = new travelshop.TravelShopClient();
        //        //сформировать строку параметров для модулей поиска
        //        var id = service.runQuery(90, searchStr);
        //        if (id == 0)
        //            throw new Exception("id таблицы равен 0. Функция не может продолжать работу");
        //        int timer = 0;
        //        //ждем пока выполнится запрос
        //        while ((!service.getState(id)._completed) && (timer < 90))
        //        {
        //            Thread.Sleep(1000);
        //            timer++;
        //        }
        //        //строка для хранимой процедуры
        //        //string sqlStrProc = searchStr.Replace("`", "'").Replace("|", ",");
        //        searchStr = searchStr.Replace("`", "'").Replace("|", ",");
        //        searchStr = "All_Request_v13_4 " + searchStr +
        //            "," + id; //processId
        //        //хз
        //        cmd.Connection.Open();
        //        cmd.CommandText = "select tempTable from online.dbo.ws_query where id=" + id.ToString();
        //        cmd.ExecuteScalar();
        //        cmd.CommandText = "select processId from online.dbo._checkProcess where olTable=(select tempTable from online.dbo.ws_query where id=" + id.ToString() + ")";
        //        var prId = cmd.ExecuteScalar();
        //        cmd.CommandText = "update online.dbo._checkProcess set olReady=1 where processId=" + prId.ToString();
        //        cmd.ExecuteScalar();
        //        cmd.Connection.Close();
        //        cmd.CommandText = searchStr;
        //        cmd.Connection.Open();
        //        List<object> allResults = new List<object>();
        //        List<object> result = null;
        //        int counter = 0;
        //        reader = cmd.ExecuteReader();
        //        while (reader.Read())
        //        {
        //            result = new List<object>();
        //            result.Add(reader["Hotel"]);
        //            result.Add(reader["CategoryDescription"]);
        //            result.Add(reader["RoomType"]);
        //            result.Add(reader["airCharge"]);
        //            result.Add(reader["priceRub"]);
        //            result.Add(reader["BDate"]);
        //            result.Add(reader["Duration"]);
        //            result.Add(reader["MealDescription"]);
        //            result.Add(reader["Destination"]);
        //            result.Add(reader["HotelKey"]);
        //            result.Add(reader["operPrice"]);
        //            result.Add(reader["aircompany"]);
        //            result.Add(reader["OperatorKey"]);
        //            result.Add(reader["destinationKey"]);
        //            result.Add(reader["onlineLink"]);
        //            result.Add(reader["info"]);
        //            counter++;
        //            allResults.Add(result);
        //        }
        //        ApiResultsResponse response = new ApiResultsResponse();
        //        response.ResponseCode = 200;
        //        response.CountOfElements = counter;
        //        response.Results = allResults;
        //        return JsonConvert.SerializeObject(response);
        //    }
        //    catch (Exception e)
        //    {
        //        //Создаем объект результата с ошибкой, заполняем его
        //        ApiErrorResponse error = formErrorResponse(e);
        //        //Сериализуем объект результата
        //        return JsonConvert.SerializeObject(error);
        //    }
        //    finally
        //    {
        //        if (reader != null && !reader.IsClosed)
        //        {
        //            reader.Close();
        //        }
        //        if (!(cmd.Connection.State == System.Data.ConnectionState.Closed))
        //        {
        //            cmd.Connection.Close();
        //        }
        //    }
        //}


        //Если переданное SqlConnection не открыто - открываем его
        //Загружаем в reader данные полученные при выполнении dbCommand
        /// <summary>
        /// Открываем соединение, выполняем команду, записываем данные из таблицы в reader
        /// </summary>
        /// <param name="con">Соединение с базой данных</param>
        /// <param name="reader">reader, который будет хранить информацию, взятую из бд</param>
        /// <param name="dbCommand">Команда к базе данных</param>
        private static SqlDataReader openSqlConnectionWithCommandExecution(SqlConnection connection, string dbCommand)
        {
            SqlCommand command = new SqlCommand(dbCommand, connection);
            //Если соединение зыкрыто - открываем с глобальным значением connectionString
            if (connection != null && connection.State == System.Data.ConnectionState.Closed)
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();
            }
            //если соединение открыто, то выполняем команду
            if (connection.State == System.Data.ConnectionState.Open)
            {
                //Создаем и выполняем команду sql, записывая результат выполнения команды в reader
                return command.ExecuteReader();
            }
            else
            {
                return null;
            }
        }

        #endregion
        #endregion

        #region SQL часть

        /// <summary>
        /// Получаем из базы данных MsSql: id, название и тэг страны, название страны на кириллице и валюту, используемую в этой стране. Полученные данные оборачиваем в json и возвращаем
        /// </summary>
        /// <returns>true или false в зависимости от того, все прошло удачно или нет</returns>
        private static ApiCountriesResponse getCountries(SqlCommand command, IList<string> Args = null)
        {
            //в SQLReader будут помещаться промежуточные данные из таблиц
            SqlDataReader reader = null;
            //экземпляр конечного результата
            command.CommandText = "SELECT countryKey, Country, CountryCode, CountryCyr, currency FROM countries";
            ApiCountriesResponse countryResponse = new ApiCountriesResponse();
            reader = command.ExecuteReader();
            //Записываем в список данные из таблицы по каждой стране
            //Список для хранения всех записей
            List<object> countryList = new List<object>();
            int counter = 0;
            while (reader.Read())
            {
                //Список для хранения одной записи
                List<object> country = new List<object>();
                country.Add(reader["CountryKey"]);
                country.Add(reader["Country"]);
                country.Add(reader["CountryCode"]);
                country.Add(reader["CountryCyr"]);
                country.Add(reader["currency"]);
                countryList.Add(country);
                counter++;
            }
            //Создаем объект результата, говорим, что запрос отработал без ошибок
            //передаем список со всеми записями и число записей
            countryResponse.ResponseCode = 200;
            countryResponse.Countries = countryList;
            countryResponse.CountOfElements = counter;
            return countryResponse;
        }



        /// <summary>
        /// Получаем из базы данных MsSql список доступных валют и их полей, которые потом оборачиваем в json и возвращаем
        /// </summary>
        /// <returns></returns>
        static ApiCurrenciesResponse getCurrencies(SqlCommand command, IList<string> args = null)
        {
            command.CommandText = "SELECT currencyKey, currencyName FROM currencies";
            //Открываем соединение, выполняем команду, записываем данные из таблицы в reader
            SqlDataReader reader = command.ExecuteReader();
            //Содержит список записей всех валют
            List<object> currenciesList = new List<object>();
            //Считает количество записей
            int counter = 0;
            List<object> currency = null;
            while (reader.Read())
            {
                //список для одной записи
                currency = new List<object>();
                currency.Add(reader["currencyKey"]);
                currency.Add(reader["currencyName"]);
                currenciesList.Add(currency);
                counter++;
            }
            //Отмечаем, что запрос обработан успешно, записываем количество записей и передаем сам список записей
            ApiCurrenciesResponse response = new ApiCurrenciesResponse();
            response.ResponseCode = 200;
            response.Currencies = currenciesList;
            response.CountOfElements = counter;
            return response;
        }



        /// <summary>
        /// Получаем из базы данных MsSql список доступных регионов и их полей, которые потом оборачиваем в json и возвращаем
        /// </summary>
        /// <returns></returns>
        static ApiRegionsResponse getRegions(SqlCommand command, IList<string> args = null)
        {
            //Переменная под результат
            string result = String.Empty;
            SqlConnection con = new SqlConnection();
            SqlDataReader reader = null;

            //Загружаем в список ID присутствующих в бд стран
            string dbCommand = "SELECT CountryCode FROM countries";
            //Открываем соединение, выполняем команду, записываем данные из таблицы в reader
            reader = openSqlConnectionWithCommandExecution(con, dbCommand);
            //Создаем список id стран и заполняем его
            List<string> countryCode = new List<string>();
            while (reader.Read())
            {
                countryCode.Add((string)reader["CountryCode"]);
            }

            //Счетчик под количество регионов
            int counter = 0;
            //Список для хранения записей всех существующих регионов
            List<object> bigList = new List<object>();
            //Список для хранения информации об одном регионе
            List<object> smallList = null;
            foreach (string code in countryCode)
            {
                //Для каждой страны берем ее регионы 
                dbCommand = "SELECT * from " + code + "_Destinations";
                //Закрываем reader, хранивший инфу о предыдущих записях(id стран)
                reader.Close();
                //Открываем соединение, выполняем команду, записываем данные из таблицы в reader
                reader = openSqlConnectionWithCommandExecution(con, dbCommand);
                while (reader.Read())
                {
                    counter++;
                    smallList = new List<object>();
                    smallList.Add(reader["CountryKey"]);
                    smallList.Add(reader["DestinationKey"]);
                    smallList.Add(reader["Destination"]);
                    smallList.Add(reader["DestinationCyr"]);
                    //Добавляем поля DisplayOrder и DestParams только у тех регионов, которые хотят отобразить
                    if (reader["DisplayActive"] != DBNull.Value && Convert.ToBoolean(reader["DisplayActive"]) == true)
                    {
                        smallList.Add(reader["DisplayOrder"]);
                        smallList.Add(reader["DestParams"]);
                    }
                    else
                    {
                        smallList.Add(null);
                        smallList.Add(null);
                    }
                    bigList.Add(smallList);
                }
            }
            //Создаем объект результата, говорим, что запрос отработал без ошибок
            //передаем список со всеми записями и число записей
            ApiRegionsResponse response = new ApiRegionsResponse();
            response.ResponseCode = 200;
            response.CountOfElements = counter;
            response.Regions = bigList;

            return response;
        }

        /// <summary>
        /// Получаем из базы данных MsSql список доступных видов питания и их полей, которые потом оборачиваем в json и возвращаем
        /// </summary>
        /// <returns></returns>
        private static IApiResponse getMeals(SqlCommand command, IList<string> args = null)
        {
            command.CommandText = "SELECT * FROM searchForm_mealElements";
            SqlDataReader reader = command.ExecuteReader();
            //Счетчик количества типов питания
            int counter = 0;
            //Список для хранения записей всех типов питания
            List<List<object>> mealsList = new List<List<object>>();
            List<object> meal;
            //Берем id типа питания, его название и параметры из таблицы searchForm_mealElements
            while (reader.Read())
            {
                meal = new List<object>();
                counter++;
                meal.Add(reader["mealElementKey"]);
                meal.Add(reader["mealCyr"]);
                meal.Add(reader["mealParams"]);
                mealsList.Add(meal);
            }
            reader.Close();
            //Берем для каждого типа питания id стран, в которых он присутствует
            command.CommandText = "SELECT * from searchForm_mealElementKey_countryKey";
            //Каждому элементу массива соответствует тип питания. В каждом элементе хранится список id-шников стран
            //в которых встречается этот тип питания
            string[] countryKeysForMeals = new string[counter];
            while (reader.Read())
            {
                //защита от некорректных типов питания, например, под номером 179
                if (Convert.ToInt32(reader["mealElementKey"]) <= counter)
                {
                    countryKeysForMeals[Convert.ToInt32(reader["mealElementKey"]) - 1] += reader["countryKey"].ToString() + ",";
                }
            }
            //Убираем запятую в конце каждой строки idшников стран
            for (int i = 0; i < counter; i++)
            {
                if (countryKeysForMeals[i] != null)
                {
                    countryKeysForMeals[i] = countryKeysForMeals[i].Remove(countryKeysForMeals[i].Length - 1);
                }
            }
            //Добавляем в конец каждого элемента bigList строку с idшниками стран
            int j = 0; //Счетчик номера категории
            foreach (List<object> list in mealsList)
            {
                list.Add(countryKeysForMeals[j]);
                j++;
            }

            //Отмечаем, что запрос обработан успешно, записываем количество записей и передаем сам список записей
            ApiMealsResponse response = new ApiMealsResponse();
            response.ResponseCode = 200;
            response.CountOfElements = counter;
            response.Meals = mealsList;
            //Сериализуем результирующий объект

            return response;
        }

        /// <summary>
        /// Получаем из базы данных MsSql список доступных категорий и их полей, которые потом оборачиваем в json и возвращаем
        /// </summary>
        /// <returns></returns>
        static ApiCategoriesResponse getCategories(SqlCommand command, IList<string> args = null)
        {
            command.CommandText = "SELECT categoryId, categoryText FROM category";

            SqlDataReader reader = command.ExecuteReader();

            //Будем хранить записи всех типов категорий
            List<object> categoriesList = new List<object>();
            //Считает количество записей
            int counter = 0;
            //Будет хранить позицию категории "HV-1". Она отличается от остальных категорий тем, что распространена
            //только в одной странуе - Турции
            int hv1Pos = 0;
            //Заполняем поля categoryId и categoryText нашего ответа
            while (reader.Read())
            {
                List<object> smallList = new List<object>(5);
                smallList.Add(reader["categoryId"]);
                Console.WriteLine(reader["categoryText"]);
                smallList.Add(reader["categoryText"]);
                categoriesList.Add(smallList);
                if (reader["categoryText"].ToString() == "HV1")
                    hv1Pos = counter;
                counter++;
            }

            //Заполняем поле params idшниками стран, в которых встречается наша категория
            //"Очищаем" reader от предыдущих записей
            reader.Close();
            command.CommandText = "SELECT CountryKey FROM countries";
            //Открываем соединение, выполняем команду, записываем данные из таблицы в reader
            //openSqlConnectionWithCommandExecution(ref con, out reader, dbCommand);
            string[] countryKeysForCategories = new string[counter];
            //Для всех категорий поле params одинаково, кроме HV1
            while (reader.Read())
            {
                countryKeysForCategories[0] += reader["CountryKey"].ToString() + ",";
            }
            //Убираем лишнюю запятую в конце строки параметров
            countryKeysForCategories[0] = countryKeysForCategories[0].Remove(countryKeysForCategories[0].Length - 1);
            for (int i = 1; i < counter; i++)
            {
                countryKeysForCategories[i] = countryKeysForCategories[0];
            }
            //Для категории "HV-1" в params идет только idшник Турции
            countryKeysForCategories[hv1Pos] = "1";
            int j = 0;
            //Добавляем каждой категории строку в id тех стран, в которых она встречается
            foreach (List<object> list in categoriesList)
            {
                list.Add(countryKeysForCategories[j]);
                j++;
            }
            //Отмечаем, что запрос обработан успешно, записываем количество записей и передаем сам список записей
            ApiCategoriesResponse response = new ApiCategoriesResponse();
            response.ResponseCode = 200;
            response.Categories = categoriesList;
            response.CountOfElements = counter;

            return response;
        }

        /// <summary>
        /// Получаем из базы данных MsSql список доступных отелей и их полей, которые потом оборачиваем в json и возвращаем
        /// </summary>
        /// <param name="CountryKey">Id страны, для которой должны передать список отелей</param>
        /// <returns></returns>
        static ApiHotelsResponse getHotels(SqlCommand command, IList<string> args)
        {
            List<string> parsed = parseArgs(args, "countryId");

            string CountryKey = parsed[0];
            //Создаем переменную под результат


            SqlDataReader reader = null;
            string dbCommand;
            //Если CountryKey = 0 выгружаем из базы тэг и id одной страны, иначе - всех стран
            if (CountryKey.CompareTo("0") != 0)
            {
                command.CommandText = "SELECT CountryCode, CountryKey FROM countries where CountryKey=" + CountryKey.ToString();
            }
            else
            {
                command.CommandText = "SELECT CountryCode, CountryKey FROM countries";
            }
            //Открываем соединение, выполняем команду, записываем данные из таблицы в reader
            //openSqlConnectionWithCommandExecution(ref con, out reader, dbCommand);
            //Объект хранит тэг и id одной страны
            List<object> country = null;
            //Объект хранит тэги и id всех стран
            List<object> countries = new List<object>();
            while (reader.Read())
            {
                country = new List<object>();
                country.Add(reader["CountryCode"]);
                country.Add(reader["CountryKey"]);
                countries.Add(country);
            }
            //Хранит запись об одном отеле
            List<object> hotel = null;
            //Хранит записи обо всех отелях
            List<object> hotels = new List<object>();
            //Считает количество записей
            int counter = 0;
            foreach (List<object> Country in countries)
            {
                //"Очищаем" reader от предыдущих записей
                reader.Close();
                //Берем только те отели, которые присутствуют в обеих таблицах: %countryCode%_hotels и all_hotels_actual
                dbCommand = "SELECT h.HotelKey, Hotel, Category, CategoryDescription, DestinationKey from " + Country[0].ToString() + "_Hotels as h, all_hotels_actual as act where act.HotelKey=h.HotelKey and act.CountryKey=" + Country[1].ToString();
                //Открываем соединение, выполняем команду, записываем данные из таблицы в reader
                //openSqlConnectionWithCommandExecution(ref con, out reader, dbCommand);
                while (reader.Read())
                {
                    counter++;
                    hotel = new List<object>();
                    //id страны, из которой этот отель
                    hotel.Add(Country[1]);
                    hotel.Add(reader["HotelKey"]);
                    hotel.Add(reader["Hotel"]);
                    hotel.Add(reader["Category"]);
                    hotel.Add(reader["CategoryDescription"]);
                    hotel.Add(reader["DestinationKey"]);
                    hotels.Add(hotel);
                }
            }
            //Отмечаем, что запрос обработан успешно, записываем количество записей и передаем сам список записей
            ApiHotelsResponse response = new ApiHotelsResponse();
            response.ResponseCode = 200;
            response.CountOfElements = counter;
            response.Hotels = hotels;
            return response;
        }

        /// <summary>
        /// Получаем из базы данных MsSql список доступных операторов и их полей, которые потом оборачиваем в json и возвращаем
        /// </summary>
        /// <returns></returns>
        static ApiOperatorsResponse getOperators(SqlCommand command, IList<string> args)
        {
            //Создаем переменную под результат
            SqlDataReader reader;

            //Берем только те операторы, которые фигурируют в таблице operatorCountries и есть хотя бы 1 активная страна
            command.CommandText = "SELECT DISTINCT o.OperatorKey as OperatorId, o.Operator as OperatorName FROM Operators as o, OnLine.dbo.operatorCountries as oc where o.OperatorKey = oc.OperatorKey and oc.";
            reader = command.ExecuteReader();
            //Открываем соединение, выполняем команду, записываем данные из таблицы в reader
            //openSqlConnectionWithCommandExecution(ref con, out reader, dbCommand);
            //счетчик количества операторов
            int counter = 0;
            //хранит информацию об одном операторе
            List<object> op = null;
            //хранит информацию обо всех операторах
            List<object> operators = new List<object>();
            while (reader.Read())
            {
                op = new List<object>();
                op.Add(reader["OperatorKey"]);
                op.Add(reader["Operator"]);
                operators.Add(op);
                counter++;
            }
            //Берем для каждого оператора id стран, в которых он действует
            foreach (List<object> oper in operators)
            {
                reader.Close();
                command.CommandText = "SELECT * from OnLine.dbo.operatorCountries where operatorKey=" + oper[0];
                //Открываем соединение, выполняем команду, записываем данные из таблицы в reader
                //openSqlConnectionWithCommandExecution(ref con, out reader, dbCommand);
                //Строка хранит idшники стран, в которых действует оператор 
                string countryKeysForOperators = String.Empty;
                while (reader.Read())
                {
                    countryKeysForOperators += reader["countryKey"].ToString() + ",";
                }
                //Убираем лишнюю запятую в конце строки - можно сделать с помощью trim
                if (countryKeysForOperators != null & countryKeysForOperators.Length > 1)
                {
                    countryKeysForOperators = countryKeysForOperators.Remove(countryKeysForOperators.Length - 1);
                }
                oper.Add(countryKeysForOperators);
            }
            //Отмечаем, что запрос обработан успешно, записываем количество записей и передаем сам список записей
            ApiOperatorsResponse response = new ApiOperatorsResponse();
            response.ResponseCode = 200;
            response.CountOfElements = counter;
            response.Operators = operators;
            return response;
        }

        static ApiResultsResponse getResults(SqlCommand command, IList<string> Args)
        {
            ApiResultsResponse response = new ApiResultsResponse();
            response.CountOfElements = 0;
            response.ResponseCode = 200;
            response.Results = "in progress";
            return response;
        }
        #endregion

        #region noFood часть
        static string getVersion(IList<string> Args)
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
        private static string hi(IList<string> Args)
        {
            Thread.Sleep(900);
            return "hello";
        }
        #endregion

        #region XML часть

        /// <summary>
        /// При задании параметра Args выводит информацию в виде строки о функциях, указанных в качестве параметра. При отсутствии параметра Args выводит информацию обо всех функциях
        /// </summary>
        /// <param name="Args">Аргументы</param>
        /// <returns></returns>
        private static XmlDocument getFunctions(IList<string> Args)
        {
            string filename = ApiXmlFilePath;
            //Загрузка xml документа по нужному адресу   
            XmlDocument doc = new XmlDocument();
            XmlDocument outDoc = new XmlDocument();
            doc.Load(filename);
            XmlNode functions;
            //Получение первой строки xml документа для передачи
            //Если запрашивается список всех функций
            //if (Args.Count == 0)
            //{
            //Получение списка всех узлов с тегом <Functions>
            functions = doc.SelectSingleNode(".//Functions");
            outDoc.LoadXml(functions.OuterXml);
            //}
            //Если в качестве параметров переданы названия не всех, а некоторых функций


            return outDoc;
        }

        #endregion
    }
}
