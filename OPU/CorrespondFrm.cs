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
using System.Configuration;

namespace OPU
{
    public partial class CorrespondFrm : Form
    {
        public CorrespondFrm(int Index)
        {
            InitializeComponent();
            tabControl1.SelectedIndex = Index;
           
        }

        
        static string conSTR = @"Data Source=NIKOLAY;Initial Catalog=OPUBase;Integrated Security=True; Pooling = True";
        public static SqlConnection con = new SqlConnection(conSTR);
        static SqlCommand cmd = new SqlCommand();
        static DataTable dt = new DataTable();
        static DataTable dt1 = new DataTable();//для вывода групп в дереве
        static DataTable dt2 = new DataTable();//для вывода пользователей в дереве
        static SqlDataAdapter ad = new SqlDataAdapter();
        static DataSet ds = new DataSet();
        Company company = new Company();
        Users user = new Users();
        Executor executor = new Executor();
        public static int ID { get; set; } //id документа, пользователя и исполнителя из Гриды или дерева
        public static string workName { get; set; } //имя пользователя, исполнителя, имя компании или имя сотрудника в компании
        public static int GroupID { get; set; } //получаем GroupID в которой находится сотрудник

        //класс контейнер для SQL запросов и сообщений
       public class SQLClass
        {
            protected internal const string sqlExecutShow= "select WorkCode, WorkName, WorkProf from WorkList";
            protected internal const string sqlExecutDelete = "delete from WorkList where WorkCode = {0}";
            protected internal const string msgExecuteAndUsersDel = "Вы действительно хотите удалить: {0} ?";
            protected internal const string sqlUsersShow = " select ROW_NUMBER() OVER(ORDER BY el.WorkCode DESC) AS Row," +
                                " wl.WorkCode, el.UserID, el.UserLogin, wl.WorkName, wl.WorkProf, gr.GroupName from DivGroupUsers dvu " +
                                " inner join ElDocUsersAdv el on el.UserID = dvu.UserID  " +
                                " left join WorkList wl on wl.WorkCode = el.WorkCode" +
                                " left join Groups gr on gr.GroupID = dvu.GroupID ";
            protected internal const string sqlUsersDel = "delete from ElDocUsersAdv  where WorkCode = {0} ";
            protected internal const string sqlUsersAdd = "  insert into DivGroupUsers (GroupID, UserID) values ({0}, {1})";
            protected internal const string sqlTreeDivision = "select DivID , DivName from Divisions";
            protected internal const string sqlTreeGroups = " select GroupID, GroupName from Groups where DivID = {0}";
            protected internal const string sqlTreeUserinGroup = " select  wl.WorkCode, el.UserLogin, wl.WorkName, wl.WorkProf, dvu.GroupID from DivGroupUsers dvu " +
                                " left join ElDocUsersAdv el on el.UserID = dvu.UserID " +
                                " left join WorkList wl on wl.WorkCode = el.WorkCode " +
                                " where dvu.GroupID = {0} order by wl.WorkName ";  //для вывода пользователей состоящих в конкретных группах
            protected internal const string sqlCompanyShowEmpl = " select ContactID, [LastName]  ,[FirstName]  ,[FatherName]  ,[Post]  ,[Phone1]" +
                               " FROM[OPUBase].[dbo].[ContactsList] cl " +
                               " where cl.CompanyID = {0}";
            protected internal const string sqlCompanyShowComp = " SELECT [CompanyID], [CompanyShortName] FROM[OPUBase].[dbo].[CompaniesList]" +
                                " where CompanyShortName like '%санкт%' or CompanyShortName like '%спб%'";
            protected internal const string sqlCompanyDelete = "  delete from CompaniesList   where CompanyID = {0};" +
                                " delete from ContactsList where CompanyID = {0}";
        }
            
      



        public class Executor: SQLClass
        {
            
            public void Show(DataGridView GridView, ToolStripItem StatusLabel)
            {
                
                using (cmd = new SqlCommand(sqlExecutShow, con))
                {
                    
                    try
                    {
                        con.Open();
                        using (dt = new DataTable())
                        {                          
                            cmd.ExecuteNonQuery();
                            ad.SelectCommand = cmd;
                            ad.Fill(dt);
                            StatusLabel.Text = Convert.ToString(dt.Rows.Count) + " строк";
                            GridView.DataSource = dt;         
                          
                        }
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
           
            public void Delete()
            {
                string comStr = String.Format(sqlExecutDelete, ID);
                string msg = String.Format(msgExecuteAndUsersDel, workName);

                var result = MessageBox.Show(msg, "Удалить", MessageBoxButtons.OKCancel,  MessageBoxIcon.Question);
                if (result == DialogResult.OK)
                {
                    using (cmd = new SqlCommand(comStr, con))
                    {
                        con.Open();
                        try
                        {
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
        }

        public class Users:SQLClass
        {
         
           
            public void Show(DataGridView GridView, ToolStripItem StatusLabel)
            {
               
                using (cmd = new SqlCommand(sqlUsersShow, con))
                {
                    try
                    {
                        con.Open();
                        using (dt = new DataTable())
                        {
                            cmd.ExecuteNonQuery();
                            ad.SelectCommand = cmd;
                            ad.Fill(dt);
                            StatusLabel.Text =Convert.ToString(dt.Rows.Count)+ " строк";
                            GridView.DataSource = dt;                                                       
                        }
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
            public void ShowUserList(DataGridView GridView, int DivId, int GroupID, ToolStripStatusLabel StatusLabel)
            {
                using (cmd = new SqlCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "sp_ShowUserList";
                    cmd.Parameters.Clear(); //для очистки коллекции передающих параметров
                    cmd.Parameters.AddWithValue("@GroupID", GroupID);
                    cmd.Parameters.AddWithValue("@DivId", DivId);
                    cmd.Parameters.AddWithValue("@WorkCode", ID);
                    try
                    {
                        con.Open();
                        using (dt = new DataTable())
                        {

                            cmd.ExecuteNonQuery();
                            ad.SelectCommand = cmd;
                            ad.Fill(dt);
                            StatusLabel.Text = Convert.ToString(dt.Rows.Count)+" строк";
                            GridView.DataSource = dt;
                            con.Close();
                        }
                       

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
            //удаляет по кнопке Удалить и по Клавише Del
            public void Delete()
            {

                 string comStr = String.Format(sqlUsersDel, ID);
                 string msg = String.Format(msgExecuteAndUsersDel, workName);

                var result = MessageBox.Show(msg, "Удалить", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (result == DialogResult.OK)
                {
                    using (cmd = new SqlCommand(comStr, con))
                    {
                        con.Open();
                        try
                        {
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
            
            public void Add()
            {
                string  comStr =String.Format(sqlUsersAdd, GroupID,ID);
                using (cmd = new SqlCommand(comStr, con))
                {
                    try
                    {
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show("Сообщение с ошибкой "+ex.Message);
                    }
                    finally
                    {
                        con.Close();
                    }
                   

                }
            }       

            
        }

        public class Tree : SQLClass
        {           
            public void Show(TreeView treeView, ToolStripItem StatusLabel)
            {
                treeView.Nodes.Clear();
                try
                {
                    using (ad = new SqlDataAdapter(sqlTreeDivision, con))
                    {
                        treeView.BeginUpdate();
                        con.Open();
                        using (dt = new DataTable())
                        {
                            ad.Fill(dt);

                            for (int i = 0; dt.Rows.Count > i; i++)
                            {

                                treeView.Nodes.Add(Convert.ToString(dt.Rows[i][0]), (string)dt.Rows[i][1]);
                                using (ad = new SqlDataAdapter(String.Format(sqlTreeGroups, i + 1), con))
                                {
                                    dt1.Clear();
                                    ad.Fill(dt1);

                                    for (int c = 0; dt1.Rows.Count > c; c++)
                                    {
                                        treeView.Nodes[i].Nodes.Add(Convert.ToString(dt1.Rows[c][0]), (string)dt1.Rows[c][1]);
                                        using (ad = new SqlDataAdapter(String.Format(sqlTreeUserinGroup, dt1.Rows[c][0]), con))
                                        {
                                            dt2.Clear();
                                            ad.Fill(dt2);
                                            for (int t = 0; dt2.Rows.Count > t; t++)
                                            {
                                                treeView.Nodes[i].Nodes[c].Nodes.Add(Convert.ToString(dt2.Rows[t][0]), Convert.ToString(dt2.Rows[t][2]));
                                            }
                                        }


                                    }
                                }


                            }
                        }


                    }
                    treeView.EndUpdate();
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

        public class Company: SQLClass
        {
            
            public void ShowEmployee(DataGridView GridView)
            {
                using (cmd = new SqlCommand(String.Format(sqlCompanyShowEmpl, ID), con))
                {
                    try
                    {
                        using (dt = new DataTable())
                        {
                            con.Open();
                            cmd.ExecuteNonQuery();
                            ad.SelectCommand = cmd;
                            ad.Fill(dt);
                            GridView.DataSource = dt;
                           
                        }
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

            public void ShowCompany(DataGridView GridView, ToolStripItem StatusLabel)
            {
                                          
                using (cmd = new SqlCommand(sqlCompanyShowComp, con))
                {
                    try
                    {
                        using (dt = new DataTable())
                        {
                            con.Open();
                            cmd.ExecuteNonQuery();
                            ad.SelectCommand = cmd;
                            ad.Fill(dt);
                            StatusLabel.Text = Convert.ToString(dt.Rows.Count) + " строк";
                            con.Close(); //не удалять
                            GridView.DataSource = dt;                           
                        }
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
            public void Delete()
            {
                string msg = String.Format("Вы действительно хотите удалить: {0} ? {1}", workName,"\n"+"Удалить сотрудника нельзя, только организацию");

                var result = MessageBox.Show(msg, "Удалить", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (result == DialogResult.OK)
                {
                     using (cmd = new SqlCommand(String.Format(sqlCompanyDelete, ID), con))
                    {
                        try
                        {
                            con.Open();
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
            
            public void Find()
            {
                /*
                create table #tabl -- создание временной таблицы
    (ID int,
    Name varchar(255))
 
insert into #tabl   -- внос инфы во временную таблицу
select WorkCode, WorkName name from WorkList
insert into #tabl 
select WorkCode, UserLogin  from ElDocUsersAdv
insert into #tabl
select WorkCode, WorkProf from WorkList
insert into #tabl
select GroupID, GroupName  from Groups


select * from #tabl 
where Name = 'Запесоцкий А.С.'

drop table #tabl*/
            }


        }




        private void CorrespondFrm_Load(object sender, EventArgs e)
        {         
            Tree tree = new Tree();
                     
            company.ShowCompany(CompanyGridView, toolStripStatusLabel1);
            executor.Show(ExecutGridView, toolStripStatusLabel1);
            user.Show(userGridView, toolStripStatusLabel1);
            tree.Show(treeView1, toolStripStatusLabel1);
        }          

        private void Companydtv_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridView dg = sender as DataGridView;

            ID = (!DBNull.Value.Equals(dg["CompanyID", dg.CurrentRow.Index].Value)) ? Convert.ToInt32(dg["CompanyID", dg.CurrentRow.Index].Value) : 0;
            workName = (!DBNull.Value.Equals(dg["CompanyShortName", dg.CurrentRow.Index].Value)) ? Convert.ToString(dg["CompanyShortName", dg.CurrentRow.Index].Value) : "";
            company.ShowEmployee(EmploeGridView);
        }
          

        private void Companydtv_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dg = sender as DataGridView;
            
            ID = (!DBNull.Value.Equals(dg["CompanyID", dg.CurrentRow.Index].Value)) ? Convert.ToInt32(dg["CompanyID", dg.CurrentRow.Index].Value) : 0;
            workName= (!DBNull.Value.Equals(dg["CompanyShortName", dg.CurrentRow.Index].Value)) ? Convert.ToString(dg["CompanyShortName", dg.CurrentRow.Index].Value) : "";
            company.ShowEmployee(EmploeGridView);
        }

       

        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem index = sender as ToolStripMenuItem;
            AddUserCompanyExecut update;
            using (update = new AddUserCompanyExecut(ID,(byte)tabControl1.SelectedIndex, index.Text))
            {
                update.ShowDialog();
            }        
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
           

            switch ((byte)tabControl1.SelectedIndex)
            {
                case 0: user.Delete(); user.Show(userGridView, toolStripStatusLabel1); break;
                case 1: executor.Delete(); executor.Show(ExecutGridView, toolStripStatusLabel1); break;
                case 2:
                   
                    company.Delete(); company.ShowCompany(CompanyGridView, toolStripStatusLabel1); break;
            }            
        }

        private void AddToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem index = sender as ToolStripMenuItem;
            AddUserCompanyExecut Add;
          
            Tree refresh = new Tree();
            ID = 0;
  //ID - workCode Пользователя или Исполнителя если добавляем = 0, выбранный TabIndex, название выполняемой операции (Добавить, Изменить)
            using (Add = new AddUserCompanyExecut(ID, (byte)tabControl1.SelectedIndex, index.Text))
            {
                if (Add.ShowDialog(this) == DialogResult.OK)
                {
                    refresh.Show(treeView1, toolStripStatusLabel1);
                    user.Show(userGridView, toolStripStatusLabel1);
                    executor.Show(ExecutGridView, toolStripStatusLabel1);
                }                          
            }           
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
          
            int divID = 0;                 
          
         
            if (e.Node.Level == 2) //для того что бы выполнить поиск по WorkCode и не трогать остальные параметры
            {
                ID = Convert.ToInt32(e.Node.Name);
                GroupID = Convert.ToInt16(e.Node.Parent.Name);
            }
            else
            {
                if (e.Node.Parent != null && e.Node.Parent.GetType() == typeof(TreeNode))
                {
                    divID = Convert.ToInt32(e.Node.Parent.Name);
                    GroupID = Convert.ToByte(e.Node.Name);
                    ID = 0;
                }                
            }
            user.ShowUserList(userGridView, divID, GroupID, toolStripStatusLabel1);
        }

       
        private void toolStripTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            MessageBox.Show(Convert.ToString(e.KeyChar));
        }
      

        private void ExecutGridView_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridView dg = sender as DataGridView;
        
              
            ID = (!DBNull.Value.Equals(dg[0, dg.CurrentRow.Index].Value)) ? Convert.ToInt32(dg[0, dg.CurrentRow.Index].Value) : 0;
            workName= (!DBNull.Value.Equals(dg[1, dg.CurrentRow.Index].Value)) ? Convert.ToString(dg[1, dg.CurrentRow.Index].Value) : "";
         
        }

        private void userGridView_MouseDown(object sender, MouseEventArgs e)
        {          
            DoDragDrop(userGridView["WorkName", userGridView.CurrentRow.Index].Value, DragDropEffects.Copy);
        }

        private void treeView1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;         
        }

        private void treeView1_DragDrop(object sender, DragEventArgs e)
        {            
            treeView1.Nodes[0].Nodes[GroupID].Nodes.Add(e.Data.GetData(DataFormats.Text).ToString());
            user.Add();
        }


        private void textBox1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            Tree tree = new Tree();
            if (e.KeyCode == Keys.Delete)
            {
                user.Delete();
                tree.Show(treeView1, toolStripStatusLabel1);
            }
                        
        }

      
        private void EmploeGridView_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridView dg = sender as DataGridView;
            
            ID = (!DBNull.Value.Equals(dg["ContactID", dg.CurrentRow.Index].Value)) ? Convert.ToInt32(dg["ContactID", dg.CurrentRow.Index].Value) : 0;
            workName = (!DBNull.Value.Equals(dg["LastName", dg.CurrentRow.Index].Value)) ? Convert.ToString(dg["LastName", dg.CurrentRow.Index].Value) : "";
           
        }

        private void userGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode==Keys.Delete)
            {
                user.Delete();
            }
        }

        private void userGridView_MouseDown_1(object sender, MouseEventArgs e)
        {
            DoDragDrop(userGridView["WorkName", userGridView.CurrentRow.Index].Value, DragDropEffects.Copy);
            ID = (! DBNull.Value.Equals(userGridView["UserID", userGridView.CurrentRow.Index].Value)) ? Convert.ToInt32(userGridView["UserID", userGridView.CurrentRow.Index].Value) : 0; 
        }

        private void treeView1_AfterExpand(object sender, TreeViewEventArgs e)
        {
            GroupID = Convert.ToInt32(e.Node.Name);
            if (e.Node.Parent != null && e.Node.Parent.GetType() == typeof(TreeNode))
            {
                MessageBox.Show(Convert.ToString(e.Node.Parent.Name));
              //  MessageBox.Show(Convert.ToString(e.Node.Parent));
                MessageBox.Show(Convert.ToString(e.Node.Level));
               // MessageBox.Show(Convert.ToString(e.Node.Index));
            }
           

        }

        private void найтиToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
