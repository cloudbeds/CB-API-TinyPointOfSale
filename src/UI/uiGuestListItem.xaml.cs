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
    /// Interaction logic for uiGuestListItem.xaml
    /// </summary>
    public partial class uiGuestListItem : UserControl
    {
        private readonly CloudbedsGuest? _guest = null;

        bool _isSelected;
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if(_isSelected != value)
                {
                    _isSelected = value;
                    UpdateIsSelectedUi();
                }
            }
        }

        internal CloudbedsGuest Guest
        {
            get
            {
                return _guest;
            }
        }

        /// <summary>
        /// Delegate and Event for when a Guest gets clicked/selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal delegate void GuestSelectedEventHandler(object sender, GuestSelectedEventArgs e);
        internal event GuestSelectedEventHandler GuestSelected;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="guest"></param>
        internal uiGuestListItem(CloudbedsGuest guest) : this()
        {
            _guest = guest;

        }

        /// <summary>
        /// Constructor
        /// </summary>
        public uiGuestListItem()
        {
            InitializeComponent();
            UpdateIsSelectedUi();
        }

        private void UpdateIsSelectedUi()
        {
            if(_isSelected)
            {
                txtIsSelectedMarker.Text = "✓";
            }
            else
            {
                txtIsSelectedMarker.Text = "";
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var guest = _guest;
            //Fill in the UI elements
            if (guest != null)
            {
                txtGuestName.Text = guest.Guest_Name;
                txtGuestRoomNumber.Text = guest.Room_Name;
                //txtGuestPhone.Text = guest.Guest_CellPhone;
            }
/*            else //Degenerate case
            {
                IwsDiagnostics.Assert(false, "1022-256: Null guest");
            }
*/
        }

        /// <summary>
        /// The list item got clicked/tapped
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //Raise the event
            GuestSelected(this, new GuestSelectedEventArgs(_guest));
        }
    }
}
