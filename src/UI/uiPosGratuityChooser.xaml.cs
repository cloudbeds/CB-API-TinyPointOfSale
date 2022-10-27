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
    /// Interaction logic for uiPosOrderList.xaml
    /// </summary>
    public partial class uiPosGratuityChooser : UserControl
    {

        /// <summary>
        /// The base we compute the tip % from
        /// </summary>
        decimal _baseForCalulatingGratuity = 0;

        /// <summary>
        /// The tip
        /// </summary>
        decimal _currentTipAmount = 0;

        /// <summary>
        /// The absolute value of the tip
        /// </summary>
        internal decimal CurrentGratuityAmount
        {
            get
            {
                return _currentTipAmount;
            }
            set
            {
                _currentTipAmount = value;
                UpdateGratuitySummary();
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public uiPosGratuityChooser()
        {
            InitializeComponent();

            txtCustomTipCurrency.Text = GlobalStrings.CurrencySymbol;
        }

        /// <summary>
        /// Set a Fixed Tip %
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTip25Commit_Click(object sender, RoutedEventArgs e)
        {
            this.CurrentGratuityAmount = CalculateAndRound(_baseForCalulatingGratuity, 0.25);
            SetCustomTipText(this.CurrentGratuityAmount);
        }


        /// <summary>
        /// Set a Fixed Tip %
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTip20Commit_Click(object sender, RoutedEventArgs e)
        {
            this.CurrentGratuityAmount = CalculateAndRound(_baseForCalulatingGratuity, 0.20);
            SetCustomTipText(this.CurrentGratuityAmount);
        }

        /// <summary>
        /// Set a Fixed Tip %
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTip18Commit_Click(object sender, RoutedEventArgs e)
        {
            this.CurrentGratuityAmount = CalculateAndRound(_baseForCalulatingGratuity, 0.18);
            SetCustomTipText(this.CurrentGratuityAmount);
        }

        private void btnTip0Commit_Click(object sender, RoutedEventArgs e)
        {
            this.CurrentGratuityAmount = 0;
            SetCustomTipText(this.CurrentGratuityAmount);
        }


        /// <summary>
        /// Sets the text of the custom tip field
        /// </summary>
        /// <param name="currentGratuityAmount"></param>
        private void SetCustomTipText(decimal currentGratuityAmount)
        {
            txtCustomTipAmount.Text = currentGratuityAmount.ToString("0.00");
        }


        /// <summary>
        /// Set the base amount of the bill that tips are calculated from
        /// </summary>
        /// <param name="value"></param>
        internal void SetBaseItemTotalForGratuity(decimal value)
        {
            _baseForCalulatingGratuity = value;


            txtTip18Amount.Text = CalculateAndRoundText(value, 0.18);
            txtTip20Amount.Text = CalculateAndRoundText(value, 0.20);
            txtTip25Amount.Text = CalculateAndRoundText(value, 0.25);

            UpdateGratuitySummary();

        }


        /// <summary>
        /// Show the gratuity amount in the UI
        /// </summary>
        private void UpdateGratuitySummary()
        {
            
            //No grauity
            if (_baseForCalulatingGratuity == 0)
            {
                txtGratuitySummary.Text = "";
                return;
            }

            string gratuityFractionText =  
                (((double)_currentTipAmount * 100.0) / (double)_baseForCalulatingGratuity).ToString("0");

            txtGratuitySummary.Text = "Current gratuity is " + GlobalStrings.FormatCurrency(_currentTipAmount) + " (" + gratuityFractionText + "%)";
        }

        /// <summary>
        /// Make it into text...
        /// </summary>
        /// <param name="baseValue"></param>
        /// <param name="fraction"></param>
        /// <returns></returns>
        string CalculateAndRoundText(decimal baseValue, double fraction)
        {
            return GlobalStrings.FormatCurrency(CalculateAndRound(baseValue, fraction));
        }

        /// <summary>
        /// Round a fraction to 2 decipal places
        /// </summary>
        /// <param name="baseValue"></param>
        /// <param name="fraction"></param>
        /// <returns></returns>
        decimal CalculateAndRound(decimal baseValue, double fraction)
        {
            return (decimal)Math.Round((double) baseValue * fraction, 2);
        }

        /// <summary>
        /// Set an exact tip amount
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCutomTipAmount_Click(object sender, RoutedEventArgs e)
        {
            decimal customTipAmount = 0;
            try
            {
                customTipAmount = decimal.Parse(txtCustomTipAmount.Text);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Incorrect number format");
                return;
            }

            this.CurrentGratuityAmount = customTipAmount;
        }

    }
}
