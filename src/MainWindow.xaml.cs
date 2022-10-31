using System;
using System.Collections.Generic;
using System.IO;
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

namespace TinySimplePointOfSale
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /*
                /// <summary>
                /// The currently selected Auth Context
                /// </summary>
                ICloudbedsAuthSessionBase _currentAuthSession;
                ICloudbedsServerInfo _currentServerInfo;
                CloudbedsGuestManager _currentGuestManager;
                PosItemManager? _posItemManager = null;

        */

        PosOrderManager? _currentPosOrderManger = null;

        //What Guest has been selected in the UI presently?
        CloudbedsGuest? _currentSelectedGuest = null;


        public MainWindow()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Shows the result of running a command
        /// </summary>
        /// <param name="text"></param>
        void UpdateCommandResultStatus(string text)
        {
            textBlockCommandResult.Text = text + " (" + DateTime.Now.ToString() + ")";
        }

        /// <summary>
        /// Shows status text in the textboxes
        /// </summary>
        /// <param name="statusLog"></param>
        private void UpdateStatusText(TaskStatusLogs statusLog, bool forceUIRefresh = false)
        {
            textBoxStatus.Text = statusLog.StatusText;
            //ScrollToEndOfTextbox(textBoxStatus);

            textBoxStatusErrors.Text = statusLog.ErrorText;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Load the normal application preferences
            AppPreferences_Load();
        }

        /// <summary>
        /// Load app preferences that we want to auto-load next time
        /// </summary>
        private void AppPreferences_Load()
        {
            try
            {
                AppPreferences_Load_Inner();
            }
            catch (Exception ex)
            {
                IwsDiagnostics.Assert(false, "819-1041: Error loading app prefernces, " + ex.Message);
            }
        }

        /// <summary>
        /// Load app preferences that we want to auto-load next time
        /// </summary>
        private void AppPreferences_Load_Inner()
        {
            txtPathToAppSecrets.Text = AppSettings.LoadPreference_PathAppSecretsConfig();
            txtPathToUserTokenSecrets.Text = AppSettings.LoadPreference_PathUserAccessTokens();

            //If any of these are blank then generate them
            txtPathToAppSecrets.Text = AppPreferences_GenerateDefaultIfBlank(txtPathToAppSecrets.Text, "Templates_Secrets\\Example_AppSecrets.xml");
            txtPathToUserTokenSecrets.Text = AppPreferences_GenerateDefaultIfBlank(txtPathToUserTokenSecrets.Text, "Templates_Secrets\\Example_UserTokenSecrets.xml");
        }

        /// <summary>
        /// If the proposed path is blank, then generate a path based on the applicaiton's path and the specified sub-path
        /// </summary>
        /// <param name="proposedPath"></param>
        /// <param name="subPath"></param>
        /// <returns></returns>
        private string AppPreferences_GenerateDefaultIfBlank(string proposedPath, string subPath)
        {
            if (!string.IsNullOrWhiteSpace(proposedPath))
            {
                return proposedPath;
            }

            var basePath = AppSettings.LocalFileSystemPath;
            return System.IO.Path.Combine(basePath, subPath);
        }

        /// <summary>
        /// Called when a GUEST is selected for us to focus on
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ctlGuestList_GuestSelected(object sender, GuestSelectedEventArgs e)
        {
            var selectedGuest = e.Guest;
            _currentSelectedGuest = selectedGuest;
            
            //Subtle update to UI to show a guest was selected
            if(selectedGuest != null)
            {
                UpdateCommandResultStatus("Selected guest: " + selectedGuest.Guest_Name);
            }
        }

        /// <summary>
        /// Force update the status logs
        /// </summary>
        private void UpdateStatusLogsText()
        {
            UpdateStatusText(TaskStatusLogsSingleton.Singleton, true);
        }


        /// <summary>
        /// Submit the order to Cloudbeds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSubmitToCloudbeds_Click(object sender, RoutedEventArgs e)
        {
            bool wasSuccess = false;
            var selectedGuest = _currentSelectedGuest;
            if (selectedGuest == null)
            {
                MessageBox.Show("No guest selected. Cannot post charge");
                return;
            }

            var posOrderManager = _currentPosOrderManger;
            if(posOrderManager == null)
            { 
                MessageBox.Show("No guest selected. Cannot post charge");
                return;            
            }


            var calculatedTotals = posOrderManager.CalculateTotals();
            if(calculatedTotals.GrandTotal <= 0)
            {
                MessageBox.Show("There are no charges to post");
                return;
            }


            //Set a note that will be shown on the order items..
            posOrderManager.DefaultNoteForLineItems = 
                "Guest: " + selectedGuest.Guest_Name + ", Date: " + DateTime.Now.ToString();

            //===============================================================
            //Post the order up to the server
            //===============================================================
            var statusLogs = TaskStatusLogsSingleton.Singleton;

            var postCharge = new CloudbedsPostChargeToGuest(
                CloudbedsSingletons.CloudbedsServerInfo,
                CloudbedsSingletons.CloudbedsAuthSession,
                TaskStatusLogsSingleton.Singleton,
                selectedGuest,
                posOrderManager);

            try
            {
                wasSuccess = postCharge.ExecuteRequest();
            }
            catch (Exception ex)
            {
                statusLogs.AddError("1025-900, error posting charge to guest: " + ex.Message);
            }


            //Show the basic result
            if (wasSuccess)
            {
                UpdateCommandResultStatus("SUCCESS posting charge");
            }
            else
            {
                UpdateCommandResultStatus("FAILURE posting charge");
            }

            //Show the response in the UI
            txtCloudbedsSubmitResponse.Text = postCharge.CommandResults_SummaryText;
            uiArea_CloudbedsSubmitResponse.Visibility = Visibility.Visible;

            //Update the detailed logs
            UpdateStatusLogsText();
        }



        /// <summary>
        /// Pull the order items from the Menu Ordering UI
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void RefreshOrderManagerFromOrderMenuUi()
        {
            var posOrderManager = EnsurePosOrderManager();

            //Out with the old....
            posOrderManager.ClearOrderItems();

            //In with the new...
            foreach (var ctlListItem in  ctlPosOrderList.PosOrderListItems)
            {
                int itemCount = ctlListItem.ItemOrderCount;
                if (itemCount > 0)
                { 
                    //If there are multiple items, add them each as individual line items
                    for(int itemIdx = 0; itemIdx < itemCount; itemIdx++)
                    {
                        posOrderManager.AddItemToOrder(
                            ctlListItem.Item_UnitPrice,
                            ctlListItem.Item_TaxAmount,
                            ctlListItem.Item_TaxName,
                            ctlListItem.Item_ClassId,
                            ctlListItem.Item_Name,
                            ctlListItem.Item_CategoryName);
                    }
                }
            }
        }



        /// <summary>
        /// Updates the order submission summary tab's contents
        /// </summary>
        private void RefreshPosSubmitSummaryUi()
        {
            var orderManager = EnsurePosOrderManager();

            var selectedGuest = _currentSelectedGuest;
            if(selectedGuest == null)
            {
                txtPOSSubmit_GuestName.Text = "PLEASE SELECT GUEST";
            }
            else
            {
                txtPOSSubmit_GuestName.Text =  selectedGuest.Guest_Name + " (" + selectedGuest.Room_Name + ")";
            }

            var calculatedTotals = orderManager.CalculateTotals();

            txtPOSSubmit_ItemsOrdered.Text = calculatedTotals.NumberItems.ToString();
            txtPOSSubmit_SubTotal.Text     = GlobalStrings.FormatCurrency(calculatedTotals.TotalItemsPrice);
            txtPOSSubmit_Tax.Text          = GlobalStrings.FormatCurrency(calculatedTotals.TotalTax);
            txtPOSSubmit_Gratuity.Text     = GlobalStrings.FormatCurrency(calculatedTotals.Gratuity);
            txtPOSSubmit_Total.Text        = GlobalStrings.FormatCurrency(calculatedTotals.GrandTotal);
        }

        /// <summary>
        /// Resize the UI based on the window size
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RepondToWindowSizeChange();
        }

        /// <summary>
        /// Resize the UI based on the window size
        /// </summary>
        private void RepondToWindowSizeChange()
        {
            var windowWidth = this.Width;
            if(this.WindowState == WindowState.Maximized)
            {
                windowWidth = SystemParameters.WorkArea.Width;
            }

            //Sanity metric...
            if (windowWidth > 10)
            {
                gridMaster.Width = windowWidth;
                tabMaster.Width = windowWidth;
            }
        }

        /// <summary>
        /// Resize the UI based on the window size
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_StateChanged(object sender, EventArgs e)
        {
            switch(this.WindowState)
            {
                case WindowState.Maximized:
                case WindowState.Normal:
                    RepondToWindowSizeChange();
                    break;
                default:
                    break;
            }
        }


        /****************************************************************************************
        TAB FOCUS EVENTS
        1. When the user switches TO a tab we want to stock the UI in the Tab with up to date data
        2. When the user switches FROM a tab we want to pull data from the UI into the POS order
        *****************************************************************************************/
        #region "Tab focus/lost-focus"
        /// <summary>
        /// Called when the Gratuity Tab gets the focus.  We want to show the gratuity options
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabPOSGratuityChooser_GotFocus(object sender, RoutedEventArgs e)
        {
            var posOrderManager = EnsurePosOrderManager();
            var currentTotals = posOrderManager.CalculateTotals();

            ctlPosGratuityChooser.SetBaseItemTotalForGratuity(currentTotals.TotalItemsPrice + currentTotals.TotalTax);
            ctlPosGratuityChooser.CurrentGratuityAmount = currentTotals.Gratuity;

        }

        /// <summary>
        /// Called when the user is leaving the Gratuity tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabPOSGratuityChooser_LostFocus(object sender, RoutedEventArgs e)
        {
            //Get the gratuity from the UI and place it in the order
            var posOrderManager = EnsurePosOrderManager();
            posOrderManager.Gratuity = ctlPosGratuityChooser.CurrentGratuityAmount;
        }

        /// <summary>
        /// Called when the Submit tab gets the focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabPOSSubmit_GotFocus(object sender, RoutedEventArgs e)
        {
            //Fill in the UI with the latest values
            RefreshPosSubmitSummaryUi();
        }


        /// <summary>
        /// We are switching OFF the POS Menu Ordering tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabPOSMenu_LostFocus(object sender, RoutedEventArgs e)
        {
            RefreshOrderManagerFromOrderMenuUi();
        }

        /// <summary>
        /// Tab for POS menu selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabPOSMenu_GotFocus(object sender, RoutedEventArgs e)
        {
            //Make sure we've stocked the menu...
            if (ctlPosOrderList.MenuItemsCount == 0)
            {
                ctlPosOrderList.FillPosOrderItemsList(CloudbedsSingletons.PointOfSaleItemManager.Items);
            }
        }

        /// <summary>
        /// Select GUEST tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabGuestsList_GotFocus(object sender, RoutedEventArgs e)
        {
            //Fill the guests list if we need to
            if (!ctlGuestList.IsGuestsListStocked)
            {
                try
                {
                    var guestManager = CloudbedsSingletons.CloudbedsGuestManager;
                    guestManager.EnsureCachedData();

                    //Fill the UI
                    ctlGuestList.FillGuestsList(guestManager.Guests);
                }
                catch (Exception ex)
                {

                    TaskStatusLogsSingleton.Singleton.AddError(
                        "1024-1137: Error getting guests " + ex.ToString());
                }
            }
        }



        #endregion

/****************************************************************************************
NEXT STEP ADVANCE BUTTONS
The UI acts as a multi-step wizard.  These buttons advance forward
*****************************************************************************************/
#region "Next step advance buttons"

        /// <summary>
        /// Advance to this step
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNextStepChooseGuest_Click(object sender, RoutedEventArgs e)
        {
            tabGuestChooser.Focus();
        }

        /// <summary>
        /// Advance to next step
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNextStepChooseGratuity_Click(object sender, RoutedEventArgs e)
        {
            tabPOSGratuityChooser.Focus();
        }

        /// <summary>
        /// Advance to next step
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNextStepReviewAndSubmit_Click(object sender, RoutedEventArgs e)
        {
            tabPOSSubmit.Focus();
        }


        /// <summary>
        /// Starts a new order (throws out the old order's data)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStartNewOrder_Click(object sender, RoutedEventArgs e)
        {
            UpdateCommandResultStatus("Loading data...");

            //---------------------------------------------------------------------
            //Load any configuration, or data from Cloudbeds that we ned
            //---------------------------------------------------------------------
            CloudbedsSingletons.WarmUpCloudbedsDataCachesIfNeeded_Async();

            //---------------------------------------------------------------------
            //Reset the Order manager UI
            //---------------------------------------------------------------------
            var posOrderManager = new PosOrderManager();
            _currentPosOrderManger = posOrderManager;
            ctlPosOrderList.FillPosOrderItemsList(CloudbedsSingletons.PointOfSaleItemManager.Items);

            //---------------------------------------------------------------------
            //Guest selector
            //---------------------------------------------------------------------
            ctlGuestList.ClearGuestSelection();
            //Clear any selections
            _currentSelectedGuest = null;

            //---------------------------------------------------------------------
            //Reset the Submit UI
            //---------------------------------------------------------------------
            uiArea_CloudbedsSubmitResponse.Visibility = Visibility.Hidden;


            tabPOSMenu.Focus();
            UpdateCommandResultStatus("Done loading data...");
        }
        #endregion

/****************************************************************************************
ENSURE XXXXXX Functions
Ensures necessary objects have been loaded and are ready to use. These are typically
called before common actions are performed to make sure the system is in a fully
prepared state
*****************************************************************************************/
#region "Ensure XXXXXXX"

        /// <summary>
        /// Create an order manager if we need one
        /// </summary>
        /// <returns></returns>
        PosOrderManager EnsurePosOrderManager()
        {
            var orderManager = _currentPosOrderManger;
            if (orderManager == null)
            {
                orderManager = new PosOrderManager();
                _currentPosOrderManger = orderManager;
            }

            return orderManager;
        }
#endregion


/****************************************************************************************
TESTING BUTTONS
Buttons used to trigger testing/diagnostic code (not part of main functional application)
*****************************************************************************************/
#region "Testing Buttons"

        /// <summary>
        /// TESTING FUNCTION: Get the list of guests in the hotel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGetGuests_Click(object sender, RoutedEventArgs e)
        {
            bool wasSuccess = false;
            var statusLogs = TaskStatusLogsSingleton.Singleton;
            try
            {
                var guestManager = CloudbedsSingletons.CloudbedsGuestManager;
                guestManager.ForceRefreshOfCachedData();

                //Fill the UI
                ctlGuestList.FillGuestsList(guestManager.Guests);
                wasSuccess = true;
            }
            catch (Exception ex)
            {
                wasSuccess = false;
                statusLogs.AddError("1020-645: Error getting guests " + ex.ToString());
            }

            UpdateStatusText(statusLogs, true);
            if (wasSuccess)
            {
                UpdateCommandResultStatus("SUCCESS getting guests");
            }
            else
            {
                UpdateCommandResultStatus("FAILURE getting guests");
            }
        }

        #endregion

/****************************************************************************************
CONFIGURATION TAB
*****************************************************************************************/
#region "Config Tab"

        /// <summary>
        /// Set a configuration override
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConfigSetPathToUserTokenSecrets_Click(object sender, RoutedEventArgs e)
        {
            string path = txtPathToUserTokenSecrets.Text;
            if(!File.Exists(path))
            {
                MessageBox.Show("File path does not exist: " + path);
                return;
            }

            //Update the override in the app settings
            AppSettings.SetOverride_UserAccessTokens(path);
            MessageBox.Show("Path set!");
        }

        /// <summary>
        /// Set a configuration override
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConfigSetPathToAppSecrets_Click(object sender, RoutedEventArgs e)
        {
            string path = txtPathToAppSecrets.Text;
            if (!File.Exists(path))
            {
                MessageBox.Show("File path does not exist: " + path);
                return;
            }
            AppSettings.SetOverride_PathAppSecretsConfig(path);
            MessageBox.Show("Path set!");
        }

    #endregion
    }
}
