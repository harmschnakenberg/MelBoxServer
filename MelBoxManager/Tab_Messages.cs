using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MelBoxManager
{
    public partial class MainWindow : Window
    {

        private void Button_RefreshTableRec_Click(object sender, RoutedEventArgs e)
        {
            var.TableRec = Sql.GetRecMsgView();
        }

        private void Button_RefreshTableSent_Click(object sender, RoutedEventArgs e)
        {
            var.TableSent = Sql.GetSentMsgView();
        }

    }
}
