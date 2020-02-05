﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for BrowseOpenItems.xaml
    /// </summary>
    public partial class BrowseOpenItems : Page
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
        private string[] arr;                       //local variable to store login-based user data
        private DataRowView priorBySystemRow;       //local variable to store the row of data in the 'Prioritization by System' DataGrid
        private string reportQuery;

        public BrowseOpenItems(string[] user_data)
        {
            InitializeComponent();
            arr = user_data;
            Helper.FillSystemComboBox(SystemComboBox);
            SystemComboBox.SelectedIndex = 0;
            BindDataGrid(ReportHelper.SystemChosen(SystemComboBox));
        }

        public void BindDataGrid(string sys)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    string query;
                    if (ReportHelper.SystemChosen(SystemComboBox) == "All")
                    {
                         query = "SELECT ID, Priority_Number, Sys_Impact as [System], [Status], Category, TFS_BC_HDFS_Num as BID_ID, " +
                                        "Assigned_To as [Owner], FORMAT(Opened_Date,'MM/dd/yyyy'), Title, Req_Name AS Req, " +
                                        "Impact, IIf(Completed_Date Is Not Null, DATEDIFF(DAY, Opened_Date, Completed_Date), DATEDIFF(DAY, Opened_Date, Getdate())) as [Days] " +
                                        "FROM New_Issues WHERE ([Status] NOT LIKE '%closed%' AND [Status] NOT LIKE '%implemented%' AND [Status] NOT LIKE '%dropped%' AND [Status] NOT LIKE '%deferred%') " +
                                        "ORDER BY Priority_Number ASC;";
                    }
                    else
                    {
                         query = "SELECT ID, Priority_Number, Sys_Impact as [System], [Status], Category, TFS_BC_HDFS_Num as BID_ID, " +
                                        "Assigned_To as [Owner], FORMAT(Opened_Date,'MM/dd/yyyy'), Title, Req_Name AS Req, " +
                                        "Impact, IIf(Completed_Date Is Not Null, DATEDIFF(DAY, Opened_Date, Completed_Date), DATEDIFF(DAY, Opened_Date, Getdate())) as [Days] " +
                                        "FROM New_Issues WHERE ([Status] NOT LIKE '%closed%' AND [Status] NOT LIKE '%implemented%' AND [Status] NOT LIKE '%dropped%' AND [Status] NOT LIKE '%deferred%') " +
                                        "AND Sys_Impact = '" + ReportHelper.SystemChosen(SystemComboBox) + "' " +
                                        "ORDER BY Priority_Number ASC;";
                    }

                    reportQuery = query;

                    con.Open();
                    SqlCommand cmd = new SqlCommand(query, con);

                    DataTable dt = new DataTable();
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    using (sda)
                    {
                        sda.Fill(dt);
                    }
                    Report.ItemsSource = dt.DefaultView;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                finally
                {
                    con.Close();
                }
        }
            private void EditButton_Click(object sender, RoutedEventArgs e)
            {
                try
                {
                    //On Edit Button click, pulls the data from that row of the datagrid, and stores it as a DataRowView object
                    priorBySystemRow = (DataRowView)((Button)e.Source).DataContext;
                    List<int> IDList = Helper.FillIDList(reportQuery);

                // this PrioritizeBySystemPage, is being passed so it can be updated
                //priorBySystemRow is a DataRowView object containing the data from that row of PBS datagrid
                EditRecord editRecord = new EditRecord(this, arr, priorBySystemRow, IDList);
                    editRecord.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            }

        private void SystemComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BindDataGrid(ReportHelper.SystemChosen(SystemComboBox));
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
                try
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(reportQuery, con); //uses query generated in BindDataGrid to fill the dataTable 

                    DataTable reports = new DataTable();
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    using (sda)
                    {
                        sda.Fill(reports);
                    }

                    Helper.ToExcelClosedXML(reports);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

                finally
                {
                    con.Close();
                }
        }
    }
}