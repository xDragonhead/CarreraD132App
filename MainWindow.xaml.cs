using System.Windows;
using Carrera.App.ViewModels;

namespace Carrera.App
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
