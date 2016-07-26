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
    public partial class AddUserCompanyExecut : Form
    {

        static string conSTR = @"Data Source=NIKOLAY;Initial Catalog=OPUBase;Integrated Security=True; Pooling = True";
        static SqlConnection con = new SqlConnection(conSTR);
        static SqlCommand cmd = new SqlCommand();
        static DataTable dt = new DataTable();
        static DataSet ds = new DataSet();
        static SqlDataAdapter ad = new SqlDataAdapter();
        public int ID; //номер документа если изменяем
        public byte Index;//№ вкладки которую показываем   
        static int worcKode;
        Users user = new Users();
        Executor executor = new Executor();
        Company company = new Company();
       public static int WorcKode { get { return worcKode; } set { if (value <= 0) { MessageBox.Show("Исполнитель не найден"); } else { worcKode = value; } } }
        public class SQLClass //класс контейнер SQL запросов
        {
            protected internal const string sqlExecutorAdd= " insert into WorkList(WorkName, WorkProf) values (@workName, @WorkProf)";
            protected internal const string sqlUsersAdd = " insert into ElDocUsersAdv (WorkCode, UserLogin, UserPassWord) " +
                                " values(@WorkCode, @UserLogin, @UserPassWord)" +
                                "  SELECT @@IDENTITY as userid ";
            protected internal const string sqlUsersinGroup = " insert into DivGroupUsers (GroupID, UserID) values ({0}, {1})";
            protected internal const string sqlGetUserWorkCode1= "select workCode from WorkList where WorkName = @WorkName ";
            protected internal const string sqlGetUserWorkCode2 = " select WorkCode from ElDocUsersAdv where WorkCode ={0}";
            protected internal const string sqlCompanyComp = " insert into CompaniesList (CompanyShortName) values (@CompanyShortName) ";
            protected internal const string sqlCompanyEmploe = "  insert into ContactsList(CompanyID,LastName,FirstName,FatherName,Post) "+
                                                                  " values(@CompanyID, @LastName, @FirstName, @FatherName, @Post) ";
        }



        public class Company: SQLClass
        {
            public void AddCompany(string CompanyName)
            {
                using (cmd = new SqlCommand(sqlCompanyComp, con))
                {
                    try
                    {
                        con.Open();
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@CompanyShortName", CompanyName);
                        cmd.ExecuteNonQuery();
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show("Ошибка с сообщением: "+ex.Message);
                    }
                    finally
                    {
                        con.Close();                        
                    }
                }


            }
            public void AddEmployee()
            {

            }
        }

        public class Executor : SQLClass
        {
            public void Add(string workName, string Workprof)
            {
               
                using (cmd = new SqlCommand(sqlExecutorAdd, con))
                {
                    try
                    {
                        con.Open();
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@workName", workName);
                        cmd.Parameters.AddWithValue("@WorkProf", Workprof);
                        cmd.ExecuteNonQuery();

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка с сообщением: " + ex.Message);
                    }
                    finally
                    {
                        con.Close();
                    }

                }
            }

        }
        public class Users : SQLClass
        {
             Int32 userId;
             byte groupId;

            public void Add(string userlogin, string userpassword)
            {
                 using (cmd = new SqlCommand(sqlUsersAdd, con))
                {
                    try
                    {
                        con.Open();
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@WorkCode", WorcKode);
                        cmd.Parameters.AddWithValue("@UserLogin", userlogin);
                        cmd.Parameters.AddWithValue("@UserPassWord", userpassword);
                        userId =Convert.ToInt32(cmd.ExecuteScalar());
                        con.Close();
                        using (cmd = new SqlCommand(String.Format(sqlUsersinGroup, groupId, userId),con))
                        {
                            con.Open();
                            cmd.ExecuteNonQuery();
                            AddUserCompanyExecut.ActiveForm.DialogResult = DialogResult.OK;
                        }                                    
                    }
                    catch (Exception ex)
                    {
                     
                        MessageBox.Show("Ошибка с сообщением: "+ex.Message);
                    }
                    finally
                    {
                        con.Close();
                    }
                }
                
            }         
                  


            public void GetCode(string username)
            {              
               
                using (cmd = new SqlCommand(sqlGetUserWorkCode1, con))
                {
                    cmd.Parameters.Add("@WorkName", SqlDbType.VarChar);
                    cmd.Parameters["@WorkName"].Value = username;
                    try
                    {
                        con.Open();
                        WorcKode = Convert.ToInt32(cmd.ExecuteScalar());
                        using (cmd = new SqlCommand(String.Format(sqlGetUserWorkCode2, WorcKode), con))
                        {
                            if(Convert.ToInt32(cmd.ExecuteScalar())!=0)
                            {
                                MessageBox.Show("Такой пользователь или Login уже существует ", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                               
                            } 
                            else
                            {
                               
                            }                       

                        }
                       
                    }

                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    finally
                    {
                        con.Close();
                    }                    
                }
            }   
            
            public void GetGroupId(string Group)
            {
                groupId= Convert.ToByte(Group);
            }      
        }                    
     

       
       
        private void textBox2_MouseClick(object sender, MouseEventArgs e)
        {
            TextBox txt = sender as TextBox;
            txt.Text = "";              
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;        
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex != Index)
            {
                button1.Enabled = false;
                button2.Enabled = false;
            }
            else
            {
                button1.Enabled = true;
                button2.Enabled = true;
            }          
        }

        public AddUserCompanyExecut(int id, byte index, string text)
        {
            InitializeComponent();
            ID = id;//номер документа если изменяем
            Text = text; //имя формы
            Index = index;
            tabControl1.SelectedIndex = Index;//№ вкладки которую показываем          
            switch (index)
            {
                case 0: panel1.Visible = true; break;
                case 1: panel2.Visible = true; break;
                case 2: panel3.Visible = true; break;
            }

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
                     
            switch (tabControl1.SelectedIndex)
            {
                #region 
                case 0: user.Add(textBox2.Text, textBox3.Text); break; 
                case 1: executor.Add(textBox5.Text, textBox6.Text); break;
                case 2: company.AddCompany(textBox7.Text); break;
                #endregion
            }
            this.DialogResult = DialogResult.OK;
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
           
        }

        private void AddUserCompanyExecut_Load(object sender, EventArgs e)
        {
            this.groupsTableAdapter.Fill(this.oPUBaseDataSet.Groups);         
        }

        private void groupsCB_Leave(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            user.GetGroupId(cb.SelectedValue.ToString());        
        }

        private void bindingNavigator1_RefreshItems(object sender, EventArgs e)
        {

        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }

        private void textBox1_Layout(object sender, LayoutEventArgs e)
        {
            
        }
    }
}
