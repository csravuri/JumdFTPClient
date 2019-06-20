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

namespace JumdFTPClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            JumdFTPService jumdFTPServiceObj = new JumdFTPService("ftp://localhost", "PUBG", "1", "G:\\ftp_local");
            List<Tuple<string,bool>> ftpItems = jumdFTPServiceObj.GetListofItemsInFtpFolder("ftp://localhost");

            foreach (string eachItem in ftpItems.Select(x => x.Item1))
            {
                lvFTPList.Items.Add(eachItem);
            }
                
            //jumdFTPServiceObj.EstablishFTPConnection();

        }
    }
}
