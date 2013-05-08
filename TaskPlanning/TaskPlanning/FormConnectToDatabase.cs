using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace TaskPlanning
{
    public partial class FormConnectToDatabase : Form
    {
        private workWithDbClass myDBConnect;

        private RegistryKey myRegistryKey;

        public FormConnectToDatabase()
        {
            InitializeComponent();


            myRegistryKey = Registry.CurrentUser.OpenSubKey("Software", true);
            myRegistryKey = myRegistryKey.CreateSubKey("TaskPlanningUserSettings\\ConnectToDBSettings");


            

            //setTextInDBTextfields();
        }

        private void buttonDBConnect_Click(object sender, EventArgs e)
        {
            connectToDb();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void connectToDb()
        {
            if ((textBoxDBHost.Text == "") || (textBoxDBName.Text == "") || (textBoxDBUser.Text == ""))// || (textBoxDBUserPswd.Text == ""))
            {
                MessageBox.Show("Some required fields are empty! Please, enter data in this fields.", "Attention!");

                return;
            }

            myDBConnect.dbHost = textBoxDBHost.Text;
            myDBConnect.dbName = textBoxDBName.Text;
            myDBConnect.dbUser = textBoxDBUser.Text;
            myDBConnect.dbUserPassword = textBoxDBUserPswd.Text;

            if (myDBConnect.myConnection.State == System.Data.ConnectionState.Closed)
            {
                myDBConnect.connectToDB();

                if (myDBConnect.myConnection.State == System.Data.ConnectionState.Open)
                {
                    writeUserSettings();

                    DialogResult = DialogResult.OK;
                }
            }
            else
            {
                MessageBox.Show("Connection is already established.", "Connect to database!");
            }
        }

        /*
        private void setTextInDBTextfields()
        {
            //textBoxDBHost.Text = "192.168.92.5";
            textBoxDBHost.Text = "localhost";
            textBoxDBName.Text = "TaskPlanning";
            //textBoxDBUser.Text = "DBuser";
            textBoxDBUser.Text = "root";
            textBoxDBUserPswd.Text = "ledevop";
        }
        */

        public void initialiseDB(workWithDbClass DB)
        {
            myDBConnect = DB;
        }

        private void readUserSettings()
        {
            string checkboxStateString = "";

            try
            {
                textBoxDBHost.Text = (string)myRegistryKey.GetValue("DbHost", "localhost");
                textBoxDBName.Text = (string)myRegistryKey.GetValue("DbName", "TaskPlanning");
                textBoxDBUser.Text = (string)myRegistryKey.GetValue("DbUser", "root");
                textBoxDBUserPswd.Text = (string)myRegistryKey.GetValue("DbPassword", "");
                checkboxStateString = (string)myRegistryKey.GetValue("DbCheckboxState", "");

                if (checkboxStateString.Equals("True"))
                    checkBoxRememberMe.Checked = true;
                else
                    checkBoxRememberMe.Checked = false;


                //textBoxDBUserPswd.Text = (string)myRegistryKey.GetValue("DbPassword", "");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not read settings!\n\n" + ex.Message, "Error reading settings!");
            }
        }

        private void writeUserSettings()
        {
            try
            {
                myRegistryKey.SetValue("DbHost", textBoxDBHost.Text);
                myRegistryKey.SetValue("DbName", textBoxDBName.Text);
                myRegistryKey.SetValue("DbUser", textBoxDBUser.Text);
                myRegistryKey.SetValue("DbCheckboxState", checkBoxRememberMe.Checked);

                if (checkBoxRememberMe.Checked)
                    myRegistryKey.SetValue("DbPassword", textBoxDBUserPswd.Text);
                else
                    myRegistryKey.SetValue("DbPassword", "");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not save settings!\n\n" + ex.Message, "Error saving settings!");
            }
        }

        private void FormConnectToDatabase_Shown(object sender, EventArgs e)
        {
            readUserSettings();
        }

        private void FormConnectToDatabase_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
                connectToDb();
        }

        
    }
}
