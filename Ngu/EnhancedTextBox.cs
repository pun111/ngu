using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ngu
{
    public class EnhancedTextBox : TextBox
    {
        public          EnhancedTextBox()
        {
            TextWrapping = System.Windows.TextWrapping.Wrap;
            VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
            HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
            BorderBrush = null;
            Background = null;
            Typography.Capitals = System.Windows.FontCapitals.AllSmallCaps;
            PreviewMouseDoubleClick += EnhancedTextBox_PreviewMouseDoubleClick;
        }

        private void    EnhancedTextBox_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextBoxSelectAll(sender, e);
        }
        private void    TextBoxSelectAll(object sender, MouseButtonEventArgs e)
        {
            // Set the event as handled
            e.Handled = true;
            // Select the Text
            var txtBox = sender as TextBox;
            txtBox.SelectAll();
            //txtBox.SelectedText = txtBox.Text;
            //(sender as TextBox).SelectAll();
        }
    }
}
