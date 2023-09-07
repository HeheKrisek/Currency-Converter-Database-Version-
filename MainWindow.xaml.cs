using System;
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
using System.Data;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Configuration;

namespace CurrencyConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SqlConnection con = new SqlConnection();
        SqlCommand cmd = new SqlCommand();
        SqlDataAdapter da = new SqlDataAdapter();

        private int currencyID = 0;
        private double fromAmount = 0;
        private double toAmount = 0;

        public MainWindow()
        {
            InitializeComponent();
            BindCurrency();
            GetData();
        }

        public void MyCon()
        {
            string conn = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            con = new SqlConnection(conn);
            con.Open();
        }

        private void BindCurrency()
        {
            //DataTable dtCurrency = new DataTable();
            //dtCurrency.Columns.Add("Text");
            //dtCurrency.Columns.Add("Value");

            //dtCurrency.Rows.Add("---SELECT---", 0);
            //dtCurrency.Rows.Add("PLN", 1);
            //dtCurrency.Rows.Add("USD", 4);
            //dtCurrency.Rows.Add("EUR", 4.5);

            MyCon();
            DataTable dataTable = new DataTable();
            cmd = new SqlCommand("select ID, CurrencyName from Currencies", con);
            cmd.CommandType = CommandType.Text;
            da = new SqlDataAdapter(cmd);
            da.Fill(dataTable);

            DataRow newRow = dataTable.NewRow();
            newRow["ID"] = 0;
            newRow["CurrencyName"] = "---SELECT---";

            dataTable.Rows.InsertAt(newRow, 0);

            if (dataTable != null && dataTable.Rows.Count > 0)
            {
                from.ItemsSource = dataTable.DefaultView;
                to.ItemsSource = dataTable.DefaultView;
            }
            con.Close();
;
            from.DisplayMemberPath = "CurrencyName";
            from.SelectedValuePath = "ID";
            from.SelectedIndex = 0;

            to.DisplayMemberPath = "CurrencyName";
            to.SelectedValuePath = "ID";
            to.SelectedIndex = 0;
        }

        private void Clear_Button_Click(object sender, RoutedEventArgs e)
        {
            result.Text = string.Empty;
            amount.Text = string.Empty;
            from.SelectedIndex = 0;
            to.SelectedIndex = 0;
            from_output.Content = $"Result:";
            to_output.Content = $".";
            amount.Focus();
        }

        private void Convert_Button_Click(object sender, RoutedEventArgs e)
        {
            double converted;

            if (amount.Text == null || amount.Text.Trim() == "")
            {
                MessageBox.Show("Please enter the amount.", "Eyy", MessageBoxButton.OK, MessageBoxImage.Information);
                amount.Focus();
                return;
            }

            else if (from.SelectedValue == null || from.SelectedIndex == 0)
            {
                MessageBox.Show("Please select first currency", "Eyy", MessageBoxButton.OK, MessageBoxImage.Information);
                from.Focus();
                return;
            }

            else if (to.SelectedValue == null || to.SelectedIndex == 0)
            {
                MessageBox.Show("Please select second currency", "Eyy", MessageBoxButton.OK, MessageBoxImage.Information);
                to.Focus();
                return;
            }

            if (from.Text == to.Text)
            {
                converted = double.Parse(amount.Text);
                result.Text = converted.ToString("N2");
            }
            else
            {
                converted = double.Parse(amount.Text) * double.Parse(from.SelectedValue.ToString()) /
                    double.Parse(to.SelectedValue.ToString());
                result.Text = converted.ToString("N2");
            }
            from_output.Content = $"{amount.Text} {from.Text} is:";
            to_output.Content = $"{to.Text}.";
            return;

        }

        private void NumberValidation(object sender, TextCompositionEventArgs e)
        {
            Regex rgx = new Regex("[^0-9]+");
            e.Handled = rgx.IsMatch(e.Text);
        }

        private void save_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (amountM.Text == null || amountM.Text.Trim() == "")
                {
                    MessageBox.Show("Please enter amount", "Eyy", MessageBoxButton.OK, MessageBoxImage.Information);
                    amountM.Focus();
                    return;
                }
                else if(cname.Text == null || cname.Text.Trim() == "")
                {
                    MessageBox.Show("Please enter currency name", "Eyy", MessageBoxButton.OK, MessageBoxImage.Information);
                    cname.Focus();
                    return;
                }
                else
                {
                    if(currencyID > 0)
                    {
                        if (MessageBox.Show("Are you sure you want to update?", "Yo", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            MyCon();
                            DataTable dt = new DataTable();
                            cmd = new SqlCommand("UPDATE Currencies SET Amount = @Amount, CurrencyName = @CurrencyName WHERE ID = @ID", con);
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@ID", currencyID);
                            cmd.Parameters.AddWithValue("@Amount", amountM.Text);
                            cmd.Parameters.AddWithValue("@CurrencyName", cname.Text);
                            cmd.ExecuteNonQuery();
                            con.Close();

                            MessageBox.Show("Data updated successfully", "Yay!", MessageBoxButton.OK, MessageBoxImage.Information);
                        }

                    }
                    else {
                        if (MessageBox.Show("Are you sure you want to save new currency?", "Yo", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            MyCon();
                            DataTable dt = new DataTable();
                            cmd = new SqlCommand("INSERT INTO Currencies(Amount, CurrencyName) VALUES(@Amount, @CurrencyName)", con);
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@Amount", amountM.Text);
                            cmd.Parameters.AddWithValue("@CurrencyName", cname.Text);
                            cmd.ExecuteNonQuery();
                            con.Close();

                            MessageBox.Show("Data updated successfully", "Yay!", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    ClearManagerInputFields();
                }

            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearManagerInputFields()
        {
            try{ 
            amountM.Text = string.Empty;
            cname.Text = string.Empty;
            save.Content = "Save";
            GetData();
            currencyID = 0;
            BindCurrency();
            amountM.Focus();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void GetData()
        {
            MyCon();
            DataTable dt = new DataTable();
            cmd = new SqlCommand("SELECT * FROM Currencies", con);
            cmd.CommandType = CommandType.Text;
            da = new SqlDataAdapter(cmd);
            da.Fill(dt);
            if (dt != null && dt.Rows.Count > 0)
            {
                currencyTable.ItemsSource = dt.DefaultView;
                Console.WriteLine(dt.DefaultView.ToString());
            }
            else
            {
                currencyTable.ItemsSource = null;
            }
            con.Close();
        }

        private void cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearManagerInputFields();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void currencyTable_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                DataGrid grd = (DataGrid)sender;
                DataRowView row_selected = grd.CurrentItem as DataRowView;
                if (row_selected != null)
                {
                    if(currencyTable.Items.Count > 0)
                    {
                        if(grd.SelectedCells.Count > 0)
                        {
                            currencyID = Int32.Parse(row_selected["ID"].ToString());

                            if (grd.SelectedCells[0].Column.DisplayIndex == 1)
                            {
                                amountM.Text = row_selected["Amount"].ToString();
                                cname.Text = row_selected["CurrencyName"].ToString();
                                save.Content = "Update";
                            }
                            if (grd.SelectedCells[0].Column.DisplayIndex == 2)
                            {
                                if (MessageBox.Show("Are you sure you want to delete this currency?", "Yo", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes){
                                    MyCon();
                                    DataTable dt = new DataTable();
                                    cmd = new SqlCommand("DELETE FROM Currencies WHERE ID = @ID", con);
                                    cmd.CommandType = CommandType.Text;
                                    cmd.Parameters.AddWithValue("@ID", currencyID);
                                    cmd.ExecuteNonQuery();
                                    con.Close();
                                    MessageBox.Show("Currency deleted.", "Aah!", MessageBoxButton.OK, MessageBoxImage.Information);
                                    ClearManagerInputFields();
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}