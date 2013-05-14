﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TaskPlanning
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                Application.Run(new Form1());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Program runtime error!");
            }
        }
    }
}
