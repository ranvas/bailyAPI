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

namespace API
{
  #region Классы
  abstract class ApiResponse
  {
    private int responseCode;

    public int ResponseCode
    {
      set
      {
        responseCode = value;
      }
      get
      {
        return responseCode;
      }
    }
  }

  class ApiCurrenciesResponse : ApiResponse
  {
    private int numberOfElements;
    private object currencies;

    public int NumberOfElements
    {
      set
      {
        numberOfElements = value;
      }
      get
      {
        return numberOfElements;
      }
    }

    public object Currencies
    {
      set
      {
        currencies = value;
      }
      get
      {
        return currencies;
      }
    }
  }

  class ApiCountriesResponse : ApiResponse
  {
    private int numberOfElements;
    private object countries;

    public int NumberOfElements
    {
      set
      {
        numberOfElements = value;
      }
      get
      {
        return numberOfElements;
      }
    }

    public object Countries
    {
      set
      {
        countries = value;
      }
      get
      {
        return countries;
      }
    }
  }

  class ApiRegionsResponse : ApiResponse
  {
    private int numberOfElements;
    private object regions;

    public int NumberOfElements
    {
      set
      {
        numberOfElements = value;
      }
      get
      {
        return numberOfElements;
      }
    }

    public object Regions
    {
      set
      {
        regions = value;
      }
      get
      {
        return regions;
      }
    }
  }

  class ApiMealsResponse : ApiResponse
  {
    private int numberOfElements;
    private object meals;

    public int NumberOfElements
    {
      set
      {
        numberOfElements = value;
      }
      get
      {
        return numberOfElements;
      }
    }

    public object Meals
    {
      set
      {
        meals = value;
      }
      get
      {
        return meals;
      }
    }
  }

  class ApiCategoriesResponse : ApiResponse
  {
    private int numberOfElements;
    private object categories;

    public int NumberOfElements
    {
      set
      {
        numberOfElements = value;
      }
      get
      {
        return numberOfElements;
      }
    }

    public object Categories
    {
      set
      {
        categories = value;
      }
      get
      {
        return categories;
      }
    }
  }

  class ApiErrorResponse : ApiResponse
  {
    private string errorMessage;

    public string ErrorMessage
    {
      set
      {
        errorMessage = value;
      }
      get
      {
        return errorMessage;
      }
    }
  }

  class ApiHotelsResponse : ApiResponse
  {
    private int numberOfElements;
    private object hotels;

    public int NumberOfElements
    {
      set
      {
        numberOfElements = value;
      }
      get
      {
        return numberOfElements;
      }
    }

    public object Hotels
    {
      set
      {
        hotels = value;
      }
      get
      {
        return hotels;
      }
    }
  }

  class ApiOperatorsResponse : ApiResponse
  {
    private int numberOfElements;
    private object operators;

    public int NumberOfElements
    {
      set
      {
        numberOfElements = value;
      }
      get
      {
        return numberOfElements;
      }
    }

    public object Operators
    {
      set
      {
        operators = value;
      }
      get
      {
        return operators;
      }
    }
  }

  class ApiResultsResponse : ApiResponse
  {
    private int numberOfElements;
    private object results;

    public int NumberOfElements
    {
      set
      {
        numberOfElements = value;
      }
      get
      {
        return numberOfElements;
      }
    }

    public object Results
    {
      set
      {
        results = value;
      }
      get
      {
        return results;
      }
    }
  }
  #endregion

  public static class Api
  {
    //Путь к файлу xml. В нем лежит описания функций и изменяемые переменные библиотеки
    static string ApiXmlFilePath = System.Windows.Forms.Application.StartupPath + @"\API.xml";
    static string connectionString;

    #region Внутренняя часть

    /// <summary>
    /// Преобразовывает список аргументов, в воспринимаемый функцией getFunctions(убирает "function="). А также исключает из списка одинаковые аргументы
    /// </summary>
    /// <param name="Args">Изменяемые аргументы</param>
    private static void parseArgs(ref IList<string> Args)
    {
      string temp;
      //Удаление одинаковых аргументов
      if (Args.Count >= 2)
        for (int i = 0; i < Args.Count - 1; i++)
          for (int j = i + 1; j < Args.Count; j++)
            if (Args[i] == Args[j])
            {
              Args.RemoveAt(j);
              j--;
            }
      //Обрезание аргументов со знаком равно включительно
      for (int i = 0; i < Args.Count; i++)
      {
        temp = Args[i].Substring(Args[i].IndexOf("=") + 1);
        Args[i] = temp;
      }
    }

    //Если переданное SqlConnection не открыто - открываем его
    //Загружаем в reader данные полученные при выполнении dbCommand
    /// <summary>
    /// Открываем соединение, выполняем команду, записываем данные из таблицы в reader
    /// </summary>
    /// <param name="con">Соединение с базой данных</param>
    /// <param name="reader">reader, который будет хранить информацию, взятую из бд</param>
    /// <param name="dbCommand">Команда к базе данных</param>
    private static void openSqlConnectionWithCommandExecution(ref SqlConnection con, out SqlDataReader reader, string dbCommand)
    {
      //connectionString бд, с которой работаем
      string stosha = connectionString;
      //Если соединение не открыто - открываем
      if (con.State != System.Data.ConnectionState.Open)
      {
        con.ConnectionString = stosha;
        con.Open();
      }
      //Создаем и выполняем команду sql, записывая результат выполнения команды в reader
      SqlCommand comm;
      comm = new SqlCommand(dbCommand, con);
      reader = comm.ExecuteReader();
    }

    /// <summary>
    /// Забираем значение переменнуой из xml файла проекта
    /// </summary>
    /// <param name="typeName">Тип переменной - узел xml документа</param>
    /// <param name="variableName">Имя переменной - атрибут "name" узла xml документа</param>
    /// <returns></returns>
    private static string getFromXML(string typeName, string variableName)
    {
      XmlTextReader reader = new XmlTextReader(ApiXmlFilePath);
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

    /// <summary>
    /// Генерируем ответ, сообщающий об ошибке 
    /// </summary>
    /// <param name="e">Объект исключения</param>
    /// <returns></returns>
    private static ApiErrorResponse formErrorResponse(Exception e)
    {
      //Создаем объект ошибочного результата
      ApiErrorResponse response = new ApiErrorResponse();
      //Генерируем код ошибки
      response.ResponseCode = 400;
      //и ее текст
      response.ErrorMessage = e.Message;
      return response;
    }

    public static string formRequestString(IList<string> Args)
    {
      string result = "";
      SqlConnection con = new SqlConnection();
      SqlDataReader reader;
      Console.WriteLine(Args[0]);
      string dbCommand = "Select CountryCode from Countries where CountryKey='" + Args[0] + "';";
      openSqlConnectionWithCommandExecution(ref con, out reader, dbCommand);
      reader.Read();
      result += "`" + reader["CountryCode"] + "`|`" + Args[1] + "`|```" + Args[2] + "```|```" + Args[3] + "```";
      for (int i = 4; i < 16; i++)
      {
        result += "|`" + Args[i] + "`";
      }
      result += "|`0`" + "|`price`" + "|`0`" + "|`1`" + "|`" + Args[16] + "`";
      return result;
    }

    public static string searchEngine(string searchStr)
    {
      SqlDataReader reader = null;
      var cmd = new SqlCommand("", new SqlConnection("Data Source=stosha;User ID=mike;Password=mxlgrv"));
      try
      {
        var service = new travelshop.TravelShopClient();
        //сформировать строку параметров для модулей поиска
        var id = service.runQuery(90, searchStr);
        if (id == 0)
          throw new Exception("id таблицы равен 0. Функция не может продолжать работу");
        int timer = 0;
        //ждем пока выполнится запрос
        while ((!service.getState(id)._completed) && (timer < 90))
        {
          Thread.Sleep(1000);
          timer++;
        }
        //строка для хранимой процедуры
        //string sqlStrProc = searchStr.Replace("`", "'").Replace("|", ",");
        searchStr = searchStr.Replace("`", "'").Replace("|", ",");
        searchStr = "All_Request_v13_4 " + searchStr +
            "," + id; //processId
        //хз
        cmd.Connection.Open();
        cmd.CommandText = "select tempTable from online.dbo.ws_query where id=" + id.ToString();
        cmd.ExecuteScalar();
        cmd.CommandText = "select processId from online.dbo._checkProcess where olTable=(select tempTable from online.dbo.ws_query where id=" + id.ToString() + ")";
        var prId = cmd.ExecuteScalar();
        cmd.CommandText = "update online.dbo._checkProcess set olReady=1 where processId=" + prId.ToString();
        cmd.ExecuteScalar();
        cmd.Connection.Close();
        cmd.CommandText = searchStr;
        cmd.Connection.Open();
        List<object> allResults = new List<object>();
        List<object> result = null;
        int counter = 0;
        reader = cmd.ExecuteReader();
        while (reader.Read())
        {
          result = new List<object>();
          result.Add(reader["Hotel"]);
          result.Add(reader["CategoryDescription"]);
          result.Add(reader["RoomType"]);
          result.Add(reader["airCharge"]);
          result.Add(reader["priceRub"]);
          result.Add(reader["BDate"]);
          result.Add(reader["Duration"]);
          result.Add(reader["MealDescription"]);
          result.Add(reader["Destination"]);
          result.Add(reader["HotelKey"]);
          result.Add(reader["operPrice"]);
          result.Add(reader["aircompany"]);
          result.Add(reader["OperatorKey"]);
          result.Add(reader["destinationKey"]);
          result.Add(reader["onlineLink"]);
          result.Add(reader["info"]);
          counter++;
          allResults.Add(result);
        }
        ApiResultsResponse response = new ApiResultsResponse();
        response.ResponseCode = 200;
        response.NumberOfElements = counter;
        response.Results = allResults;
        return JsonConvert.SerializeObject(response);
      }
      catch (Exception e)
      {
        //Создаем объект результата с ошибкой, заполняем его
        ApiErrorResponse error = formErrorResponse(e);
        //Сериализуем объект результата
        return JsonConvert.SerializeObject(error);
      }
      finally
      {
        if (reader != null && !reader.IsClosed)
        {
          reader.Close();
        }
        if (!(cmd.Connection.State == System.Data.ConnectionState.Closed))
        {
          cmd.Connection.Close();
        }
      }
    }

    #endregion

    #region Тестовая часть

    public static string hi()
    {
      Thread.Sleep(90000);
      return "hi";
    }

    #endregion

    #region Внешняя распределительная часть
    /// <summary>
    /// Генерирует ответ на запрос methodName
    /// </summary>
    /// <param name="methodName">Название функции</param>
    /// <param name="content">Тип возвращяемого значения</param>
    /// <param name="Args">Аргументы</param>
    /// <returns></returns>
    public static string ApiGenerateResponse(string methodName, out string content, string connectionString, IList<string> Args = null)
    {
      //Присвоение статичной переменной connectionString - connectionString полученной из Listener
      Api.connectionString = connectionString;
      methodName = methodName.ToLower();
      //На пустой запрос или "getFunctions" вызываем эту функцию
      if (string.Equals(methodName, "/getFunctions", StringComparison.OrdinalIgnoreCase) | string.Equals(methodName, "/", StringComparison.OrdinalIgnoreCase))
      {
        content = "text/xml";
        //Если аргументы переданы, парсим их до нужного состояния
        if (Args.Count > 0)
        {
          parseArgs(ref Args);
        }
        //Возвращаем результат выполнения getFunctions
        return getFunctions(Args);
      }
      //На все остальные запросы вызываем другие функции
      else
      {
        content = "application/json";
        return invokeApiFunction(methodName, Args);
      }
    }
    
    /// <summary>
    /// Вызывает функция переданную в качестве параметра или же сообщение о ненахождении запрашиваемой функции
    /// </summary>
    /// <param name="method">Название запрашиваемой функции</param>
    /// <returns></returns>
    public static string invokeApiFunction(string method, IList<string> Args = null)
    {
      try
      {
        string methodName = method.ToLower();
        string answer = "Должна быть вызвана функция: " + method;
        switch (methodName)
        {
          //возращается объект типа ApiResponse
          case "/getcountries":
            return getCountries();
          case "/getcurrencies":
            return getCurrencies();
          case "/getmeals":
            return getMeals();
          case "/getcategories":
            return getCategories();
          case "/getregions":
            return getRegions();
          case "/gethotels":
            {
              if (Args.Count > 0)
              {
                parseArgs(ref Args);
                return getHotels(Args[0]);
              }
              else
                return getHotels();
            }
          case "/getresults":
            return getResults(Args);
          case "/getoperators":
            return getOperators();
          case "/version":
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
          case "/hi":
            return hi();
          default:
            //Здесь должен заполняться отчет об ошибке - нужная функция не найдена
            return answer + " that doesn't exist";
        }
      }
      catch (Exception e)
      {
        return e.Message;
      }
    }

    #endregion

    #region Внешняя часть

    /// <summary>
    /// При задании параметра Args выводит информацию в виде строки о функциях, указанных в качестве параметра. При отсутствии параметра Args выводит информацию обо всех функциях
    /// </summary>
    /// <param name="Args">Аргументы</param>
    /// <returns></returns>
    public static string getFunctions(IList<string> Args = null)
    {
      string filename = ApiXmlFilePath;
      //переменная под результат
      string answer = String.Empty;
      //Загрузка xml документа по нужному адресу   
      XmlDocument doc = new XmlDocument();
      doc.Load(filename);
      XmlNodeList nodeList = null;
      //Получение адреса xml файла, в котором хранится список функций
      try
      {
        //Получение первой строки xml документа для передачи
        string fullDoc = doc.OuterXml;
        int length = fullDoc.IndexOf(">") + 1;
        string firstString = fullDoc.Substring(0, length);
        answer = firstString;
        //Если запрашивается список всех функций
        if (Args.Count == 0)
        {
          //Получение списка всех узлов с тегом <Functions>
          nodeList = doc.GetElementsByTagName("Functions");
          XmlAttribute version = doc.CreateAttribute("version");
          version.Value = Assembly.GetAssembly(typeof(Api)).GetName().Version.ToString();
          nodeList[0].Attributes.Append(version);
          answer += nodeList[0].OuterXml;
        }
        //Если в качестве параметров переданы названия не всех, а некоторых функций
        else
        {
          //Создаем элемент, который послужит корнем возвращаемого xml файла
          XmlElement functions = doc.CreateElement("Functions");
          //Строка на случай ненахождения запрашиваемых функций
          string errorString = "Not all functions your were searching for were found. Not Found -";
          //В этом узле будет храниться информация о ненайденных функциях
          XmlElement error = null;
          //Перебираем все переданные названия функций. Что находим - возвращаем. 
          //Сообщаем, если не находим нужное название, в errorString
          for (int i = 0; i < Args.Count; i++)
          {
            nodeList = doc.GetElementsByTagName(Args[i]);
            if (nodeList.Count == 0)
            {
              if (error == null)
              {
                error = doc.CreateElement("error");
                error.InnerText = errorString;
              }
              error.InnerText += " " + Args[i];
            }
            else
            {
              functions.AppendChild(nodeList[0]);
            }
          }
          //Если были сообщения о ненайденных функциях - добавляем его в выходной xml
          if (error != null)
            functions.AppendChild(error);
          return answer + functions.OuterXml;
        }
      }
      catch (Exception e)
      {
        return "Failed: " + e.Message;
      }
      return answer;
    }

    /// <summary>
    /// Получаем из базы данных MsSql список доступных валют и их полей, которые потом оборачиваем в json и возвращаем
    /// </summary>
    /// <returns></returns>
    public static string getCurrencies()
    {
      //Создаем переменную под результат
      string result = String.Empty;
      SqlConnection con = new SqlConnection();
      try
      {
        string dbCommand = "SELECT currencyKey, currencyName FROM currencies";
        SqlDataReader reader;
        //Открываем соединение, выполняем команду, записываем данные из таблицы в reader
        openSqlConnectionWithCommandExecution(ref con, out reader, dbCommand);
        //Содержит список записей всех валют
        List<object> bigList = new List<object>();
        //Считает количество записей
        int counter = 0;
        List<object> smallList = null;
        while (reader.Read())
        {
          //список для одной записи
          smallList = new List<object>(2);
          smallList.Add(reader["currencyKey"]);
          smallList.Add(reader["currencyName"]);
          bigList.Add(smallList);
          counter++;
        }

        //Отмечаем, что запрос обработан успешно, записываем количество записей и передаем сам список записей
        ApiCurrenciesResponse response = new ApiCurrenciesResponse();
        response.ResponseCode = 200;
        response.Currencies = bigList;
        response.NumberOfElements = counter;
        //Сериализуем результирующий объект
        result = JsonConvert.SerializeObject(response);
      }
      catch (Exception e)
      {
        //Создаем объект результата с ошибкой, заполняем его
        ApiErrorResponse response = formErrorResponse(e);
        //Сериализуем объект результата
        result = JsonConvert.SerializeObject(response);
      }
      finally
      {
        //Если соединение не закрыто - закрываем его
        if (con.State != System.Data.ConnectionState.Closed)
          con.Close();
      }
      return result;
    }

    /// <summary>
    /// Получаем из базы данных MsSql: id, название и тэг страны, название страны на кириллице и валюту, используемую в этой стране. Полученные данные оборачиваем в json и возвращаем
    /// </summary>
    /// <returns>Строку, в которую завернут объект json</returns>
    public static string getCountries()
    {
      //Переменная под строку результата
      string result = String.Empty;
      SqlConnection con = new SqlConnection();
      try
      {
        string dbCommand = "SELECT countryKey, Country, CountryCode, CountryCyr, currency FROM countries";
        SqlDataReader reader;
        //Открываем соединение, выполняем команду, записываем данные из таблицы в reader
        openSqlConnectionWithCommandExecution(ref con, out reader, dbCommand);
        //Записываем в список данные из таблицы по каждой стране
        //Список для хранения всех записей
        List<object> bigList = new List<object>();
        //счетчик количества записей
        int counter = 0;
        while (reader.Read())
        {
          //Список для хранения одной записи
          List<object> smallList = new List<object>(5);
          smallList.Add(reader["CountryKey"]);
          smallList.Add(reader["Country"]);
          smallList.Add(reader["CountryCode"]);
          smallList.Add(reader["CountryCyr"]);
          smallList.Add(reader["currency"]);
          bigList.Add(smallList);
          counter++;
        }
        //Создаем объект результата, говорим, что запрос отработал без ошибок
        //передаем список со всеми записями и число записей
        ApiCountriesResponse response = new ApiCountriesResponse();
        response.ResponseCode = 200;
        response.Countries = bigList;
        response.NumberOfElements = counter;
        //Сериализуем объект результата
        result = JsonConvert.SerializeObject(response);
      }
      catch (Exception e)
      {
        //Создаем объект результата с ошибкой, заполняем его
        ApiErrorResponse error = formErrorResponse(e);
        //Сериализуем объект результата
        result = JsonConvert.SerializeObject(error);
      }
      finally
      {
        //Если соединение не закрыто - закрываем его
        if (con.State != System.Data.ConnectionState.Closed)
          con.Close();
      }
      return result;
    }

    /// <summary>
    /// Получаем из базы данных MsSql список доступных регионов и их полей, которые потом оборачиваем в json и возвращаем
    /// </summary>
    /// <returns></returns>
    public static string getRegions()
    {
      //Переменная под результат
      string result = String.Empty;
      SqlConnection con = new SqlConnection();
      SqlDataReader reader = null;
      try
      {
        //Загружаем в список ID присутствующих в бд стран
        string dbCommand = "SELECT CountryCode FROM countries";
        //Открываем соединение, выполняем команду, записываем данные из таблицы в reader
        openSqlConnectionWithCommandExecution(ref con, out reader, dbCommand);
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
          openSqlConnectionWithCommandExecution(ref con, out reader, dbCommand);
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
        response.NumberOfElements = counter;
        response.Regions = bigList;
        //Сериализуем объект результата
        result = JsonConvert.SerializeObject(response);
      }
      catch (Exception e)
      {
        //Создаем объект результата с ошибкой, заполняем его
        ApiErrorResponse error = formErrorResponse(e);
        //Сериализуем объект результата
        result = JsonConvert.SerializeObject(error);
      }
      finally
      {
        //Если соединение не закрыто - закрываем его
        if (con.State != System.Data.ConnectionState.Closed)
          con.Close();
      }
      return result;
    }

    /// <summary>
    /// Получаем из базы данных MsSql список доступных видов питания и их полей, которые потом оборачиваем в json и возвращаем
    /// </summary>
    /// <returns></returns>
    public static string getMeals()
    {
      //Переменная под результат
      string result = String.Empty;
      SqlConnection con = new SqlConnection();
      SqlDataReader reader = null;
      try
      {
        string dbCommand = "SELECT * FROM searchForm_mealElements";
        //Открываем соединение, выполняем команду, записываем данные из таблицы в reader
        openSqlConnectionWithCommandExecution(ref con, out reader, dbCommand);

        //Счетчик количества типов питания
        int counter = 0;
        //Список для хранения записей всех типов питания
        List<List<object>> bigList = new List<List<object>>();
        List<object> smallList = null;
        //Берем id типа питания, его название и параметры из таблицы searchForm_mealElements
        while (reader.Read())
        {
          smallList = new List<object>();
          counter++;
          smallList.Add(reader["mealElementKey"]);
          smallList.Add(reader["mealCyr"]);
          smallList.Add(reader["mealParams"]);
          bigList.Add(smallList);
        }

        //Берем для каждого типа питания id стран, в которых он присутствует
        dbCommand = "SELECT * from searchForm_mealElementKey_countryKey";
        reader.Close();
        openSqlConnectionWithCommandExecution(ref con, out reader, dbCommand);
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
        foreach (List<object> list in bigList)
        {
          list.Add(countryKeysForMeals[j]);
          j++;
        }

        //Отмечаем, что запрос обработан успешно, записываем количество записей и передаем сам список записей
        ApiMealsResponse response = new ApiMealsResponse();
        response.ResponseCode = 200;
        response.NumberOfElements = counter;
        response.Meals = bigList;
        //Сериализуем результирующий объект
        result = JsonConvert.SerializeObject(response);
      }
      catch (Exception e)
      {
        //Создаем объект результата с ошибкой, заполняем его
        ApiErrorResponse error = formErrorResponse(e);
        //Сериализуем объект результата
        result = JsonConvert.SerializeObject(error);
      }
      finally
      {
        //Если соединение еще не закрыто закрываем его
        if (con.State != System.Data.ConnectionState.Closed)
          con.Close();
      }
      return result;
    }

    /// <summary>
    /// Получаем из базы данных MsSql список доступных категорий и их полей, которые потом оборачиваем в json и возвращаем
    /// </summary>
    /// <returns></returns>
    public static string getCategories()
    {
      //Создаем переменную под результат
      string result = String.Empty;
      SqlConnection con = new SqlConnection();
      try
      {
        string dbCommand = "SELECT categoryId, categoryText FROM category";
        SqlDataReader reader;
        //Открываем соединение, выполняем команду, записываем данные из таблицы в reader
        openSqlConnectionWithCommandExecution(ref con, out reader, dbCommand);

        //Будем хранить записи всех типов категорий
        List<object> bigList = new List<object>();
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
          bigList.Add(smallList);
          if (reader["categoryText"].ToString() == "HV1")
            hv1Pos = counter;
          counter++;
        }

        //Заполняем поле params idшниками стран, в которых встречается наша категория
        //"Очищаем" reader от предыдущих записей
        reader.Close();
        dbCommand = "SELECT CountryKey FROM countries";
        //Открываем соединение, выполняем команду, записываем данные из таблицы в reader
        openSqlConnectionWithCommandExecution(ref con, out reader, dbCommand);
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
        foreach (List<object> list in bigList)
        {
          list.Add(countryKeysForCategories[j]);
          j++;
        }

        //Отмечаем, что запрос обработан успешно, записываем количество записей и передаем сам список записей
        ApiCategoriesResponse response = new ApiCategoriesResponse();
        response.ResponseCode = 200;
        response.Categories = bigList;
        response.NumberOfElements = counter;
        //Сериализуем результирующий объект
        result = JsonConvert.SerializeObject(response);
      }
      catch (Exception e)
      {
        //Создаем объект результата с ошибкой, заполняем его
        ApiErrorResponse error = formErrorResponse(e);
        //Сериализуем объект результата
        result = JsonConvert.SerializeObject(error);
      }
      finally
      {
        //Если соединение еще не закрыто - закрываем его
        if (con.State != System.Data.ConnectionState.Closed)
          con.Close();
      }
      return result;
    }

    /// <summary>
    /// Получаем из базы данных MsSql список доступных отелей и их полей, которые потом оборачиваем в json и возвращаем
    /// </summary>
    /// <param name="CountryKey">Id страны, для которой должны передать список отелей</param>
    /// <returns></returns>
    public static string getHotels(string CountryKey = "0")
    {
      //Создаем переменную под результат
      string result = String.Empty;
      SqlConnection con = new SqlConnection();
      SqlDataReader reader = null;
      try
      {
        string dbCommand;
        //Если CountryKey = 0 выгружаем из базы тэг и id одной страны, иначе - всех стран
        if (CountryKey.CompareTo("0") != 0)
          dbCommand = "SELECT CountryCode, CountryKey FROM countries where CountryKey=" + CountryKey.ToString();
        else
          dbCommand = "SELECT CountryCode, CountryKey FROM countries";
        //Открываем соединение, выполняем команду, записываем данные из таблицы в reader
        openSqlConnectionWithCommandExecution(ref con, out reader, dbCommand);
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
          openSqlConnectionWithCommandExecution(ref con, out reader, dbCommand);
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
        response.NumberOfElements = counter;
        response.Hotels = hotels;
        //Сериализуем результирующий объект
        result = JsonConvert.SerializeObject(response);
      }
      catch (Exception e)
      {
        //Создаем объект результата с ошибкой, заполняем его
        ApiErrorResponse error = formErrorResponse(e);
        //Сериализуем объект результата
        result = JsonConvert.SerializeObject(error);
      }
      finally
      {
        //Если соединение не закрыто - закрываем его
        if (con.State != System.Data.ConnectionState.Closed)
          con.Close();
      }
      return result;
    }

    /// <summary>
    /// Получаем из базы данных MsSql список доступных операторов и их полей, которые потом оборачиваем в json и возвращаем
    /// </summary>
    /// <returns></returns>
    public static string getOperators()
    {
      //Создаем переменную под результат
      string result = String.Empty;
      SqlConnection con = new SqlConnection();
      SqlDataReader reader = null;
      try
      {

        //Берем только те операторы, которые фигурируют в таблицах Operators и all_operators_actual
        string dbCommand = "SELECT DISTINCT o.OperatorKey, Operator FROM Operators as o, all_operators_actual as a where o.OperatorKey = a.operatorKey";
        //Открываем соединение, выполняем команду, записываем данные из таблицы в reader
        openSqlConnectionWithCommandExecution(ref con, out reader, dbCommand);
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
          dbCommand = "SELECT * from all_operators_actual where operatorKey=" + oper[0];
          reader.Close();
          //Открываем соединение, выполняем команду, записываем данные из таблицы в reader
          openSqlConnectionWithCommandExecution(ref con, out reader, dbCommand);
          //Строка хранит idшники стран, в которых действует оператор 
          string countryKeysForOperators = String.Empty;
          while (reader.Read())
          {
            countryKeysForOperators += reader["countryKey"].ToString() + ",";
          }
          //Убираем лишнюю запятую в конце строки
          if (countryKeysForOperators != null & countryKeysForOperators.Length > 1)
          {
            countryKeysForOperators = countryKeysForOperators.Remove(countryKeysForOperators.Length - 1);
          }
          oper.Add(countryKeysForOperators);
        }
        //Отмечаем, что запрос обработан успешно, записываем количество записей и передаем сам список записей
        ApiOperatorsResponse response = new ApiOperatorsResponse();
        response.ResponseCode = 200;
        response.NumberOfElements = counter;
        response.Operators = operators;
        //Сериализуем результирующий объект
        result = JsonConvert.SerializeObject(response);
      }
      catch (Exception e)
      {
        //Создаем объект результата с ошибкой, заполняем его
        ApiErrorResponse error = formErrorResponse(e);
        //Сериализуем объект результата
        result = JsonConvert.SerializeObject(error);
      }
      finally
      {
        //Если соединение не закрыто - закрываем его
        if (con.State != System.Data.ConnectionState.Closed)
          con.Close();
      }
      return result;
    }

    public static string getResults(IList<string> Args)
    {
      string searchStr = formRequestString(Args);
      return searchEngine(searchStr);
    }
    #endregion
  }
}
