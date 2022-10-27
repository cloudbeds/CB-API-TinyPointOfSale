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
    /// 
    /// </summary>
    public partial class uiPosOrderListItem : UserControl
    {
        private readonly PosItem? _posItem = null;
        private int _numberItemsOrdered = 0;

        /// <summary>
        /// Items ordered
        /// </summary>
        public int ItemOrderCount
        {
            get
            {
                return _numberItemsOrdered;
            }
        }

       public decimal Item_UnitPrice
        {
            get
            {
                return  _posItem.ItemChargeAmount;
            }
        }

        public decimal Item_TaxAmount
        {
            get
            {
                return _posItem.TaxAmount;
            }
        }

        public string Item_TaxName
        {
            get
            {
                return _posItem.TaxName;
            }
        }

        public string Item_ClassId
        {
            get
            {
                return _posItem.Item_ClassId;
            }
        }
        public string Item_Name
        {
            get
            {
                return _posItem.Item_Name;
            }
        }

        public string Item_CategoryName
        {
            get
            {
                return _posItem.Item_CategoryName;
            }
        }

        /// <summary>
        /// Delegate and Event for when a Guest gets clicked/selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal delegate void PosItemSelectedEventHandler(object sender, PosItemSelectedEventArgs e);
        internal event PosItemSelectedEventHandler PosItemSelected;

        /// <summary>
        /// Called when the order was updated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal delegate void OrderUpdatedEventHandler(object sender, EventArgs e);
        internal event OrderUpdatedEventHandler OrderUpdated;


        /// <summary>
        /// Return the price for 0 to N items in order
        /// </summary>
        /// <returns></returns>
        public decimal CalculatePrice_PreTax()
        {
            return _numberItemsOrdered * _posItem.ItemChargeAmount;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="guest"></param>
        internal uiPosOrderListItem(PosItem posItem) : this()
        {
            _posItem = posItem;

        }

        /// <summary>
        /// Constructor
        /// </summary>
        public uiPosOrderListItem()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var posItem = _posItem;
            //Fill in the UI elements
            if (posItem != null)
            {
                txtPosItemName.Text = posItem.Item_Name;
                txtPosItemCategory.Text = posItem.Item_CategoryName;
                //txtNumberOrdered.Text = posItem.Guest_CellPhone;
                UpdateNumberOrderedUi();
            }
/*            else //Degenerate case
            {
                IwsDiagnostics.Assert(false, "1022-256: Null guest");
            }
*/
        }

        /// <summary>
        /// Update the # of items ordered UI
        /// </summary>
        private void UpdateNumberOrderedUi()
        {
            if(_numberItemsOrdered <= 0)
            {
                txtNumberOrdered.Text = "";

                btnAddItems.Background = GraphicsGlobals.LightButton;
                btnRemoveItems.Visibility = Visibility.Hidden;
            }
            else
            {
                btnAddItems.Background = GraphicsGlobals.MediumButton;
                txtNumberOrdered.Text = _numberItemsOrdered.ToString();
                btnRemoveItems.Visibility = Visibility.Visible;
            }

        }

        /// <summary>
        /// The list item got clicked/tapped
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //Raise the event
            PosItemSelected(this, new PosItemSelectedEventArgs(_posItem));
        }

        /// <summary>
        /// Add an item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddItems_Click(object sender, RoutedEventArgs e)
        {
            _numberItemsOrdered++;
            UpdateNumberOrderedUi();

            TriggerOrderUpdatedEvent();
        }


        /// <summary>
        /// Remove an item from the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRemoveItems_Click(object sender, RoutedEventArgs e)
        {
            var newNumberItems = _numberItemsOrdered - 1;
            if(newNumberItems < 0)
            {
                newNumberItems = 0;
            }
            _numberItemsOrdered = newNumberItems;
            UpdateNumberOrderedUi(); //Remove items from list

            TriggerOrderUpdatedEvent();
        }

        /// <summary>
        /// Trigger the order event updated
        /// </summary>
        private void TriggerOrderUpdatedEvent()
        {
            var evt = OrderUpdated;
            if(evt != null)
            {
                evt(this, EventArgs.Empty);
            }
        }
    }
}
