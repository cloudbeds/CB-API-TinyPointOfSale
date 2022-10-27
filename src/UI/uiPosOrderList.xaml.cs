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

namespace TinySimplePointOfSale
{
    /// <summary>
    /// This UI CONTROL shows the menu of orderable items and allows the user
    /// to add items to their order menu
    /// </summary>
    public partial class uiPosOrderList : UserControl
    {
        /// <summary>
        /// Delegate and Event for when a PosOrderItem gets clicked/selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal delegate void PosOrderItemSelectedEventHandler(object sender, PosItemSelectedEventArgs e);
        internal event PosOrderItemSelectedEventHandler? PosOrderItemSelected;


        List<uiPosOrderListItem> _menuUiItems = new List<uiPosOrderListItem>();
        
        /// <summary>
        /// The array of list items
        /// </summary>
        internal ICollection<uiPosOrderListItem> PosOrderListItems
        {
            get { return _menuUiItems.AsReadOnly(); }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public uiPosOrderList()
        {
            InitializeComponent();
        }


        /// <summary>
        /// # menu items
        /// </summary>
        public int MenuItemsCount
        {
            get
            {
                if(_menuUiItems == null)
                {
                    return 0;

                }
                return _menuUiItems.Count;
            }
        }


        /// <summary>
        /// Called when an individual PosOrderItem list item is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EventHander_PosOrderItemSelectedEventHandler(object sender, PosItemSelectedEventArgs e)
        {
            if(PosOrderItemSelected == null)
            {
                return;
            }

            //Bubble the event upward
            PosOrderItemSelected(this, e);

            //MessageBox.Show("PosOrderItem selected: " + e.PosOrderItem.PosOrderItem_Name);
        }


        /// <summary>
        /// Fill the visible list with controls...
        /// </summary>
        /// <param name="posItems"></param>
        internal void FillPosOrderItemsList(ICollection<PosItem>? posItems)
        {
            var uiChildren = spListOfPosOrderItems.Children;
            var menuItems = new List<uiPosOrderListItem>();

            //Get rid of all the existing items
            uiChildren.Clear();
            if((posItems == null) || (posItems.Count == 0))
            {

                //We could show a fancier custom control here to indicate
                //that there are no controls
                var txtCtl = new TextBlock();
                txtCtl.Text = "No POS Items in list";

                uiChildren.Add(txtCtl);
                return;
            }


            //==============================================
            //Add a control for each guest
            //==============================================
            foreach(var thisPosOrderItem in posItems)
            {
                //Create the control
                var ctlPosOrderItemListItem = new uiPosOrderListItem(thisPosOrderItem);
                //Hook up the event to listen to it here...
                ctlPosOrderItemListItem.PosItemSelected += EventHander_PosOrderItemSelectedEventHandler;

                //Listen to events of the order being updated
                ctlPosOrderItemListItem.OrderUpdated += CtlPosOrderItemListItem_OrderUpdated;

                uiChildren.Add(ctlPosOrderItemListItem);

                //Add it to our logical list of menu items we care about
                menuItems.Add(ctlPosOrderItemListItem);
            }

            _menuUiItems = menuItems;

            //Update the totals
            RecalculateAndUpdateOrderSummaryText();
        }

        /// <summary>
        /// Called whenever any of the order sub-items is updates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void CtlPosOrderItemListItem_OrderUpdated(object sender, EventArgs e)
        {
            RecalculateAndUpdateOrderSummaryText();
        }


        /// <summary>
        /// Sum up the menu items
        /// </summary>
        private void RecalculateAndUpdateOrderSummaryText()
        {
            decimal runningTotal = 0;
            foreach(var ctlMenuItem in _menuUiItems)
            {
                runningTotal += ctlMenuItem.CalculatePrice_PreTax();
            }

            txtSummaryText.Text = "Pre tax/tip total: " + GlobalStrings.FormatCurrency(runningTotal);
        }
    }
}
