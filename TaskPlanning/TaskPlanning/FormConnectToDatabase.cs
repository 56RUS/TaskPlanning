using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace TaskPlanning
{
    public partial class FormConnectToDatabase : Form
    {
        private workWithDbClass myDBConnect;        // Переменная для работы с БД
        private RegistryKey myRegistryKey;          // Переменная для работы с реестром (хранятся настройки подключения к БД)


        public FormConnectToDatabase()
        {
            InitializeComponent();

            // Инициализирую ключ реестра, где хранятся настройки
            myRegistryKey = Registry.CurrentUser.OpenSubKey("Software", true);
            myRegistryKey = myRegistryKey.CreateSubKey("TaskPlanningUserSettings\\ConnectToDBSettings");
        }

        // Инициализирую указатель на класс для работы с БД
        public void initialiseDB(workWithDbClass DB)
        {
            myDBConnect = DB;
        }

        // Метод, выполняющий подключение к БД
        private void connectToDb()
        {
            // Если пусто одно из полей, которые должны быть заполнены
            if ((textBoxDBHost.Text == "") || (textBoxDBName.Text == "") || (textBoxDBUser.Text == ""))
            {
                MessageBox.Show("Some required fields are empty! Please, enter data in this fields.", "Attention!");
                return;
            }

            // Из текстовых полей беру параметры подключения к БД
            myDBConnect.dbHost = textBoxDBHost.Text;
            myDBConnect.dbName = textBoxDBName.Text;
            myDBConnect.dbUser = textBoxDBUser.Text;
            myDBConnect.dbUserPassword = textBoxDBUserPswd.Text;

            // Если подключение к БД отсутствует
            if (myDBConnect.myConnection.State == System.Data.ConnectionState.Closed)
            {
                myDBConnect.connectToDB();      // Подключаюсь к БД

                // Если успешно подключился к БД
                if (myDBConnect.myConnection.State == System.Data.ConnectionState.Open)
                {
                    writeUserSettings();                // Сохраняю настройки подключения к БД
                    DialogResult = DialogResult.OK;
                }
            }
            else
            {
                MessageBox.Show("Connection is already established.", "Connect to database!");
            }
        }

        // Читаю из реестра настройки подключения к БД
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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not read settings!\n\n" + ex.Message, "Error reading settings!");
            }
        }

        //Записываю в реестр настройки подключения к БД
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

        // Обработчик нажатия кнопки "Connect"
        private void buttonDBConnect_Click(object sender, EventArgs e)
        {
            connectToDb();      // Подключаюсь к БД
        }

        // Обработчик нажатия кнопки "Cancel"
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        // Событие, возникающие при отображении формы
        private void FormConnectToDatabase_Shown(object sender, EventArgs e)
        {
            readUserSettings();     // Читаю настройки подключения к БД
        }

        // Событие, возникающие при нажатии любой клавиши
        private void FormConnectToDatabase_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Если нажата клавиша "Enter" (на любом элементе формы), то пытаюсь подключиться к БД
            if (e.KeyChar == (char)Keys.Enter)
                connectToDb();

            // Если нажата клавиша "Esc", то закрываю форму
            if (e.KeyChar == (char)Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }
    }
}
