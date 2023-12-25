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

namespace DotaParser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ReportGenerator reportGenerator = new();
        public MainWindow()
        {
            InitializeComponent();
        }
        static async Task Main()
        {
            await Parser.Parse(@"https://dota2.fandom.com/wiki/Dota_2_Wiki");
        }

        private async void Start_Parsing_Click(object sender, RoutedEventArgs e)
        {
            await Main();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            EditWindow editWindow = new EditWindow();
            editWindow.DataContext = this.DataContext;
            editWindow.Show();
        }

        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            reportGenerator.GenerateReport();
        }
    }
}
