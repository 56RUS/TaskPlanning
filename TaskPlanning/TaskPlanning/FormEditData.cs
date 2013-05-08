using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using TaskPlanning;

namespace TaskPlanning
{
    enum enumTypeOperation
    {
        none,
        addNewRow,
        editSelectedRow
    }

    

    struct structDataGridComboBoxCell
    {
        public string name;
        public DataGridViewComboBoxCell cell;
    }

    public partial class FormEditData : Form
    {
        //private const int amountComboBoxCells = 4;

        //private const string nameColumnStartTime = "StartTime";
        private const string nameColumnEndTime = "EndTime";
        //private const string nameColumnReporter = "Reporter";
        private const string nameColumnCurrentStatus = "CurrentStatus";
        //public string stringFormatDateTime = "yyyy.MM.dd HH:mm:ss";
        public string stringFormatDateTime = "yyyy.MM.dd HH:mm:ss";

        private workWithDbClass myDBChildForm;

        private structDataGridComboBoxCell[] comboBoxCells;

        private workWithEmailClass myEmailChild;


        public int currentOperation;


        private ListBox ownerListBox;

        //public List<string> ownerListBox

        private int ownerListBoxRowHeight;


        List<string> currentTaskOwnersList;// = new List<string>();

        List<string> deletedOwnersList;

        List<string> newOwnersList;// = new List<string>();



        //private DateTime myDateTime;

        public FormEditData()
        {
            InitializeComponent();

            currentOperation = (int)enumTypeOperation.none;

            comboBoxCells = new structDataGridComboBoxCell[(int)enumNameColumnsWithComboBox.length];

           // myDateTime = new DateTime();

            newOwnersList = new List<string>();


          //  myEmailChild = new workWithEmailClass();

            for (int count = 0; count < (int)enumNameColumnsWithComboBox.length; count++)
                comboBoxCells[count].name = "";



            ownerListBox = new ListBox();
            ownerListBoxRowHeight = 13;

            ownerListBox.Location = new Point (100, 100);
            ownerListBox.SelectionMode = SelectionMode.MultiSimple;
            ownerListBox.HorizontalScrollbar = true;

            ownerListBox.SelectedIndexChanged += new EventHandler(ListBox_MouseClick);


            currentTaskOwnersList = new List<string>();


            deletedOwnersList = new List<string>();
            
            /*
            ownerListBox.Items.Add("1");
            ownerListBox.Items.Add("2");
            ownerListBox.Items.Add("3");
            ownerListBox.Items.Add("3");
            ownerListBox.Items.Add("3");
            */
            
            /*ownerListBox.Items.Add("3");
            ownerListBox.Items.Add("3");
            ownerListBox.Items.Add("3");
            ownerListBox.Items.Add("3");
            ownerListBox.Items.Add("3");
            ownerListBox.Items.Add("3");
            ownerListBox.Items.Add("3");
            ownerListBox.Items.Add("3");*/


            dataGridViewAddData.Controls.Add(ownerListBox);

        }


        private void buttonOk_Click(object sender, EventArgs e)
        {
            string query = "";
           // string secondQuery = "";

         //   string deletedOwners = "";
         //   string newOwners = "";

            string emailMessage = "";
            string emailSubject = "";
            string emailReceivers = "";
            int startIndex = 0;
            int i = 0;


            if (!myDBChildForm.isConnectToDb())
            {
                //disableFormControlsOnDisconnectedFromDb();
                return;
            }


            switch (currentOperation)
            {
                case ((int)enumTypeOperation.addNewRow):
                    {
                        emailSubject = "In \"OCTAVIAN Task Planning\" create new task for you";

                        emailMessage = "In \"OCTAVIAN Task Planning\" create new task for you.\n\n\n";
                        emailMessage += "Description of created task:\n";
                        emailMessage += "--------------------------------------------------\n";

                       // firstQuery = "CALL addDataToTableTasks (\"-1\",";
                        query = "CALL addDataToTableTasks (";
                        startIndex = 1;
                        //startIndex = 0;

                        break;
                    }

                case ((int)enumTypeOperation.editSelectedRow):
                    {
                        emailSubject = "In \"OCTAVIAN Task Planning\" change description of task, where you owner";

                        emailMessage = "In \"OCTAVIAN Task Planning\" change description of task, where you owner.\n\n\n";
                        emailMessage += "Modified task description:\n";
                        emailMessage += "--------------------------------------------------\n";

                        query = "CALL updateDataInTableTasks (";
                       // firstQuery = "CALL addDataToTableTasks (";
                        startIndex = 0;

                        break;
                    }
            }


            for (int count = startIndex; count <= (int)enumNamesColumnsMainTable.CurrentStatus/*dataGridViewAddData.Columns.Count*/; count++)
            {
                if ((dataGridViewAddData.Rows[0].Cells[count].Value != null) && (dataGridViewAddData.Rows[0].Cells[count].Value.ToString() != ""))
                {
                    if (count != (int)enumNamesColumnsMainTable.Owner)
                    {
                        // Если в строке имеются спецсимволы, то они будут заэкранированы MySqlHelper.EscapeString
                        query += "\"" + MySqlHelper.EscapeString(dataGridViewAddData.Rows[0].Cells[count].Value.ToString());

                   //     if (count != (dataGridViewAddData.Columns.Count - 1))
                            query += "\",";
                   //     else
                   //         firstQuery += "\")";
                    }
                    else
                    {
                        switch (currentOperation)
                        {
                            case ((int)enumTypeOperation.addNewRow):
                                {
                                    query += "\"" + MySqlHelper.EscapeString(dataGridViewAddData.Rows[0].Cells[count].Value.ToString());
                                    query += "\",";

                                    break;
                                }

                            case ((int)enumTypeOperation.editSelectedRow):
                                {
                                    createAddAndDeletedUsersList();

                                    query += "\"";

                                    for (i = 0; i < newOwnersList.Count; i++)
                                        query += newOwnersList[i] + myDBChildForm.delimiter + "\n";

                                    query += "\",\"";


                                    for (i = 0; i < deletedOwnersList.Count; i++)
                                        query += deletedOwnersList[i] + myDBChildForm.delimiter + "\n";

                                    query += "\",";

                                    break;
                                }
                        }

                        emailReceivers = dataGridViewAddData.Rows[0].Cells[count].Value.ToString();
                    }


                    emailMessage += dataGridViewAddData.Columns[count].HeaderText + ":     ";
                    emailMessage += dataGridViewAddData.Rows[0].Cells[count].Value.ToString();
                    emailMessage += "\n--------------------------------------------------\n";
                }
                else
                {
                    if ((count == (int)enumNamesColumnsMainTable.StartTime) || (count == (int)enumNamesColumnsMainTable.Project) || (count == (int)enumNamesColumnsMainTable.Description) ||
                        (count == (int)enumNamesColumnsMainTable.Owner) || /*(count == (int)enumNamesColumnsMainTable.Reporter) ||*/ (count == (int)enumNamesColumnsMainTable.CurrentStatus))
                    {
                        //MessageBox.Show("Some off required fields are empty!\nPlease insert data to this fields!", "ERROR!");
                        MessageBox.Show("Column '" + dataGridViewAddData.Columns[count].Name.ToString() + "' is empty!" + "\nPlease insert data to this column!", "ERROR!");
                        return;
                    }
                    else
                    {
                      //  if (count != (dataGridViewAddData.Columns.Count - 1))
                            query += "\"\",";
                     //   else
                     //       firstQuery += "\"\")";



                        emailMessage += dataGridViewAddData.Columns[count].HeaderText + ":     ";
                        if (dataGridViewAddData.Rows[0].Cells[count].Value != null)
                            emailMessage += dataGridViewAddData.Rows[0].Cells[count].Value.ToString();
                        else
                            emailMessage += "";
                        emailMessage += "\n--------------------------------------------------\n";
                    }

                }
            }

            // Добавляю значение времени последнего изменения
            query += "\"Time: " + DateTime.Now.ToString(stringFormatDateTime) + "\\r\\n";
            query += "User: " + myDBChildForm.currentDBUserName.fullName + "\")";


            myDBChildForm.insertDataToDB(query);

            if (myDBChildForm.stateLastQuery == (int)enumDbQueryStates.success)
            {
                //generateEmailMessage("" ,emailSubject, emailMessage);

                myEmailChild.addMessageToSendQueue(myDBChildForm.getEmailAddresOnUsersNames(emailReceivers), emailSubject, emailMessage);

                DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("File 'FormEditData.cs' method 'buttonOk_Click'.\n\n" + "Problem in insert data in database!\nCheck connection with database and try again!", "ERROR!");
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            /*
            if (!myDBChildForm.isConnectToDb())
                return;
            */

            DialogResult = DialogResult.Cancel;
        }

        public void initialiseAttributes(workWithDbClass myDB, workWithEmailClass myEmail)
        {
            if (myDB != null)
                myDBChildForm = myDB;
            else
            {
                myDBChildForm.tableProjects.clearStruct();
                myDBChildForm.tableUsers.clearStruct();
                myDBChildForm.tableTaskStates.clearStruct();

                myDBChildForm = null;
            }


            myEmailChild = myEmail;

            for (int count = 0; count < (int)enumNameColumnsWithComboBox.length; count++)
            {
                if (myDBChildForm != null)
                    comboBoxCells[count].name = myDBChildForm.nameColumnsWithComboBox[count];
                else
                    comboBoxCells[count].name = "";
            }
        }

        public void setNamesForColumnsDataGrid(string[] namesColumns)
        {
            for (int count = 0; count < namesColumns.Length; count++)
            {
                if ((count == (int)enumNamesColumnsMainTable.StartTime) || (count == (int)enumNamesColumnsMainTable.Project) || (count == (int)enumNamesColumnsMainTable.Description) ||
                       (count == (int)enumNamesColumnsMainTable.Owner) || (count == (int)enumNamesColumnsMainTable.CurrentStatus))
                {
                    dataGridViewAddData.Columns.Add(namesColumns[count], namesColumns[count] + myDBChildForm.columnNotBeNullString);
                }
                else
                    dataGridViewAddData.Columns.Add(namesColumns[count], namesColumns[count]);


                dataGridViewAddData.Columns[count].Width = myDBChildForm.columnsWidth[count];
            }

            try
            {
              //  dataGridViewAddData.Columns["ID"].ReadOnly = true;
                dataGridViewAddData.Columns["ID"].Visible = false;
                dataGridViewAddData.Columns["Owner"].ReadOnly = true;
                dataGridViewAddData.Columns["Reporter"].ReadOnly = true;
                dataGridViewAddData.Columns["LastChange"].ReadOnly = true;
                dataGridViewAddData.Columns["AmountChanges"].ReadOnly = true;
               // dataGridViewAddData.Columns["LastChange"].ReadOnly = true;
                dataGridViewAddData.Columns["LastChange"].Visible = false;
               // dataGridViewAddData.Columns["AmountChanges"].ReadOnly = true;
                dataGridViewAddData.Columns["AmountChanges"].Visible = false;
               // dataGridViewAddData.Columns["IsLocked"].ReadOnly = true;
                dataGridViewAddData.Columns["IsLocked"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("File 'FormEditData.cs' method 'setNamesForColumnsDataGrid'.\nIn 'dataGridViewAddData' not found column with name 'ID' or 'Reporter'!\n\n"
                        + ex.Message, "Runtime error!");
            }
        }

        public void setCellTypesDataGrid()
        {
            structTable currentTable = new structTable();
            int numberColumnName = -1;
          //  int numberGroupInArray = -1;
            int numberColumnInDataGrid = -1;
            int i = 0;


            for (int count = 0; count < comboBoxCells.Length; count++)
            {
                switch (count)
                {
                    case ((int)enumNameColumnsWithComboBox.Project):
                        {
                            currentTable = myDBChildForm.tableProjects;
                            break;
                        }

                    /*case ((int)enumNameColumnsWithCombobox.Owner):
                        {
                            currentTable = myDBChildForm.tableUsers;
                            break;
                        }*/

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

                if ((currentTable.columnNames != null) && (currentTable.columnNames.Length > 0))
                {
                    numberColumnName = myDBChildForm.getColumnIndexOnColumnNameInTable(currentTable, "Name");

                    if (numberColumnName >= 0)
                    {
                        // Создаю ячейку ComboBoxCell, если она не создана
                        if (comboBoxCells[count].cell == null)
                            comboBoxCells[count].cell = new DataGridViewComboBoxCell();

                        // Добавляю значения в список ComboBoxCell
                        for (i = 0; i < currentTable.listValues.Count; i++)
                        {
                            // Если создаю раскрывающийся список для столбца "Owner"
                          /*  if (count == (int)enumNameColumnsWithCombobox.Owner)
                            {
                                numberGroupInArray = myDBChildForm.getColumnIndexOnColumnNameInTable(currentTable, "Group");

                                // Исключаю из списка пользователей из группы "Guest"
                                if (!currentTable.listValues[i][numberGroupInArray].Equals(myDBChildForm.nameGuestGroup))
                                    comboBoxCells[count].cell.Items.Add(currentTable.listValues[i][numberColumnName].ToString());

                                // Исключаю из списка пользователей из группы "Guest"
                                //if (!currentTable.listValues[i][numberGroupInArray].Equals(myDBChildForm.nameGuestGroup))
                               //     ownerListBox.Items.Add(currentTable.listValues[i][numberColumnName].ToString());
                            }
                            else*/
                                comboBoxCells[count].cell.Items.Add(currentTable.listValues[i][numberColumnName].ToString());
                        }

                        try
                        {
                            numberColumnInDataGrid = dataGridViewAddData.Columns[comboBoxCells[count].name].Index;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("File 'FormEditData.cs' method 'setCellTypesDataGrid'.\nNot found column with name '"
                                + comboBoxCells[count].name + "' in DataGrid.\n\n" + ex.Message, "Runtime error!");

                            return;
                        }

                        comboBoxCells[count].cell.Value = dataGridViewAddData.Rows[0].Cells[numberColumnInDataGrid].Value;
                        // Присваиваю 0 строке numberColumnInDataGrid столбцу объекта DataGridView созданную ячейку ComboBoxCell
                        //dataGridViewAddData.Rows[0].Height = 100;
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



              //  comboBoxCells[count].cell.
            }

            addDataToOwnerListBox();
            
        }


        private void addDataToOwnerListBox()
        {
            structTable currentTable = myDBChildForm.tableUsers;
            int numberColumnName = -1;
            int numberGroupInArray = -1;
         //   int numberColumnInDataGrid = -1;
            int countItemsInListBox = 0;

            int maxShowedItemsinList = 10;

      //      ownerListBox.Height = ownerListBoxRowHeight;

         //   CheckBox ch = new CheckBox();
         //   ch.Name = "tt";
         //   ch.Text = "1111";
         //   ch.Checked = true;
/*
            CheckBox ch1 = new CheckBox();
            ch1.Name = "tt2";
            ch1.Text = "222";
            ch1.Checked = true;
            */

            ownerListBox.Height = 4;
            ownerListBox.Items.Clear();
            ownerListBox.Visible = false;

           // ownerListBox


      //      ownerListBox.Controls.Add(ch);
          //  ownerListBox.Controls.Add(ch1);


            if ((currentTable.columnNames != null) && (currentTable.columnNames.Length > 0))
            {
                numberColumnName = myDBChildForm.getColumnIndexOnColumnNameInTable(currentTable, "Name");

                if (numberColumnName >= 0)
                {
                    // Добавляю значения в список ComboBoxCell
                    for (int i = 0; i < currentTable.listValues.Count; i++)
                    {
                        numberGroupInArray = myDBChildForm.getColumnIndexOnColumnNameInTable(currentTable, "Group");

                        // Исключаю из списка пользователей из группы "Guest"
                        if (!currentTable.listValues[i][numberGroupInArray].Equals(myDBChildForm.nameGuestGroup))
                        {
                            ownerListBox.Items.Add(currentTable.listValues[i][numberColumnName].ToString());
                            //ownerListBox.Items.Add(ch);


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
                    MessageBox.Show("File 'FormEditData.cs' method 'addDataToOwnerListBox'.\n" + "'numberColumnName' = " + numberColumnName.ToString(), "Runtime error!");
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

          //  ownerListBox.Height = 0;
        }




        public void addRowValuesForDataGrid(string[] rowsValues)
        {
            dataGridViewAddData.Rows.Add(rowsValues);

            myDBChildForm.readUsersProjectsTaskStatusFromDB();

            setCellTypesDataGrid();

            addOwnersToTask();
        }

        public void clearDataGrid()
        {
            dataGridViewAddData.Rows.Clear();
            dataGridViewAddData.Columns.Clear();

            ownerListBox.Items.Clear();
            currentTaskOwnersList.Clear();
            newOwnersList.Clear();
            deletedOwnersList.Clear();
            
            for (int count = 0; count < comboBoxCells.Length; count++)
            {
                if (comboBoxCells[count].cell != null)
                {
                    comboBoxCells[count].cell.Dispose();
                    comboBoxCells[count].cell = null;
                }
            }
            
        }

        private void FormEditData_FormClosed(object sender, FormClosedEventArgs e)
        {
            currentOperation = (int)enumTypeOperation.none;
            myDBChildForm.currentProgramState = (int)enumProgramStates.activeMainForm;
            clearDataGrid();
        }

        private void FormEditData_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult != DialogResult.OK)
            {
                if (!myDBChildForm.isConnectToDb())
                {
                    e.Cancel = true;
                }
            }
        }

        private void dataGridViewAddData_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            int numberColumn =  dataGridViewAddData.Columns[nameColumnCurrentStatus].Index;

            if (e.ColumnIndex == numberColumn)
            {
                switch(dataGridViewAddData.Rows[e.RowIndex].Cells[numberColumn].Value.ToString())
                {
                    case "Closed":
                        {
                            dataGridViewAddData.Rows[e.RowIndex].Cells[nameColumnEndTime].Value = DateTime.Now.ToString(stringFormatDateTime);

                            break;
                        }

                    case "Opened":
                        {
                            dataGridViewAddData.Rows[e.RowIndex].Cells[nameColumnEndTime].Value = "";

                            break;
                        }

                    default:
                        {
                            MessageBox.Show("File 'FormEditData.cs' method 'dataGridViewAddData_CellValueChanged'.\nInvalid Value 'dataGridViewAddData.Rows[" +
                                e.RowIndex.ToString() + "].Cells[" + numberColumn.ToString() + "].Value' = " + dataGridViewAddData.Rows[e.RowIndex].Cells[numberColumn].Value.ToString(), "Runtime error!");

                            break;
                        }
                }
            }
        }

        /*
        public void generateEmailMessage(string mailReceiver, string subject, string message)
        {
            //int numberIdInArray = -1;
            int numberEmailInArray = -1;
            int numberNameInArray = -1;
            //int numberOwnerInArray = -1;

            string nameOwner = "";
            string receiverEmailAddres = "";


            if (mailReceiver == "")
            {
                try
                {
                    nameOwner = dataGridViewAddData.Rows[0].Cells["Owner"].Value.ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("File 'FormEditData.cs' method 'generateEmailMessage'.\nIn 'dataGridViewAddData' not found column with name 'Owner'!\n\n" + ex.Message, "Runtime error!");

                    return;
                }
            }
            else
                nameOwner = mailReceiver;

            
            //numberIdInArray = myDB.getColumnIndexOnColumnNameInTable(myDB.tableUsers, "ID");
            numberNameInArray = myDBChildForm.getColumnIndexOnColumnNameInTable(myDBChildForm.tableUsers, "Name");
            numberEmailInArray = myDBChildForm.getColumnIndexOnColumnNameInTable(myDBChildForm.tableUsers, "Email");

            if ((numberNameInArray >= 0) && (numberEmailInArray >= 0))
            {
                for (int i = 0; i < myDBChildForm.tableUsers.listValues.Count; i++)
                {
                    if (myDBChildForm.tableUsers.listValues[i][numberNameInArray].Equals(nameOwner))
                    {
                        receiverEmailAddres = myDBChildForm.tableUsers.listValues[i][numberEmailInArray];
                        break;
                    }
                }
            }
            else
            {
                MessageBox.Show("File 'FormEditData.cs' method 'generateEmailMessage'.\n" +
                    "'numberNameInArray' = " + numberNameInArray.ToString() + " ;'numberGroupInArray' = " + numberEmailInArray.ToString(), "Runtime error!");
            }

            //if ((receiverEmailAddres != "") && (message != ""))
            if (message != "")
            {
                myEmailChild.sendMessage(receiverEmailAddres, subject, message);
            }
        }
        */
        


        private void FormEditData_Activated(object sender, EventArgs e)
        {
            myDBChildForm.currentProgramState = (int)enumProgramStates.activeEditDataForm;
        }

        private void FormEditData_MouseClick(object sender, MouseEventArgs e)
        {
            //int heig = 0;

            /*
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                //heig = 20 * ccc;

                 //heig = ownerListBox.Height + 20;

                ownerListBox.Height = heig;

                ownerListBox.Items[0] = heig.ToString();

                ccc++;
            }
            */
        }

        private void dataGridViewAddData_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            int columnNumber = -1;
            Point location = new Point(0, 0);

            if (e.Button != System.Windows.Forms.MouseButtons.Left)
                return;

            if ((e.RowIndex < 0) || (e.ColumnIndex < 0))
                return;

            columnNumber = getColumnNumberOnColumnNameInDataGrid(myDBChildForm.nameColumnsMainTable[(int)enumNamesColumnsMainTable.Owner]);
            if (columnNumber < 0)
            {
                MessageBox.Show("File 'FormEditData.cs' method 'dataGridViewAddData_CellMouseClick'.\n" + "'columnNumber' = " + columnNumber.ToString(), "Runtime error!");
                return;
            }

            if (e.ColumnIndex == columnNumber)
            {
                ownerListBox.Visible = !ownerListBox.Visible;

                location.Y = dataGridViewAddData.ColumnHeadersHeight + dataGridViewAddData.Rows[e.RowIndex].Height;
                location.X += dataGridViewAddData.TopLeftHeaderCell.Size.Width - 31;

                for (int i = 0; i < columnNumber; i++)
                    location.X += dataGridViewAddData.Columns[i].Width;

                ownerListBox.Location = location;
            }
            else
                ownerListBox.Visible = false;
        }


        private void ListBox_MouseClick(object sender, EventArgs e)
        {
            int columnNumber = -1;
            string textValue = "";

            columnNumber = getColumnNumberOnColumnNameInDataGrid(myDBChildForm.nameColumnsMainTable[(int)enumNamesColumnsMainTable.Owner]);
            if (columnNumber < 0)
            {
                MessageBox.Show("File 'FormEditData.cs' method 'dataGridViewAddData_CellMouseClick'.\n" + "'columnNumber' = " + columnNumber.ToString(), "Runtime error!");
                return;
            }

            newOwnersList.Clear();

            for (int count = 0; count < ownerListBox.SelectedIndices.Count; count++)
            {
                newOwnersList.Add(ownerListBox.SelectedItems[count].ToString());

                textValue += ownerListBox.SelectedItems[count].ToString() + myDBChildForm.delimiter + "\n";

        //        if (count < (ownerListBox.SelectedIndices.Count - 1))
        //            textValue += "\n";
            }

            dataGridViewAddData.Rows[0].Cells[columnNumber].Value = textValue;
        }


        private void addOwnersToTask()
        {
          //  int numberColumnId = -1;
            int numberColumnOwner = -1;
            string ownersFromDataGrid = "";
            //List<string> listTaskOwners = new List<string>();

            currentTaskOwnersList.Clear();

            numberColumnOwner = getColumnNumberOnColumnNameInDataGrid(myDBChildForm.nameColumnsMainTable[(int)enumNamesColumnsMainTable.Owner]);

            if (numberColumnOwner < 0)
            {
                MessageBox.Show("File 'FormEditData.cs' method 'addOwnersToTask'.\n'numberColumnOwner' = " + numberColumnOwner.ToString() , "Runtime error!");
                return;
            }

            ownersFromDataGrid = dataGridViewAddData.Rows[0].Cells[numberColumnOwner].Value.ToString();

            currentTaskOwnersList = myDBChildForm.parseOwnersFromString(ownersFromDataGrid);

            selectItemsInOwnerListBox();
        }


        private void selectItemsInOwnerListBox()
        {
            for (int count = 0; count < currentTaskOwnersList.Count; count++)
            {
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

        private void createAddAndDeletedUsersList()
        {
            bool isOwnerFindInNewOwners = false;

           // newOwnersList.Clear();
            deletedOwnersList.Clear();

            /*
            for (int j = 0; j < ownerListBox.Items.Count; j++)
            {
                if (ownerListBox.Items[j])
            }
                */

            for (int count = 0; count < currentTaskOwnersList.Count; count++)
            {
                isOwnerFindInNewOwners = false;

                for (int i = 0; i < newOwnersList.Count; i++)
                {
                    if (newOwnersList[i].Equals(currentTaskOwnersList[count]))
                    {
                        newOwnersList.RemoveAt(i);
                        isOwnerFindInNewOwners = true;

                        break;
                    }
                    /*
                    if (!isOwnerFindInNewOwners)
                    {
                        deletedOwnersList.Add(currentTaskOwnersList[count]);
                    }
                    */
                }

                if (!isOwnerFindInNewOwners)
                {
                    deletedOwnersList.Add(currentTaskOwnersList[count]);
                }
            }
        }


        private int getColumnNumberOnColumnNameInDataGrid(string name)
        {
            int number = -1;

            try
            {
                number = dataGridViewAddData.Columns[name].Index;
            }
            catch (Exception ex)
            {
                MessageBox.Show("File 'FormEditData.cs' method 'getColumnNumberOnColumnName'.\n" + "Can't found column with name '" + name + "' in DataGrid.\n\n" + ex.Message, "Runtime error!");
            }


            return number;
        }

        private void dataGridViewAddData_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            Point location = new Point(0, ownerListBox.Location.Y);
            int columnNumber = -1;

            if ((dataGridViewAddData.Columns.Count < 10) || (!ownerListBox.Visible))
                return;

            columnNumber = getColumnNumberOnColumnNameInDataGrid(myDBChildForm.nameColumnsMainTable[(int)enumNamesColumnsMainTable.Owner]);
            if (columnNumber < 0)
            {
                MessageBox.Show("File 'FormEditData.cs' method 'dataGridViewAddData_ColumnWidthChanged'.\n" + "'columnNumber' = " + columnNumber.ToString(), "Runtime error!");
                return;
            }

            location.X = dataGridViewAddData.TopLeftHeaderCell.Size.Width - 31;
            for (int i = 0; i < columnNumber; i++)
                location.X += dataGridViewAddData.Columns[i].Width;

            ownerListBox.Location = location;
            ownerListBox.Invalidate();
        }

        private void dataGridViewAddData_RowHeightChanged(object sender, DataGridViewRowEventArgs e)
        {
            Point location = new Point(ownerListBox.Location.X, 0);

            if (!ownerListBox.Visible)
                return;

            location.Y = dataGridViewAddData.ColumnHeadersHeight + dataGridViewAddData.Rows[e.Row.Index].Height;
            ownerListBox.Location = location;
            ownerListBox.Invalidate();
        }
        
    }
}
