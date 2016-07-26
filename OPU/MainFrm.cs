using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OPU
{
    public partial class MainFrm : Form
    {
        static SqlDataAdapter adapter = new SqlDataAdapter();
        static SqlCommand cmd = new SqlCommand();
        static DataTable dt = new DataTable();
        static string conSTR = @"Data Source=localhost; Initial Catalog=OPUBase;Integrated Security=True; Pooling = True";
        static SqlConnection con = new SqlConnection(conSTR);
        static SqlDataAdapter da = new SqlDataAdapter();
        static DataSet doclist = new DataSet();
        static DataSet ds = new DataSet();
        static DataSet docType = new DataSet();

        public class SQLClass //класс контейнер SQL запросов
        {
            protected internal const string sqlLoadDB0 = "select DivID , DivName from Divisions ";
            protected internal const string sqlLoadDB1 = "select DocID, DocBrief from DocList  where (DATEADD(MM,-12,GETDATE()) < DocDate ) and (DivID = {0}) and (DocCode = {1})";
            protected internal const string sqlLoadDB2 = " select distinct h.DocCode, dt.DocName from DocList h  left join DocType dt on dt.DocCode = h.DocCode " +
                                 " where  (DATEADD(MM,-12,GETDATE()) < DocDate ) and (DivID = {0} )";
            
        }



        public MainFrm()
        {
            InitializeComponent();
            LoadDB LDB = new LoadDB();
            LDB.LoadDocList(treeView1);
        }
         public int Index { get; set; }

        private void MainFrm_Load(object sender, EventArgs e)
        {
           /* UserLogin userLog = new UserLogin();
            if (userLog.ShowDialog() == DialogResult.Cancel)
            {
                Application.Exit();
            }   */      

        }

        //Показывает дерево
        public class LoadDB : SQLClass
        {

            public void LoadDocList(TreeView treeView)
            {       
                
                try
                {
                    con.Open();
                    using (da = new SqlDataAdapter(sqlLoadDB0, con))
                    {
                        da.Fill(ds);
                        for (int i = 0; ds.Tables[0].Rows.Count > i; i++)
                        {
                            treeView.Nodes.Add(Convert.ToString(ds.Tables[0].Rows[i][0]), (string)ds.Tables[0].Rows[i][1]);
                            docType.Clear();

                            using (da = new SqlDataAdapter(String.Format(sqlLoadDB2, i + 1), con))
                            {
                                da.Fill(docType);

                                for (int c = 0; docType.Tables[0].Rows.Count > c; c++)
                                {
                                    treeView.Nodes[i].Nodes.Add(Convert.ToString(docType.Tables[0].Rows[c][0]), (string)docType.Tables[0].Rows[c][1]);
                                    doclist.Clear();

                                    using (da = new SqlDataAdapter(String.Format(sqlLoadDB1, i + 1, docType.Tables[0].Rows[c][0]), con))
                                    {
                                        da.Fill(doclist);

                                        for (int d = 0; doclist.Tables[0].Rows.Count > d; d++)
                                        {
                                            treeView.Nodes[i].Nodes[c].Nodes.Add(Convert.ToString(doclist.Tables[0].Rows[d][0]), (string)doclist.Tables[0].Rows[d][1]);
                                        }
                                    }
                                }
                            }
                        }
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

        public class ShowDocumentList
        {
            public void ShowDocList(DataGridView dGV, byte DocCode, int DivId, int docID, ToolStripStatusLabel StatusLabel)
            {
                cmd.Connection = con;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_ShowDocList";
                cmd.Parameters.Clear(); //для очистки коллекции передающих параметров
                cmd.Parameters.AddWithValue("@DocCode", DocCode);
                cmd.Parameters.AddWithValue("@DivId", DivId);
                cmd.Parameters.AddWithValue("@docId", docID);
                try
                {
                    con.Open();
                    using (dt = new DataTable())
                    {

                        cmd.ExecuteNonQuery();
                        adapter.SelectCommand = cmd;
                        adapter.Fill(dt);
                        StatusLabel.Text = "Количество строк: " + Convert.ToString(dt.Rows.Count);
                        dGV.DataSource = dt;
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

       
        //ДЛЯ ОБНОВЛЕНИЯ ДЕРЕВА 
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ShowDocumentList showDocument = new ShowDocumentList();

            byte doccode = 0;
            int divID = 0;
            int docID = 0;

            if (e.Node.Level == 2) //для того что бы выполнить поиск по DocID и не трогать остальные параметры
            {
                docID = Convert.ToInt32(e.Node.Name);
            }
            else
            {
                if (e.Node.Parent != null && e.Node.Parent.GetType() == typeof(TreeNode))
                {
                    divID = Convert.ToInt32(e.Node.Parent.Name);
                    doccode = Convert.ToByte(e.Node.Name);
                }
            }
            showDocument.ShowDocList(dataGridView1, doccode, divID, docID, toolStripStatusLabel1);

        }      

        private void UserToolStripMenuItem_Click(object sender, EventArgs e)
        {           
            CorrespondFrm corFrm;
            using (corFrm = new CorrespondFrm(0))
            {
                corFrm.ShowDialog();
            }             

        }

        private void ExecutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CorrespondFrm corFrm;
            using (corFrm = new CorrespondFrm(1))
            {
                corFrm.ShowDialog();
            }
        }

        private void CompanyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CorrespondFrm corFrm;
            using (corFrm = new CorrespondFrm(2))
            {
                corFrm.ShowDialog();
            }
        }

        private void просмотрДокументовToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
