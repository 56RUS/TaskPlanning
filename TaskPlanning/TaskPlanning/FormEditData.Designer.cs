namespace TaskPlanning
{
    partial class FormEditData
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dataGridViewAddData = new System.Windows.Forms.DataGridView();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewAddData)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridViewAddData
            // 
            this.dataGridViewAddData.AllowUserToAddRows = false;
            this.dataGridViewAddData.AllowUserToDeleteRows = false;
            this.dataGridViewAddData.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGridViewAddData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewAddData.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dataGridViewAddData.Location = new System.Drawing.Point(12, 12);
            this.dataGridViewAddData.Name = "dataGridViewAddData";
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewAddData.RowsDefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridViewAddData.Size = new System.Drawing.Size(1178, 353);
            this.dataGridViewAddData.TabIndex = 0;
            this.dataGridViewAddData.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridViewAddData_CellMouseClick);
            this.dataGridViewAddData.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewAddData_CellValueChanged);
            this.dataGridViewAddData.ColumnWidthChanged += new System.Windows.Forms.DataGridViewColumnEventHandler(this.dataGridViewAddData_ColumnWidthChanged);
            this.dataGridViewAddData.RowHeightChanged += new System.Windows.Forms.DataGridViewRowEventHandler(this.dataGridViewAddData_RowHeightChanged);
            // 
            // buttonOk
            // 
            this.buttonOk.Location = new System.Drawing.Point(12, 371);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(463, 23);
            this.buttonOk.TabIndex = 1;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(722, 371);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(463, 23);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(493, 376);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(210, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Help: to enter new line press \'Shift\' + \'Enter\'";
            // 
            // FormEditData
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(1205, 406);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.dataGridViewAddData);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormEditData";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FormEditData";
            this.Activated += new System.EventHandler(this.FormEditData_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormEditData_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormEditData_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewAddData)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridViewAddData;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label label1;
    }
}