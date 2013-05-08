using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Windows.Forms;

namespace TaskPlanning
{
    enum enumDbConnectionStates
    {
        closed,
        open
    }

    enum enumDbQueryStates
    {
        error = 0,
        success = 1
    }

    enum enumNameColumnsWithCombobox
    {
        Project,
        Owner,
        //Reporter,
        CurrentStatus,
        length
    }

    enum enumNameColumnsGetNameOnID
    {
        Project,
        Owner,
        Reporter,
        CurrentStatus,
        length
    }

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
        CurrentStatus
    }

    public struct structTable
    {
        public string[] columnNames;
        public List<string[]> listValues;

        public void clearStruct()
        {
            columnNames = null;
            listValues.RemoveRange(0, listValues.Count);
        }
    }

    public class DBWork
    {
        private const string nameTableUsers = "users";
        private const string nameTableTaskStates = "taskstatus";
        private const string nameTableProjects = "projects";
        public const string nameTableTasks = "tasks";

        public int[] columnsWidth = { 30, 70, 70, 60, 270, 270, 100, 70, 70, 70, 85 };
        public string columnNotBeNullString = " *";

        public string dbHost;
        public string dbName;
        public string dbUser;
        public string dbUserPassword;
        public int connectionState;
        public int stateLastQuery;

        
        //public List<string> taskStatesList;
        //public List<string> projectNamesList;
        //public string manTableColumnsNamesList;

        public structTable tableUsers;
        public structTable tableProjects;
        public structTable tableTaskStates;


        private MySqlConnection myConnection;
        private MySqlCommand myCommand;
        private MySqlDataReader myDataReader;


        public string[] nameColumnsWithComboBox;

        public string[] nameColumnsGetNameOnID;



        public string currentDBUserName;


        public DBWork()
        {
            dbHost = "";
            dbName = "";
            dbUser = "";
            dbUserPassword = "";
            connectionState = (int)enumDbConnectionStates.closed;
            stateLastQuery = (int)enumDbQueryStates.error;

            //usersNamesList = new List<string[]>();
            tableUsers = new structTable();
            tableUsers.listValues = new List<string[]>();

            tableProjects = new structTable();
            tableProjects.listValues = new List<string[]>();

            tableTaskStates = new structTable();
            tableTaskStates.listValues = new List<string[]>();


            //taskStatesList = new List<string>();
            //projectNamesList = new List<string>();
            //manTableColumnsNamesList = new List<string>();

            myConnection = new MySqlConnection();
            myCommand = new MySqlCommand();
            //myDataReader.Close();


            nameColumnsWithComboBox = new string[(int)enumNameColumnsWithCombobox.length];
            nameColumnsWithComboBox[(int)enumNameColumnsWithCombobox.Project] = "Project";
            nameColumnsWithComboBox[(int)enumNameColumnsWithCombobox.Owner] = "Owner";
            //nameColumnsWithComboBox[(int)enumNameColumnsWithCombobox.Reporter] = "Reporter";
            nameColumnsWithComboBox[(int)enumNameColumnsWithCombobox.CurrentStatus] = "CurrentStatus";


            nameColumnsGetNameOnID = new string[(int)enumNameColumnsGetNameOnID.length];
            nameColumnsGetNameOnID[(int)enumNameColumnsGetNameOnID.Project] = "Project";
            nameColumnsGetNameOnID[(int)enumNameColumnsGetNameOnID.Owner] = "Owner";
            nameColumnsGetNameOnID[(int)enumNameColumnsGetNameOnID.Reporter] = "Reporter";
            nameColumnsGetNameOnID[(int)enumNameColumnsGetNameOnID.CurrentStatus] = "CurrentStatus";
        }


        public void connectToDB()
        {
            //bool returnRes = false;
            //string connectString = "Database=" + dbName + "; Data Source=" + dbHost + ";User Id=" + dbUser + ";Password=" + dbUserPassword;

            string connectString = new MySqlConnectionStringBuilder {
                Database = dbName,
                Server = dbHost,
                UserID =  dbUser,
                Password = dbUserPassword }.ToString();

            try
            {
                if (myConnection.State != System.Data.ConnectionState.Open)
                {
                    myConnection.ConnectionString = connectString;
                    myConnection.Open();

                    if (myConnection.State == System.Data.ConnectionState.Open)
                    {
                        myCommand.Connection = myConnection;
                        connectionState = (int)enumDbConnectionStates.open;

                        currentDBUserName = dbUser;

                        MessageBox.Show("Successful connect to database.", "Connect to database!");
                       //MessageBox.Show("Круто: " + myConnection.State.ToString(), "Success!");

                        //readDataFromDB("SELECT * FROM animal");
                        //readDataFromDB("SELECT * FROM animal");
                        //insertDataToDB("INSERT INTO animal VALUES(126, 'aaa', '***')");


                        readUsersNamesFromDB();
                        readProjectsNamesFromDB();
                        readTaskStatesFromDB();
                    }
                    else
                    {
                        //MessageBox.Show("Не могу подключиться: " + myConnection.State.ToString());
                        MessageBox.Show("Could not connect to database! Connect state: " + myConnection.State.ToString(), "Connect to database!");
                    }
                }
              /*  else
                {
                    MessageBox.Show("Connection is already established.", "Connect to database!");
                    //MessageBox.Show("Соединение уже установлено!");
                }*/
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unexpected exception in connect to database!\n\nError: " + ex.Message, "Connect to database!");
                //MessageBox.Show("Ошибка: " + ex.Message);
            }

           // return returnRes;
        }

        public void disconnectFromDB()
        {
            if (myConnection.State == System.Data.ConnectionState.Open)
            {
                myConnection.Close();

                if (myConnection.State != System.Data.ConnectionState.Open)
                {
                    connectionState = (int)enumDbConnectionStates.closed;

                    MessageBox.Show("Successful disconnect from database.", "Connect to database!");
                    //MessageBox.Show("Отключился от БД!");
                }
            }
        }

        public void readDataFromDB(string query)
        {
            myCommand.CommandText = query;

            //query = MySqlHelper.EscapeString(query);

            stateLastQuery = (int)enumDbQueryStates.error;

            try
            {
                if (myDataReader == null)
                {
                    myDataReader = myCommand.ExecuteReader();
                    stateLastQuery = (int)enumDbQueryStates.success;
                }
                else
                {
                    if (myDataReader.IsClosed)
                    {
                        myDataReader = myCommand.ExecuteReader();
                        stateLastQuery = (int)enumDbQueryStates.success;
                    }
                    else
                    {
                        MessageBox.Show("Could not read data from database!\nPrevious session work with database is not closed!", "ERROR in work with database!");
                        //MessageBox.Show("myDataReader is not closed!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("File 'DBWork.cs' method 'readDataFromDB'.\nCould not read data from database!\n\nQuery string: " + myCommand.CommandText + "\n\nError: " + ex.Message, "ERROR in work with database!");
                //MessageBox.Show("Ошибка чтения данных из БД: " + ex.Message);
            }

            myCommand.CommandText = "";
        }

        public void insertDataToDB(string query)
        {
            myCommand.CommandText = query;

            //myCommand.CommandText

            //query = MySqlHelper.EscapeString(query);

            stateLastQuery = (int)enumDbQueryStates.error;

            try
            {
                if (myDataReader == null)
                    myCommand.ExecuteNonQuery();
                else
                {
                    if (myDataReader.IsClosed)
                    {
                        myCommand.ExecuteNonQuery();
                        stateLastQuery = (int)enumDbQueryStates.success;
                    }
                    else
                    {
                        MessageBox.Show("Could not insert data into database!\nPrevious session work with database is not closed!", "ERROR in work with database!");
                        //MessageBox.Show("myDataReader is not closed!");
                    }
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("File 'DBWork.cs' method 'insertDataToDB'.\nCould not change data in database!\n\nQuery string: " + myCommand.CommandText + "\n\nError: " + ex.Message, "ERROR in work with database!");

                //MessageBox.Show("Ошибка вставки данных в БД: " + ex.Message, "Error!");
            }

            myCommand.CommandText = "";
        }


        public bool isDataRead()
        {
            if (myDataReader != null)
            {
                if (myDataReader.Read())
                    return true;
                else
                {
                    myDataReader.Close();
                    return false;
                }
            }
            else
                return false;
        }

        public string[] getColumnsNames()
        {
            int amountColumns = myDataReader.FieldCount;
            string[] resultStr = new string[amountColumns];

            for (int column = 0; column < amountColumns; column++)
            {
                resultStr[column] = myDataReader.GetName(column).Substring(2);
            }

            return resultStr;
        }

        public string[] getRowValues()
        {
            int amountColumns = myDataReader.FieldCount;
            string[] resultStr = new string[amountColumns];

            for (int column = 0; column < amountColumns; column++)
            {
                if (!myDataReader.IsDBNull(column))
                    resultStr[column] = myDataReader.GetString(column);
                else
                    resultStr[column] = "";
            }

            return resultStr;
        }


        private void readUsersNamesFromDB()
        {
            string query = "SELECT * FROM " + nameTableUsers;

            readDataFromDB(query);

            if (stateLastQuery == (int)enumDbQueryStates.success)
            {
                tableUsers.columnNames = getColumnsNames();

                while (isDataRead())
                    tableUsers.listValues.Add(getRowValues());
            }
            else
            {
                //MessageBox.Show("File 'DBWork.cs' method 'readUsersNamesFromDB'.\nInvalid query = \"" + query + "\"", "ERROR in work with database!");
            }
        }

        private void readTaskStatesFromDB()
        {
            string query = "SELECT * FROM " + nameTableTaskStates;

            readDataFromDB(query);


            if (stateLastQuery == (int)enumDbQueryStates.success)
            {
                tableTaskStates.columnNames = getColumnsNames();

                while (isDataRead())
                    tableTaskStates.listValues.Add(getRowValues());
            }
            else
            {
                //MessageBox.Show("File 'DBWork.cs' method 'readTaskStatesFromDB'.\nInvalid query = \"" + query + "\"", "ERROR in work with database!");
            }
        }

        private void readProjectsNamesFromDB()
        {
            string query = "SELECT * FROM " + nameTableProjects;

            readDataFromDB(query);

            if (stateLastQuery == (int)enumDbQueryStates.success)
            {
                tableProjects.columnNames = getColumnsNames();

                while (isDataRead())
                    tableProjects.listValues.Add(getRowValues());
            }
            else
            {
                //MessageBox.Show("File 'DBWork.cs' method 'readProjectsNamesFromDB'.\nInvalid query = \"" + query + "\"", "ERROR in work with database!");
            }
        }

        public int getColumnIndexOnColumnNameInTable(structTable currentTable, string name)
        {

            if ((currentTable.columnNames != null) && (currentTable.columnNames.Length > 0))
            {
                for (int index = 0; index < currentTable.columnNames.Length; index++)
                {
                    if (currentTable.columnNames[index].Equals(name))
                        return index;
                }
            }
            else
            {
                if (currentTable.columnNames == null)
                    MessageBox.Show("File 'DBWork.cs' method 'getColumnIndexOnColumnNameInTable'.\n" + "'currentTable.columnNames' = NULL!", "Runtime error!");
                else
                {
                    if (currentTable.columnNames.Length == 0)
                        MessageBox.Show("File 'DBWork.cs' method 'getColumnIndexOnColumnNameInTable'.\n" + "'currentTable.columnNames.Length' = 0!", "Runtime error!");
                }
            }

            return -1;
        }

        /*
        public void parsingSqlQuery()
        {
            int amountColumns = myDataReader.FieldCount;
            int count = 0;

            string[] str = new string[amountColumns];

            string[] str_res = new string[amountColumns];

            int records = myDataReader.RecordsAffected;

            for (int i = 0; i < amountColumns; i++)
            {
                str[i] = myDataReader.GetName(i);
            }

            while (myDataReader.Read())
            {
                str_res[count] = "";



                count++;

                
            }

            myDataReader.Close();

            //str[0] = str[0].Remove(1);
        }
        */

    }
}
