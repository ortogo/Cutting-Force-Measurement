using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingForceMeasurement.ViewModels
{
    public class MessageDialogViewModel : BaseViewModel
    {
        private string _message;
        private string _buttonLabel;

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        public string ButtonLabel
        {
            get => _buttonLabel;
            set
            {
                _buttonLabel = value;
                OnPropertyChanged(nameof(ButtonLabel));
            }
        }
    }
}
