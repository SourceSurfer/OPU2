using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OPU
{
    public partial class UserLogin : Form
    {
        SqlConnectionStringBuilder builder =
            new SqlConnectionStringBuilder();
        SqlConnection con = new SqlConnection();
       



        public UserLogin()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
                builder.DataSource = @"NIKOLAY";
                builder.InitialCatalog = "OPUBase";
                builder.UserID = textBox1.Text;
                builder.Password = textBox2.Text;
            try
            {
                con.ConnectionString = builder.ConnectionString;
                con.Open();
                button1.DialogResult = DialogResult.OK;             

            }
            catch (Exception ex)
            {
                MessageBox.Show("Не правильнно введен логин или пароль " + ex.Message);
            }
            finally
            {
                con.Close();
            }
        

        }
     

        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Для восстановления пароля, обратитесь в ОПУ");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            button3.DialogResult = DialogResult.Cancel;
            this.Close();        
        }
    }
}
