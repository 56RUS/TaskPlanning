using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//using Microsoft.Win32;
using TaskPlanning;


namespace TaskPlanning
{
    enum enumStatesLockedReadData
    {
        unlocked,
        locked
    }

    enum enumCurrentOperations
    {
        addNewTask,
        editSelectedTask
    }

    public partial class Form1 : Form
    {
        private const long timerIntervalUpdateDataFromDB = 10000;
        private const long timerIntervalSendMessageFromQueue = 60000;


        private workWithDbClass myDB;
        
        private FormEditData myFormEditData;

        private FormConnectToDatabase myFormConnectToDb;

        private workWithEmailClass myEmail;

        private System.Timers.Timer myTimerUpdateDataFromDB;

        private System.Timers.Timer myTimerSendEmailFromQueue;

        //private RegistryKey myRegKey;

    //    int countTimer = 0;

        private int isLockedReadData;

   //     private bool isRowDeleted;

        private int amountCustomQueryCheckedFields;


        private delegate void invokeDelegate();

        private SaveFileDialog mySaveFileDialog;


        private List<string[]> listTaskOwners;

      //  public delegate void SampleEventHandler();

       // private event SampleEventHandler myEvent;


        public Form1()
        {
            InitializeComponent();

            //groupBoxViewDataFromDB.Size = new System.Drawing.Size(this.);


            //this.Width = 1500;

            myEmail = new workWithEmailClass();

            labelLoginInfo.Text = "";

            isLockedReadData = 0;

            textBoxCustomQueryStartTime.Enabled = false;
            comboBoxCustomQueryProject.Enabled = false;
            comboBoxCustomQueryOwner.Enabled = false;
            comboBoxCustomQueryReporter.Enabled = false;
            comboBoxCustomQueryCurrentStatus.Enabled = false;


   //         isRowDeleted = false;

            amountCustomQueryCheckedFields = 0;


            listTaskOwners = new List<string[]>();
           // listTaskOwners.


           // setTextInDBTextfields();

            myDB = new workWithDbClass();
            
            myFormEditData = new FormEditData();

            myFormConnectToDb = new FormConnectToDatabase();
            myFormConnectToDb.initialiseDB(myDB);

            mySaveFileDialog = new SaveFileDialog();
            mySaveFileDialog.InitialDirectory = "C:\\";
            mySaveFileDialog.FileName = "Taskplanning database";
            mySaveFileDialog.OverwritePrompt = true;
        //    mySaveFileDialog.CheckFileExists = true;
            mySaveFileDialog.CheckPathExists = true;
            mySaveFileDialog.DefaultExt = "*.csv";
            mySaveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            mySaveFileDialog.FilterIndex = 1;
            mySaveFileDialog.FileOk += new CancelEventHandler(mySaveFileDialog_FileOk);

       //     mySaveFileDialog.Filter = "Text documents (.txt)|*.txt";
       //     mySaveFileDialog.Filter = "txt files (*.txt)|*.txt";//|All files (*.*)|*.*";

            


            myTimerUpdateDataFromDB = new System.Timers.Timer(timerIntervalUpdateDataFromDB);
            myTimerUpdateDataFromDB.Elapsed += new System.Timers.ElapsedEventHandler(timerElapsedUpdateDataFromDB);
            myTimerUpdateDataFromDB.Start();

            myTimerSendEmailFromQueue = new System.Timers.Timer(timerIntervalSendMessageFromQueue);
            myTimerSendEmailFromQueue.Elapsed += new System.Timers.ElapsedEventHandler(timerElapsedSendEmailFromQueue);
            myTimerSendEmailFromQueue.Start();


        //    this.myEvent += new SampleEventHandler(setDataFromDbToDataGrid);




            //myRegKey = Registry.CurrentUser.OpenSubKey("Software", true);

            //RegistryKey rk = myRegKey.CreateSubKey("TaskPlanningSettings");

            //rk.SetValue("test", "test");
            //rk.Close();

            //readUserSettings();

            //writeUserSettings();


            //myEmail.sendMessage("56rusya@gmail.com", "test send message");

        }

        /*
        private void setTextInDBTextfields()
        {
            //textBoxDBHost.Text = "192.168.92.5";
            //textBoxDBHost.Text = "localhost";
            //textBoxDBName.Text = "TaskPlanning";
            //textBoxDBUser.Text = "DBuser";
            //textBoxDBUser.Text = "root";
            //textBoxDBUserPswd.Text = "ledevop";
        }
        */

        private void buttonDBConnect_Click(object sender, EventArgs e)
        {
            /*
            myDB.dbHost = textBoxDBHost.Text;
            myDB.dbName = textBoxDBName.Text;
            myDB.dbUser = textBoxDBUser.Text;
            myDB.dbUserPassword = textBoxDBUserPswd.Text;

            if ((myDB.dbHost == "") || (myDB.dbName == "") || (myDB.dbUser == "") || (myDB.dbUserPassword == ""))
            {
                MessageBox.Show("Some required fields are empty! Please, enter data in this fields.", "Attention!");

                return;
            }
            */

            DialogResult myDialogResult;

            myDialogResult = myFormConnectToDb.ShowDialog();


           // if (myDB.connectionState == (int)enumDbConnectionStates.closed)
            {
           //     myDB.connectToDB();

                if (myDialogResult == DialogResult.OK)
                {

                    if (myDB.myConnection.State == System.Data.ConnectionState.Open)
                    {
                    //    textBoxDBHost.Enabled = false;
                   //     textBoxDBName.Enabled = false;
                    //    textBoxDBUser.Enabled = false;
                   //     textBoxDBUserPswd.Enabled = false;
                        buttonDBConnect.Enabled = false;
                        buttonDBDisconnect.Enabled = true;
                        groupBoxViewDataFromDB.Enabled = true;

                        myDB.currentProgramState = (int)enumProgramStates.activeMainForm;

                        setCurrentDbUserProperties();
                        myFormEditData.initialiseAttributes(myDB, myEmail);
                        setDataFromDbToDataGrid();
                    }
                }
            }
                /*
            else
            {
                MessageBox.Show("Connection is already established.", "Connect to database!");
            }
                 * */
        }

        private void buttonDBDisconnect_Click(object sender, EventArgs e)
        {
            if (myDB.myConnection.State == System.Data.ConnectionState.Open)
            {
                myDB.disconnectFromDB();

                disableFormControlsOnDisconnectedFromDb();

                /*
                if (myDB.myConnection.State == System.Data.ConnectionState.Closed)
                {
                    
                    buttonDBConnect.Enabled = true;
                    buttonDBDisconnect.Enabled = false;
                    groupBoxViewDataFromDB.Enabled = false;

                    labelLoginInfo.Text = "";
                    labelLoginInfo.ForeColor = Color.Black;
                    
                    dataGridViewDataFromDB.Columns.Clear();
                    dataGridViewDataFromDB.Rows.Clear();

                    myFormEditData.initialiseAttributes(null);
                    

                    disableFormControlsOnDisconnectedFromDb();

                }
                */
            }
        }

        private void disableFormControlsOnDisconnectedFromDb()
        {
            if (myDB.myConnection.State == System.Data.ConnectionState.Closed)
            {
                buttonDBConnect.Enabled = true;
                buttonDBDisconnect.Enabled = false;
                groupBoxViewDataFromDB.Enabled = false;

                labelLoginInfo.Text = "";
                labelLoginInfo.ForeColor = Color.Black;

                myDB.currentProgramState = (int)enumProgramStates.activeConnectToDbForm;
                    
                dataGridViewDataFromDB.Columns.Clear();
                dataGridViewDataFromDB.Rows.Clear();

                myFormEditData.initialiseAttributes(null, null);
            }
        }

        private void setCurrentDbUserProperties()
        {
            int numberIdInArray = -1;
            int numberGroupInArray = -1;
            int numberNameInArray = -1;


            numberIdInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableUsers, "ID");
            numberNameInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableUsers, "Name");
            numberGroupInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableUsers, "Group");

            if ((numberIdInArray >= 0) && (numberNameInArray >= 0) && (numberGroupInArray >= 0))
            {
                for (int i = 0; i < myDB.tableUsers.listValues.Count; i++)
                {
                    if (myDB.tableUsers.listValues[i][numberIdInArray].Equals(myDB.currentDBUserName.loginName))
                    {
                        /*
                        labelLoginInfo.Text = "You login as: \"";
                        labelLoginInfo.Text += myDB.tableUsers.listValues[i][numberNameInArray];
                        labelLoginInfo.Text += "\" (" + myDB.currentDBUserName.loginName + "), ";
                        labelLoginInfo.Text += "group \"" + myDB.tableUsers.listValues[i][numberGroupInArray] + "\".";

                        labelLoginInfo.ForeColor = Color.Green;
                        break;
                         */

                        myDB.currentDBUserName.fullName = myDB.tableUsers.listValues[i][numberNameInArray]; ;
                        myDB.currentDBUserName.group = myDB.tableUsers.listValues[i][numberGroupInArray];

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
                MessageBox.Show("File 'Form1.cs' method 'setTextInLoginInfoLabel'.\n'numberIdInArray' = " + numberIdInArray.ToString() +
                    " ;'numberNameInArray' = " + numberNameInArray.ToString() + " ;'numberGroupInArray' = " + numberGroupInArray.ToString(), "Runtime error!");
            }
        }

        private void buttonReadDataFromDB_Click(object sender, EventArgs e)
        {
            if (!myDB.isConnectToDb())
            {
                disableFormControlsOnDisconnectedFromDb();
                return;
            }

            setDataFromDbToDataGrid();
        }

        private void buttonAddNewRow_Click(object sender, EventArgs e)
        {
            if (!myDB.isConnectToDb())
            {
                disableFormControlsOnDisconnectedFromDb();
                return;
            }


      //      if (!myDB.currentDBUserName.group.Equals(myDB.nameGuestGroup))
            if (checkCanUserAddOrEditData((int)enumCurrentOperations.addNewTask))
            {
                myFormEditData.Text = "Add new row in database";
                myFormEditData.currentOperation = (int)enumTypeOperation.addNewRow;

                addNewOrEditRow();
            }
        }

        private void buttonEditSelectedRow_Click(object sender, EventArgs e)
        {
            if (!myDB.isConnectToDb())
            {
                disableFormControlsOnDisconnectedFromDb();
                return;
            }


           // if (!myDB.currentDBUserName.group.Equals(myDB.nameGuestGroup))
            {
                if (checkCanUserAddOrEditData((int)enumCurrentOperations.editSelectedTask))
                {
                    myFormEditData.Text = "Edit selected row";
                    myFormEditData.currentOperation = (int)enumTypeOperation.editSelectedRow;

                    addNewOrEditRow();
                }
            }
        }

        private void buttonDeleteSelectedRow_Click(object sender, EventArgs e)
        {
            int selectedRow = -1;
            int column = -1;
            string ID = "";
            string isCanEditRow = "";

            string emailReceiver = "";
            string emailMessage = "";
            string emailSubject = "";


            if (isLockedReadData == (int)enumStatesLockedReadData.locked)
                return;

            isLockedReadData = (int)enumStatesLockedReadData.locked;


            if (!myDB.isConnectToDb())
            {
                disableFormControlsOnDisconnectedFromDb();
                return;
            }


         //   if (!myDB.currentDBUserName.group.Equals(myDB.nameGuestGroup))
            {
                if (checkCanUserAddOrEditData((int)enumCurrentOperations.editSelectedTask))
                {
                    /*
                    try
                    {
                        selectedRow = dataGridViewDataFromDB.CurrentRow.Index;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("File 'Form1.cs' method 'buttonDeleteSelectedRow_Click'.\nCan't get number of selected row!" + ex.Message, "Runtime error!");
                        return;
                    }
                    */


                    // Если в базе еще нет ни одной записи
                    if (dataGridViewDataFromDB.Rows.Count == 0)
                        return;
                    else
                        selectedRow = dataGridViewDataFromDB.CurrentRow.Index;  // Получаю индекс выбранной строки


                    isCanEditRow = isCanEditSelectedRow(selectedRow);

                    if (isCanEditRow.Equals(""))
                        return;


                    DialogResult myDialogResult = MessageBox.Show("Are you realy want to delete row?", "Attention!", MessageBoxButtons.YesNo);

                    if (myDialogResult == DialogResult.Yes)
                    {
                        column = dataGridViewDataFromDB.Columns["ID"].Index;

                        if ((selectedRow >= 0) && (column >= 0))
                        {
                            emailSubject = "In \"OCTAVIAN Task Planning\" delete task, where you owner";

                            emailMessage = "In \"OCTAVIAN Task Planning\" delete task, where you owner.\n\n\n";
                            emailMessage += "Description of deleted task:\n";
                            emailMessage += "--------------------------------------------------\n";


                            for (int count = 0; count < dataGridViewDataFromDB.Columns.Count; count++)
                            {
                                if ((dataGridViewDataFromDB.Rows[selectedRow].Cells[count].Value != null) && (dataGridViewDataFromDB.Rows[selectedRow].Cells[count].Value.ToString() != ""))
                                {
                                    if (count == (int)enumNamesColumnsMainTable.Owner)
                                        emailReceiver = dataGridViewDataFromDB.Rows[selectedRow].Cells[count].Value.ToString();

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



                            ID = dataGridViewDataFromDB[column, selectedRow].Value.ToString();

                            string query = "CALL deleteDataFromTableTasks (" + ID + ")";

                            myDB.insertDataToDB(query);

                            if (myDB.stateLastQuery == (int)enumDbQueryStates.success)
                            {
                                //myFormEditData.generateEmailMessage(emailReceiver, emailSubject, emailMessage);

                                myEmail.addMessageToSendQueue(myDB.getEmailAddresOnUsersNames(emailReceiver), emailSubject, emailMessage);

                                setDataFromDbToDataGrid();
                            }
                            else
                            {
                                MessageBox.Show("File 'Form1.cs' method 'buttonDeleteSelectedRow_Click'.\n\n" + "Problem delete data from database!\nCheck connection with database and try again!", "ERROR!");
                            }
                        }
                        else
                        {
                            MessageBox.Show("File 'Form1.cs' method 'buttonDeleteSelectedRow_Click'.\n'row' = " + selectedRow.ToString() +
                                " ;'column' = " + column.ToString(), "Runtime error!");
                        }
                    }

                    if (myDialogResult == DialogResult.No)
                    {
                        if (!isCanEditRow.Equals(""))
                        {
                            // Разблокирую текущую строку
                            myDB.setLockToRow(isCanEditRow, 0);
                        }
                    }
                }
            }


            isLockedReadData = (int)enumStatesLockedReadData.unlocked;
            setDataFromDbToDataGrid();
        }

        private bool checkCanUserAddOrEditData(int currentOperation)
        {
            int i = 0;
            int numberColumnOwnerInDataGrid = -1;
            int numberColumnReporterInDataGrid = -1;
            int selectedRow = -1;// dataGridViewDataFromDB.CurrentRow.Index;
            List<string> taskOwnersList = new List<string>();

            // Проверяю, состоит ли пользователь, от имени которого было подключение к БД, членом группы "Guest"
            if (myDB.currentDBUserName.group.Equals(myDB.nameGuestGroup))
                return false;

            // Добавить новый таск может любой пользователь (кроме членов группы "Guest")
            if (currentOperation == (int)enumCurrentOperations.addNewTask)
                return true;

            // Проверяю, состоит ли пользователь, от имени которого было подключение к БД, членом группы "Administrator"
            if (myDB.currentDBUserName.group.Equals(myDB.nameAdminGroup))
                return true;

            // Если в базе еще нет ни одной записи
            if (dataGridViewDataFromDB.Rows.Count == 0)
                return true;
            else
                selectedRow = dataGridViewDataFromDB.CurrentRow.Index;  // Получаю индекс выбранной строки

            numberColumnOwnerInDataGrid = getColumnNumberOnColumnNameInDataGrid("Owner");
            numberColumnReporterInDataGrid = getColumnNumberOnColumnNameInDataGrid("Reporter");
            if ((numberColumnOwnerInDataGrid < 0) || (numberColumnReporterInDataGrid < 0) || (selectedRow < 0))
            {
                MessageBox.Show("File 'Form1.cs' method 'checkCanUserEditSelectedRow'.\n'numberOwnerInArray' = " + numberColumnOwnerInDataGrid.ToString() +
                    " ;'numberReporterInDataGrid' = " + numberColumnReporterInDataGrid.ToString() + " ;'selectedRow' = " + selectedRow.ToString(), "Runtime error!");
                return false;
            }

            // Проверяю, является ли пользователь, от имени которого было подключение к БД, reporter'ом данного таска
            if (myDB.currentDBUserName.fullName.Equals(dataGridViewDataFromDB.Rows[selectedRow].Cells[numberColumnReporterInDataGrid].Value.ToString()))
                return true;

            // Проверяю, является ли пользователь, от имени которого было подключение к БД, одним из owner'ов данного таска
            taskOwnersList = myDB.parseOwnersFromString(dataGridViewDataFromDB.Rows[selectedRow].Cells[numberColumnOwnerInDataGrid].Value.ToString());
            for (i = 0; i < taskOwnersList.Count; i++)
            {
                if (myDB.currentDBUserName.fullName.Equals(taskOwnersList[i]))
                    return true;
            }


            MessageBox.Show("You can't edit this row! Only owner(s), or reporter '" + dataGridViewDataFromDB.Rows[selectedRow].Cells["Reporter"].Value.ToString() +
                "', or any user consist in group 'Administrator' can edit this row. You login as '" + myDB.currentDBUserName.fullName +
                "' (" + myDB.currentDBUserName.loginName + ") and consist in group '" + myDB.currentDBUserName.group + "'.", "Permissions denied!");
            return false;
        }




        /*
        private bool checkCanUserEditSelectedRow()
        {
            int numberIdInArray = -1;
            int numberGroupInArray = -1;
            int numberNameInArray = -1;
            string idUserOwnerSelectedRow = "";
            string idUserReporterSelectedRow = "";
            int selectedRow = dataGridViewDataFromDB.CurrentRow.Index;


            numberIdInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableUsers, "ID");
            numberNameInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableUsers, "Name");
            numberGroupInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableUsers, "Group");

            if ((numberIdInArray >= 0) && (numberNameInArray >= 0) && (numberGroupInArray >= 0))
            {

                for (int i = 0; i < myDB.tableUsers.listValues.Count; i++)
                {
                    if (myDB.tableUsers.listValues[i][numberNameInArray].Equals(dataGridViewDataFromDB.Rows[selectedRow].Cells["Owner"].Value.ToString()))
                    {
                        idUserOwnerSelectedRow = myDB.tableUsers.listValues[i][numberIdInArray];
                        //break;
                    }

                    if (myDB.tableUsers.listValues[i][numberNameInArray].Equals(dataGridViewDataFromDB.Rows[selectedRow].Cells["Reporter"].Value.ToString()))
                    {
                        idUserReporterSelectedRow = myDB.tableUsers.listValues[i][numberIdInArray];
                        //break;
                    }
                }

                if (myDB.currentDBUserName.loginName.Equals(idUserOwnerSelectedRow) || myDB.currentDBUserName.loginName.Equals(idUserReporterSelectedRow))
                    return true;


                if (myDB.currentDBUserName.group.Equals(myDB.nameAdminGroup))
                    return true;
            }
            else
            {
                MessageBox.Show("File 'Form1.cs' method 'checkCanUserEditSelectedRow'.\n'numberIdInArray' = " + numberIdInArray.ToString() +
                    " ;'numberNameInArray' = " + numberNameInArray.ToString() + " ;'numberGroupInArray' = " + numberGroupInArray.ToString(), "Runtime error!");
            }

            MessageBox.Show("You can't edit this row! Only owner '" + dataGridViewDataFromDB.Rows[selectedRow].Cells["Owner"].Value.ToString() +
                "', or reporter '" + dataGridViewDataFromDB.Rows[selectedRow].Cells["Reporter"].Value.ToString() +
                "', or any user consist in group 'Administrator' can edit this row. You login as '" + myDB.currentDBUserName.fullName +
                "' (" + myDB.currentDBUserName.loginName + ") and consist in group '" + myDB.currentDBUserName.group + "'.", "Permissions denied!");


            return false;
        }
        */




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


            if (myFormEditData.currentOperation == (int)enumTypeOperation.editSelectedRow)
            {
                selectedRow = dataGridViewDataFromDB.CurrentRow.Index;
                isCanEditRow = isCanEditSelectedRow(selectedRow);

                if (isCanEditRow.Equals(""))
                    return;
            }

            for (int count = 0; count < amountColumns; count++)
            {
                namesColumns[count] = dataGridViewDataFromDB.Columns[count].Name;

                if (myFormEditData.currentOperation == (int)enumTypeOperation.addNewRow)
                {
                    switch (dataGridViewDataFromDB.Columns[count].Name)
                    {
                        case "Reporter":
                            {
                                numberIdInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableUsers, "ID");
                                numberNameInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableUsers, "Name");

                                if ((numberIdInArray >= 0) && (numberNameInArray >= 0))
                                {
                                    for (i = 0; i < myDB.tableUsers.listValues.Count; i++)
                                    {
                                        if (myDB.tableUsers.listValues[i][numberIdInArray].Equals(myDB.currentDBUserName.loginName))
                                        {
                                            valuesCells[count] = myDB.tableUsers.listValues[i][numberNameInArray];
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

                        case "StartTime":
                            {
                                valuesCells[count] = DateTime.Now.ToString(myFormEditData.stringFormatDateTime);
                                break;
                            }

                        case "CurrentStatus":
                            {
                                numberIdInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableTaskStates, "ID");
                                numberNameInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableTaskStates, "Name");

                                if ((numberIdInArray >= 0) && (numberNameInArray >= 0))
                                {
                                    for (i = 0; i < myDB.tableTaskStates.listValues.Count; i++)
                                    {
                                        if (myDB.tableTaskStates.listValues[i][numberIdInArray].Equals(myDB.idStateOpened))
                                        {
                                            valuesCells[count] = myDB.tableTaskStates.listValues[i][numberNameInArray];
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

                if (myFormEditData.currentOperation == (int)enumTypeOperation.editSelectedRow)
                {
                    valuesCells[count] = dataGridViewDataFromDB.Rows[selectedRow].Cells[count].Value.ToString();
                }
            }

            myFormEditData.setNamesForColumnsDataGrid(namesColumns);
            myFormEditData.addRowValuesForDataGrid(valuesCells);

            myFormEditData.ShowDialog();

            if (!isCanEditRow.Equals(""))
            {
                if (!myDB.isConnectToDb())
                {
                    disableFormControlsOnDisconnectedFromDb();
                    return;
                }

                // Разблокирую текущую строку
                myDB.setLockToRow(isCanEditRow, 0);

//                System.Threading.Thread.Sleep(5000);
            }

         //   setDataFromDbToDataGrid();
        }


        private string isCanEditSelectedRow(int selectedRow)
        {
            int numberIdInArray = -1;
            string idSelectedRow = "";
            string rowId = "";
            string query = "";
            string[] rowValues = { "" };

            rowId = dataGridViewDataFromDB.Rows[selectedRow].Cells["ID"].Value.ToString();
            query = "SELECT `c_IsLocked` FROM `Tasks` WHERE `c_ID` = \"" + rowId + "\"";
            myDB.readDataFromDB(query);

            if (myDB.stateLastQuery == (int)enumDbQueryStates.success)
            {
                while (myDB.isDataRead())
                    rowValues = myDB.getRowValues();

                if (rowValues[0] != "0")
                {
                    MessageBox.Show("You can't edit selected string because another user edit it at this time.", "Edit selected string!");
                    return "";
                }

                numberIdInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableUsers, "ID");
                if (numberIdInArray >= 0)
                {
                    idSelectedRow = dataGridViewDataFromDB.Rows[selectedRow].Cells[numberIdInArray].Value.ToString();

                    // Блокирую текущую строку
                    myDB.setLockToRow(idSelectedRow, 1);
                    return idSelectedRow;
                }
                else
                {
                    MessageBox.Show("File 'Form1.cs' method 'addNewOrEditRow'.\n'numberIdInArray' = " + numberIdInArray.ToString(), "Runtime error!");
                    
                }
            }
            else
            {
                MessageBox.Show("File 'FormEditData.cs' method 'buttonOk_Click'.\n\n" + "Problem in insert data in database!\nCheck connection with database and try again!", "ERROR!");
                return "";
            }

            return "";
        }
        


        private void setDataFromDbToDataGrid()
        {
            string query = "";// "SELECT * FROM " + workWithDbClass.nameTableTasks;//nameMainTableInDB;
            string[] columnNames;
            string[] rowValues;
            int numberRow = -1;
            //string taskId = "";
            int numberColumnIsLocked = -1;

            string taskState = "";



            if (isLockedReadData == (int)enumStatesLockedReadData.locked)
                return;


            isLockedReadData = (int)enumStatesLockedReadData.locked;

            myDB.readUsersProjectsTaskStatusFromDB();


            query = generateQuerySelectDataFromDB();

            myDB.readDataFromDB(query);

            if (myDB.stateLastQuery == (int)enumDbQueryStates.success)
            {
                dataGridViewDataFromDB.Columns.Clear();
                dataGridViewDataFromDB.Rows.Clear();
                
            //    for (int i = 0; i < listTaskOwners.Length; i++)
            //        listTaskOwners[i].Clear();

                columnNames = myDB.getColumnsNames();

                for (int count = 0; count < columnNames.Length; count++)
                {
                    dataGridViewDataFromDB.Columns.Add(columnNames[count], columnNames[count]);
                    dataGridViewDataFromDB.Columns[count].Width = myDB.columnsWidth[count];
                }


                while (myDB.isDataRead())
                {
                    rowValues = myDB.getRowValues();

                    dataGridViewDataFromDB.Rows.Add(rowValues);


     //               taskId = rowValues[(int)enumNamesColumnsMainTable.Id];
    //                getOwnersOnTaskId(taskId);

                    numberRow = dataGridViewDataFromDB.Rows.Count - 1;

                    for (int count = 0; count < myDB.nameColumnsGetNameOnID.Length; count++)
                        setNameOnId(numberRow, count);

     //               setOwnersToTask(numberRow);

                    //////////////////////////
                    taskState = rowValues[(int)enumNamesColumnsMainTable.CurrentStatus];

                    if (taskState.Equals(myDB.idStateClosed))
                    {
                        for (int count = 0; count < columnNames.Length; count++)
                            dataGridViewDataFromDB.Rows[dataGridViewDataFromDB.Rows.Count - 1].Cells[count].Style.BackColor = Color.LightGray;
                    }
                }

                setOwnersToTask();


                


                /////////////////////////////////////////////
                /*
                try
                {
                    dataGridViewDataFromDB.Columns["IsLocked"].ReadOnly = true;
                    dataGridViewDataFromDB.Columns["IsLocked"].Visible = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("File 'Form1.cs' method 'setDataFromDbToDataGrid'.\n" + "Can't found column with name 'IsLocked' in current table.\n\n" + ex.Message, "Runtime error!");
                }
                */



                numberColumnIsLocked = getColumnNumberOnColumnNameInDataGrid(myDB.nameColumnsMainTable[(int)enumNamesColumnsMainTable.IsLocked]);

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

                //

                if (!myDB.currentDBUserName.group.Equals(myDB.nameGuestGroup))
                {
                    if (dataGridViewDataFromDB.Rows.Count > 0)
                    {
                        // Выбираю столбец, по которому будут сортироваться данные и тип сортировки
                        dataGridViewDataFromDB.Sort(dataGridViewDataFromDB.Columns["StartTime"], ListSortDirection.Descending);

                        buttonEditSelectedRow.Enabled = true;
                        buttonDeleteSelectedRow.Enabled = true;
                        buttonExportData.Enabled = true;
                    }
                    else
                    {
                        buttonEditSelectedRow.Enabled = false;
                        buttonDeleteSelectedRow.Enabled = false;
                        buttonExportData.Enabled = false;
                    }
                }
                else
                {
                    if (dataGridViewDataFromDB.Rows.Count > 0)
                    {
                        // Выбираю столбец, по которому будут сортироваться данные и тип сортировки
                        dataGridViewDataFromDB.Sort(dataGridViewDataFromDB.Columns["StartTime"], ListSortDirection.Descending);

                        buttonExportData.Enabled = true;
                    }
                    else
                    {
                        buttonExportData.Enabled = false;
                    }

                    buttonAddNewRow.Enabled = false;
                    buttonEditSelectedRow.Enabled = false;
                    buttonDeleteSelectedRow.Enabled = false;
                }

                
            }

            

            isLockedReadData = (int)enumStatesLockedReadData.unlocked;
          //  }
        }


        private void setOwnersToTask()//int numberRow)//, string[] taskOwners)
        {
            string query = "";
            string rowID = "";
            int numberColumnId = -1;
            int numberColumnOwner = -1;
            List<string> taskOwners = new List<string>();


            numberColumnId = getColumnNumberOnColumnNameInDataGrid(myDB.nameColumnsMainTable[(int)enumNamesColumnsMainTable.Id]);
            numberColumnOwner = getColumnNumberOnColumnNameInDataGrid(myDB.nameColumnsMainTable[(int)enumNamesColumnsMainTable.Owner]);

            if ((numberColumnId < 0) || (numberColumnOwner < 0))
            {
                MessageBox.Show("File 'Form1.cs' method 'setOwnersToTask'.\n'numberColumnId' = " + numberColumnId.ToString() +
                                        " ;'numberColumnOwner' = " + numberColumnOwner.ToString(), "Runtime error!");
                return;
            }

            for (int numberRow = 0; numberRow < dataGridViewDataFromDB.Rows.Count; numberRow++)
            {
                rowID = dataGridViewDataFromDB.Rows[numberRow].Cells[numberColumnId].Value.ToString();

                query = "CALL getOwnersOnTaskId (" + rowID + ")";


                myDB.readDataFromDB(query);

                if (myDB.stateLastQuery == (int)enumDbQueryStates.success)
                {
                    taskOwners.Clear();

                    while (myDB.isDataRead())
                    {
                        taskOwners.Add(myDB.getRowValues()[0]);
                    }

                    dataGridViewDataFromDB.Rows[numberRow].Cells[numberColumnOwner].Value = "";

                    for (int count = 0; count < taskOwners.Count; count++)
                     dataGridViewDataFromDB.Rows[numberRow].Cells[numberColumnOwner].Value += taskOwners[count] + myDB.delimiter + "\n";
                }
            }
        }


        private void setNameOnId(int numberRow, int columnIndex)
        {

            int numberColumn = -1;
            int numberIdInArray = -1;
            int numberNameInArray = -1;
            int i = 0;

            structTable currentTable = new structTable();

            switch (columnIndex)
            {
                case ((int)enumNameColumnsGetNameOnID.Project):
                    {
                        currentTable = myDB.tableProjects;
                        break;
                    }

                //case ((int)enumNameColumnsGetNameOnID.Owner):
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




            /*
            try
            {
                numberColumn = dataGridViewDataFromDB.Columns[myDB.nameColumnsGetNameOnID[columnIndex]].Index;
            }
            catch (Exception ex)
            {
                MessageBox.Show("File 'Form1.cs' method 'setNameOnId'.\nInvalid index 'columnIndex' = " + columnIndex + ".\n\n" + ex.Message, "Runtime error!");

                return;
            }
            */


            numberColumn = getColumnNumberOnColumnNameInDataGrid(myDB.nameColumnsGetNameOnID[columnIndex]);

            if (numberColumn < 0)
            {
                MessageBox.Show("File 'Form1.cs' method 'setNameOnId'.\n'numberColumn' = " + numberColumn.ToString(), "Runtime error!");
                return;
            }
            




            if ((currentTable.columnNames != null) && (currentTable.columnNames.Length > 0))
            {
                numberIdInArray = myDB.getColumnIndexOnColumnNameInTable(currentTable, "ID");
                numberNameInArray = myDB.getColumnIndexOnColumnNameInTable(currentTable, "Name");

                if ((numberRow >= 0) && (numberColumn >= 0) && (numberIdInArray >= 0) && (numberNameInArray >= 0))
                {
                    for (i = 0; i < currentTable.listValues.Count; i++)
                    {
                        if (currentTable.listValues[i][numberIdInArray].Equals(dataGridViewDataFromDB.Rows[numberRow].Cells[numberColumn].Value.ToString()))
                        {
                            dataGridViewDataFromDB.Rows[numberRow].Cells[numberColumn].Value = currentTable.listValues[i][numberNameInArray];

                            break;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("File 'Form1.cs' method 'setNameOnId'.\n'numberRow' = " + numberRow.ToString() + "; 'numberColumn' = "
                        + numberColumn.ToString() + " ;'numberIdInArray' = " + numberIdInArray.ToString() + " ;'numberNameInArray' = " + numberNameInArray.ToString(), "Runtime error!");
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

        /*
        private void getOwnersOnTaskId(string taskId)
        {
            string query = "CALL updateDataInTableTasks (" + taskId + ")";

          //  string[] nameOwners;

            int listLength = -1;

            List<string[]> nameOwners = new List<string[]>();

         //   listLength = listTaskOwners.Count;

            //listTaskOwners.a

            myDB.readDataFromDB(query);

            if (myDB.stateLastQuery == (int)enumDbQueryStates.success)
            {
               // listTaskOwners.
                while (myDB.isDataRead())
                    nameOwners.Add(myDB.getRowValues());
                 //   nameOwners.Concat<string[]>(myDB.getRowValues());

              //  listTaskOwners.Add(nameOwners);
            }
        }
        */

        private void dataGridViewDataFromDB_DoubleClick(object sender, EventArgs e)
        {
            MouseEventArgs myMouseButton = (MouseEventArgs)e;


            if (!myDB.isConnectToDb())
            {
                disableFormControlsOnDisconnectedFromDb();
                return;
            }


          //  if (!myDB.currentDBUserName.group.Equals(myDB.nameGuestGroup))
        //    if (checkCanUserAddOrEditData())
            {
                switch (myMouseButton.Button)
                {
                    case MouseButtons.Left:
                        {
                            if (buttonEditSelectedRow.Enabled == true)
                            {
                                if (checkCanUserAddOrEditData((int)enumCurrentOperations.editSelectedTask))
                                {
                                    myFormEditData.Text = "Edit selected task";
                                    myFormEditData.currentOperation = (int)enumTypeOperation.editSelectedRow;
                                    addNewOrEditRow();
                                }
                               /* else
                                {
                                }*/
                            }

                            break;
                        }

                    case MouseButtons.Right:
                        {
                            if (checkCanUserAddOrEditData((int)enumCurrentOperations.addNewTask))
                            {
                                myFormEditData.Text = "Add new task in database";
                                myFormEditData.currentOperation = (int)enumTypeOperation.addNewRow;
                                addNewOrEditRow();
                            }

                            break;
                        }
                }
            }
        }


        private void timerElapsedUpdateDataFromDB(object sender, System.Timers.ElapsedEventArgs e)
        {
         //   /*

            if (checkBoxAutoupdateData.Checked)
            {
                if ((myDB.myConnection.State == System.Data.ConnectionState.Open) && (myDB.stateLastQuery == (int)enumDbQueryStates.success))
                {

                    if (!myDB.isConnectToDb())
                    {
                        BeginInvoke(new invokeDelegate(disableFormControlsOnDisconnectedFromDb));
                        return;
                    }


                    if (myFormEditData.currentOperation == (int)enumTypeOperation.none)
                    {
                        BeginInvoke(new invokeDelegate(setDataFromDbToDataGrid));

                        //      BeginInvoke(new invokeDelegate(changeButtonName));
                    }
                }
            }
         //   */
        }





        private void timerElapsedSendEmailFromQueue(object sender, System.Timers.ElapsedEventArgs e)
        {
            BeginInvoke(new invokeDelegate(myEmail.sendMessage));
        }




        /*
        public void changeButtonName()
        {
            buttonAddNewRow.Text = "set text from timer " + countTimer.ToString();

            countTimer++;
        }
        */

        private void Form1_Activated(object sender, EventArgs e)
        {
            
            if ((myDB.myConnection.State == System.Data.ConnectionState.Open) && (myDB.stateLastQuery == (int)enumDbQueryStates.success))
            {
                if (!myDB.isConnectToDb())
                {
                    disableFormControlsOnDisconnectedFromDb();
                    return;
                }

                setDataFromDbToDataGrid();
            }
            
        }

        private int getColumnNumberOnColumnNameInDataGrid(string name)
        {
            int number = -1;

            /*
            try
            {
                number = dataGridViewDataFromDB.Columns[name].Index;
            }
            catch (Exception ex)
            {
                MessageBox.Show("File 'Form1.cs' method 'getColumnNumberOnColumnName'.\n" + "Can't found column with name '" + name + "' in DataGrid.\n\n" + ex.Message, "Runtime error!");
            }
            */

            if (dataGridViewDataFromDB.Columns.Contains(name))
                number = dataGridViewDataFromDB.Columns[name].Index;
            else
                MessageBox.Show("File 'Form1.cs' method 'getColumnNumberOnColumnName'.\n" + "Can't found column with name '" + name + "' in DataGrid.", "Runtime error!");

            return number;
        }

        private string generateQuerySelectDataFromDB()
        {
            string query = "";
            string id = "";
            int countCheckBoxes = amountCustomQueryCheckedFields;

            int numberIdInArray = -1;
            int numberNameInArray = -1;
            int i = 0;


        //    numberColumnName = myDB.getColumnIndexOnColumnNameInTable(myDB.tableUsers, "Name");
        //    numberGroupInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableUsers, "Group");

            bool isNeedAddAND = false;
            bool isNeedAddWHERE = true;

            query = "SELECT * FROM " + workWithDbClass.nameTableTasks;

        //    if (!checkBoxCustomQueryStartTime.Checked && !checkBoxCustomQueryProject.Checked && !checkBoxCustomQueryOwner.Checked
        //        && !checkBoxCustomQueryReporter.Checked && !checkBoxCustomQueryCurrentStatus.Checked)
           
            /*if (amountCustomQueryCheckedFields == 0)
            {
                query = "SELECT * FROM " + workWithDbClass.nameTableTasks;
            }
            */

          //  if ((amountCustomQueryCheckedFields > 0) && ((textBoxCustomQueryStartTime.Text.Length > 0) || (comboBoxCustomQueryProject.Text.Length > 0)
          //      || (comboBoxCustomQueryOwner.Text.Length > 0) || (comboBoxCustomQueryReporter.Text.Length > 0) || (comboBoxCustomQueryCurrentStatus.Text.Length > 0)))
            if (amountCustomQueryCheckedFields > 0)
            {
                //query = "SELECT * FROM " + workWithDbClass.nameTableTasks + " WHERE (";

                if (checkBoxCustomQueryStartTime.Checked)
                {
                    if (textBoxCustomQueryStartTime.Text.Length > 0)
                    {
                        if (isNeedAddWHERE)
                        {
                            query += " WHERE (";
                            isNeedAddWHERE = false;
                        }

                        query += workWithDbClass.nameTableTasks + ".c_StartTime LIKE '" + textBoxCustomQueryStartTime.Text + "%'";
                    }

                    isNeedAddAND = true;
                }

                if (checkBoxCustomQueryProject.Checked)
                {
                    if (comboBoxCustomQueryProject.Text.Length > 0)
                    {
                        numberIdInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableProjects, "ID");
                        numberNameInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableProjects, "Name");

                        for (i = 0; i < myDB.tableProjects.listValues.Count; i++)
                        {
                            if (myDB.tableProjects.listValues[i][numberNameInArray].Equals(comboBoxCustomQueryProject.Text))
                            {
                                id = myDB.tableProjects.listValues[i][numberIdInArray];
                                break;
                            }
                        }

                        if (isNeedAddWHERE)
                        {
                            query += " WHERE (";
                            isNeedAddWHERE = false;
                        }

                        if (isNeedAddAND)
                            query += " AND ";

                        //    if (checkBoxCustomQueryProject.Text.Length > 0)
                        query += workWithDbClass.nameTableTasks + ".c_Project = " + "\"" + id + "\"";

                        isNeedAddAND = true;
                    }
                }

                if (checkBoxCustomQueryOwner.Checked)
                {
                    if (comboBoxCustomQueryOwner.Text.Length > 0)
                    {
                        numberIdInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableUsers, "ID");
                        numberNameInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableUsers, "Name");

                        for (i = 0; i < myDB.tableUsers.listValues.Count; i++)
                        {
                            if (myDB.tableUsers.listValues[i][numberNameInArray].Equals(comboBoxCustomQueryOwner.Text))
                            {
                                id = myDB.tableUsers.listValues[i][numberIdInArray];
                                break;
                            }
                        }

                        // WHERE (tasks.c_ID IN (SELECT c_TaskID FROM ownersoftask WHERE c_OwnerID = "b.boichenko"))

                        if (isNeedAddWHERE)
                        {
                            query += " WHERE (";
                            isNeedAddWHERE = false;
                        }

                        if (isNeedAddAND)
                            query += " AND ";

                        //if (checkBoxCustomQueryOwner.Text.Length > 0)
                        query += workWithDbClass.nameTableTasks + ".c_ID IN (SELECT c_TaskID FROM ownersoftask WHERE c_OwnerID = " + "\"" + id + "\")";

                        isNeedAddAND = true;
                    }
                }

                if (checkBoxCustomQueryReporter.Checked)
                {
                    if (comboBoxCustomQueryReporter.Text.Length > 0)
                    {
                        numberIdInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableUsers, "ID");
                        numberNameInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableUsers, "Name");

                        for (i = 0; i < myDB.tableUsers.listValues.Count; i++)
                        {
                            if (myDB.tableUsers.listValues[i][numberNameInArray].Equals(comboBoxCustomQueryReporter.Text))
                            {
                                id = myDB.tableUsers.listValues[i][numberIdInArray];
                                break;
                            }
                        }

                        if (isNeedAddWHERE)
                        {
                            query += " WHERE (";
                            isNeedAddWHERE = false;
                        }

                        if (isNeedAddAND)
                            query += " AND ";

                        //   if (checkBoxCustomQueryReporter.Text.Length > 0)
                        query += workWithDbClass.nameTableTasks + ".c_Reporter = " + "\"" + id + "\"";

                        isNeedAddAND = true;
                    }
                }

                if (checkBoxCustomQueryCurrentStatus.Checked)
                {
                    if (comboBoxCustomQueryCurrentStatus.Text.Length > 0)
                    {
                        numberIdInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableTaskStates, "ID");
                        numberNameInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableTaskStates, "Name");

                        for (i = 0; i < myDB.tableTaskStates.listValues.Count; i++)
                        {
                            if (myDB.tableTaskStates.listValues[i][numberNameInArray].Equals(comboBoxCustomQueryCurrentStatus.Text))
                            {
                                id = myDB.tableTaskStates.listValues[i][numberIdInArray];
                                break;
                            }
                        }

                        if (isNeedAddWHERE)
                        {
                            query += " WHERE (";
                            isNeedAddWHERE = false;
                        }

                        if (isNeedAddAND)
                            query += " AND ";

                        //if (checkBoxCustomQueryCurrentStatus.Text.Length > 0)
                        query += workWithDbClass.nameTableTasks + ".c_CurrentStatus = " + "\"" + id + "\"";

                        isNeedAddAND = true;
                    }
                }

                if (!isNeedAddWHERE)
                    query += ")";
            }
         /*   else
            {
                query = "SELECT * FROM " + workWithDbClass.nameTableTasks;
            }*/

            return query;
        }

        private void checkBoxCustomQueryStartTime_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxCustomQueryStartTime.Checked)
            {
                textBoxCustomQueryStartTime.Enabled = true;
                amountCustomQueryCheckedFields++;

                if (textBoxCustomQueryStartTime.Text.Length == 0)
                    textBoxCustomQueryStartTime.Text = DateTime.Now.ToString(myFormEditData.stringFormatDateTime.Substring(0, 10));
            }
            else
            {
                textBoxCustomQueryStartTime.Enabled = false;
                amountCustomQueryCheckedFields--;

           //     setDataFromDbToDataGrid();
            }

            setDataFromDbToDataGrid();
        }

        private void checkBoxCustomQueryProject_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxCustomQueryProject.Checked)
            {
                comboBoxCustomQueryProject.Enabled = true;
                amountCustomQueryCheckedFields++;
            }
            else
            {
                comboBoxCustomQueryProject.Enabled = false;
                amountCustomQueryCheckedFields--;

       //         setDataFromDbToDataGrid();
            }

            setDataFromDbToDataGrid();
        }

        private void checkBoxCustomQueryOwner_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxCustomQueryOwner.Checked)
            {
                comboBoxCustomQueryOwner.Enabled = true;
                amountCustomQueryCheckedFields++;
            }
            else
            {
                comboBoxCustomQueryOwner.Enabled = false;
                amountCustomQueryCheckedFields--;

         //       setDataFromDbToDataGrid();
            }

            setDataFromDbToDataGrid();
        }

        private void checkBoxCustomQueryReporter_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxCustomQueryReporter.Checked)
            {
                comboBoxCustomQueryReporter.Enabled = true;
                amountCustomQueryCheckedFields++;
            }
            else
            {
                comboBoxCustomQueryReporter.Enabled = false;
                amountCustomQueryCheckedFields--;

         //       setDataFromDbToDataGrid();
            }

            setDataFromDbToDataGrid();
        }

        private void checkBoxCustomQueryCurrentStatus_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxCustomQueryCurrentStatus.Checked)
            {
                comboBoxCustomQueryCurrentStatus.Enabled = true;
                amountCustomQueryCheckedFields++;
            }
            else
            {
                comboBoxCustomQueryCurrentStatus.Enabled = false;
                amountCustomQueryCheckedFields--;

              //  setDataFromDbToDataGrid();
            }

            setDataFromDbToDataGrid();
        }

        private void comboBoxCustomQueryProject_MouseClick(object sender, MouseEventArgs e)
        {
            int numberColumnName = -1;

            if (!myDB.isConnectToDb())
                return;

            numberColumnName = myDB.getColumnIndexOnColumnNameInTable(myDB.tableProjects, "Name");
            if (numberColumnName < 0)
                return;

            myDB.readUsersProjectsTaskStatusFromDB();
            comboBoxCustomQueryProject.Items.Clear();

            for (int i = 0; i < myDB.tableProjects.listValues.Count; i++)
                comboBoxCustomQueryProject.Items.Add(myDB.tableProjects.listValues[i][numberColumnName].ToString());
        }

        private void comboBoxCustomQueryOwner_MouseClick(object sender, MouseEventArgs e)
        {
            int numberColumnName = -1;
            int numberGroupInArray = -1;

            if (!myDB.isConnectToDb())
                return;

            numberColumnName = myDB.getColumnIndexOnColumnNameInTable(myDB.tableUsers, "Name");
            numberGroupInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableUsers, "Group");

            if ((numberColumnName < 0) || (numberGroupInArray < 0))
                return;

            myDB.readUsersProjectsTaskStatusFromDB();
            comboBoxCustomQueryOwner.Items.Clear();

            for (int i = 0; i < myDB.tableUsers.listValues.Count; i++)
            {
                // Исключаю из списка пользователей из группы "Guest"
                if (!myDB.tableUsers.listValues[i][numberGroupInArray].Equals(myDB.nameGuestGroup))
                    comboBoxCustomQueryOwner.Items.Add(myDB.tableUsers.listValues[i][numberColumnName].ToString());
            }
        }

        private void comboBoxCustomQueryReporter_MouseClick(object sender, MouseEventArgs e)
        {
            int numberColumnName = -1;
            int numberGroupInArray = -1;

            if (!myDB.isConnectToDb())
                return;

            numberColumnName = myDB.getColumnIndexOnColumnNameInTable(myDB.tableUsers, "Name");
            numberGroupInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableUsers, "Group");

            if ((numberColumnName < 0) || (numberGroupInArray < 0))
                return;

            myDB.readUsersProjectsTaskStatusFromDB();
            comboBoxCustomQueryReporter.Items.Clear();

            for (int i = 0; i < myDB.tableUsers.listValues.Count; i++)
            {
                // Исключаю из списка пользователей из группы "Guest"
                if (!myDB.tableUsers.listValues[i][numberGroupInArray].Equals(myDB.nameGuestGroup))
                    comboBoxCustomQueryReporter.Items.Add(myDB.tableUsers.listValues[i][numberColumnName].ToString());
            }
        }

        private void comboBoxCustomQueryCurrentStatus_MouseClick(object sender, MouseEventArgs e)
        {
            int numberColumnName = -1;

            if (!myDB.isConnectToDb())
                return;

            numberColumnName = myDB.getColumnIndexOnColumnNameInTable(myDB.tableTaskStates, "Name");
            if (numberColumnName < 0)
                return;

            myDB.readUsersProjectsTaskStatusFromDB();
            comboBoxCustomQueryCurrentStatus.Items.Clear();

            for (int i = 0; i < myDB.tableTaskStates.listValues.Count; i++)
                comboBoxCustomQueryCurrentStatus.Items.Add(myDB.tableTaskStates.listValues[i][numberColumnName].ToString());
        }

        private void comboBoxCustomQueryProject_SelectedValueChanged(object sender, EventArgs e)
        {
            setDataFromDbToDataGrid();
        }

        private void comboBoxCustomQueryOwner_SelectedValueChanged(object sender, EventArgs e)
        {
            setDataFromDbToDataGrid();
        }

        private void comboBoxCustomQueryReporter_SelectedValueChanged(object sender, EventArgs e)
        {
            setDataFromDbToDataGrid();
        }

        private void comboBoxCustomQueryCurrentStatus_SelectedValueChanged(object sender, EventArgs e)
        {
            setDataFromDbToDataGrid();
        }

        private void buttonExportData_Click(object sender, EventArgs e)
        {
          //  DialogResult mySaveFileDialogResult;

            mySaveFileDialog.ShowDialog();

          //  if ()
        }

        private void mySaveFileDialog_FileOk(object obj, CancelEventArgs e)
        {
            string pathToSaveFile = mySaveFileDialog.FileName;
            int amountSavedColumns = -1;
            int column = 0;

            amountSavedColumns = getColumnNumberOnColumnNameInDataGrid(myDB.nameColumnsMainTable[(int)enumNamesColumnsMainTable.AmountChanges]);
            if (amountSavedColumns < 0)
            {
                MessageBox.Show("File 'Form1.cs' method 'mySaveFileDialog_FileOk'.\n" + "'amountSavedColumns' = " + amountSavedColumns.ToString(), "Runtime error!");
                return;
            }

            try
            {
                using (StreamWriter myStreamWrite = new StreamWriter(pathToSaveFile, false, Encoding.UTF8))
                {
                    for (column = 0; column < amountSavedColumns; column++)
                        myStreamWrite.Write("\"" + dataGridViewDataFromDB.Columns[column].Name.ToString() + "\";");

                    myStreamWrite.WriteLine();

                    for (int row = 0; row < dataGridViewDataFromDB.Rows.Count; row++)
                    {
                        for (column = 0; column < amountSavedColumns; column++)
                            myStreamWrite.Write("\"" + dataGridViewDataFromDB.Rows[row].Cells[column].Value.ToString() + "\";");

                        myStreamWrite.WriteLine();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Runtime error!");
            }

        }

    }
}
