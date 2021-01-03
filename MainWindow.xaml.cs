using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Text.RegularExpressions;


namespace AccountList
{
    /// <summary>
    /// Displays an Account List and gives options to manipulate with account list data
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes the main window components and binds the ListView to Database Data Context
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Get Company Name text from App.config file settings
            string companyName = ConfigurationManager.AppSettings.Get("CompanyName");

            // Add Company Name to CompanyName Label content
            CompanyNameLabel.Content = companyName;

            // Bind the ListView to the DataContext Table from Accounts Database
            UpdateListViewFromDatabase();
        }

        /// <summary>
        /// Gets the text input from TextBoxes, creates an Account Entity and inserts it into Accounts Database
        /// </summary>
        /// <param name="sender">Button Control</param>
        /// <param name="e">Evenet handler delegate</param>
        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            // Show warning message in case not all the required textboxes contain data
            if (String.IsNullOrEmpty(FirstNameTextBox.Text) || String.IsNullOrEmpty(LastNameTextBox.Text) || String.IsNullOrEmpty(PassportTextBox.Text))
            {
                MessageBox.Show("Please, complete the form.", "Incomplete data",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            // If all textboces contain data, create a new Account object with the data from textboxes
            else 
            {
                // Create variables with the filled-in text from respective TextBoxes
                string firstName = FirstNameTextBox.Text;
                string lastName = LastNameTextBox.Text;
                string passportID = PassportTextBox.Text;

                // Create AccountsDataContext class instance
                using (AccountsDataContext accountsDataBase = new AccountsDataContext())
                { 
                    // Create a new Account instance that should be inserted into Accounts Database
                    Account accountToInsert = new Account
                    {
                        First_Name = firstName,
                        Last_Name = lastName,
                        Passport = passportID
                    };

                    // Insert the created Account instance into Acocunts Table
                    accountsDataBase.Accounts.InsertOnSubmit(accountToInsert);

                    // Submit changes into Accounts Database
                    accountsDataBase.SubmitChanges();
                }

                // Reset window to default state
                ResetWindow();
            }
        }

        /// <summary>
        /// Displays the selected Accounts properties into the respective Textboxes
        /// </summary>
        /// <param name="sender">Selected ListView Control</param>
        /// <param name="e">Evenet handler delegate</param>
        private void AccountList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AccountList.SelectedItem != null) 
            {
                // Cast the sender into ListView
                ListView selectedListView = sender as ListView;

                // Create an Account object from the selected item of the ListView
                Account selectedAccount = selectedListView.SelectedItem as Account;

                // Display Account object properties in respective TextBoxes
                FirstNameTextBox.Text = selectedAccount.First_Name;
                LastNameTextBox.Text = selectedAccount.Last_Name;
                PassportTextBox.Text = selectedAccount.Passport;

                // Enable "Update the Account" and "Delete the Account" buttons
                UpdateButton.IsEnabled = true;
                DeleteButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Updates the selected Account from ListView in Database with new data in respective Textboxes. 
        /// </summary>
        /// <param name="sender">Button Control</param>
        /// <param name="e">Evenet handler delegate</param>
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            // Create an Account object from the selected item of the ListView
            Account selectedAccount = AccountList.SelectedItem as Account;

            // Get the Account Id property
            int accountID = selectedAccount.Id;

            // Get the updated data from respective TextBoxes
            string firstName = FirstNameTextBox.Text;
            string lastName = LastNameTextBox.Text;
            string passportID = PassportTextBox.Text;

            // Create new DataContext object
            using (AccountsDataContext accountsDataBase = new AccountsDataContext())
            {
                // Get from the Database the Account which had the same Id as the Account selected in ListView
                Account newAccountInstance = accountsDataBase.Accounts.SingleOrDefault(item => item.Id == accountID);
                
                // Updated the Account in Databse
                newAccountInstance.First_Name = firstName;
                newAccountInstance.Last_Name = lastName;
                newAccountInstance.Passport = passportID;

                // Submit changes into Accounts Database
                accountsDataBase.SubmitChanges();
            }

            // Reset window to default state
            ResetWindow();

            // Display message for user about the succefull completion of transacton
            MessageBox.Show("Account data is successfully updated .", "Success", 
                            MessageBoxButton.OK, MessageBoxImage.Information);            
        }

        /// <summary>
        /// Deletes the selected Account from  Accounts Database and update the data in ListView
        /// </summary>
        /// <param name="sender">Button Control</param>
        /// <param name="e">Evenet handler delegate</param>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // Create an Account object from the selected item of the ListView
            Account selectedAccount = AccountList.SelectedItem as Account;

            // Get the Account Id property
            int accountID = selectedAccount.Id;

            // Create new DataContext object
            using (AccountsDataContext accountsDataBase = new AccountsDataContext())
            {
                // Get from the Database the Account which had the same Id as the Account selected in ListView
                Account accountToDelete = accountsDataBase.Accounts.SingleOrDefault(item => item.Id == accountID);
                
                // Delete the Account from Accounts Databse
                accountsDataBase.Accounts.DeleteOnSubmit(accountToDelete);

                // Submit changes into Accounts Database
                accountsDataBase.SubmitChanges();
            }

            // Reset window to default state
            ResetWindow();

            // Display message for user about the succefull completion of transacton
            MessageBox.Show("Account is successfully removed from the list.", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Searches in Accoutns Databse for the Account with specified data in search TextBoxes
        /// </summary>
        /// <param name="sender">Button Control</param>
        /// <param name="e">Evenet handler delegate</param>
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the text input from respective TextBoxes
            string firstName = FirstNameSearchTextBox.Text;
            string lastName = LastNameSearchTextBox.Text;
            string passportID = PassportSearchtextBox.Text;

            // Create List of Accounts that will match the data in seach Textboxes
            List<Account> matchedAccounts = new List<Account>();

            // Create new DataContext object
            using (AccountsDataContext accountsDataBase = new AccountsDataContext()) 
            {
                // For each Account in AccountsDataContext search to find matches with data from search Textboxes
                foreach (Account account in accountsDataBase.Accounts)
                {
                    if (Regex.IsMatch(account.First_Name, firstName) &&
                        Regex.IsMatch(account.Last_Name, lastName) &&
                        Regex.IsMatch(account.Passport, passportID))
                    {
                        // Add matched accounts into Account List
                        matchedAccounts.Add(account);
                    }
                }
            }

            // Update the ListView with the new source of matched Accounts List
            AccountList.ItemsSource = matchedAccounts;
        }

        /// <summary>
        /// Unselects ListView item when mouse clickes outside of the ListView.
        /// Updates the ListView source to be the DataContext object
        /// </summary>
        /// <param name="sender">Button Control</param>
        /// <param name="e">Evenet handler delegate</param>
        //
        private void Accounts_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Unselect all items from Accounts List View
            AccountList.UnselectAll();

            // Reset window to default state
            ResetWindow();
        }

        /// <summary>
        /// Empties the textboxes in the main window grid
        /// </summary>
        /// <param name="mainWindowGrid">Grid Control</param>
        private void ClearTextBoxesContent(Grid mainWindowGrid)
        {
            // Iterates through all controls in the grid
            foreach (Control control in mainWindowGrid.Children)
            {
                // If the current control is of type Textbox, clear its content
                if (control.GetType() == typeof(TextBox))
                {
                    (control as TextBox).Text = String.Empty;
                }
            }
        }

        /// <summary>
        ///  Disables "Update the Account" and "Delete the Account" buttons
        /// </summary>
        private void DisableButtons() 
        {
            //Disable "Update the Account" and "Delete the Account" buttons
            UpdateButton.IsEnabled = false;
            DeleteButton.IsEnabled = false;
        }

        /// <summary>
        /// Updates the ListView Control to be binded to AccountsDataContext
        /// </summary>
        private void UpdateListViewFromDatabase()
        {
            // Create new DataContext object
            AccountsDataContext accountsDataBase = new AccountsDataContext();

            // Bind the ListView to DataContext object
            AccountList.ItemsSource = accountsDataBase.Accounts;
        }

        /// <summary>
        ///  Reset the window to dafault state
        /// </summary>
        private void ResetWindow() 
        {
            // Empty the textboxes in the main window grid
            ClearTextBoxesContent(MainGrid);

            // Disable "Update the Account" and "Delete the Account" buttons
            DisableButtons();

            //Updates the ListView Control to be binded to AccountsDataContext
            UpdateListViewFromDatabase();

            // Unselect the selected row in account ListView
            AccountList.UnselectAll();
        }
    }
}
