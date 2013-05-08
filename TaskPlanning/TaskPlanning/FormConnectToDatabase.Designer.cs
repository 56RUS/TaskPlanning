namespace TaskPlanning
{
    partial class FormConnectToDatabase
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
            this.groupBoxConnectToDB = new System.Windows.Forms.GroupBox();
            this.checkBoxRememberMe = new System.Windows.Forms.CheckBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonDBConnect = new System.Windows.Forms.Button();
            this.textBoxDBUserPswd = new System.Windows.Forms.TextBox();
            this.textBoxDBUser = new System.Windows.Forms.TextBox();
            this.textBoxDBName = new System.Windows.Forms.TextBox();
            this.textBoxDBHost = new System.Windows.Forms.TextBox();
            this.labelDBUserPswd = new System.Windows.Forms.Label();
            this.labelDBUser = new System.Windows.Forms.Label();
            this.labelDBName = new System.Windows.Forms.Label();
            this.labelDBHost = new System.Windows.Forms.Label();
            this.groupBoxConnectToDB.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxConnectToDB
            // 
            this.groupBoxConnectToDB.Controls.Add(this.checkBoxRememberMe);
            this.groupBoxConnectToDB.Controls.Add(this.buttonCancel);
            this.groupBoxConnectToDB.Controls.Add(this.buttonDBConnect);
            this.groupBoxConnectToDB.Controls.Add(this.textBoxDBUserPswd);
            this.groupBoxConnectToDB.Controls.Add(this.textBoxDBUser);
            this.groupBoxConnectToDB.Controls.Add(this.textBoxDBName);
            this.groupBoxConnectToDB.Controls.Add(this.textBoxDBHost);
            this.groupBoxConnectToDB.Controls.Add(this.labelDBUserPswd);
            this.groupBoxConnectToDB.Controls.Add(this.labelDBUser);
            this.groupBoxConnectToDB.Controls.Add(this.labelDBName);
            this.groupBoxConnectToDB.Controls.Add(this.labelDBHost);
            this.groupBoxConnectToDB.Location = new System.Drawing.Point(12, 12);
            this.groupBoxConnectToDB.Name = "groupBoxConnectToDB";
            this.groupBoxConnectToDB.Size = new System.Drawing.Size(704, 88);
            this.groupBoxConnectToDB.TabIndex = 0;
            this.groupBoxConnectToDB.TabStop = false;
            this.groupBoxConnectToDB.Text = "Connect to database:";
            // 
            // checkBoxRememberMe
            // 
            this.checkBoxRememberMe.AutoSize = true;
            this.checkBoxRememberMe.Location = new System.Drawing.Point(307, 59);
            this.checkBoxRememberMe.Name = "checkBoxRememberMe";
            this.checkBoxRememberMe.Size = new System.Drawing.Size(94, 17);
            this.checkBoxRememberMe.TabIndex = 2;
            this.checkBoxRememberMe.Text = "Remember me";
            this.checkBoxRememberMe.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(407, 55);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(287, 23);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonDBConnect
            // 
            this.buttonDBConnect.Location = new System.Drawing.Point(9, 55);
            this.buttonDBConnect.Name = "buttonDBConnect";
            this.buttonDBConnect.Size = new System.Drawing.Size(287, 23);
            this.buttonDBConnect.TabIndex = 0;
            this.buttonDBConnect.Text = "Connect";
            this.buttonDBConnect.UseVisualStyleBackColor = true;
            this.buttonDBConnect.Click += new System.EventHandler(this.buttonDBConnect_Click);
            // 
            // textBoxDBUserPswd
            // 
            this.textBoxDBUserPswd.Location = new System.Drawing.Point(594, 19);
            this.textBoxDBUserPswd.Name = "textBoxDBUserPswd";
            this.textBoxDBUserPswd.Size = new System.Drawing.Size(100, 20);
            this.textBoxDBUserPswd.TabIndex = 6;
            this.textBoxDBUserPswd.UseSystemPasswordChar = true;
            // 
            // textBoxDBUser
            // 
            this.textBoxDBUser.Location = new System.Drawing.Point(407, 19);
            this.textBoxDBUser.Name = "textBoxDBUser";
            this.textBoxDBUser.Size = new System.Drawing.Size(100, 20);
            this.textBoxDBUser.TabIndex = 5;
            // 
            // textBoxDBName
            // 
            this.textBoxDBName.Location = new System.Drawing.Point(227, 19);
            this.textBoxDBName.Name = "textBoxDBName";
            this.textBoxDBName.Size = new System.Drawing.Size(100, 20);
            this.textBoxDBName.TabIndex = 4;
            // 
            // textBoxDBHost
            // 
            this.textBoxDBHost.Location = new System.Drawing.Point(44, 19);
            this.textBoxDBHost.Name = "textBoxDBHost";
            this.textBoxDBHost.Size = new System.Drawing.Size(100, 20);
            this.textBoxDBHost.TabIndex = 3;
            // 
            // labelDBUserPswd
            // 
            this.labelDBUserPswd.AutoSize = true;
            this.labelDBUserPswd.Location = new System.Drawing.Point(532, 22);
            this.labelDBUserPswd.Name = "labelDBUserPswd";
            this.labelDBUserPswd.Size = new System.Drawing.Size(56, 13);
            this.labelDBUserPswd.TabIndex = 3;
            this.labelDBUserPswd.Text = "Password:";
            // 
            // labelDBUser
            // 
            this.labelDBUser.AutoSize = true;
            this.labelDBUser.Location = new System.Drawing.Point(353, 22);
            this.labelDBUser.Name = "labelDBUser";
            this.labelDBUser.Size = new System.Drawing.Size(48, 13);
            this.labelDBUser.TabIndex = 2;
            this.labelDBUser.Text = "DB user:";
            // 
            // labelDBName
            // 
            this.labelDBName.AutoSize = true;
            this.labelDBName.Location = new System.Drawing.Point(167, 22);
            this.labelDBName.Name = "labelDBName";
            this.labelDBName.Size = new System.Drawing.Size(54, 13);
            this.labelDBName.TabIndex = 1;
            this.labelDBName.Text = "DB name:";
            // 
            // labelDBHost
            // 
            this.labelDBHost.AutoSize = true;
            this.labelDBHost.Location = new System.Drawing.Point(6, 22);
            this.labelDBHost.Name = "labelDBHost";
            this.labelDBHost.Size = new System.Drawing.Size(32, 13);
            this.labelDBHost.TabIndex = 0;
            this.labelDBHost.Text = "Host:";
            // 
            // FormConnectToDatabase
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(727, 110);
            this.Controls.Add(this.groupBoxConnectToDB);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormConnectToDatabase";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Connect to database";
            this.Shown += new System.EventHandler(this.FormConnectToDatabase_Shown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.FormConnectToDatabase_KeyPress);
            this.groupBoxConnectToDB.ResumeLayout(false);
            this.groupBoxConnectToDB.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxConnectToDB;
        private System.Windows.Forms.Label labelDBUserPswd;
        private System.Windows.Forms.Label labelDBUser;
        private System.Windows.Forms.Label labelDBName;
        private System.Windows.Forms.Label labelDBHost;
        private System.Windows.Forms.TextBox textBoxDBUserPswd;
        private System.Windows.Forms.TextBox textBoxDBUser;
        private System.Windows.Forms.TextBox textBoxDBName;
        private System.Windows.Forms.TextBox textBoxDBHost;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonDBConnect;
        private System.Windows.Forms.CheckBox checkBoxRememberMe;
    }
}