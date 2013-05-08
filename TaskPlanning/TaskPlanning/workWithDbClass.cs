using System;
//using System.Linq;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace TaskPlanning
{
    // Возможные состояния программы
    enum enumProgramStates
    {
        activeConnectToDbForm,                  // Активно окно подключения к БД
        activeMainForm,                         // Активно главное окно программы
        activeEditDataForm                      // Активно окно добавления или редактирования задания
    }

    // Состояние выполнения запроса к БД
    enum enumDbQueryStates
    {
        error = 0,                              // Запрос к БД не был выполнен (вернул ошибку)
        success = 1                             // Запрос к БД успешно выполнен
    }

    // Столбцы в DataGrid окна добавления или редактирования задания, которые представлены в виде выпадающего списка (ComboBox)
    enum enumNameColumnsWithComboBox
    {
        Project,
        CurrentStatus,
        length
    }

    // Столбцы, значение которых берется из другой таблицы, используя primary key этой таблицы
    enum enumNameColumnsGetNameOnID
    {
        Project,
        Reporter,
        CurrentStatus,
        length
    }

    // Столбцы главной таблицы БД
    enum enumNamesColumnsMainTable
    {
        Id,
        StartTime,
        EndTime,
        Project,
        Description,
        Result,
        Note,
        Ticket,
        Owner,
        Reporter,
        CurrentStatus,
        LastChange,
        AmountChanges,
        IsLocked,
        length
    }

    // Структура, хранящая данные из таблицы БД
    public struct structTable
    {
        public string[] columnNames;                // Название столбцов таблицы
        public List<string[]> listValues;           // Список записей, прочитанных из БД

        // Обнуление структуры таблицы
        public void clearStruct()
        {
            columnNames = null;
            listValues.RemoveRange(0, listValues.Count);
        }
    }

    // Структура для хранения данных о пользователе, подключенном к БД
    public struct structCurrentDbUser
    {
        public string loginName;                    // Логин (столбец "c_ID" таблицы "Users" из БД)
        public string fullName;                     // Полное имя (столбец "c_Name" таблицы "Users" из БД)
        public string group;                        // Группа, в которой состоит пользователь (столбец "c_Group" таблицы "Users" из БД)
    }

    public class workWithDbClass
    {
        private const string nameTableUsers = "users";              // Имя таблицы в БД, которая содержит список имен пользователей
        private const string nameTableTaskStates = "taskstatus";    // Имя таблицы в БД, которая содержит список состояний задания
        private const string nameTableProjects = "projects";        // Имя таблицы в БД, которая содержит список проектов
        public const string nameTableTasks = "tasks";               // Имя таблицы в БД, которая содержит задания (tasks)

        public int[] columnsWidth = { 30, 70, 70, 60, 270, 270, 100, 70, 70, 70, 85, 150, 90, 60 };     // Ширина столбцов в DataGrid
        public string columnNotBeNullString = " *";                                                     // Этим знаком помечены столбцы, которые обязательны для заполнения при добавлении нового задания

        public string delimiter = ";";                              // Используемый разделитель данных
        public string nameAdminGroup = "Administrator";             // Имя группы пользователей, имеющих суперправа (администраторы)
        public string nameGuestGroup = "Guest";                     // Имя группы пользователей, имеющих ограниченные права (гости)
        public string idStateOpened = "opened";                     // ID для статуса "Opened" из таблицы "TaskStatus" БД
        public string idStateClosed = "closed";                     // ID для статуса "Closed" из таблицы "TaskStatus" БД

        public string dbHost;                                       // Адрес хоста, на котором находится БД
        public string dbName;                                       // Имя БД
        public string dbUser;                                       // Имя пользователя, подключившегося к БД
        public string dbUserPassword;                               // Пароль пользователя

        public structCurrentDbUser currentDBUserName;               // Данные текущего пользователя БД

        public structTable tableUsers;                              // Данные, прочитанные из таблицы "Users" БД
        public structTable tableProjects;                           // Данные, прочитанные из таблицы "Projects" БД
        public structTable tableTaskStates;                         // Данные, прочитанные из таблицы "TaskStatus" БД

        public MySqlConnection myConnection;                        // Подключение к БД
        private MySqlCommand myCommand;                             // Запрос к БД
        private MySqlDataReader myDataReader;                       // Данные, прочитанные из БД
        public int stateLastQuery;                                  // Состояние последнего выполненого запроса к БД (принимает значения из enumDbQueryStates)

        public string[] nameColumnsWithComboBox;                    // Имена столбцов в DataGrid окна добавления или редактирования задания, которые представлены в виде выпадающего списка (ComboBox)
        public string[] nameColumnsGetNameOnID;                     // Имена столбцов, значение которых берется из другой таблицы, используя primary key этой таблицы
        public string[] nameColumnsMainTable;                       // Имена столбцов главной таблицы БД

        public int currentProgramState;                             // Текущее состояние программы (принимает значения из enumProgramStates)


        public workWithDbClass()
        {
            myConnection = new MySqlConnection();
            myCommand = new MySqlCommand();

            dbHost = "";
            dbName = "";
            dbUser = "";
            dbUserPassword = "";
            stateLastQuery = (int)enumDbQueryStates.error;

            currentDBUserName = new structCurrentDbUser();
            currentDBUserName.loginName = "";
            currentDBUserName.fullName = "";
            currentDBUserName.group = "";

            currentProgramState = (int)enumProgramStates.activeConnectToDbForm;

            tableUsers = new structTable();
            tableUsers.listValues = new List<string[]>();

            tableProjects = new structTable();
            tableProjects.listValues = new List<string[]>();

            tableTaskStates = new structTable();
            tableTaskStates.listValues = new List<string[]>();

            nameColumnsWithComboBox = new string[(int)enumNameColumnsWithComboBox.length];
            nameColumnsWithComboBox[(int)enumNameColumnsWithComboBox.Project] = "Project";
            nameColumnsWithComboBox[(int)enumNameColumnsWithComboBox.CurrentStatus] = "CurrentStatus";

            nameColumnsGetNameOnID = new string[(int)enumNameColumnsGetNameOnID.length];
            nameColumnsGetNameOnID[(int)enumNameColumnsGetNameOnID.Project] = "Project";
            nameColumnsGetNameOnID[(int)enumNameColumnsGetNameOnID.Reporter] = "Reporter";
            nameColumnsGetNameOnID[(int)enumNameColumnsGetNameOnID.CurrentStatus] = "CurrentStatus";

            nameColumnsMainTable = new string[(int)enumNamesColumnsMainTable.length];
            nameColumnsMainTable[(int)enumNamesColumnsMainTable.Id] = "ID";
            nameColumnsMainTable[(int)enumNamesColumnsMainTable.StartTime] = "StartTime";
            nameColumnsMainTable[(int)enumNamesColumnsMainTable.EndTime] = "EndTime";
            nameColumnsMainTable[(int)enumNamesColumnsMainTable.Project] = "Project";
            nameColumnsMainTable[(int)enumNamesColumnsMainTable.Description] = "Description";
            nameColumnsMainTable[(int)enumNamesColumnsMainTable.Result] = "Result";
            nameColumnsMainTable[(int)enumNamesColumnsMainTable.Note] = "Note";
            nameColumnsMainTable[(int)enumNamesColumnsMainTable.Ticket] = "Ticket";
            nameColumnsMainTable[(int)enumNamesColumnsMainTable.Owner] = "Owner";
            nameColumnsMainTable[(int)enumNamesColumnsMainTable.Reporter] = "REporter";
            nameColumnsMainTable[(int)enumNamesColumnsMainTable.CurrentStatus] = "CurrentStatus";
            nameColumnsMainTable[(int)enumNamesColumnsMainTable.LastChange] = "LastChange";
            nameColumnsMainTable[(int)enumNamesColumnsMainTable.AmountChanges] = "AmountChanges";
            nameColumnsMainTable[(int)enumNamesColumnsMainTable.IsLocked] = "IsLocked";
        }


        // Подключение к БД
        public void connectToDB()
        {
            // Данные для подключения к БД
            string connectString = new MySqlConnectionStringBuilder {
                Database = dbName,
                Server = dbHost,
                UserID =  dbUser,
                Password = dbUserPassword }.ToString();

            try
            {
                // Если не подключен к БД
                if (myConnection.State != System.Data.ConnectionState.Open)
                {
                    myConnection.ConnectionString = connectString;
                    myConnection.Open();                            // Подключаюсь к БД

                    // Если удалось подключиться к БД
                    if (myConnection.State == System.Data.ConnectionState.Open)
                    {
                        myCommand.Connection = myConnection;        // Сохраняю переменную, указывающую на текущее подключение к БД
                        currentDBUserName.loginName = dbUser;       // Сохраняю в структуре имя пользователя, который подключился к БД
                        readUsersProjectsTaskStatusFromDB();        // Читаю из БД данные из таблиц "Users", "Projects", "TaskStatus"
                        //MessageBox.Show("Successful connect to database.", "Connect to database!");
                    }
                    else    // Если не удалось подключиться к БД
                    {
                        MessageBox.Show("Could not connect to database! Connect state: " + myConnection.State.ToString(), "Connect to database!");
                    }
                }
            }
            catch           // Перехватываю исключения, которые вернул оператор "myConnection.Open();" (возникают при неправильных данных для подключения к БД)
            {
                if (currentProgramState == (int)enumProgramStates.activeConnectToDbForm)
                    MessageBox.Show("Can't connect to database! Check host name, name database, login, password or state MySQL server and try again!\n", "Error!");

                if (currentProgramState == (int)enumProgramStates.activeMainForm)
                    MessageBox.Show("Lost connection with database! Check state MySQL server and try again!\n", "Error!");

                if (currentProgramState == (int)enumProgramStates.activeEditDataForm)
                    MessageBox.Show("Lost connect to database! Check state MySQL server and try again!\n\n" +
                        "ATTENTION: current changes not be saved in database! Save this changes in other place! " +
                        "If you close window edit row data, current edited row was BLOCKED FOREVER! And you can't change it in future!\n\n" +
                        "RECOMENDATION: Don't close window edit row data! Wait when connection to database will restored and then close it!", "Error!");
            }
        }

        // Отключение от БД
        public void disconnectFromDB()
        {
            // Если подключение к БД отсутствует, то выхожу из функции
            if (myConnection.State != System.Data.ConnectionState.Open)
                return;

            try
            {
                myConnection.Close();       // Отключаюсь от БД
            }
            catch (Exception ex)            // Перехватываю исключения
            {
                MessageBox.Show("Unexpected exception in disconnect from database!\n\nError: " + ex.Message, "Disconnect from database!");
            }
        }
        
        // Чтение данных из БД (query - строка запроса к БД)
        public void readDataFromDB(string query)
        {
            // Если подключение к БД отсутствует, то выхожу из функции
            if (myConnection.State != System.Data.ConnectionState.Open)
                return;

            myCommand.CommandText = query;                                  // Задаю запрос к БД
            stateLastQuery = (int)enumDbQueryStates.error;                  // Устанавливаю ошибочное состояние последнего выполненного запроса

            try
            {
                if (myDataReader == null)
                {
                    myDataReader = myCommand.ExecuteReader();               // Выполняю запрос к БД
                    stateLastQuery = (int)enumDbQueryStates.success;        // Устанавливаю успешное состояние последнего выполненного запроса
                }
                else
                {
                    // Если прочитаны все данные, которые вернул предыдущий запрос к БД
                    if (myDataReader.IsClosed)
                    {
                        myDataReader = myCommand.ExecuteReader();           // Выполняю запрос к БД
                        stateLastQuery = (int)enumDbQueryStates.success;    // Устанавливаю успешное состояние последнего выполненного запроса
                    }
                    else
                    {
                        //MessageBox.Show("Could not read data from database!\nPrevious session work with database is not closed!", "ERROR in work with database!");
                    }
                }
            }
            catch //(Exception ex)                                          // Перехватываю исключения
            {
                //MessageBox.Show("File 'DBWork.cs' method 'readDataFromDB'.\nCould not read data from database!\n\nQuery string: " + myCommand.CommandText + "\n\nError: " + ex.Message, "ERROR in work with database!");
            }

            myCommand.CommandText = "";                                     // Сбрасываю запрос к БД
        }

        // Запись данных в БД (query - строка запроса к БД)
        public void insertDataToDB(string query)
        {
            // Если подключение к БД отсутствует, то выхожу из функции
            if (myConnection.State != System.Data.ConnectionState.Open)
                return;

            myCommand.CommandText = query;                                  // Задаю запрос к БД
            stateLastQuery = (int)enumDbQueryStates.error;                  // Устанавливаю ошибочное состояние последнего выполненного запроса

            try
            {
                if (myDataReader == null)
                {
                    myCommand.ExecuteNonQuery();                            // Выполняю запрос к БД
                    stateLastQuery = (int)enumDbQueryStates.success;        // Устанавливаю успешное состояние последнего выполненного запроса
                }
                else
                {
                    // Если прочитаны все данные, которые вернул предыдущий запрос к БД
                    if (myDataReader.IsClosed)
                    {
                        myCommand.ExecuteNonQuery();                        // Выполняю запрос к БД
                        stateLastQuery = (int)enumDbQueryStates.success;    // Устанавливаю успешное состояние последнего выполненного запроса
                    }
                    else
                    {
                        //MessageBox.Show("Could not insert data into database!\nPrevious session work with database is not closed!", "ERROR in work with database!");
                    }
                }
            }
            catch //(Exception ex)                                          // Перехватываю исключения
            {
                //MessageBox.Show("File 'DBWork.cs' method 'insertDataToDB'.\nCould not change data in database!\n\nQuery string: " + myCommand.CommandText + "\n\nError: " + ex.Message, "ERROR in work with database!");
            }

            myCommand.CommandText = "";                                     // Сбрасываю запрос к БД
        }

        // Чтение данных, которые вернул запрос к БД (возвращаемый резултат: true - данные успешно прочитаны; false - нет данных, доступных для чтения)
        public bool isDataRead()
        {
            // Если прочитаны не все данные
            if ((myDataReader != null) && (!myDataReader.IsClosed))
            {
                // Если удалось прочитать данные
                if (myDataReader.Read())
                    return true;
                else
                {
                    myDataReader.Close();                                    // Закрываю чтение данных - все данные текущего запроса прочитаны
                    return false;
                }
            }
            else
                return false;
        }

        // Проверка наличия подключения к БД (вызываю эту функцию перед каждым обращением (чтение/запись) к БД)
        public bool isConnectToDb()
        {
            readDataFromDB("SHOW TABLES");                                  // Выполняю запрос к БД

            // Если нет подключения к БД
            if (myConnection.State != System.Data.ConnectionState.Open)
            {
                connectToDB();                                              // Подключаюсь к БД

                // Если не удалось подключиться к БД
                if (myConnection.State != System.Data.ConnectionState.Open)
                    return false;
                //else
                    //MessageBox.Show("Restore connect to database!\n", "Success!");
            }
            else
            {
                // Если запрос в начале функции был успешно выполнен
                if (stateLastQuery == (int)enumDbQueryStates.success)
                    while (isDataRead()) ;                                  // Читаю все данные, которые вернул запрос к БД
            }

            return true;
        }

        // Получение названия столбцов для данных, которые вернул запрос к БД (столбец - поле таблицы в БД)
        public string[] getColumnsNames()
        {
            int amountColumns = myDataReader.FieldCount;                        // Получаю число столбцов из результатов запроса к БД
            string[] resultStr = new string[amountColumns];

            for (int column = 0; column < amountColumns; column++)
                resultStr[column] = myDataReader.GetName(column).Substring(2);  // Сохраняю названия столбцов в переменную (названия столбцов сохраняю начиная со 2-го символа, т.к. в БД все названия имеют вид "c_*")

            return resultStr;                                                   // Возвращаю прочитаные имена столбцов
        }

        // Получение строки с данными, которые вернул запрос к БД
        public string[] getRowValues()
        {
            int amountColumns = myDataReader.FieldCount;                        // Число столбцов в строке
            string[] resultStr = new string[amountColumns];

            // Получаю данные для каждого столбца
            for (int column = 0; column < amountColumns; column++)
            {
                // Если столбец содержит значение NULL, то присваиваю значению пустую строку
                if (!myDataReader.IsDBNull(column))
                    resultStr[column] = myDataReader.GetString(column);
                else
                    resultStr[column] = "";
            }

            return resultStr;
        }

        // Чтение данных о пользователях из таблицы БД
        private void readUsersNamesFromDB()
        {
            string query = "SELECT * FROM " + nameTableUsers;                                   // Формирую запрос
            tableUsers.clearStruct();                                                           // Очищаю структуру, содержащую данные пользователей
            readDataFromDB(query);                                                              // Читаю данные из БД

            // Если запрос успешно выполнен
            if (stateLastQuery == (int)enumDbQueryStates.success)
            {
                tableUsers.columnNames = getColumnsNames();                                     // Получаю имена столбцов в таблице
                // Читаю данные, которые вернул запрос к БД
                while (isDataRead())
                    tableUsers.listValues.Add(getRowValues());                                  // Добавляю прочитанную запись в список со свойствами пользователей
            }
            else
            {
                //MessageBox.Show("File 'DBWork.cs' method 'readUsersNamesFromDB'.\nInvalid query = \"" + query + "\"", "ERROR in work with database!");
            }
        }

        // Чтение данных о возможных состояниях задания из таблицы БД
        private void readTaskStatusFromDB()
        {
            string query = "SELECT * FROM " + nameTableTaskStates;                              // Формирую запрос
            tableTaskStates.clearStruct();                                                      // Очищаю структуру, содержащую данные состояний задания
            readDataFromDB(query);                                                              // Читаю данные из БД

            // Если запрос успешно выполнен
            if (stateLastQuery == (int)enumDbQueryStates.success)
            {
                tableTaskStates.columnNames = getColumnsNames();                                // Получаю имена столбцов в таблице
                // Читаю данные, которые вернул запрос к БД
                while (isDataRead())
                    tableTaskStates.listValues.Add(getRowValues());                             // Добавляю прочитанную запись в список состояний задания
            }
            else
            {
                //MessageBox.Show("File 'DBWork.cs' method 'readTaskStatesFromDB'.\nInvalid query = \"" + query + "\"", "ERROR in work with database!");
            }
        }

        // Чтение данных о проектах из таблицы БД
        private void readProjectsNamesFromDB()
        {
            string query = "SELECT * FROM " + nameTableProjects;                              // Формирую запрос
            tableProjects.clearStruct();                                                      // Очищаю структуру, содержащую список проетов
            readDataFromDB(query);                                                            // Читаю данные из БД

            // Если запрос успешно выполнен
            if (stateLastQuery == (int)enumDbQueryStates.success)
            {
                tableProjects.columnNames = getColumnsNames();                                // Получаю имена столбцов в таблице
                // Читаю данные, которые вернул запрос к БД
                while (isDataRead())
                    tableProjects.listValues.Add(getRowValues());                             // Добавляю прочитанную запись в список проектов
            }
            else
            {
                //MessageBox.Show("File 'DBWork.cs' method 'readProjectsNamesFromDB'.\nInvalid query = \"" + query + "\"", "ERROR in work with database!");
            }
        }

        // Читаю из БД и заполняю соответствующие структуры для пользователей, проектов и состояний задания
        public void readUsersProjectsTaskStatusFromDB()
        {
            readUsersNamesFromDB();
            readProjectsNamesFromDB();
            readTaskStatusFromDB();
        }

        // Получаю индекс стоблца по его имени в указанной таблице
        public int getColumnIndexOnColumnNameInTable(structTable currentTable, string name)
        {
            // Если в таблице есть столбцы и их число больше нуля
            if ((currentTable.columnNames != null) && (currentTable.columnNames.Length > 0))
            {
                // Последовательно перебираю все индексы и проверяю не совпадет ли содержимое данного индекса с указанным именем столбца
                for (int index = 0; index < currentTable.columnNames.Length; index++)
                {
                    if (currentTable.columnNames[index].Equals(name))
                        return index;                                   // Содержимое ячейки массива и указанное имя стоблца совпали, значит искомый элемент найден и возвращаю его индекс
                }
            }
            else
            {
                // Если в структуре отсутсвует указатель на массив с именами столбцов
                if (currentTable.columnNames == null)
                    MessageBox.Show("File 'DBWork.cs' method 'getColumnIndexOnColumnNameInTable'.\n" + "'currentTable.columnNames' = NULL!", "Runtime error!");
                else
                {
                    // Если число столбцов равно нулю
                    if (currentTable.columnNames.Length == 0)
                        MessageBox.Show("File 'DBWork.cs' method 'getColumnIndexOnColumnNameInTable'.\n" + "'currentTable.columnNames.Length' = 0!", "Runtime error!");
                }
            }

            MessageBox.Show("File 'DBWork.cs' method 'getColumnIndexOnColumnNameInTable'.\n" + "Can't found column with name '" + name + "' in current table.", "Runtime error!");
            return -1;          // Возвращаю -1
        }

        // idRow - ID строки; setLock - текущая блокировка (0 - снять блокировку, 1 - установить блокировку)
        public void setLockToRow (string idRow, int setLock)
        {
            insertDataToDB("UPDATE `taskplanning`.`Tasks` SET `c_IsLocked` = \"" + setLock.ToString() + "\" WHERE `c_ID` = " + idRow + ";");
        }

        // Из входной строки "inputString" выделяю имена owner'ов
        public List<string> parseOwnersFromString(string inputString)
        {
            List<string> owners = new List<string>();
            string delimiterString = delimiter + "\n";      // Последовательность, отделяющая owner'ов друг от друга
            int delimiterIndex = -1;

            // Пока не просмотрел всю строку
            while (inputString.Length > 0)
            {
                // Получаю позицию, с которой начинается разделитель
                delimiterIndex = inputString.IndexOf(delimiterString);
                // Добавляю owner'а (строка от начала до разделителя)
                owners.Add(inputString.Substring(0, delimiterIndex));
                // Удаляю из строки уже распознанного owner'а
                inputString = inputString.Substring(delimiterIndex + delimiterString.Length);
            }

            return owners;
        }

        // Получаю email адрес каждого owner'а ("usersNames" - строка, содержащая owner'ов)
        public List<string> getEmailAddresOnUsersNames(string usersNames)
        {
            int numberEmailInArray = -1;
            int numberNameInArray = -1;
            List<string> listUsersNames = parseOwnersFromString(usersNames);    // Из строки "usersNames" получаю список owner'ов
            List<string> listUsersEmailAddreses = new List<string>();

            numberNameInArray = getColumnIndexOnColumnNameInTable(tableUsers, "Name");
            numberEmailInArray = getColumnIndexOnColumnNameInTable(tableUsers, "Email");
            if ((numberNameInArray < 0) || (numberEmailInArray < 0))
            {
                MessageBox.Show("File 'DBWork.cs' method 'getEmailAddresOnUserName'.\n" +
                    "'numberNameInArray' = " + numberNameInArray.ToString() + " ;'numberGroupInArray' = " + numberEmailInArray.ToString(), "Runtime error!");

                return listUsersEmailAddreses;
            }

            // Для каждого owner'а из таблицы получаю адрес email и добавляю его в список "listUsersEmailAddreses"
            for (int listCount = 0; listCount < listUsersNames.Count; listCount++)
            {
                for (int tableCount = 0; tableCount < tableUsers.listValues.Count; tableCount++)
                {
                    if (tableUsers.listValues[tableCount][numberNameInArray].Equals(listUsersNames[listCount]))
                    {
                        if (tableUsers.listValues[tableCount][numberEmailInArray] != "")
                            listUsersEmailAddreses.Add(tableUsers.listValues[tableCount][numberEmailInArray]);

                        break;
                    }
                }
            }

            return listUsersEmailAddreses;
        }
    }
}
