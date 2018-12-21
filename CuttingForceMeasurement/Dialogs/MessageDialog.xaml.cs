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

using CuttingForceMeasurement.ViewModels;
using MaterialDesignThemes.Wpf;

namespace CuttingForceMeasurement.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для MessageDialog.xaml
    /// </summary>
    public partial class MessageDialog : UserControl
    {
        private const string defaultButtonLabel = "Хорошо";

        public MessageDialogViewModel ViewModel { get; set; }

        public MessageDialog()
        {
            InitializeComponent();
            ViewModel = new MessageDialogViewModel();
            DataContext = ViewModel;
        }

        public async void Show(string message, string buttonLabel, DialogClosingEventHandler dialogClosing)
        {
            ViewModel.Message = message;
            ViewModel.ButtonLabel = buttonLabel;
            // MessageDialogHost.DialogClosing += dialogClosing;

            await DialogHost.Show(this, MainWindow.MainIdentifier, dialogClosing);
        }

        public async void Show(string message, string buttonLabel)
        {
            ViewModel.Message = message;
            ViewModel.ButtonLabel = buttonLabel;

            await DialogHost.Show(this, MainWindow.MainIdentifier);
        }

        public async void Show(string message)
        {
            ViewModel.Message = message;
            ViewModel.ButtonLabel = defaultButtonLabel;

            await DialogHost.Show(this, MainWindow.MainIdentifier);
        }
    }
}
