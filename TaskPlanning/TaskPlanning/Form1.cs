using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TaskPlanning;

namespace TaskPlanning
{
    // Состояние блокировки чтения данных из БД
    enum enumStatesLockedReadData
    {
        unlocked,       // Разблокировано
        locked          // Заблокировано
    }

    public partial class Form1 : Form
    {
        private const long timerIntervalUpdateDataFromDB = 10000;           // Интервал чтения данных из БД (при включенном автообновлении данных)
        private const long timerIntervalSendMessageFromQueue = 60000;       // Интервал отправки email сообщений из очереди

        private workWithDbClass myDB;                                       // Объект для работы с БД
        private workWithEmailClass myEmail;                                 // Объект для работы с email сообщениями
        
        private FormEditData myFormEditData;                                // Объект для работы с формой редактирования task'ов
        private FormConnectToDatabase myFormConnectToDb;                    // Объект для работы с формой подключения к БД
        private SaveFileDialog mySaveFileDialog;                            // Диалог сохранения файла

        private System.Timers.Timer myTimerUpdateDataFromDB;                // Таймер чтения данных из БД
        private System.Timers.Timer myTimerSendEmailFromQueue;              // Таймер отправки email сообщений из очереди

        private int isLockedReadData;                                       // Состояние блокировки чтения данных из БД
        private int amountCustomQueryCheckedFields;                         // Количество полей, проверяемых при формировании выборочного запроса к БД

        private delegate void invokeDelegate();                             // Делегат метода (для вызова методов по таймеру, запускаемых через Invoke())


        public Form1()
        {
            InitializeComponent();

            myDB = new workWithDbClass();
            myEmail = new workWithEmailClass();

            myFormEditData = new FormEditData();

            myFormConnectToDb = new FormConnectToDatabase();
            myFormConnectToDb.initialiseDB(myDB);

            isLockedReadData = (int)enumStatesLockedReadData.unlocked;
            amountCustomQueryCheckedFields = 0;
            labelLoginInfo.Text = "";

            // Делаю неактивными все элементы, относящиеся к выборочному запросу из БД
            textBoxCustomQueryStartTime.Enabled = false;
            comboBoxCustomQueryProject.Enabled = false;
            comboBoxCustomQueryOwner.Enabled = false;
            comboBoxCustomQueryReporter.Enabled = false;
            comboBoxCustomQueryCurrentStatus.Enabled = false;
            
            // Настраиваю диалог сохранения файла
            mySaveFileDialog = new SaveFileDialog();
            mySaveFileDialog.InitialDirectory = "C:\\";
            mySaveFileDialog.FileName = "Taskplanning database";
            mySaveFileDialog.OverwritePrompt = true;
            mySaveFileDialog.CheckPathExists = true;
            mySaveFileDialog.DefaultExt = "*.csv";
            mySaveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            mySaveFileDialog.FilterIndex = 1;
            mySaveFileDialog.FileOk += new CancelEventHandler(mySaveFileDialog_FileOk);

            // Инициализирую таймер чтения данных из БД
            myTimerUpdateDataFromDB = new System.Timers.Timer(timerIntervalUpdateDataFromDB);
            myTimerUpdateDataFromDB.Elapsed += new System.Timers.ElapsedEventHandler(timerElapsedUpdateDataFromDB);
            myTimerUpdateDataFromDB.Start();

            // Инициализирую таймер отправки email сообщений из очереди
            myTimerSendEmailFromQueue = new System.Timers.Timer(timerIntervalSendMessageFromQueue);
            myTimerSendEmailFromQueue.Elapsed += new System.Timers.ElapsedEventHandler(timerElapsedSendEmailFromQueue);
            myTimerSendEmailFromQueue.Start();
        }

        // Изменение активности контролов на форме при отсутствующем подключении к БД
        private void disableFormControlsOnDisconnectedFromDb()
        {
            // Если соединение с БД закрыто
            if (myDB.myConnection.State == System.Data.ConnectionState.Closed)
            {
                buttonDBConnect.Enabled = true;
                buttonDBDisconnect.Enabled = false;
                groupBoxViewDataFromDB.Enabled = false;

                labelLoginInfo.Text = "";
                labelLoginInfo.ForeColor = Color.Black;

                myDB.currentProgramState = (int)enumProgramStates.activeConnectToDbForm;        // Изменяю текущее состояние программы

                // Обнуляю DataGrid
                dataGridViewDataFromDB.Columns.Clear();
                dataGridViewDataFromDB.Rows.Clear();

                myFormEditData.initialiseAttributes(null, null);                                // Инициализирую свойства формы добавления и редакторования task'ов null'ами
            }
        }

        // Установка свойств текущего пользователя, от имени которого было подключение к БД
        private void setCurrentDbUserProperties()
        {
            int numberIdInArray = -1;
            int numberGroupInArray = -1;
            int numberNameInArray = -1;

            numberIdInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableUsers, "ID");            // В текущей таблице получаю индекс столбца с именем "ID"
            numberNameInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableUsers, "Name");        // В текущей таблице получаю индекс столбца с именем "Name"
            numberGroupInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableUsers, "Group");      // В текущей таблице получаю индекс столбца с именем "Group"

            // Если все столбцы имеются
            if ((numberIdInArray >= 0) && (numberNameInArray >= 0) && (numberGroupInArray >= 0))
            {
                // Перебираю всех пользователей из соответствующей таблицы БД
                for (int i = 0; i < myDB.tableUsers.listValues.Count; i++)
                {
                    // Если имя из таблицы БД совпало с именем, от которого было подключение к БД
                    if (myDB.tableUsers.listValues[i][numberIdInArray].Equals(myDB.currentDBUserName.loginName))
                    {
                        myDB.currentDBUserName.fullName = myDB.tableUsers.listValues[i][numberNameInArray];        // Из таблицы БД получаю полное имя пользователя
                        myDB.currentDBUserName.group = myDB.tableUsers.listValues[i][numberGroupInArray];          // Из таблицы БД получаю группу, к которой принадлежит пользователь

                        // Вывожу эту информацию на форме
                        labelLoginInfo.Text = "You login as: \"";
                        labelLoginInfo.Text += myDB.currentDBUserName.fullName;
                        labelLoginInfo.Text += "\" (" + myDB.currentDBUserName.loginName + "), ";
                        labelLoginInfo.Text += "group \"" + myDB.currentDBUserName.group + "\".";
                        labelLoginInfo.ForeColor = Color.Green;

                        break;
                    }
                }
            }
            else
            {
                MessageBox.Show("File 'Form1.cs' method 'setCurrentDbUserProperties'.\n'numberIdInArray' = " + numberIdInArray.ToString() +
                    " ;'numberNameInArray' = " + numberNameInArray.ToString() + " ;'numberGroupInArray' = " + numberGroupInArray.ToString(), "Runtime error!");
            }
        }

        // Добавление нового / редактирование меющегося task'а
        private void addNewOrEditRow()
        {
            int amountColumns = dataGridViewDataFromDB.Columns.Count;
            int selectedRow = -1;
            string[] namesColumns = new string[amountColumns];
            string[] valuesCells = new string[amountColumns];
            int numberIdInArray = -1;
            int numberNameInArray = -1;
            int i = 0;
            string isCanEditRow = "";

            // Если тип выполняемой операции - редактирование task'а
            if (myFormEditData.currentOperation == (int)enumTypeOperation.editSelectedRow)
            {
                // Если в базе еще нет ни одной записи
                if (dataGridViewDataFromDB.Rows.Count == 0)
                    return;
                else
                    selectedRow = dataGridViewDataFromDB.CurrentRow.Index;  // Получаю индекс выбранной строки

                // Проверяю можно ли удалить данный task, то есть что он не заблокирован (возможно, что в данный момент его редактирует другой пользователь)
                isCanEditRow = isCanEditSelectedRow(selectedRow);
                // Если метод вернул пустую строку, значит, что task заблокирован для удаления / редактирования
                if (isCanEditRow.Equals(""))
                    return;
            }

            // Просматриваю все столбцы DataGrid
            for (int count = 0; count < amountColumns; count++)
            {
                // Сохраняю имена столбцов в массив
                namesColumns[count] = dataGridViewDataFromDB.Columns[count].Name;

                // Если тип выполняемой операции - добавление нового task'а
                if (myFormEditData.currentOperation == (int)enumTypeOperation.addNewRow)
                {
                    // Если текущий столбец ...
                    switch (dataGridViewDataFromDB.Columns[count].Name)
                    {
                        // ... с именем "Reporter"
                        case "Reporter":
                            {
                                numberIdInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableUsers, "ID");            // В текущей таблице получаю индекс столбца с именем "ID"
                                numberNameInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableUsers, "Name");        // В текущей таблице получаю индекс столбца с именем "Name"

                                // Если все столбцы имеются
                                if ((numberIdInArray >= 0) && (numberNameInArray >= 0))
                                {
                                    // Просматриваю соответствующую таблицу из БД
                                    for (i = 0; i < myDB.tableUsers.listValues.Count; i++)
                                    {
                                        // Если ID в таблице БД совпал с именем пользователя, от имени которого было подключение к БД
                                        if (myDB.tableUsers.listValues[i][numberIdInArray].Equals(myDB.currentDBUserName.loginName))
                                        {
                                            valuesCells[count] = myDB.tableUsers.listValues[i][numberNameInArray];      // Получаю полное имя пользователя из таблицы БД
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("File 'Form1.cs' method 'addNewOrEditRow'.\n'numberIdInArray' = " + numberIdInArray.ToString() +
                                        " ;'numberNameInArray' = " + numberNameInArray.ToString(), "Runtime error!");

                                    return;
                                }

                                break;
                            }

                        // ... с именем "StartTime"
                        case "StartTime":
                            {
                                // Получаю текущее дату и время в соотвествии с форматом myFormEditData.stringFormatDateTime
                                valuesCells[count] = DateTime.Now.ToString(myFormEditData.stringFormatDateTime);

                                break;
                            }

                        // ... с именем "CurrentStatus"
                        case "CurrentStatus":
                            {
                                numberIdInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableTaskStates, "ID");           // В текущей таблице получаю индекс столбца с именем "ID"
                                numberNameInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableTaskStates, "Name");       // В текущей таблице получаю индекс столбца с именем "Name"

                                // Если все столбцы имеются
                                if ((numberIdInArray >= 0) && (numberNameInArray >= 0))
                                {
                                    // Просматриваю соответствующую таблицу из БД
                                    for (i = 0; i < myDB.tableTaskStates.listValues.Count; i++)
                                    {
                                        // Если ID в таблице БД совпал с ID статуса "opened"
                                        if (myDB.tableTaskStates.listValues[i][numberIdInArray].Equals(myDB.idStateOpened))
                                        {
                                            valuesCells[count] = myDB.tableTaskStates.listValues[i][numberNameInArray];         // Получаю полное название сатуса
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("File 'Form1.cs' method 'addNewOrEditRow'.\n'numberIdInArray' = " + numberIdInArray.ToString() +
                                        " ;'numberNameInArray' = " + numberNameInArray.ToString(), "Runtime error!");

                                    return;
                                }

                                break;
                            }

                        default:
                            {
                                valuesCells[count] = "";

                                break;
                            }
                    }
                }

                // Если тип выполняемой операции - редактирование task'а
                if (myFormEditData.currentOperation == (int)enumTypeOperation.editSelectedRow)
                {
                    // Копирую в массив содержимое ячеек из DataGrid
                    valuesCells[count] = dataGridViewDataFromDB.Rows[selectedRow].Cells[count].Value.ToString();
                }
            }

            // Устанавливаю свойства DataGrid формы добавления / редактирования task'а
            myFormEditData.setNamesForColumnsDataGrid(namesColumns);
            // Заполняю данными ячейки DataGrid формы добавления / редактирования task'а
            myFormEditData.addRowValuesForDataGrid(valuesCells);
            // Показываю форму добавления / редактирования task'а
            myFormEditData.ShowDialog();

            // Если "isCanEditRow" содержит ID удаляемого task'а
            if (!isCanEditRow.Equals(""))
            {
                // Если подключение к БД отсутствует
                if (!myDB.isConnectToDb())
                {
                    disableFormControlsOnDisconnectedFromDb();                  // Изменяю активность контролов на форме
                    return;
                }

                // Разблокирую текущую строку
                myDB.setLockToRow(isCanEditRow, 0);
            }
        }

        // Проверка, может ли пользователь, от имени которого было подключение к БД, выполнить операцию "currentOperation"
        private bool checkCanUserAddOrEditData(int currentOperation)
        {
            int i = 0;
            int numberColumnOwnerInDataGrid = -1;
            int numberColumnReporterInDataGrid = -1;
            int selectedRow = -1;
            List<string> taskOwnersList = new List<string>();

            // Проверяю, состоит ли пользователь, от имени которого было подключение к БД, членом группы "Guest"
            // Если да, то этот пользователь не может ни добавлять, ни редактировать, ни удалять task'и
            if (myDB.currentDBUserName.group.Equals(myDB.nameGuestGroup))
                return false;

            // Добавить новый таск может любой пользователь (кроме членов группы "Guest")
            if (currentOperation == (int)enumTypeOperation.addNewRow)
                return true;

            // Проверяю, состоит ли пользователь, от имени которого было подключение к БД, членом группы "Administrator"
            // член группы "Administrator" может редактировать все task'и (даже те, у которых он не является ни reporter'ом, ни owner'ов)
            if (myDB.currentDBUserName.group.Equals(myDB.nameAdminGroup))
                return true;

            // Если в базе еще нет ни одной записи
            if (dataGridViewDataFromDB.Rows.Count == 0)
                return true;
            else
                selectedRow = dataGridViewDataFromDB.CurrentRow.Index;  // Получаю индекс выбранной строки

            // В DataGrid получаю индекс столбца с именем "myDB.nameColumnsMainTable[(int)enumNamesColumnsMainTable.Owner]"
            numberColumnOwnerInDataGrid = getColumnNumberOnColumnNameInDataGrid(myDB.nameColumnsMainTable[(int)enumNamesColumnsMainTable.Owner]);
            // В DataGrid получаю индекс столбца с именем "myDB.nameColumnsMainTable[(int)enumNamesColumnsMainTable.Reporter]"
            numberColumnReporterInDataGrid = getColumnNumberOnColumnNameInDataGrid(myDB.nameColumnsMainTable[(int)enumNamesColumnsMainTable.Reporter]);
            // Если в DataGrid нет таких столбцов
            if ((numberColumnOwnerInDataGrid < 0) || (numberColumnReporterInDataGrid < 0) || (selectedRow < 0))
            {
                MessageBox.Show("File 'Form1.cs' method 'checkCanUserEditSelectedRow'.\n'numberColumnOwnerInDataGrid' = " + numberColumnOwnerInDataGrid.ToString() +
                    " ;'numberColumnReporterInDataGrid' = " + numberColumnReporterInDataGrid.ToString() + " ;'selectedRow' = " + selectedRow.ToString(), "Runtime error!");
                return false;
            }

            // Проверяю, является ли пользователь, от имени которого было подключение к БД, reporter'ом данного таска
            if (myDB.currentDBUserName.fullName.Equals(dataGridViewDataFromDB.Rows[selectedRow].Cells[numberColumnReporterInDataGrid].Value.ToString()))
                return true;

            // Проверяю, является ли пользователь, от имени которого было подключение к БД, одним из owner'ов данного таска
            // Вначале получаю список владельцев выбранного task'а
            taskOwnersList = myDB.parseOwnersFromString(dataGridViewDataFromDB.Rows[selectedRow].Cells[numberColumnOwnerInDataGrid].Value.ToString());
            for (i = 0; i < taskOwnersList.Count; i++)
            {
                if (myDB.currentDBUserName.fullName.Equals(taskOwnersList[i]))
                    return true;
            }

            // Если пользователь не является ни членом группы "Administrator", ни владельцем, ни репортером выбранного task'а, то он не может его редактировать
            MessageBox.Show("You can't edit this row! Only owner(s), or reporter '" + dataGridViewDataFromDB.Rows[selectedRow].Cells["Reporter"].Value.ToString() +
                "', or any user consist in group 'Administrator' can edit this row. You login as '" + myDB.currentDBUserName.fullName +
                "' (" + myDB.currentDBUserName.loginName + ") and consist in group '" + myDB.currentDBUserName.group + "'.", "Permissions denied!");

            return false;
        }

        // Проверка, не заблокирован ли данный task в БД. Необходимо для избежания ситуации, что несколько пользователей
        // одновременно редактируют один и тот же task. Если task заблокирован, то метод возвращает пустую строку.
        // Если task не заблокирован, то метод блокирует его в БД и возвращает ID данного task'а
        private string isCanEditSelectedRow(int selectedRow)
        {
            int numberColumnIdInDataGrid = -1;
            string rowId = "";
            string query = "";
            string[] rowValues = { "" };

            // В DataGrid получаю индекс столбца с именем "myDB.nameColumnsMainTable[(int)enumNamesColumnsMainTable.Id]"
            numberColumnIdInDataGrid = getColumnNumberOnColumnNameInDataGrid(myDB.nameColumnsMainTable[(int)enumNamesColumnsMainTable.Id]);
            // Если в DataGrid нет такого столбца
            if (numberColumnIdInDataGrid < 0)
            {
                MessageBox.Show("File 'Form1.cs' method 'isCanEditSelectedRow'.\n" + "'numberColumnIdInDataGrid' = " + numberColumnIdInDataGrid.ToString(), "Runtime error!");
                return "";
            }

            // Получаю ID выбранного task'а
            rowId = dataGridViewDataFromDB.Rows[selectedRow].Cells[numberColumnIdInDataGrid].Value.ToString();

            // Формирую запрос к БД (для указанного task'а по ID получаю значение поля "c_IsLocked".
            // Если он равен нулю, то task разблокирован, если равен 1 - то заблокирован)
            query = "SELECT `c_IsLocked` FROM `Tasks` WHERE `c_ID` = \"" + rowId + "\"";
            // Выполняю запрос к БД
            myDB.readDataFromDB(query);
            // Если запрос выполнен успешно
            if (myDB.stateLastQuery == (int)enumDbQueryStates.success)
            {
                // Читаю данные, которые вернул запрос к БД
                while (myDB.isDataRead())
                    rowValues = myDB.getRowValues();

                // Если запрос вернул строку, отличную от нуля, это значит task заблокирован в БД
                if (rowValues[0] != "0")
                {
                    MessageBox.Show("You can't edit selected string because another user edit it at this time.", "Edit selected string!");
                    return "";
                }
                
                // Блокирую task в БД по его ID
                myDB.setLockToRow(rowId, 1);
                // Возвращаю ID выбранного task'а
                return rowId;
            }
            else
            {
                MessageBox.Show("Problem in insert data in database!\nCheck connection to database and try again!", "ERROR!");
                return "";
            }
        }
        
        // Чтение task'ов из БД в DataGrid
        private void setDataFromDbToDataGrid()
        {
            string query = "";
            string[] columnNames;
            string[] rowValues;
            int numberRow = -1;
            int numberColumnIsLocked = -1;
            string taskState = "";

            // Если чтение данных из БД заблокировано, то выхожу
            if (isLockedReadData == (int)enumStatesLockedReadData.locked)
                return;

            // Блокирую чтение данных из БД
            isLockedReadData = (int)enumStatesLockedReadData.locked;

            // Если подключение к БД отсутствует
            if (!myDB.isConnectToDb())
            {
                disableFormControlsOnDisconnectedFromDb();                                      // Изменяю активность контролов на форме
                return;
            }

            // Получаю данные из таблиц БД
            myDB.readUsersProjectsTaskStatusFromDB();
            // Формирую запрос к БД на выборку данных
            query = generateQuerySelectDataFromDB();
            // Выполняю запрос к БД
            myDB.readDataFromDB(query);
            // Если запрос был выполнен успешно
            if (myDB.stateLastQuery == (int)enumDbQueryStates.success)
            {
                // Очищаю DataGrid
                dataGridViewDataFromDB.Columns.Clear();
                dataGridViewDataFromDB.Rows.Clear();

                // Получаю названия столбоц из результата, который вернул запрос к БД
                columnNames = myDB.getColumnsNames();

                // Именам столбцов DataGrid присваиваю имена столбцов из запроса к БД и устанавливаю ширину для каждого столбца
                for (int count = 0; count < columnNames.Length; count++)
                {
                    dataGridViewDataFromDB.Columns.Add(columnNames[count], columnNames[count]);
                    dataGridViewDataFromDB.Columns[count].Width = myDB.columnsWidth[count];
                }

                // Читаю строки из результата, который вернул запрос к БД
                while (myDB.isDataRead())
                {
                    // Получаю значение строки из результата запроса к БД
                    rowValues = myDB.getRowValues();
                    // Добавляю результат в DataGrid
                    dataGridViewDataFromDB.Rows.Add(rowValues);

                    // Получаю номер текущей добавленной строки
                    numberRow = dataGridViewDataFromDB.Rows.Count - 1;

                    // В данной строке для столбцов, в которых находится не значение, а ID значения
                    // получаю по этому ID полное значение из соответствующей таблицы БД
                    for (int count = 0; count < myDB.nameColumnsGetNameOnID.Length; count++)
                        setNameOnId(numberRow, count);

                    // Получаю текущее состояние task'а
                    taskState = rowValues[(int)enumNamesColumnsMainTable.CurrentStatus];
                    // Если состояние равно "closed", то делаю серый фон ячеек строки
                    if (taskState.Equals(myDB.idStateClosed))
                    {
                        for (int count = 0; count < columnNames.Length; count++)
                            dataGridViewDataFromDB.Rows[dataGridViewDataFromDB.Rows.Count - 1].Cells[count].Style.BackColor = Color.LightGray;
                    }
                }

                // Добавляю владельцев для всех task'ов
                setOwnersToTask();

                // В DataGrid получаю индекс столбца с именем "myDB.nameColumnsMainTable[(int)enumNamesColumnsMainTable.IsLocked]"
                numberColumnIsLocked = getColumnNumberOnColumnNameInDataGrid(myDB.nameColumnsMainTable[(int)enumNamesColumnsMainTable.IsLocked]);
                // Если в DataGrid есть такой столбец, то скрываю его (он не будет отображен в DataGrid)
                if (numberColumnIsLocked >= 0)
                {
                    dataGridViewDataFromDB.Columns[numberColumnIsLocked].ReadOnly = true;
                    dataGridViewDataFromDB.Columns[numberColumnIsLocked].Visible = false;
                }
                else
                {
                    MessageBox.Show("File 'Form1.cs' method 'setDataFromDbToDataGrid'.\n'numberColumnIsLocked' = " + numberColumnIsLocked.ToString(), "Runtime error!");
                    return;
                }

                // Если пользователь, от имени которого было подключение к БД, не является членом группы "Guest"
                if (!myDB.currentDBUserName.group.Equals(myDB.nameGuestGroup))
                {
                    // Если в DataGrid есть данные
                    if (dataGridViewDataFromDB.Rows.Count > 0)
                    {
                        // Выбираю столбец, по которому будут сортироваться данные и тип сортировки
                        dataGridViewDataFromDB.Sort(dataGridViewDataFromDB.Columns["StartTime"], ListSortDirection.Descending);

                        // Делаю активными соответствующие кнопки
                        buttonEditSelectedRow.Enabled = true;
                        buttonDeleteSelectedRow.Enabled = true;
                        buttonExportData.Enabled = true;
                    }
                    else
                    {
                        // Делаю неактивными соответствующие кнопки
                        buttonEditSelectedRow.Enabled = false;
                        buttonDeleteSelectedRow.Enabled = false;
                        buttonExportData.Enabled = false;
                    }

                    buttonAddNewRow.Enabled = true;
                }
                else
                {
                    // Если в DataGrid есть данные
                    if (dataGridViewDataFromDB.Rows.Count > 0)
                    {
                        // Выбираю столбец, по которому будут сортироваться данные и тип сортировки
                        dataGridViewDataFromDB.Sort(dataGridViewDataFromDB.Columns["StartTime"], ListSortDirection.Descending);

                        // Делаю активными соответствующие кнопки
                        buttonExportData.Enabled = true;
                    }
                    else
                    {
                        // Делаю неактивными соответствующие кнопки
                        buttonExportData.Enabled = false;
                    }

                    // Делаю неактивными соответствующие кнопки
                    buttonAddNewRow.Enabled = false;
                    buttonEditSelectedRow.Enabled = false;
                    buttonDeleteSelectedRow.Enabled = false;
                }
            }

            // Разблокирую чтение данных из БД
            isLockedReadData = (int)enumStatesLockedReadData.unlocked;
        }

        // Добавление владельцев для всех task'ов в DataGrid
        private void setOwnersToTask()
        {
            string query = "";
            string rowID = "";
            int numberColumnIdInDataGrid = -1;
            int numberColumnOwnerInDataGrid = -1;
            List<string> taskOwners = new List<string>();

            // В DataGrid получаю индекс столбца с именем "myDB.nameColumnsMainTable[(int)enumNamesColumnsMainTable.Id]"
            numberColumnIdInDataGrid = getColumnNumberOnColumnNameInDataGrid(myDB.nameColumnsMainTable[(int)enumNamesColumnsMainTable.Id]);
            // В DataGrid получаю индекс столбца с именем "myDB.nameColumnsMainTable[(int)enumNamesColumnsMainTable.Owner]"
            numberColumnOwnerInDataGrid = getColumnNumberOnColumnNameInDataGrid(myDB.nameColumnsMainTable[(int)enumNamesColumnsMainTable.Owner]);
            // Если в DataGrid нет таких столбцов
            if ((numberColumnIdInDataGrid < 0) || (numberColumnOwnerInDataGrid < 0))
            {
                MessageBox.Show("File 'Form1.cs' method 'setOwnersToTask'.\n'numberColumnIdInDataGrid' = " + numberColumnIdInDataGrid.ToString() +
                                        " ;'numberColumnOwnerInDataGrid' = " + numberColumnOwnerInDataGrid.ToString(), "Runtime error!");
                return;
            }

            // Просматриваю все строки в DataGrid
            for (int numberRow = 0; numberRow < dataGridViewDataFromDB.Rows.Count; numberRow++)
            {
                // Получаю ID текущей строки
                rowID = dataGridViewDataFromDB.Rows[numberRow].Cells[numberColumnIdInDataGrid].Value.ToString();
                // Формирую запрос к БД
                query = "CALL getOwnersOnTaskId (" + rowID + ")";
                // Выполняю запрос к БД
                myDB.readDataFromDB(query);
                // Если запрос был выполнен успешно
                if (myDB.stateLastQuery == (int)enumDbQueryStates.success)
                {
                    // Очищаю список владельцев текущего task'а
                    taskOwners.Clear();

                    // Добавляю в список строки результата, который вернул запрос к БД
                    while (myDB.isDataRead())
                        taskOwners.Add(myDB.getRowValues()[0]);

                    dataGridViewDataFromDB.Rows[numberRow].Cells[numberColumnOwnerInDataGrid].Value = "";
                    // В соответствующую ячейку DataGrid добавляю данные из списка, разделяя их соответствующей последовательностью
                    for (int count = 0; count < taskOwners.Count; count++)
                     dataGridViewDataFromDB.Rows[numberRow].Cells[numberColumnOwnerInDataGrid].Value += taskOwners[count] + myDB.delimiter + "\n";
                }
            }
        }

        // Получение полного названия по его ID из соответствующей таблицы
        private void setNameOnId(int numberRow, int columnIndex)
        {
            int numberColumnInDataGrid = -1;
            int numberIdInArray = -1;
            int numberNameInArray = -1;
            int i = 0;
            structTable currentTable = new structTable();

            // По "columnIndex" определяю какую использовать таблицу из БД
            switch (columnIndex)
            {
                case ((int)enumNameColumnsGetNameOnID.Project):
                    {
                        currentTable = myDB.tableProjects;
                        break;
                    }

                case ((int)enumNameColumnsGetNameOnID.Reporter):
                    {
                        currentTable = myDB.tableUsers;
                        break;
                    }

                case ((int)enumNameColumnsGetNameOnID.CurrentStatus):
                    {
                        currentTable = myDB.tableTaskStates;
                        break;
                    }
                default:
                    {
                        MessageBox.Show("File 'Form1.cs' method 'setNameOnId'. Invalid index 'columnIndex' = " + columnIndex, "Runtime error!");
                        return;
                    }
            }

            // В DataGrid получаю индекс столбца с именем "myDB.nameColumnsGetNameOnID[columnIndex]"
            numberColumnInDataGrid = getColumnNumberOnColumnNameInDataGrid(myDB.nameColumnsGetNameOnID[columnIndex]);
            // Если в DataGrid нет такого столбца
            if (numberColumnInDataGrid < 0)
            {
                MessageBox.Show("File 'Form1.cs' method 'setNameOnId'.\n'numberColumnInDataGrid' = " + numberColumnInDataGrid.ToString(), "Runtime error!");
                return;
            }

            // Если текущая таблица содержит столбцы и их число больше нуля
            if ((currentTable.columnNames != null) && (currentTable.columnNames.Length > 0))
            {
                numberIdInArray = myDB.getColumnIndexOnColumnNameInTable(currentTable, "ID");           // В текущей таблице получаю индекс столбца с именем "ID"
                numberNameInArray = myDB.getColumnIndexOnColumnNameInTable(currentTable, "Name");       // В текущей таблице получаю индекс столбца с именем "Name"

                // Если все столбцы имеются
                if ((numberRow >= 0) && (numberIdInArray >= 0) && (numberNameInArray >= 0))
                {
                    // Просматриваю все значения в соответствующей таблице
                    for (i = 0; i < currentTable.listValues.Count; i++)
                    {
                        // Если значение столбца ID таблицы из БД совпадает с ID находящимся в соответсвующей ячейке DataGrid
                        if (currentTable.listValues[i][numberIdInArray].Equals(dataGridViewDataFromDB.Rows[numberRow].Cells[numberColumnInDataGrid].Value.ToString()))
                        {
                            // Соответсвующей ячейке DataGrid присваиваю полное название из таблицы БД
                            dataGridViewDataFromDB.Rows[numberRow].Cells[numberColumnInDataGrid].Value = currentTable.listValues[i][numberNameInArray];
                            break;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("File 'Form1.cs' method 'setNameOnId'.\n'numberRow' = " + numberRow.ToString() + " ;'numberIdInArray' = " +
                        numberIdInArray.ToString() + " ;'numberNameInArray' = " + numberNameInArray.ToString(), "Runtime error!");
                }
            }
            else
            {
                if (currentTable.columnNames == null)
                    MessageBox.Show("File 'Form1.cs' method 'setNameOnId'.\n" + "'currentTable.columnNames' = NULL!", "Runtime error!");
                else
                {
                    if (currentTable.columnNames.Length == 0)
                        MessageBox.Show("File 'Form1.cs' method 'setNameOnId'.\n" + "'currentTable.columnNames.Length' = 0!", "Runtime error!");
                }
            }
        }

        // Создаю запрос к БД для получения данных из БД в DataGrid.
        // Метод необходим для формирования выборочно запроса к БД
        private string generateQuerySelectDataFromDB()
        {
            string query = "";
            string id = "";
            int countCheckBoxes = amountCustomQueryCheckedFields;       // Число активных полей для выборочного запроса к БД
            int numberIdInArray = -1;
            int numberNameInArray = -1;
            int i = 0;
            bool isNeedAddAND = false;                                  // Нужно ли добавить в запрос слово "AND"
            bool isNeedAddWHERE = true;                                 // Нужно ли добавить в запрос слово "WHERE"

            // Начинаю формировать запрос к БД
            query = "SELECT * FROM " + workWithDbClass.nameTableTasks;

            // Если нужно выполнить выборочный запрос к БД
            if (amountCustomQueryCheckedFields > 0)
            {
                // Проверяю CheckBox для фильтрации запроса по полю "StartTime"
                if (checkBoxCustomQueryStartTime.Checked)
                {
                    // Если в соответствующем поле введен текст
                    if (textBoxCustomQueryStartTime.Text.Length > 0)
                    {
                        // Если нужно добавить в запрос слово "WHERE"
                        if (isNeedAddWHERE)
                        {
                            query += " WHERE (";                        // Дополняю запрос к БД
                            isNeedAddWHERE = false;
                        }

                        // Дополняю запрос к БД
                        query += workWithDbClass.nameTableTasks + ".c_StartTime LIKE '" + textBoxCustomQueryStartTime.Text + "%'";
                    }

                    // Необходимо добавить в запрос слово "AND"
                    isNeedAddAND = true;
                }

                // Проверяю CheckBox для фильтрации запроса по полю "Project"
                if (checkBoxCustomQueryProject.Checked)
                {
                    // Если в соответствующем поле введен текст
                    if (comboBoxCustomQueryProject.Text.Length > 0)
                    {
                        numberIdInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableProjects, "ID");         // В текущей таблице получаю индекс столбца с именем "ID"
                        numberNameInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableProjects, "Name");     // В текущей таблице получаю индекс столбца с именем "Name"

                        // Если не все столбцы имеются
                        if ((numberIdInArray < 0) || (numberNameInArray < 0))
                        {
                            MessageBox.Show("File 'Form1.cs' method 'generateQuerySelectDataFromDB'.\n'numberIdInArray' = " +
                                numberIdInArray.ToString() + " ;'numberNameInArray' = " + numberNameInArray.ToString(), "Runtime error!");

                            // Возвращаю стандартный запрос к БД
                            return "SELECT * FROM " + workWithDbClass.nameTableTasks;
                        }

                        // Получаю ID проекта по его имени из соответсвующей таблицы БД
                        for (i = 0; i < myDB.tableProjects.listValues.Count; i++)
                        {
                            if (myDB.tableProjects.listValues[i][numberNameInArray].Equals(comboBoxCustomQueryProject.Text))
                            {
                                id = myDB.tableProjects.listValues[i][numberIdInArray];
                                break;
                            }
                        }

                        // Если нужно добавить в запрос слово "WHERE"
                        if (isNeedAddWHERE)
                        {
                            query += " WHERE (";                        // Дополняю запрос к БД
                            isNeedAddWHERE = false;
                        }

                        // Если нужно добавить в запрос слово "AND"
                        if (isNeedAddAND)
                            query += " AND ";                           // Дополняю запрос к БД

                        // Дополняю запрос к БД
                        query += workWithDbClass.nameTableTasks + ".c_Project = " + "\"" + id + "\"";

                        // Необходимо добавить в запрос слово "AND"
                        isNeedAddAND = true;
                    }
                }

                // Проверяю CheckBox для фильтрации запроса по полю "Owner"
                if (checkBoxCustomQueryOwner.Checked)
                {
                    // Если в соответствующем поле введен текст
                    if (comboBoxCustomQueryOwner.Text.Length > 0)
                    {
                        numberIdInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableUsers, "ID");         // В текущей таблице получаю индекс столбца с именем "ID"
                        numberNameInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableUsers, "Name");     // В текущей таблице получаю индекс столбца с именем "Name"

                        // Если не все столбцы имеются
                        if ((numberIdInArray < 0) || (numberNameInArray < 0))
                        {
                            MessageBox.Show("File 'Form1.cs' method 'generateQuerySelectDataFromDB'.\n'numberIdInArray' = " +
                                numberIdInArray.ToString() + " ;'numberNameInArray' = " + numberNameInArray.ToString(), "Runtime error!");

                            // Возвращаю стандартный запрос к БД
                            return "SELECT * FROM " + workWithDbClass.nameTableTasks;
                        }

                        // Получаю ID пользователя по его имени из соответсвующей таблицы БД
                        for (i = 0; i < myDB.tableUsers.listValues.Count; i++)
                        {
                            if (myDB.tableUsers.listValues[i][numberNameInArray].Equals(comboBoxCustomQueryOwner.Text))
                            {
                                id = myDB.tableUsers.listValues[i][numberIdInArray];
                                break;
                            }
                        }

                        // Если нужно добавить в запрос слово "WHERE"
                        if (isNeedAddWHERE)
                        {
                            query += " WHERE (";                        // Дополняю запрос к БД
                            isNeedAddWHERE = false;
                        }

                        // Если нужно добавить в запрос слово "AND"
                        if (isNeedAddAND)
                            query += " AND ";                           // Дополняю запрос к БД

                        // Дополняю запрос к БД
                        query += workWithDbClass.nameTableTasks + ".c_ID IN (SELECT c_TaskID FROM ownersoftask WHERE c_OwnerID = " + "\"" + id + "\")";

                        // Необходимо добавить в запрос слово "AND"
                        isNeedAddAND = true;
                    }
                }

                // Проверяю CheckBox для фильтрации запроса по полю "Reporter"
                if (checkBoxCustomQueryReporter.Checked)
                {
                    // Если в соответствующем поле введен текст
                    if (comboBoxCustomQueryReporter.Text.Length > 0)
                    {
                        numberIdInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableUsers, "ID");         // В текущей таблице получаю индекс столбца с именем "ID"
                        numberNameInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableUsers, "Name");     // В текущей таблице получаю индекс столбца с именем "Name"

                        // Если не все столбцы имеются
                        if ((numberIdInArray < 0) || (numberNameInArray < 0))
                        {
                            MessageBox.Show("File 'Form1.cs' method 'generateQuerySelectDataFromDB'.\n'numberIdInArray' = " +
                                numberIdInArray.ToString() + " ;'numberNameInArray' = " + numberNameInArray.ToString(), "Runtime error!");

                            // Возвращаю стандартный запрос к БД
                            return "SELECT * FROM " + workWithDbClass.nameTableTasks;
                        }

                        // Получаю ID пользователя по его имени из соответсвующей таблицы БД
                        for (i = 0; i < myDB.tableUsers.listValues.Count; i++)
                        {
                            if (myDB.tableUsers.listValues[i][numberNameInArray].Equals(comboBoxCustomQueryReporter.Text))
                            {
                                id = myDB.tableUsers.listValues[i][numberIdInArray];
                                break;
                            }
                        }

                        // Если нужно добавить в запрос слово "WHERE"
                        if (isNeedAddWHERE)
                        {
                            query += " WHERE (";                        // Дополняю запрос к БД
                            isNeedAddWHERE = false;
                        }

                        // Если нужно добавить в запрос слово "AND"
                        if (isNeedAddAND)
                            query += " AND ";                           // Дополняю запрос к БД

                        // Дополняю запрос к БД
                        query += workWithDbClass.nameTableTasks + ".c_Reporter = " + "\"" + id + "\"";

                        // Необходимо добавить в запрос слово "AND"
                        isNeedAddAND = true;
                    }
                }

                // Проверяю CheckBox для фильтрации запроса по полю "CurrentStatus"
                if (checkBoxCustomQueryCurrentStatus.Checked)
                {
                    // Если в соответствующем поле введен текст
                    if (comboBoxCustomQueryCurrentStatus.Text.Length > 0)
                    {
                        numberIdInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableTaskStates, "ID");         // В текущей таблице получаю индекс столбца с именем "ID"
                        numberNameInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableTaskStates, "Name");     // В текущей таблице получаю индекс столбца с именем "Name"

                        // Если не все столбцы имеются
                        if ((numberIdInArray < 0) || (numberNameInArray < 0))
                        {
                            MessageBox.Show("File 'Form1.cs' method 'generateQuerySelectDataFromDB'.\n'numberIdInArray' = " +
                                numberIdInArray.ToString() + " ;'numberNameInArray' = " + numberNameInArray.ToString(), "Runtime error!");

                            // Возвращаю стандартный запрос к БД
                            return "SELECT * FROM " + workWithDbClass.nameTableTasks;
                        }

                        // Получаю ID пользователя по его имени из соответсвующей таблицы БД
                        for (i = 0; i < myDB.tableTaskStates.listValues.Count; i++)
                        {
                            if (myDB.tableTaskStates.listValues[i][numberNameInArray].Equals(comboBoxCustomQueryCurrentStatus.Text))
                            {
                                id = myDB.tableTaskStates.listValues[i][numberIdInArray];
                                break;
                            }
                        }

                        // Если нужно добавить в запрос слово "WHERE"
                        if (isNeedAddWHERE)
                        {
                            query += " WHERE (";                        // Дополняю запрос к БД
                            isNeedAddWHERE = false;
                        }

                        // Если нужно добавить в запрос слово "AND"
                        if (isNeedAddAND)
                            query += " AND ";                           // Дополняю запрос к БД

                        // Дополняю запрос к БД
                        query += workWithDbClass.nameTableTasks + ".c_CurrentStatus = " + "\"" + id + "\"";

                        // Необходимо добавить в запрос слово "AND"
                        isNeedAddAND = true;
                    }
                }

                // Если в запросе было использовано слово "WHERE", то нужно закрыть скобку условия
                if (!isNeedAddWHERE)
                    query += ")";                                       // Дополняю запрос к БД
            }

            // Возвращаю полную результирующую строку запроса к БД
            return query;
        }

        // В DataGrid получаю индекс столбца по его имени (-1 - столбец с указанным именем отсутсвует в DataGrid)
        private int getColumnNumberOnColumnNameInDataGrid(string name)
        {
            int number = -1;

            // Если столбец с указанным именем есть в DataGrid
            if (dataGridViewDataFromDB.Columns.Contains(name))
                number = dataGridViewDataFromDB.Columns[name].Index;        // Получаю его индекс
            else
                MessageBox.Show("File 'Form1.cs' method 'getColumnNumberOnColumnNameInDataGrid'.\n" + "Can't found column with name '" + name + "' in DataGrid.", "Runtime error!");

            return number;
        }

        // Событие, вызываемое во время активации формы
        private void Form1_Activated(object sender, EventArgs e)
        {
            // Выполняю следующие действия только, если имеется подключение к БД и успешно выполнен последний запрос к БД
            if ((myDB.myConnection.State == System.Data.ConnectionState.Open) && (myDB.stateLastQuery == (int)enumDbQueryStates.success))
            {
                // Если подключение к БД отсутствует
                if (!myDB.isConnectToDb())
                {
                    disableFormControlsOnDisconnectedFromDb();          // Изменяю активность контролов на форме.
                    return;
                }

                // Читаю данные из БД в DataGrid.
                setDataFromDbToDataGrid();
            }
        }

        // Событие таймера для обновления данных из БД в DataGrid
        private void timerElapsedUpdateDataFromDB(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Если обновление по таймеру включено
            if (checkBoxAutoupdateData.Checked)
            {
                // Выполняю следующие действия только, если имеется подключение к БД и успешно выполнен последний запрос к БД
                if ((myDB.myConnection.State == System.Data.ConnectionState.Open) && (myDB.stateLastQuery == (int)enumDbQueryStates.success))
                {
                    // Если подключение к БД отсутствует
                    if (!myDB.isConnectToDb())
                    {
                        // Изменяю активность контролов на форме. Так как запуск происходит из параллельного потока,
                        // то для обеспечения безопасности запускаю метод через BeginInvoke(...)
                        BeginInvoke(new invokeDelegate(disableFormControlsOnDisconnectedFromDb));
                        return;
                    }

                    // Если в данный момент не добавляется или не редактируется task
                    if (myFormEditData.currentOperation == (int)enumTypeOperation.none)
                    {
                        // Читаю данные из БД в DataGrid. Так как запуск происходит из параллельного потока,
                        // то для обеспечения безопасности запускаю метод через BeginInvoke(...)
                        BeginInvoke(new invokeDelegate(setDataFromDbToDataGrid));
                    }
                }
            }
        }

        // Событие таймера для отправки email сообщений из очереди
        private void timerElapsedSendEmailFromQueue(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Вызываю метод отправки email сообщений из очереди. Так как запуск происходит из параллельного потока,
            // то для обеспечения безопасности запускаю метод через BeginInvoke(...)
            BeginInvoke(new invokeDelegate(myEmail.sendMessage));
        }

        // Обработчик нажатия кнопки "Connect"
        private void buttonDBConnect_Click(object sender, EventArgs e)
        {
            DialogResult myDialogResult;
            // Вызываю форму подключения к БД
            myDialogResult = myFormConnectToDb.ShowDialog();

            // Если форма была закрыта по нажатию кнопки "OK"
            if (myDialogResult == DialogResult.OK)
            {
                // Если имется подключение к БД
                if (myDB.myConnection.State == System.Data.ConnectionState.Open)
                {
                    // Изменяю активность контролов на форме
                    buttonDBConnect.Enabled = false;
                    buttonDBDisconnect.Enabled = true;
                    groupBoxViewDataFromDB.Enabled = true;

                    // Изменяю текущее состояние программы
                    myDB.currentProgramState = (int)enumProgramStates.activeMainForm;

                    setCurrentDbUserProperties();                           // Устанавливаю свойства для текущего пользователя, от имени которого было подключение к БД
                    myFormEditData.initialiseAttributes(myDB, myEmail);     // Инициализирую свойства формы добавления и редактироания task'ов
                    setDataFromDbToDataGrid();                              // Читаю имеющиеся task'и из БД и отображаю их в DataGrid
                }
            }
        }

        // Обработчик нажатия кнопки "Disconnect"
        private void buttonDBDisconnect_Click(object sender, EventArgs e)
        {
            // Если имеется подключение к БД
            if (myDB.myConnection.State == System.Data.ConnectionState.Open)
            {
                myDB.disconnectFromDB();                        // Отключаюсь от БД
                disableFormControlsOnDisconnectedFromDb();      // Изменяю активность контролов на форме
            }
        }

        // Обработчик нажатия кнопки чтения данных из БД
        private void buttonReadDataFromDB_Click(object sender, EventArgs e)
        {
            // Если подключение к БД отсутствует
            if (!myDB.isConnectToDb())
            {
                disableFormControlsOnDisconnectedFromDb();      // Изменяю активность контролов на форме
                return;
            }

            // Читаю данные из БД в DataGrid
            setDataFromDbToDataGrid();
        }

        // Обработчик нажатия кнопки добавления нового task'а
        private void buttonAddNewRow_Click(object sender, EventArgs e)
        {
            // Если подключение к БД отсутствует
            if (!myDB.isConnectToDb())
            {
                disableFormControlsOnDisconnectedFromDb();                              // Изменяю активность контролов на форме
                return;
            }

            // Проверяю, может ли данный пользователь добавлять данные в БД
            if (checkCanUserAddOrEditData((int)enumTypeOperation.addNewRow))
            {
                myFormEditData.Text = "Add new row in database";                        // Изменяю название формы
                myFormEditData.currentOperation = (int)enumTypeOperation.addNewRow;     // Устанавливаю текущий тип операции

                // Вызываю метод для добавления / редактирования task'а
                addNewOrEditRow();
            }
        }

        // Обработчик нажатия кнопки редактирования выбранного task'а
        private void buttonEditSelectedRow_Click(object sender, EventArgs e)
        {
            // Если подключение к БД отсутствует
            if (!myDB.isConnectToDb())
            {
                disableFormControlsOnDisconnectedFromDb();                                       // Изменяю активность контролов на форме
                return;
            }

            // Проверяю, может ли данный пользователь редактировать данные БД
            if (checkCanUserAddOrEditData((int)enumTypeOperation.editSelectedRow))
            {
                myFormEditData.Text = "Edit selected row";                                      // Изменяю название формы
                myFormEditData.currentOperation = (int)enumTypeOperation.editSelectedRow;       // Устанавливаю текущий тип операции

                // Вызываю метод для добавления / редактирования task'а
                addNewOrEditRow();
            }
        }

        // Обработчик нажатия кнопки удаления выбранного task'а
        private void buttonDeleteSelectedRow_Click(object sender, EventArgs e)
        {
            int selectedRow = -1;
            int numberColumnIdInDataGrid = -1;
            string ID = "";
            string isCanEditRow = "";
            string query = "";

            string emailReceivers = "";
            string emailMessage = "";
            string emailSubject = "";

            // Если чтение данных из БД заблокировано, то выхожу
            if (isLockedReadData == (int)enumStatesLockedReadData.locked)
                return;

            // Блокирую чтение данных из БД
            isLockedReadData = (int)enumStatesLockedReadData.locked;

            // Если подключение к БД отсутствует
            if (!myDB.isConnectToDb())
            {
                disableFormControlsOnDisconnectedFromDb();                                      // Изменяю активность контролов на форме
                return;
            }

            // Проверяю, может ли данный пользователь редактировать данные БД
            if (checkCanUserAddOrEditData((int)enumTypeOperation.editSelectedRow))
            {
                // Если в базе еще нет ни одной записи
                if (dataGridViewDataFromDB.Rows.Count == 0)
                    return;
                else
                    selectedRow = dataGridViewDataFromDB.CurrentRow.Index;                      // Получаю индекс выбранной строки

                // Проверяю можно ли удалить данный task, то есть что он не заблокирован (возможно, что в данный момент его редактирует другой пользователь)
                isCanEditRow = isCanEditSelectedRow(selectedRow);
                // Если метод вернул пустую строку, значит, что task заблокирован для удаления / редактирования
                if (isCanEditRow.Equals(""))
                    return;

                // Показываю диалоговое окно для подтверждения удаления task'а
                DialogResult myDialogResult = MessageBox.Show("Are you realy want to delete row?", "Attention!", MessageBoxButtons.YesNo);

                // Если на диалоговом окне нажата кнопка "Yes"
                if (myDialogResult == DialogResult.Yes)
                {
                    // В DataGrid получаю индекс столбца с именем "myDB.nameColumnsMainTable[(int)enumNamesColumnsMainTable.Id]"
                    numberColumnIdInDataGrid = getColumnNumberOnColumnNameInDataGrid(myDB.nameColumnsMainTable[(int)enumNamesColumnsMainTable.Id]);
                    // Если в DataGrid нет такого столбца
                    if (numberColumnIdInDataGrid < 0)
                    {
                        MessageBox.Show("File 'Form1.cs' method 'buttonDeleteSelectedRow_Click'.\n" + "'numberColumnIdInDataGrid' = " + numberColumnIdInDataGrid.ToString(), "Runtime error!");
                        return;
                    }

                    // Если выбрана строка в DataGrid
                    if (selectedRow >= 0)
                    {
                        // Формирую email сообщение
                        emailSubject = "In \"OCTAVIAN Task Planning\" delete task, where you owner";
                        emailMessage = "In \"OCTAVIAN Task Planning\" delete task, where you owner.\n\n\n";
                        emailMessage += "Description of deleted task:\n";
                        emailMessage += "--------------------------------------------------\n";

                        for (int count = 0; count < dataGridViewDataFromDB.Columns.Count; count++)
                        {
                            if ((dataGridViewDataFromDB.Rows[selectedRow].Cells[count].Value != null) && (dataGridViewDataFromDB.Rows[selectedRow].Cells[count].Value.ToString() != ""))
                            {
                                // Если текущий столбец содержит владельцев task'а, то сохраняю их в переменной
                                if (count == (int)enumNamesColumnsMainTable.Owner)
                                    emailReceivers = dataGridViewDataFromDB.Rows[selectedRow].Cells[count].Value.ToString();

                                emailMessage += dataGridViewDataFromDB.Columns[count].HeaderText + ":     ";
                                emailMessage += dataGridViewDataFromDB.Rows[selectedRow].Cells[count].Value.ToString();
                                emailMessage += "\n--------------------------------------------------\n";
                            }
                            else
                            {
                                emailMessage += dataGridViewDataFromDB.Columns[count].HeaderText + ":     ";

                                if (dataGridViewDataFromDB.Rows[selectedRow].Cells[count].Value != null)
                                    emailMessage += dataGridViewDataFromDB.Rows[selectedRow].Cells[count].Value.ToString();
                                else
                                    emailMessage += "";

                                emailMessage += "\n--------------------------------------------------\n";
                            }
                        }

                        // Получаю ID выбранного task'а
                        ID = dataGridViewDataFromDB[numberColumnIdInDataGrid, selectedRow].Value.ToString();
                        // Формирую запрос к БД
                        query = "CALL deleteDataFromTableTasks (" + ID + ")";
                        // Выполняю запрос к БД
                        myDB.insertDataToDB(query);

                        // Если запрос был выполнен успешно
                        if (myDB.stateLastQuery == (int)enumDbQueryStates.success)
                        {
                            // Добавляю email сообщение в очередь сообщений на отправку
                            // При этом функцией "getEmailAddresOnUsersNames" получаю email адреса для владельцев task'а, содержащихся в переменной "emailReceivers"
                            myEmail.addMessageToSendQueue(myDB.getEmailAddresOnUsersNames(emailReceivers), emailSubject, emailMessage);
                            // Читаю данные из БД в DataGrid
                            setDataFromDbToDataGrid();
                        }
                        else
                        {
                            MessageBox.Show("File 'Form1.cs' method 'buttonDeleteSelectedRow_Click'.\n\n" + "Problem delete data from database!\nCheck connection with database and try again!", "ERROR!");
                        }
                    }
                    else
                    {
                        MessageBox.Show("File 'Form1.cs' method 'buttonDeleteSelectedRow_Click'.\n'selectedRow' = " + selectedRow.ToString(), "Runtime error!");
                    }
                }

                // Если на диалоговом окне нажата кнопка "No"
                if (myDialogResult == DialogResult.No)
                {
                    // Если "isCanEditRow" содержит ID удаляемого task'а
                    if (!isCanEditRow.Equals(""))
                    {
                        // Разблокирую текущую строку
                        myDB.setLockToRow(isCanEditRow, 0);
                    }
                }
            }

            // Разблокирую чтение данных из БД
            isLockedReadData = (int)enumStatesLockedReadData.unlocked;
            // Читаю данные из БД в DataGrid
            setDataFromDbToDataGrid();
        }

        // Событие, возникающее при нажатии на кнопку сохранения данных DataGrid в файл
        private void buttonExportData_Click(object sender, EventArgs e)
        {
            // Показываю диалог сохранения файла
            mySaveFileDialog.ShowDialog();
        }

        // Событие, возникающее при двойном щелчке кнопкой мышки
        private void dataGridViewDataFromDB_DoubleClick(object sender, EventArgs e)
        {
            MouseEventArgs myMouseButton = (MouseEventArgs)e;

            // Если подключение к БД отсутствует
            if (!myDB.isConnectToDb())
            {
                disableFormControlsOnDisconnectedFromDb();          // Изменяю активность контролов на форме
                return;
            }

            // Если был двойной щелчок ...
            switch (myMouseButton.Button)
            {
                // ... левой кнопкой мышки
                case MouseButtons.Left:
                    {
                        // Проверяю активна ли кнопка редактирования task'а
                        if (buttonEditSelectedRow.Enabled == true)
                        {
                            if (checkCanUserAddOrEditData((int)enumTypeOperation.editSelectedRow))
                            {
                                myFormEditData.Text = "Edit selected task";
                                myFormEditData.currentOperation = (int)enumTypeOperation.editSelectedRow;
                                addNewOrEditRow();
                            }
                        }

                        break;
                    }

                // ... правой кнопкой мышки
                case MouseButtons.Right:
                    {
                        // Проверяю активна ли кнопка добавления нового task'а
                        if (buttonAddNewRow.Enabled == true)
                        {
                            if (checkCanUserAddOrEditData((int)enumTypeOperation.addNewRow))
                            {
                                myFormEditData.Text = "Add new task in database";
                                myFormEditData.currentOperation = (int)enumTypeOperation.addNewRow;
                                addNewOrEditRow();
                            }
                        }

                        break;
                    }
            }
        }

        // Событие, возникающее при изменении состояния CheckBox "StatrTime"
        private void checkBoxCustomQueryStartTime_CheckedChanged(object sender, EventArgs e)
        {
            // Если флажок установлен
            if (checkBoxCustomQueryStartTime.Checked)
            {
                textBoxCustomQueryStartTime.Enabled = true;             // Делаю активным соответствующий TextBox
                amountCustomQueryCheckedFields++;                       // Увеличиваю число полей, которые нужно проверять при составлении выборочного запроса к БД

                // Если TextBox пустой, то присваиваю ему значение текущей даты (без времени)
                if (textBoxCustomQueryStartTime.Text.Length == 0)
                    textBoxCustomQueryStartTime.Text = DateTime.Now.ToString(myFormEditData.stringFormatDateTime.Substring(0, 10));
            }
            else
            {
                textBoxCustomQueryStartTime.Enabled = false;            // Делаю неактивным соответствующий TextBox
                amountCustomQueryCheckedFields--;                       // Уменьшаю число полей, которые нужно проверять при составлении выборочного запроса к БД
            }

            // Читаю данные из БД в DataGrid
            setDataFromDbToDataGrid();
        }

        // Событие, возникающее при изменении состояния CheckBox "Project"
        private void checkBoxCustomQueryProject_CheckedChanged(object sender, EventArgs e)
        {
            // Если флажок установлен
            if (checkBoxCustomQueryProject.Checked)
            {
                comboBoxCustomQueryProject.Enabled = true;              // Делаю активным соответствующий TextBox
                amountCustomQueryCheckedFields++;                       // Увеличиваю число полей, которые нужно проверять при составлении выборочного запроса к БД
            }
            else
            {
                comboBoxCustomQueryProject.Enabled = false;             // Делаю неактивным соответствующий TextBox
                amountCustomQueryCheckedFields--;                       // Уменьшаю число полей, которые нужно проверять при составлении выборочного запроса к БД
            }

            // Читаю данные из БД в DataGrid
            setDataFromDbToDataGrid();
        }

        // Событие, возникающее при изменении состояния CheckBox "Owner"
        private void checkBoxCustomQueryOwner_CheckedChanged(object sender, EventArgs e)
        {
            // Если флажок установлен
            if (checkBoxCustomQueryOwner.Checked)
            {
                comboBoxCustomQueryOwner.Enabled = true;                // Делаю активным соответствующий TextBox
                amountCustomQueryCheckedFields++;                       // Увеличиваю число полей, которые нужно проверять при составлении выборочного запроса к БД
            }
            else
            {
                comboBoxCustomQueryOwner.Enabled = false;               // Делаю неактивным соответствующий TextBox
                amountCustomQueryCheckedFields--;                       // Уменьшаю число полей, которые нужно проверять при составлении выборочного запроса к БД
            }

            // Читаю данные из БД в DataGrid
            setDataFromDbToDataGrid();
        }

        // Событие, возникающее при изменении состояния CheckBox "Reporter"
        private void checkBoxCustomQueryReporter_CheckedChanged(object sender, EventArgs e)
        {
            // Если флажок установлен
            if (checkBoxCustomQueryReporter.Checked)
            {
                comboBoxCustomQueryReporter.Enabled = true;             // Делаю активным соответствующий TextBox
                amountCustomQueryCheckedFields++;                       // Увеличиваю число полей, которые нужно проверять при составлении выборочного запроса к БД
            }
            else
            {
                comboBoxCustomQueryReporter.Enabled = false;            // Делаю неактивным соответствующий TextBox
                amountCustomQueryCheckedFields--;                       // Уменьшаю число полей, которые нужно проверять при составлении выборочного запроса к БД
            }

            // Читаю данные из БД в DataGrid
            setDataFromDbToDataGrid();
        }

        // Событие, возникающее при изменении состояния CheckBox "CurrentStatus"
        private void checkBoxCustomQueryCurrentStatus_CheckedChanged(object sender, EventArgs e)
        {
            // Если флажок установлен
            if (checkBoxCustomQueryCurrentStatus.Checked)
            {
                comboBoxCustomQueryCurrentStatus.Enabled = true;        // Делаю активным соответствующий TextBox
                amountCustomQueryCheckedFields++;                       // Увеличиваю число полей, которые нужно проверять при составлении выборочного запроса к БД
            }
            else
            {
                comboBoxCustomQueryCurrentStatus.Enabled = false;       // Делаю неактивным соответствующий TextBox
                amountCustomQueryCheckedFields--;                       // Уменьшаю число полей, которые нужно проверять при составлении выборочного запроса к БД
            }

            // Читаю данные из БД в DataGrid
            setDataFromDbToDataGrid();
        }

        // Событие, возникающее при щелчке мышкой над ComboBox "Project"
        private void comboBoxCustomQueryProject_MouseClick(object sender, MouseEventArgs e)
        {
            int numberColumnName = -1;

            // Если подключение к БД отсутствует
            if (!myDB.isConnectToDb())
            {
                disableFormControlsOnDisconnectedFromDb();          // Изменяю активность контролов на форме
                return;
            }

            numberColumnName = myDB.getColumnIndexOnColumnNameInTable(myDB.tableProjects, "Name");      // В текущей таблице получаю индекс столбца с именем "Name"
            // Если такого столбца нет
            if (numberColumnName < 0)
            {
                MessageBox.Show("File 'Form1.cs' method 'comboBoxCustomQueryProject_MouseClick'.\n'numberColumnName' = " + numberColumnName.ToString(), "Runtime error!");
                return;
            }

            // Читаю данные из таблиц БД
            myDB.readUsersProjectsTaskStatusFromDB();
            // Очищаю соответствующий ComboBox
            comboBoxCustomQueryProject.Items.Clear();
            // Добавляю в ComboBox данные, прочитанные из соответсвующей таблицы БД
            for (int i = 0; i < myDB.tableProjects.listValues.Count; i++)
                comboBoxCustomQueryProject.Items.Add(myDB.tableProjects.listValues[i][numberColumnName].ToString());
        }

        // Событие, возникающее при щелчке мышкой над ComboBox "Owner"
        private void comboBoxCustomQueryOwner_MouseClick(object sender, MouseEventArgs e)
        {
            int numberColumnName = -1;
            int numberGroupInArray = -1;

            // Если подключение к БД отсутствует
            if (!myDB.isConnectToDb())
            {
                disableFormControlsOnDisconnectedFromDb();          // Изменяю активность контролов на форме
                return;
            }

            numberColumnName = myDB.getColumnIndexOnColumnNameInTable(myDB.tableUsers, "Name");         // В текущей таблице получаю индекс столбца с именем "Name"
            numberGroupInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableUsers, "Group");      // В текущей таблице получаю индекс столбца с именем "Group"
            // Если таких столбцов нет
            if ((numberColumnName < 0) || (numberGroupInArray < 0))
            {
                MessageBox.Show("File 'Form1.cs' method 'comboBoxCustomQueryOwner_MouseClick'.\n'numberColumnName' = " + numberColumnName.ToString() +
                                        " ;'numberGroupInArray' = " + numberGroupInArray.ToString(), "Runtime error!");

                return;
            }

            // Читаю данные из таблиц БД
            myDB.readUsersProjectsTaskStatusFromDB();
            // Очищаю соответствующий ComboBox
            comboBoxCustomQueryOwner.Items.Clear();
            // Добавляю в ComboBox данные, прочитанные из соответсвующей таблицы БД
            for (int i = 0; i < myDB.tableUsers.listValues.Count; i++)
            {
                // Исключаю из списка пользователей из группы "Guest"
                if (!myDB.tableUsers.listValues[i][numberGroupInArray].Equals(myDB.nameGuestGroup))
                    comboBoxCustomQueryOwner.Items.Add(myDB.tableUsers.listValues[i][numberColumnName].ToString());
            }
        }

        // Событие, возникающее при щелчке мышкой над ComboBox "Reporter"
        private void comboBoxCustomQueryReporter_MouseClick(object sender, MouseEventArgs e)
        {
            int numberColumnName = -1;
            int numberGroupInArray = -1;

            // Если подключение к БД отсутствует
            if (!myDB.isConnectToDb())
            {
                disableFormControlsOnDisconnectedFromDb();          // Изменяю активность контролов на форме
                return;
            }

            numberColumnName = myDB.getColumnIndexOnColumnNameInTable(myDB.tableUsers, "Name");         // В текущей таблице получаю индекс столбца с именем "Name"
            numberGroupInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableUsers, "Group");      // В текущей таблице получаю индекс столбца с именем "Group"

            if ((numberColumnName < 0) || (numberGroupInArray < 0))
            {
                MessageBox.Show("File 'Form1.cs' method 'comboBoxCustomQueryReporter_MouseClick'.\n'numberColumnName' = " + numberColumnName.ToString() +
                                        " ;'numberGroupInArray' = " + numberGroupInArray.ToString(), "Runtime error!");

                return;
            }

            // Читаю данные из таблиц БД
            myDB.readUsersProjectsTaskStatusFromDB();
            // Очищаю соответствующий ComboBox
            comboBoxCustomQueryReporter.Items.Clear();
            // Добавляю в ComboBox данные, прочитанные из соответсвующей таблицы БД
            for (int i = 0; i < myDB.tableUsers.listValues.Count; i++)
            {
                // Исключаю из списка пользователей из группы "Guest"
                if (!myDB.tableUsers.listValues[i][numberGroupInArray].Equals(myDB.nameGuestGroup))
                    comboBoxCustomQueryReporter.Items.Add(myDB.tableUsers.listValues[i][numberColumnName].ToString());
            }
        }

        // Событие, возникающее при щелчке мышкой над ComboBox "CurrentStatus"
        private void comboBoxCustomQueryCurrentStatus_MouseClick(object sender, MouseEventArgs e)
        {
            int numberColumnName = -1;

            // Если подключение к БД отсутствует
            if (!myDB.isConnectToDb())
            {
                disableFormControlsOnDisconnectedFromDb();          // Изменяю активность контролов на форме
                return;
            }

            numberColumnName = myDB.getColumnIndexOnColumnNameInTable(myDB.tableTaskStates, "Name");        // В текущей таблице получаю индекс столбца с именем "Name"
            // Если такого столбца нет
            if (numberColumnName < 0)
            {
                MessageBox.Show("File 'Form1.cs' method 'comboBoxCustomQueryCurrentStatus_MouseClick'.\n'numberColumnName' = " + numberColumnName.ToString(), "Runtime error!");
                return;
            }

            // Читаю данные из таблиц БД
            myDB.readUsersProjectsTaskStatusFromDB();
            // Очищаю соответствующий ComboBox
            comboBoxCustomQueryCurrentStatus.Items.Clear();
            // Добавляю в ComboBox данные, прочитанные из соответсвующей таблицы БД
            for (int i = 0; i < myDB.tableTaskStates.listValues.Count; i++)
                comboBoxCustomQueryCurrentStatus.Items.Add(myDB.tableTaskStates.listValues[i][numberColumnName].ToString());
        }

        // Событие, возникающее при изменении выбранного значения ComboBox "Project"
        private void comboBoxCustomQueryProject_SelectedValueChanged(object sender, EventArgs e)
        {
            // Читаю данные из БД в DataGrid
            setDataFromDbToDataGrid();
        }

        // Событие, возникающее при изменении выбранного значения ComboBox "Owner"
        private void comboBoxCustomQueryOwner_SelectedValueChanged(object sender, EventArgs e)
        {
            // Читаю данные из БД в DataGrid
            setDataFromDbToDataGrid();
        }

        // Событие, возникающее при изменении выбранного значения ComboBox "Reporter"
        private void comboBoxCustomQueryReporter_SelectedValueChanged(object sender, EventArgs e)
        {
            // Читаю данные из БД в DataGrid
            setDataFromDbToDataGrid();
        }

        // Событие, возникающее при изменении выбранного значения ComboBox "CurrentStatus"
        private void comboBoxCustomQueryCurrentStatus_SelectedValueChanged(object sender, EventArgs e)
        {
            // Читаю данные из БД в DataGrid
            setDataFromDbToDataGrid();
        }

        // Событие, возникающее при нажатии на кнопки "OK" в диалоговом окне сохранения БД в файл
        private void mySaveFileDialog_FileOk(object obj, CancelEventArgs e)
        {
            string pathToSaveFile = mySaveFileDialog.FileName;          // Путь к сохраняемому файлу
            int amountSavedColumns = -1;                                // Число столбцов DataGrid, которое нужно сохранить
            int column = 0;

            // В DataGrid получаю индекс столбца с именем "myDB.nameColumnsMainTable[(int)enumNamesColumnsMainTable.AmountChanges]"
            amountSavedColumns = getColumnNumberOnColumnNameInDataGrid(myDB.nameColumnsMainTable[(int)enumNamesColumnsMainTable.AmountChanges]);
            // Если в DataGrid нет такого столбца
            if (amountSavedColumns < 0)
            {
                MessageBox.Show("File 'Form1.cs' method 'mySaveFileDialog_FileOk'.\n" + "'amountSavedColumns' = " + amountSavedColumns.ToString(), "Runtime error!");
                return;
            }

            // Записываю данные DataGrid в файл
            try
            {
                using (StreamWriter myStreamWrite = new StreamWriter(pathToSaveFile, false, Encoding.UTF8))
                {
                    // Сохраняю названия столбцов DataGrid
                    for (column = 0; column < amountSavedColumns; column++)
                        myStreamWrite.Write("\"" + dataGridViewDataFromDB.Columns[column].Name.ToString() + "\";");

                    // Перевожу каретку на следующую строку
                    myStreamWrite.WriteLine();
                    // Просматриваю все строки
                    for (int row = 0; row < dataGridViewDataFromDB.Rows.Count; row++)
                    {
                        // Сохраняю ячейки текущей строки
                        for (column = 0; column < amountSavedColumns; column++)
                            myStreamWrite.Write("\"" + dataGridViewDataFromDB.Rows[row].Cells[column].Value.ToString() + "\";");

                        // Перевожу каретку на следующую строку
                        myStreamWrite.WriteLine();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("File 'Form1.cs' method 'mySaveFileDialog_FileOk'.\nError in save data in file!\n\n" + ex.Message, "Runtime error!");
            }
        }
    }
}
