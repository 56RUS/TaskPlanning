using System;
using System.Windows.Forms;



namespace TaskPlanning
{
    partial class Form1
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (myDB != null)
                myDB.disconnectFromDB();

            //writeUserSettings();


            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.groupBoxConnectToDB = new System.Windows.Forms.GroupBox();
            this.labelLoginInfo = new System.Windows.Forms.Label();
            this.buttonDBDisconnect = new System.Windows.Forms.Button();
            this.buttonDBConnect = new System.Windows.Forms.Button();
            this.groupBoxViewDataFromDB = new System.Windows.Forms.GroupBox();
            this.buttonExportData = new System.Windows.Forms.Button();
            this.groupBoxCustomQueryToDB = new System.Windows.Forms.GroupBox();
            this.comboBoxCustomQueryCurrentStatus = new System.Windows.Forms.ComboBox();
            this.comboBoxCustomQueryReporter = new System.Windows.Forms.ComboBox();
            this.comboBoxCustomQueryOwner = new System.Windows.Forms.ComboBox();
            this.comboBoxCustomQueryProject = new System.Windows.Forms.ComboBox();
            this.textBoxCustomQueryStartTime = new System.Windows.Forms.TextBox();
            this.checkBoxCustomQueryCurrentStatus = new System.Windows.Forms.CheckBox();
            this.checkBoxCustomQueryReporter = new System.Windows.Forms.CheckBox();
            this.checkBoxCustomQueryOwner = new System.Windows.Forms.CheckBox();
            this.checkBoxCustomQueryProject = new System.Windows.Forms.CheckBox();
            this.checkBoxCustomQueryStartTime = new System.Windows.Forms.CheckBox();
            this.checkBoxAutoupdateData = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonDeleteSelectedRow = new System.Windows.Forms.Button();
            this.buttonAddNewRow = new System.Windows.Forms.Button();
            this.buttonEditSelectedRow = new System.Windows.Forms.Button();
            this.buttonReadDataFromDB = new System.Windows.Forms.Button();
            this.dataGridViewDataFromDB = new System.Windows.Forms.DataGridView();
            this.groupBoxConnectToDB.SuspendLayout();
            this.groupBoxViewDataFromDB.SuspendLayout();
            this.groupBoxCustomQueryToDB.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDataFromDB)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxConnectToDB
            // 
            this.groupBoxConnectToDB.Controls.Add(this.labelLoginInfo);
            this.groupBoxConnectToDB.Controls.Add(this.buttonDBDisconnect);
            this.groupBoxConnectToDB.Controls.Add(this.buttonDBConnect);
            this.groupBoxConnectToDB.Location = new System.Drawing.Point(12, 12);
            this.groupBoxConnectToDB.Name = "groupBoxConnectToDB";
            this.groupBoxConnectToDB.Size = new System.Drawing.Size(1227, 43);
            this.groupBoxConnectToDB.TabIndex = 0;
            this.groupBoxConnectToDB.TabStop = false;
            this.groupBoxConnectToDB.Text = "Connect to database:";
            // 
            // labelLoginInfo
            // 
            this.labelLoginInfo.AutoSize = true;
            this.labelLoginInfo.Location = new System.Drawing.Point(185, 21);
            this.labelLoginInfo.Name = "labelLoginInfo";
            this.labelLoginInfo.Size = new System.Drawing.Size(29, 13);
            this.labelLoginInfo.TabIndex = 5;
            this.labelLoginInfo.Text = "label";
            // 
            // buttonDBDisconnect
            // 
            this.buttonDBDisconnect.Enabled = false;
            this.buttonDBDisconnect.Location = new System.Drawing.Point(96, 17);
            this.buttonDBDisconnect.Name = "buttonDBDisconnect";
            this.buttonDBDisconnect.Size = new System.Drawing.Size(75, 20);
            this.buttonDBDisconnect.TabIndex = 4;
            this.buttonDBDisconnect.Text = "Disconnect";
            this.buttonDBDisconnect.UseVisualStyleBackColor = true;
            this.buttonDBDisconnect.Click += new System.EventHandler(this.buttonDBDisconnect_Click);
            // 
            // buttonDBConnect
            // 
            this.buttonDBConnect.Location = new System.Drawing.Point(6, 17);
            this.buttonDBConnect.Name = "buttonDBConnect";
            this.buttonDBConnect.Size = new System.Drawing.Size(75, 20);
            this.buttonDBConnect.TabIndex = 0;
            this.buttonDBConnect.Text = "Connect";
            this.buttonDBConnect.UseVisualStyleBackColor = true;
            this.buttonDBConnect.Click += new System.EventHandler(this.buttonDBConnect_Click);
            // 
            // groupBoxViewDataFromDB
            // 
            this.groupBoxViewDataFromDB.Controls.Add(this.buttonExportData);
            this.groupBoxViewDataFromDB.Controls.Add(this.groupBoxCustomQueryToDB);
            this.groupBoxViewDataFromDB.Controls.Add(this.checkBoxAutoupdateData);
            this.groupBoxViewDataFromDB.Controls.Add(this.label2);
            this.groupBoxViewDataFromDB.Controls.Add(this.label1);
            this.groupBoxViewDataFromDB.Controls.Add(this.buttonDeleteSelectedRow);
            this.groupBoxViewDataFromDB.Controls.Add(this.buttonAddNewRow);
            this.groupBoxViewDataFromDB.Controls.Add(this.buttonEditSelectedRow);
            this.groupBoxViewDataFromDB.Controls.Add(this.buttonReadDataFromDB);
            this.groupBoxViewDataFromDB.Controls.Add(this.dataGridViewDataFromDB);
            this.groupBoxViewDataFromDB.Enabled = false;
            this.groupBoxViewDataFromDB.Location = new System.Drawing.Point(12, 61);
            this.groupBoxViewDataFromDB.Name = "groupBoxViewDataFromDB";
            this.groupBoxViewDataFromDB.Size = new System.Drawing.Size(1227, 761);
            this.groupBoxViewDataFromDB.TabIndex = 1;
            this.groupBoxViewDataFromDB.TabStop = false;
            this.groupBoxViewDataFromDB.Text = "Data from database:";
            // 
            // buttonExportData
            // 
            this.buttonExportData.Enabled = false;
            this.buttonExportData.Location = new System.Drawing.Point(613, 75);
            this.buttonExportData.Name = "buttonExportData";
            this.buttonExportData.Size = new System.Drawing.Size(145, 24);
            this.buttonExportData.TabIndex = 9;
            this.buttonExportData.Text = "Export data";
            this.buttonExportData.UseVisualStyleBackColor = true;
            this.buttonExportData.Click += new System.EventHandler(this.buttonExportData_Click);
            // 
            // groupBoxCustomQueryToDB
            // 
            this.groupBoxCustomQueryToDB.Controls.Add(this.comboBoxCustomQueryCurrentStatus);
            this.groupBoxCustomQueryToDB.Controls.Add(this.comboBoxCustomQueryReporter);
            this.groupBoxCustomQueryToDB.Controls.Add(this.comboBoxCustomQueryOwner);
            this.groupBoxCustomQueryToDB.Controls.Add(this.comboBoxCustomQueryProject);
            this.groupBoxCustomQueryToDB.Controls.Add(this.textBoxCustomQueryStartTime);
            this.groupBoxCustomQueryToDB.Controls.Add(this.checkBoxCustomQueryCurrentStatus);
            this.groupBoxCustomQueryToDB.Controls.Add(this.checkBoxCustomQueryReporter);
            this.groupBoxCustomQueryToDB.Controls.Add(this.checkBoxCustomQueryOwner);
            this.groupBoxCustomQueryToDB.Controls.Add(this.checkBoxCustomQueryProject);
            this.groupBoxCustomQueryToDB.Controls.Add(this.checkBoxCustomQueryStartTime);
            this.groupBoxCustomQueryToDB.Location = new System.Drawing.Point(9, 20);
            this.groupBoxCustomQueryToDB.Name = "groupBoxCustomQueryToDB";
            this.groupBoxCustomQueryToDB.Size = new System.Drawing.Size(1059, 49);
            this.groupBoxCustomQueryToDB.TabIndex = 8;
            this.groupBoxCustomQueryToDB.TabStop = false;
            this.groupBoxCustomQueryToDB.Text = "Custom query:";
            // 
            // comboBoxCustomQueryCurrentStatus
            // 
            this.comboBoxCustomQueryCurrentStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxCustomQueryCurrentStatus.FormattingEnabled = true;
            this.comboBoxCustomQueryCurrentStatus.Location = new System.Drawing.Point(928, 17);
            this.comboBoxCustomQueryCurrentStatus.Name = "comboBoxCustomQueryCurrentStatus";
            this.comboBoxCustomQueryCurrentStatus.Size = new System.Drawing.Size(121, 21);
            this.comboBoxCustomQueryCurrentStatus.TabIndex = 9;
            this.comboBoxCustomQueryCurrentStatus.SelectedValueChanged += new System.EventHandler(this.comboBoxCustomQueryCurrentStatus_SelectedValueChanged);
            this.comboBoxCustomQueryCurrentStatus.MouseClick += new System.Windows.Forms.MouseEventHandler(this.comboBoxCustomQueryCurrentStatus_MouseClick);
            // 
            // comboBoxCustomQueryReporter
            // 
            this.comboBoxCustomQueryReporter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxCustomQueryReporter.FormattingEnabled = true;
            this.comboBoxCustomQueryReporter.Location = new System.Drawing.Point(683, 17);
            this.comboBoxCustomQueryReporter.Name = "comboBoxCustomQueryReporter";
            this.comboBoxCustomQueryReporter.Size = new System.Drawing.Size(121, 21);
            this.comboBoxCustomQueryReporter.TabIndex = 8;
            this.comboBoxCustomQueryReporter.SelectedValueChanged += new System.EventHandler(this.comboBoxCustomQueryReporter_SelectedValueChanged);
            this.comboBoxCustomQueryReporter.MouseClick += new System.Windows.Forms.MouseEventHandler(this.comboBoxCustomQueryReporter_MouseClick);
            // 
            // comboBoxCustomQueryOwner
            // 
            this.comboBoxCustomQueryOwner.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxCustomQueryOwner.FormattingEnabled = true;
            this.comboBoxCustomQueryOwner.Location = new System.Drawing.Point(467, 18);
            this.comboBoxCustomQueryOwner.Name = "comboBoxCustomQueryOwner";
            this.comboBoxCustomQueryOwner.Size = new System.Drawing.Size(121, 21);
            this.comboBoxCustomQueryOwner.TabIndex = 7;
            this.comboBoxCustomQueryOwner.SelectedValueChanged += new System.EventHandler(this.comboBoxCustomQueryOwner_SelectedValueChanged);
            this.comboBoxCustomQueryOwner.MouseClick += new System.Windows.Forms.MouseEventHandler(this.comboBoxCustomQueryOwner_MouseClick);
            // 
            // comboBoxCustomQueryProject
            // 
            this.comboBoxCustomQueryProject.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxCustomQueryProject.FormattingEnabled = true;
            this.comboBoxCustomQueryProject.Location = new System.Drawing.Point(257, 17);
            this.comboBoxCustomQueryProject.Name = "comboBoxCustomQueryProject";
            this.comboBoxCustomQueryProject.Size = new System.Drawing.Size(121, 21);
            this.comboBoxCustomQueryProject.TabIndex = 6;
            this.comboBoxCustomQueryProject.SelectedValueChanged += new System.EventHandler(this.comboBoxCustomQueryProject_SelectedValueChanged);
            this.comboBoxCustomQueryProject.MouseClick += new System.Windows.Forms.MouseEventHandler(this.comboBoxCustomQueryProject_MouseClick);
            // 
            // textBoxCustomQueryStartTime
            // 
            this.textBoxCustomQueryStartTime.Location = new System.Drawing.Point(75, 18);
            this.textBoxCustomQueryStartTime.Name = "textBoxCustomQueryStartTime";
            this.textBoxCustomQueryStartTime.Size = new System.Drawing.Size(100, 20);
            this.textBoxCustomQueryStartTime.TabIndex = 5;
            // 
            // checkBoxCustomQueryCurrentStatus
            // 
            this.checkBoxCustomQueryCurrentStatus.AutoSize = true;
            this.checkBoxCustomQueryCurrentStatus.Location = new System.Drawing.Point(838, 19);
            this.checkBoxCustomQueryCurrentStatus.Name = "checkBoxCustomQueryCurrentStatus";
            this.checkBoxCustomQueryCurrentStatus.Size = new System.Drawing.Size(94, 17);
            this.checkBoxCustomQueryCurrentStatus.TabIndex = 4;
            this.checkBoxCustomQueryCurrentStatus.Text = "Current status:";
            this.checkBoxCustomQueryCurrentStatus.UseVisualStyleBackColor = true;
            this.checkBoxCustomQueryCurrentStatus.CheckedChanged += new System.EventHandler(this.checkBoxCustomQueryCurrentStatus_CheckedChanged);
            // 
            // checkBoxCustomQueryReporter
            // 
            this.checkBoxCustomQueryReporter.AutoSize = true;
            this.checkBoxCustomQueryReporter.Location = new System.Drawing.Point(616, 20);
            this.checkBoxCustomQueryReporter.Name = "checkBoxCustomQueryReporter";
            this.checkBoxCustomQueryReporter.Size = new System.Drawing.Size(70, 17);
            this.checkBoxCustomQueryReporter.TabIndex = 3;
            this.checkBoxCustomQueryReporter.Text = "Reporter:";
            this.checkBoxCustomQueryReporter.UseVisualStyleBackColor = true;
            this.checkBoxCustomQueryReporter.CheckedChanged += new System.EventHandler(this.checkBoxCustomQueryReporter_CheckedChanged);
            // 
            // checkBoxCustomQueryOwner
            // 
            this.checkBoxCustomQueryOwner.AutoSize = true;
            this.checkBoxCustomQueryOwner.Location = new System.Drawing.Point(411, 20);
            this.checkBoxCustomQueryOwner.Name = "checkBoxCustomQueryOwner";
            this.checkBoxCustomQueryOwner.Size = new System.Drawing.Size(60, 17);
            this.checkBoxCustomQueryOwner.TabIndex = 2;
            this.checkBoxCustomQueryOwner.Text = "Owner:";
            this.checkBoxCustomQueryOwner.UseVisualStyleBackColor = true;
            this.checkBoxCustomQueryOwner.CheckedChanged += new System.EventHandler(this.checkBoxCustomQueryOwner_CheckedChanged);
            // 
            // checkBoxCustomQueryProject
            // 
            this.checkBoxCustomQueryProject.AutoSize = true;
            this.checkBoxCustomQueryProject.Location = new System.Drawing.Point(201, 20);
            this.checkBoxCustomQueryProject.Name = "checkBoxCustomQueryProject";
            this.checkBoxCustomQueryProject.Size = new System.Drawing.Size(62, 17);
            this.checkBoxCustomQueryProject.TabIndex = 1;
            this.checkBoxCustomQueryProject.Text = "Project:";
            this.checkBoxCustomQueryProject.UseVisualStyleBackColor = true;
            this.checkBoxCustomQueryProject.CheckedChanged += new System.EventHandler(this.checkBoxCustomQueryProject_CheckedChanged);
            // 
            // checkBoxCustomQueryStartTime
            // 
            this.checkBoxCustomQueryStartTime.AutoSize = true;
            this.checkBoxCustomQueryStartTime.Location = new System.Drawing.Point(7, 20);
            this.checkBoxCustomQueryStartTime.Name = "checkBoxCustomQueryStartTime";
            this.checkBoxCustomQueryStartTime.Size = new System.Drawing.Size(73, 17);
            this.checkBoxCustomQueryStartTime.TabIndex = 0;
            this.checkBoxCustomQueryStartTime.Text = "Start time:";
            this.checkBoxCustomQueryStartTime.UseVisualStyleBackColor = true;
            this.checkBoxCustomQueryStartTime.CheckedChanged += new System.EventHandler(this.checkBoxCustomQueryStartTime_CheckedChanged);
            // 
            // checkBoxAutoupdateData
            // 
            this.checkBoxAutoupdateData.AutoSize = true;
            this.checkBoxAutoupdateData.Location = new System.Drawing.Point(764, 80);
            this.checkBoxAutoupdateData.Name = "checkBoxAutoupdateData";
            this.checkBoxAutoupdateData.Size = new System.Drawing.Size(105, 17);
            this.checkBoxAutoupdateData.TabIndex = 7;
            this.checkBoxAutoupdateData.Text = "Autoupdate data";
            this.checkBoxAutoupdateData.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.checkBoxAutoupdateData.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(914, 86);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(287, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Help: double click right mouse button initiate ADD new task";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(914, 72);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(303, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Help: double click left mouse button initiate EDIT selected task";
            // 
            // buttonDeleteSelectedRow
            // 
            this.buttonDeleteSelectedRow.Enabled = false;
            this.buttonDeleteSelectedRow.Location = new System.Drawing.Point(462, 75);
            this.buttonDeleteSelectedRow.Name = "buttonDeleteSelectedRow";
            this.buttonDeleteSelectedRow.Size = new System.Drawing.Size(145, 24);
            this.buttonDeleteSelectedRow.TabIndex = 4;
            this.buttonDeleteSelectedRow.Text = "Delete selected task";
            this.buttonDeleteSelectedRow.UseVisualStyleBackColor = true;
            this.buttonDeleteSelectedRow.Click += new System.EventHandler(this.buttonDeleteSelectedRow_Click);
            // 
            // buttonAddNewRow
            // 
            this.buttonAddNewRow.Location = new System.Drawing.Point(160, 75);
            this.buttonAddNewRow.Name = "buttonAddNewRow";
            this.buttonAddNewRow.Size = new System.Drawing.Size(145, 24);
            this.buttonAddNewRow.TabIndex = 3;
            this.buttonAddNewRow.Text = "Add new task";
            this.buttonAddNewRow.UseVisualStyleBackColor = true;
            this.buttonAddNewRow.Click += new System.EventHandler(this.buttonAddNewRow_Click);
            // 
            // buttonEditSelectedRow
            // 
            this.buttonEditSelectedRow.Enabled = false;
            this.buttonEditSelectedRow.Location = new System.Drawing.Point(311, 75);
            this.buttonEditSelectedRow.Name = "buttonEditSelectedRow";
            this.buttonEditSelectedRow.Size = new System.Drawing.Size(145, 24);
            this.buttonEditSelectedRow.TabIndex = 2;
            this.buttonEditSelectedRow.Text = "Edit selected task";
            this.buttonEditSelectedRow.UseVisualStyleBackColor = true;
            this.buttonEditSelectedRow.Click += new System.EventHandler(this.buttonEditSelectedRow_Click);
            // 
            // buttonReadDataFromDB
            // 
            this.buttonReadDataFromDB.Location = new System.Drawing.Point(9, 75);
            this.buttonReadDataFromDB.Name = "buttonReadDataFromDB";
            this.buttonReadDataFromDB.Size = new System.Drawing.Size(145, 24);
            this.buttonReadDataFromDB.TabIndex = 1;
            this.buttonReadDataFromDB.Text = "Update data from database";
            this.buttonReadDataFromDB.UseVisualStyleBackColor = true;
            this.buttonReadDataFromDB.Click += new System.EventHandler(this.buttonReadDataFromDB_Click);
            // 
            // dataGridViewDataFromDB
            // 
            this.dataGridViewDataFromDB.AllowUserToAddRows = false;
            this.dataGridViewDataFromDB.AllowUserToDeleteRows = false;
            this.dataGridViewDataFromDB.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGridViewDataFromDB.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewDataFromDB.Location = new System.Drawing.Point(9, 105);
            this.dataGridViewDataFromDB.MultiSelect = false;
            this.dataGridViewDataFromDB.Name = "dataGridViewDataFromDB";
            this.dataGridViewDataFromDB.ReadOnly = true;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewDataFromDB.RowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewDataFromDB.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewDataFromDB.Size = new System.Drawing.Size(1208, 647);
            this.dataGridViewDataFromDB.TabIndex = 0;
            this.dataGridViewDataFromDB.DoubleClick += new System.EventHandler(this.dataGridViewDataFromDB_DoubleClick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(1249, 831);
            this.Controls.Add(this.groupBoxViewDataFromDB);
            this.Controls.Add(this.groupBoxConnectToDB);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Task Planning";
            this.Activated += new System.EventHandler(this.Form1_Activated);
            this.groupBoxConnectToDB.ResumeLayout(false);
            this.groupBoxConnectToDB.PerformLayout();
            this.groupBoxViewDataFromDB.ResumeLayout(false);
            this.groupBoxViewDataFromDB.PerformLayout();
            this.groupBoxCustomQueryToDB.ResumeLayout(false);
            this.groupBoxCustomQueryToDB.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDataFromDB)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion


        private GroupBox groupBoxConnectToDB;
        private Button buttonDBConnect;
        private Button buttonDBDisconnect;
        private GroupBox groupBoxViewDataFromDB;
        private Button buttonReadDataFromDB;
        private DataGridView dataGridViewDataFromDB;
        private Button buttonEditSelectedRow;
        private Button buttonAddNewRow;
        private Button buttonDeleteSelectedRow;
        private Label label2;
        private Label label1;
        private Label labelLoginInfo;
        private CheckBox checkBoxAutoupdateData;
        private GroupBox groupBoxCustomQueryToDB;
        private CheckBox checkBoxCustomQueryStartTime;
        private CheckBox checkBoxCustomQueryCurrentStatus;
        private CheckBox checkBoxCustomQueryReporter;
        private CheckBox checkBoxCustomQueryOwner;
        private CheckBox checkBoxCustomQueryProject;
        private ComboBox comboBoxCustomQueryCurrentStatus;
        private ComboBox comboBoxCustomQueryReporter;
        private ComboBox comboBoxCustomQueryOwner;
        private ComboBox comboBoxCustomQueryProject;
        private TextBox textBoxCustomQueryStartTime;
        private Button buttonExportData;
    }
}



