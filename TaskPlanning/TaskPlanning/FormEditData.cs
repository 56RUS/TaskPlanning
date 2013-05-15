using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using TaskPlanning;

namespace TaskPlanning
{
    // Текущий тип выполняемой операции
    enum enumTypeOperation
    {
        none,               // Нет
        addNewRow,          // Добавление нового task'а
        editSelectedRow     // Редактирование имеющегося task'а
    }

    // Структура для ячейки с выпадающим списком
    struct structDataGridComboBoxCell
    {
        public string name;                     // Название
        public DataGridViewComboBoxCell cell;   // Ячейка ComboBox (выпадающий список)
    }

    public partial class FormEditData : Form
    {
        public string stringFormatDateTime = "yyyy.MM.dd HH:mm:ss";         // Формат представления даты и времени в соответствующих столбцах

        private workWithDbClass myDBChildForm;                              // ССылка на объект для работы с БД (инициализируется классом, создающим объект данного класса)
        private workWithEmailClass myEmailChild;                            // ССылка на объект для работы с email (инициализируется классом, создающим объект данного класса)

        private structDataGridComboBoxCell[] comboBoxCells;                 // Массив, содержащий ячекйки с ComboBox

        private ListBox ownerListBox;                                       // ListBox для столбца, содержащего владельцев данного task'а
        private int ownerListBoxRowHeight;                                  // Высота строки в ListBox

        List<string> currentTaskOwnersList;                                 // Список текущих владельцев  task'а
        List<string> deletedOwnersList;                                     // Список, удаляемых владельцев task'а
        List<string> newOwnersList;                                         // Список добавляемых владельцев task'а

        public int currentOperation;                                        // Определяет тип текущей операции


        public FormEditData()
        {
            InitializeComponent();

            currentOperation = (int)enumTypeOperation.none;
            currentTaskOwnersList = new List<string>();
            deletedOwnersList = new List<string>();
            newOwnersList = new List<string>();

            comboBoxCells = new structDataGridComboBoxCell[(int)enumNameColumnsWithComboBox.length];
            for (int count = 0; count < (int)enumNameColumnsWithComboBox.length; count++)
                comboBoxCells[count].name = "";

            ownerListBox = new ListBox();
            ownerListBox.Location = new Point (100, 100);
            ownerListBox.SelectionMode = SelectionMode.MultiSimple;
            ownerListBox.HorizontalScrollbar = true;
            ownerListBox.SelectedIndexChanged += new EventHandler(ListBox_MouseClick);
            ownerListBoxRowHeight = 13;
            dataGridViewAddData.Controls.Add(ownerListBox);     // Добавляю ListBox в список контролов формы
        }

        // Инициализирую атрибуты класса
        public void initialiseAttributes(workWithDbClass myDB, workWithEmailClass myEmail)
        {
            // Если указатель на БД не нулевой, то инициализирую его
            if (myDB != null)
                myDBChildForm = myDB;
            else
            {
                // Если указатель на БД нулевой, то обнуляю все таблицы
                myDBChildForm.tableProjects.clearStruct();
                myDBChildForm.tableUsers.clearStruct();
                myDBChildForm.tableTaskStates.clearStruct();

                myDBChildForm = null;       // Обнуляю указатель
            }

            myEmailChild = myEmail;         // Инициализирую указатель на объект для отправки сообщений (значение: указатель, либо null)

            for (int count = 0; count < (int)enumNameColumnsWithComboBox.length; count++)
            {
                if (myDBChildForm != null)
                    comboBoxCells[count].name = myDBChildForm.nameColumnsWithComboBox[count];
                else
                    comboBoxCells[count].name = "";
            }
        }
        
        // Добавляю названия для столбцов DataGrid и некоторые скрываю или делаю доступными только для чтения
        public void setNamesForColumnsDataGrid(string[] namesColumns)
        {
            for (int count = 0; count < namesColumns.Length; count++)
            {
                // Для столбцов, которые обязательно дожны быть заполнены в название добавляю символ из переменной "myDBChildForm.columnNotBeNullString"
                if ((count == (int)enumNamesColumnsMainTable.StartTime) || (count == (int)enumNamesColumnsMainTable.Project) || (count == (int)enumNamesColumnsMainTable.Description) ||
                       (count == (int)enumNamesColumnsMainTable.Owner) || (count == (int)enumNamesColumnsMainTable.CurrentStatus))
                {
                    dataGridViewAddData.Columns.Add(namesColumns[count], namesColumns[count] + myDBChildForm.columnNotBeNullString);
                }
                else
                    dataGridViewAddData.Columns.Add(namesColumns[count], namesColumns[count]);

                // Для каждого столбца устанавливаю определнную ширину
                dataGridViewAddData.Columns[count].Width = myDBChildForm.columnsWidth[count];
            }

            // Устанавливаю свойства для столбцов (либо только для чтения, либо вообще скрываю)
            try
            {
                dataGridViewAddData.Columns["ID"].Visible = false;
                dataGridViewAddData.Columns["Owner"].ReadOnly = true;
                dataGridViewAddData.Columns["Reporter"].ReadOnly = true;
                dataGridViewAddData.Columns["LastChange"].ReadOnly = true;
                dataGridViewAddData.Columns["AmountChanges"].ReadOnly = true;
                dataGridViewAddData.Columns["LastChange"].Visible = false;
                dataGridViewAddData.Columns["AmountChanges"].Visible = false;
                dataGridViewAddData.Columns["IsLocked"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("File 'FormEditData.cs' method 'setNamesForColumnsDataGrid'.\nIn 'dataGridViewAddData' not found column with  names!\n\n"
                        + ex.Message, "Runtime error!");
            }
        }

        // Заполняю ComboBox и ListBox ячейки соответствующими значениями
        public void setCellTypesDataGrid()
        {
            structTable currentTable = new structTable();
            int numberColumnName = -1;
            int numberColumnInDataGrid = -1;
            int i = 0;

            // Перебираю все ComboBox ячейки
            for (int count = 0; count < comboBoxCells.Length; count++)
            {
                switch (count)
                {
                    // Инициализирую указатель "currentTable" на соответствующую таблицу
                    case ((int)enumNameColumnsWithComboBox.Project):
                        {
                            currentTable = myDBChildForm.tableProjects;
                            break;
                        }

                    case ((int)enumNameColumnsWithComboBox.CurrentStatus):
                        {
                            currentTable = myDBChildForm.tableTaskStates;
                            break;
                        }
                    default:
                        {
                            MessageBox.Show("File 'FormEditData.cs' method 'setCellTypesDataGrid'. Invalid variable 'comboBoxCells[count].name' = " + comboBoxCells[count].name, "Runtime error!");
                            return;
                        }
                }

                // Если в данной таблице есть список имен столбцов и он не нулевой длины
                if ((currentTable.columnNames != null) && (currentTable.columnNames.Length > 0))
                {
                    // В текущей таблице получаю индекс столбца с именем "Name"
                    numberColumnName = myDBChildForm.getColumnIndexOnColumnNameInTable(currentTable, "Name");
                    // Если в текущей таблице имеется столбец с именем "Name"
                    if (numberColumnName >= 0)
                    {
                        // Создаю ячейку ComboBoxCell, если она не создана
                        if (comboBoxCells[count].cell == null)
                            comboBoxCells[count].cell = new DataGridViewComboBoxCell();

                        // Добавляю значения в список ComboBoxCell
                        for (i = 0; i < currentTable.listValues.Count; i++)
                            comboBoxCells[count].cell.Items.Add(currentTable.listValues[i][numberColumnName].ToString());

                        // В DataGrid получаю индекс столбца с именем "comboBoxCells[count].name"
                        numberColumnInDataGrid = getColumnNumberOnColumnNameInDataGrid(comboBoxCells[count].name);
                        // Если в DataGrid нет такого столбца
                        if (numberColumnInDataGrid < 0)
                        {
                            MessageBox.Show("File 'FormEditData.cs' method 'setCellTypesDataGrid'.\n" + "'numberColumnInDataGrid' = " + numberColumnInDataGrid.ToString(), "Runtime error!");
                            return;
                        }
                        // В текущее значение ячейки ComboBox устанавливаю значение из соответствующего столбца DataGrid
                        comboBoxCells[count].cell.Value = dataGridViewAddData.Rows[0].Cells[numberColumnInDataGrid].Value;
                        // Присваиваю нулевой строке столбцу с индексом "numberColumnInDataGrid" объекта DataGridView созданную ячейку ComboBoxCell
                        dataGridViewAddData.Rows[0].Cells[numberColumnInDataGrid] = comboBoxCells[count].cell;
                    }
                    else
                    {
                        MessageBox.Show("File 'FormEditData.cs' method 'setCellTypesDataGrid'.\n" + "'numberColumnName' = " + numberColumnName.ToString(), "Runtime error!");
                    }
                }
                else
                {
                    if (currentTable.columnNames == null)
                        MessageBox.Show("File 'FormEditData.cs' method 'setCellTypesDataGrid'.\n" + "'currentTable.columnNames' = NULL!", "Runtime error!");
                    else
                    {
                        if (currentTable.columnNames.Length == 0)
                            MessageBox.Show("File 'FormEditData.cs' method 'setCellTypesDataGrid'.\n" + "'currentTable.columnNames.Length' = 0!", "Runtime error!");
                    }
                }
            }

            // Добавляю значения в список, содержащий всех возможных владельцев task'а
            addDataToOwnerListBox();
        }

        // Добавляю значения в список, содержащий всех возможных владельцев task'а
        private void addDataToOwnerListBox()
        {
            structTable currentTable = myDBChildForm.tableUsers;
            int numberColumnName = -1;
            int numberColumnGroup = -1;
            int countItemsInListBox = 0;
            int maxShowedItemsinList = 10;      // Максимальное число владельцев, отображаемое в списке (если их больше, то появится полоса вертикальной прокрутки)

            ownerListBox.Height = 4;            // Устанавливаю начальную высоту списка
            ownerListBox.Items.Clear();         // Очищаю список
            ownerListBox.Visible = false;       // Делаю список невидимым

            // Если в данной таблице есть список имен столбцов и он не нулевой длины
            if ((currentTable.columnNames != null) && (currentTable.columnNames.Length > 0))
            {
                // В текущей таблице получаю индекс столбца с именем "Name"
                numberColumnName = myDBChildForm.getColumnIndexOnColumnNameInTable(currentTable, "Name");
                // В текущей таблице получаю индекс столбца с именем "Group"
                numberColumnGroup = myDBChildForm.getColumnIndexOnColumnNameInTable(currentTable, "Group");

                // Если в текущей таблице имеются столбецы с именами "Name" и "Group"
                if ((numberColumnName >= 0) && (numberColumnGroup >= 0))
                {
                    // Добавляю значения в список ComboBoxCell
                    for (int i = 0; i < currentTable.listValues.Count; i++)
                    {
                        // Исключаю из списка пользователей из группы "Guest"
                        if (!currentTable.listValues[i][numberColumnGroup].Equals(myDBChildForm.nameGuestGroup))
                        {
                            ownerListBox.Items.Add(currentTable.listValues[i][numberColumnName].ToString());

                            // Пока число элементов в списке меньше "maxShowedItemsinList" увеличиваю высоту списка
                            if (countItemsInListBox < maxShowedItemsinList)
                            {
                                ownerListBox.Height = ownerListBox.Height + ownerListBoxRowHeight;
                                countItemsInListBox++;
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("File 'FormEditData.cs' method 'addDataToOwnerListBox'.\n" + "'numberColumnName' = " + numberColumnName.ToString() +
                        "; 'numberColumnGroup' = " + numberColumnGroup.ToString(), "Runtime error!");
                }
            }
            else
            {
                if (currentTable.columnNames == null)
                    MessageBox.Show("File 'FormEditData.cs' method 'addDataToOwnerListBox'.\n" + "'currentTable.columnNames' = NULL!", "Runtime error!");
                else
                {
                    if (currentTable.columnNames.Length == 0)
                        MessageBox.Show("File 'FormEditData.cs' method 'addDataToOwnerListBox'.\n" + "'currentTable.columnNames.Length' = 0!", "Runtime error!");
                }
            }
        }

        // Добавляю строку в DataGrid
        public void addRowValuesForDataGrid(string[] rowsValues)
        {
            dataGridViewAddData.Rows.Add(rowsValues);               // Добавляю строку
            myDBChildForm.readUsersProjectsTaskStatusFromDB();      // Читаю из БД значения из таблиц
            setCellTypesDataGrid();                                 // Заполняю ComboBox и ListBox ячейки соответствующими значениями
            addOwnersToTask();                                      // В списке ListBox выделяю текущих владельцев task'а
        }

        // Добавляю владельцев к task'у
        private void addOwnersToTask()
        {
            int numberColumnOwnerInDataGrid = -1;
            string ownersFromDataGrid = "";

            // В DataGrid получаю индекс столбца с именем "myDBChildForm.nameColumnsMainTable[(int)enumNamesColumnsMainTable.Owner]"
            numberColumnOwnerInDataGrid = getColumnNumberOnColumnNameInDataGrid(myDBChildForm.nameColumnsMainTable[(int)enumNamesColumnsMainTable.Owner]);
            // Если в DataGrid нет такого столбца
            if (numberColumnOwnerInDataGrid < 0)
            {
                MessageBox.Show("File 'FormEditData.cs' method 'addOwnersToTask'.\n'numberColumnOwnerInDataGrid' = " + numberColumnOwnerInDataGrid.ToString(), "Runtime error!");
                return;
            }

            // Очищаю список текущих владельцев task'а
            currentTaskOwnersList.Clear();
            // Получаю текущих владельцев task'а из соответствующего столбца DataGrid
            ownersFromDataGrid = dataGridViewAddData.Rows[0].Cells[numberColumnOwnerInDataGrid].Value.ToString();
            // Создаю список строки текущих владельцев task'а
            currentTaskOwnersList = myDBChildForm.parseOwnersFromString(ownersFromDataGrid);
            // В списке возможных владельцев task'а выделяю текущих владельцев task'а
            selectItemsInOwnerListBox();
        }

        // В списке ListBox выделяю текущих владельцев task'а
        private void selectItemsInOwnerListBox()
        {
            // Просматриваю список текущих владельцев task'а
            for (int count = 0; count < currentTaskOwnersList.Count; count++)
            {
                // Просматриваю весь список ListBox
                for (int i = 0; i < ownerListBox.Items.Count; i++)
                {
                    if (ownerListBox.Items[i].ToString() == currentTaskOwnersList[count])
                    {
                        ownerListBox.SetSelected(i, true);
                        break;
                    }
                }
            }
        }

        // Очистка DataGrid
        public void clearDataGrid()
        {
            // Обнуляю контрол DataGrid
            dataGridViewAddData.Rows.Clear();
            dataGridViewAddData.Columns.Clear();

            // Обнуляю все списки
            ownerListBox.Items.Clear();
            currentTaskOwnersList.Clear();
            newOwnersList.Clear();
            deletedOwnersList.Clear();
            
            // удаляю ячейки ComboBox
            for (int count = 0; count < comboBoxCells.Length; count++)
            {
                if (comboBoxCells[count].cell != null)
                {
                    comboBoxCells[count].cell.Dispose();
                    comboBoxCells[count].cell = null;
                }
            }
        }

        // Создание списков новых и удаляемых владельцев task'а
        private void createAddAndDeletedUsersList()
        {
            // Флаг, показывающий, найден ли текущий владелец task'а в списке новых владельцев task'а
            bool isOwnerFindInNewOwners = false;

            // Очищаю список удаляемых владельцев task'а
            deletedOwnersList.Clear();

            // Просматриваю список текущих владельцев task'а
            for (int count = 0; count < currentTaskOwnersList.Count; count++)
            {
                isOwnerFindInNewOwners = false;

                // Просматриваю список новых  владельцев task'а
                for (int i = 0; i < newOwnersList.Count; i++)
                {
                    // Если текущий  владелец task'а найден в списке новых владельцев task'а
                    if (newOwnersList[i].Equals(currentTaskOwnersList[count]))
                    {
                        newOwnersList.RemoveAt(i);          // Удаляю его из списка новых владельцев task'а (он не новый, он уже был)
                        isOwnerFindInNewOwners = true;      // Устанавливаю флаг

                        break;
                    }
                }

                // Если текущий владелец не найден в списке новых владельцев task'а
                if (!isOwnerFindInNewOwners)
                {
                    deletedOwnersList.Add(currentTaskOwnersList[count]);    // Добавляю его в список удаляемых владельцев task'а
                }
            }
        }

        // В DataGrid получаю индекс столбца по его имени (-1 - столбец с указанным именем отсутсвует в DataGrid)
        private int getColumnNumberOnColumnNameInDataGrid(string name)
        {
            int number = -1;

            // Если столбец с указанным именем есть в DataGrid
            if (dataGridViewAddData.Columns.Contains(name))
                number = dataGridViewAddData.Columns[name].Index;       // Получаю его индекс
            else
                MessageBox.Show("File 'FormEditData.cs' method 'getColumnNumberOnColumnNameInDataGrid'.\n" + "Can't found column with name '" + name + "' in DataGrid.", "Runtime error!");

            return number;
        }

        // Событие, вызываемое во время активации формы
        private void FormEditData_Activated(object sender, EventArgs e)
        {
            myDBChildForm.currentProgramState = (int)enumProgramStates.activeEditDataForm;      // Изменяю текущее состояние программы
        }

        // Событие, вызываемое перед закрытием формы
        private void FormEditData_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Если нажата кнопка "OK"
            if (DialogResult != DialogResult.OK)
            {
                // И отсутствует подключение к БД
                if (!myDBChildForm.isConnectToDb())
                {
                    // То не даю форме закрыться (на экране появится соответствующее сообщение генерируемое методом "myDBChildForm.connectToDB")
                    // так как не сохраненные данные не были записаны в БД и будут потеряны
                    e.Cancel = true;
                }
            }
        }

        // Событие, вызываемое после закрытия формы
        private void FormEditData_FormClosed(object sender, FormClosedEventArgs e)
        {
            currentOperation = (int)enumTypeOperation.none;                                 // Изменяю тип текущей операции на "none"
            myDBChildForm.currentProgramState = (int)enumProgramStates.activeMainForm;      // Изменяю текущее состояние программы
            clearDataGrid();                                                                // Очищаю все, что связано с DataGrid
        }

        // Обработчик нажатия кнопки "OK"
        private void buttonOk_Click(object sender, EventArgs e)
        {
            string query = "";              // Строка запроса к БД
            string emailMessage = "";       // Строка текста сообщения
            string emailSubject = "";       // Строка темы сообщения
            string emailReceivers = "";     // Строка, содержащая получателей сообщения
            int i = 0;

            // Номер столбца, с которого добавлять данные для запроса к БД
            // Нулевой столбец содержит ID task'а (если добавляем новый task, то ID не нужно передавать в запрос (БД сама сгенерирует новый уникальный ID))
            int startIndex = 0;

            // Если отсутствует подключение к БД, то выхожу из функции
            if (!myDBChildForm.isConnectToDb())
                return;

            // В зависимости от типа текущей операции выбираю дальнейшие действия
            switch (currentOperation)
            {
                // Добавление нового task'а
                case ((int)enumTypeOperation.addNewRow):
                    {
                        emailSubject = "In \"OCTAVIAN Task Planning\" create new task for you";
                        emailMessage = "In \"OCTAVIAN Task Planning\" create new task for you.\n\n\n";
                        emailMessage += "Description of created task:\n";
                        emailMessage += "--------------------------------------------------\n";

                        query = "CALL addDataToTableTasks (";           // Вызов хранимой процедуры addDataToTableTasks( ... )
                        startIndex = 1;

                        break;
                    }

                // Редактирование имеющегося task'а
                case ((int)enumTypeOperation.editSelectedRow):
                    {
                        emailSubject = "In \"OCTAVIAN Task Planning\" change description of task, where you owner";
                        emailMessage = "In \"OCTAVIAN Task Planning\" change description of task, where you owner.\n\n\n";
                        emailMessage += "Modified task description:\n";
                        emailMessage += "--------------------------------------------------\n";

                        query = "CALL updateDataInTableTasks (";        // Вызов хранимой процедуры updateDataInTableTasks( ... )
                        startIndex = 0;

                        break;
                    }
            }

            // Перебираю все столбцы со столбца с индексом "startIndex" до столбца с именем "CurrentStatus"
            for (int count = startIndex; count <= (int)enumNamesColumnsMainTable.CurrentStatus; count++)
            {
                if ((dataGridViewAddData.Rows[0].Cells[count].Value != null) && (dataGridViewAddData.Rows[0].Cells[count].Value.ToString() != ""))
                {
                    // Если не столбец, содержащий владельцев task'а
                    if (count != (int)enumNamesColumnsMainTable.Owner)
                    {
                        // Если в строке имеются спецсимволы, то они будут заэкранированы функцией "MySqlHelper.EscapeString"
                        // Значение каждого поля заключаю в кавычки
                        query += "\"" + MySqlHelper.EscapeString(dataGridViewAddData.Rows[0].Cells[count].Value.ToString()) + "\",";
                    }
                    else
                    {
                        // В зависимости от типа текущей операции
                        switch (currentOperation)
                        {
                            // Если добавление нового task'а, то просто добавляю значение этой ячейки в запрос
                            case ((int)enumTypeOperation.addNewRow):
                                {
                                    query += "\"" + MySqlHelper.EscapeString(dataGridViewAddData.Rows[0].Cells[count].Value.ToString()) + "\",";

                                    break;
                                }

                            // Если редактирование имеющегося task'а, то нужно провести дополнительные действия, так как
                            // мог измениться список владельцев task'а. Следовательно, надо будет добавить в БД новых владельцев
                            // и удалить из БД лишних владельцев
                            case ((int)enumTypeOperation.editSelectedRow):
                                {
                                    createAddAndDeletedUsersList();         // Создаю списки с добавляемыми и удаляемыми владельцами task'ов

                                    // Добавляю в запрос новых владельцев task'а
                                    query += "\"";
                                    for (i = 0; i < newOwnersList.Count; i++)
                                        query += newOwnersList[i] + myDBChildForm.delimiter + "\n";

                                    // Добавляю в запрос удаляемых владельцев task'а
                                    query += "\",\"";
                                    for (i = 0; i < deletedOwnersList.Count; i++)
                                        query += deletedOwnersList[i] + myDBChildForm.delimiter + "\n";

                                    query += "\",";

                                    break;
                                }
                        }

                        emailReceivers = dataGridViewAddData.Rows[0].Cells[count].Value.ToString();     // Строка с получателями сообщения
                    }

                    emailMessage += dataGridViewAddData.Columns[count].HeaderText + ":     ";
                    emailMessage += dataGridViewAddData.Rows[0].Cells[count].Value.ToString();
                    emailMessage += "\n--------------------------------------------------\n";
                }
                else
                {
                    // Данные ячейки должны быть заполнены. Если не заполнены - вывожу сообщение об ошибке
                    if ((count == (int)enumNamesColumnsMainTable.StartTime) || (count == (int)enumNamesColumnsMainTable.Project) || (count == (int)enumNamesColumnsMainTable.Description) ||
                        (count == (int)enumNamesColumnsMainTable.Owner) || (count == (int)enumNamesColumnsMainTable.CurrentStatus))
                    {
                        MessageBox.Show("Column '" + dataGridViewAddData.Columns[count].Name.ToString() + "' is empty!" + "\nPlease insert data to this column!", "ERROR!");
                        return;
                    }
                    else
                    {
                        query += "\"\",";

                        emailMessage += dataGridViewAddData.Columns[count].HeaderText + ":     ";
                        if (dataGridViewAddData.Rows[0].Cells[count].Value != null)
                            emailMessage += dataGridViewAddData.Rows[0].Cells[count].Value.ToString();
                        else
                            emailMessage += "";
                        emailMessage += "\n--------------------------------------------------\n";
                    }
                }
            }

            // Добавляю в запрос значение времени последнего изменения
            query += "\"Time: " + DateTime.Now.ToString(stringFormatDateTime) + "\\r\\n";
            query += "User: " + myDBChildForm.currentDBUserName.fullName + "\")";

            // Выполняю запрос к БД
            myDBChildForm.insertDataToDB(query);

            // Если запрос был выполнен успешно
            if (myDBChildForm.stateLastQuery == (int)enumDbQueryStates.success)
            {
                // Добавляю email сообщение в очередь сообщений на отправку
                // При этом функцией "getEmailAddresOnUsersNames" получаю email адреса для владельцев task'а, содержащихся в переменной "emailReceivers"
                myEmailChild.addMessageToSendQueue(myDBChildForm.getEmailAddresOnUsersNames(emailReceivers), emailSubject, emailMessage);

                DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("File 'FormEditData.cs' method 'buttonOk_Click'.\n\n" + "Problem in insert data in database!\nCheck connection with database and try again!", "ERROR!");
            }
        }

        // Обработчик нажатия кнопки "Cancel"
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        // Событие, вызываемое при клике мышкой на ListBox
        private void ListBox_MouseClick(object sender, EventArgs e)
        {
            int numberColumnOwnerInDataGrid = -1;
            string textValue = "";

            // В DataGrid получаю индекс столбца с именем "myDBChildForm.nameColumnsMainTable[(int)enumNamesColumnsMainTable.Owner]"
            numberColumnOwnerInDataGrid = getColumnNumberOnColumnNameInDataGrid(myDBChildForm.nameColumnsMainTable[(int)enumNamesColumnsMainTable.Owner]);
            // Если в DataGrid нет такого столбца
            if (numberColumnOwnerInDataGrid < 0)
            {
                MessageBox.Show("File 'FormEditData.cs' method 'ListBox_MouseClick'.\n" + "'numberColumnOwnerInDataGrid' = " + numberColumnOwnerInDataGrid.ToString(), "Runtime error!");
                return;
            }

            // Очищаю список новых владельцев task'а
            newOwnersList.Clear();
            // Перебираю все выделенные строки в ListBox
            for (int count = 0; count < ownerListBox.SelectedIndices.Count; count++)
            {
                // Добавляю в список владельцев выделенную строку
                newOwnersList.Add(ownerListBox.SelectedItems[count].ToString());
                // Добавляю выделенную строку, после которой ставлю "myDBChildForm.delimiter"
                textValue += ownerListBox.SelectedItems[count].ToString() + myDBChildForm.delimiter + "\n";
            }

            // Значению столбца "Owner" в DataGrid присваиваю текст
            dataGridViewAddData.Rows[0].Cells[numberColumnOwnerInDataGrid].Value = textValue;
        }

        // Событие, вызываемое при изменении значения ячеек DataGrid
        private void dataGridViewAddData_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            int numberColumnCurrentStatusInDataGrid = -1;
            int numberColumnEndTimeInDataGrid = -1;

            // В DataGrid получаю индекс столбца с именем "myDBChildForm.nameColumnsMainTable[(int)enumNamesColumnsMainTable.CurrentStatus]"
            numberColumnCurrentStatusInDataGrid = getColumnNumberOnColumnNameInDataGrid(myDBChildForm.nameColumnsMainTable[(int)enumNamesColumnsMainTable.CurrentStatus]);
            // В DataGrid получаю индекс столбца с именем "myDBChildForm.nameColumnsMainTable[(int)enumNamesColumnsMainTable.EndTime]"
            numberColumnEndTimeInDataGrid = getColumnNumberOnColumnNameInDataGrid(myDBChildForm.nameColumnsMainTable[(int)enumNamesColumnsMainTable.EndTime]);

            // Если в DataGrid нет таких столбцов
            if ((numberColumnCurrentStatusInDataGrid < 0) || (numberColumnEndTimeInDataGrid < 0))
            {
                MessageBox.Show("File 'FormEditData.cs' method 'dataGridViewAddData_CellValueChanged'.\n" + "'numberColumnCurrentStatusInDataGrid' = " + numberColumnCurrentStatusInDataGrid.ToString() +
                    "; 'numberColumnDateTimeInDataGrid' = " + numberColumnEndTimeInDataGrid.ToString(), "Runtime error!");
                return;
            }
            
            // Если изменилось значение в ячейке "CurrentStatus"
            if (e.ColumnIndex == numberColumnCurrentStatusInDataGrid)
            {
                // Если текущее значение ячейки ...
                switch(dataGridViewAddData.Rows[e.RowIndex].Cells[numberColumnCurrentStatusInDataGrid].Value.ToString())
                {
                    // ... равно "Closed"
                    case "Closed":
                        {
                            // Присваиваю столбцу "EndTime" значение текущего времени в соответствии с форматом "stringFormatDateTime"
                            dataGridViewAddData.Rows[e.RowIndex].Cells[numberColumnEndTimeInDataGrid].Value = DateTime.Now.ToString(stringFormatDateTime);

                            break;
                        }

                    // ... равно "Opened"
                    case "Opened":
                        {
                            // Присваиваю столбцу "EndTime" пустую строку
                            dataGridViewAddData.Rows[e.RowIndex].Cells[numberColumnEndTimeInDataGrid].Value = "";

                            break;
                        }

                    default:
                        {
                            MessageBox.Show("File 'FormEditData.cs' method 'dataGridViewAddData_CellValueChanged'.\nInvalid Value 'dataGridViewAddData.Rows[" +
                                e.RowIndex.ToString() + "].Cells[" + numberColumnCurrentStatusInDataGrid.ToString() + "].Value' = " + dataGridViewAddData.Rows[e.RowIndex].Cells[numberColumnCurrentStatusInDataGrid].Value.ToString(), "Runtime error!");

                            break;
                        }
                }
            }
        }

        // Событие, вызываемое при клике мышкой в ячейках DataGrid
        private void dataGridViewAddData_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            Point location = new Point(0, 0);
            int numberColumnOwnerInDataGrid = -1;

            // Если нажата не левая кнопка мыши, то выход
            if (e.Button != System.Windows.Forms.MouseButtons.Left)
                return;

            if ((e.RowIndex < 0) || (e.ColumnIndex < 0))
                return;

            // В DataGrid получаю индекс столбца с именем "myDBChildForm.nameColumnsMainTable[(int)enumNamesColumnsMainTable.Owner]"
            numberColumnOwnerInDataGrid = getColumnNumberOnColumnNameInDataGrid(myDBChildForm.nameColumnsMainTable[(int)enumNamesColumnsMainTable.Owner]);
            // Если в DataGrid нет такого столбца
            if (numberColumnOwnerInDataGrid < 0)
            {
                MessageBox.Show("File 'FormEditData.cs' method 'dataGridViewAddData_CellMouseClick'.\n" + "'numberColumnOwnerInDataGrid' = " + numberColumnOwnerInDataGrid.ToString(), "Runtime error!");
                return;
            }

            // Если клик левой кнопкой мыши был над ячейкой "Owner"
            if (e.ColumnIndex == numberColumnOwnerInDataGrid)
            {
                ownerListBox.Visible = !ownerListBox.Visible;       // Инвертирую видимость ListBox

                // Если список отображается на форме
                if (ownerListBox.Visible)
                {
                    // Вычисляю позицию по y
                    location.Y = dataGridViewAddData.ColumnHeadersHeight + dataGridViewAddData.Rows[e.RowIndex].Height;
                    // Вычисляю позицию по x
                    location.X += dataGridViewAddData.TopLeftHeaderCell.Size.Width - 31;
                    for (int i = 0; i < numberColumnOwnerInDataGrid; i++)
                        location.X += dataGridViewAddData.Columns[i].Width;

                    ownerListBox.Location = location;
                }
            }
            else
                ownerListBox.Visible = false;                       // Делаю ListBox невидимым
        }

        // Событие, вызываемое при изменении ширины колонок DataGrid (необходимо для изменения позиции ListBox по горизонтали)
        private void dataGridViewAddData_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            Point location = new Point(0, ownerListBox.Location.Y);
            int numberColumnOwnerInDataGrid = -1;

            // Если ListBox невидим, то выход
            if ((dataGridViewAddData.Columns.Count < 10) || (!ownerListBox.Visible))
                return;

            // В DataGrid получаю индекс столбца с именем "myDBChildForm.nameColumnsMainTable[(int)enumNamesColumnsMainTable.Owner]"
            numberColumnOwnerInDataGrid = getColumnNumberOnColumnNameInDataGrid(myDBChildForm.nameColumnsMainTable[(int)enumNamesColumnsMainTable.Owner]);
            // Если в DataGrid нет такого столбца
            if (numberColumnOwnerInDataGrid < 0)
            {
                MessageBox.Show("File 'FormEditData.cs' method 'dataGridViewAddData_ColumnWidthChanged'.\n" + "'numberColumnOwnerInDataGrid' = " + numberColumnOwnerInDataGrid.ToString(), "Runtime error!");
                return;
            }

            // Вычисляю позицию по x
            location.X = dataGridViewAddData.TopLeftHeaderCell.Size.Width - 31;
            for (int i = 0; i < numberColumnOwnerInDataGrid; i++)
                location.X += dataGridViewAddData.Columns[i].Width;

            ownerListBox.Location = location;
            ownerListBox.Invalidate();              // Перерисовываю ListBox
        }

        // Событие, вызываемое при изменении высоты строки DataGrid (необходимо для изменения позиции ListBox по вертикали)
        private void dataGridViewAddData_RowHeightChanged(object sender, DataGridViewRowEventArgs e)
        {
            Point location = new Point(ownerListBox.Location.X, 0);

            // Если ListBox невидим, то выход
            if (!ownerListBox.Visible)
                return;

            // Вычисляю позицию по y
            location.Y = dataGridViewAddData.ColumnHeadersHeight + dataGridViewAddData.Rows[e.Row.Index].Height;
            ownerListBox.Location = location;
            ownerListBox.Invalidate();              // Перерисовываю ListBox
        }
    }
}
