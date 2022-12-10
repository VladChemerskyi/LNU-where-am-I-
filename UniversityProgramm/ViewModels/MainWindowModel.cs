using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace UniversityProgramm.ViewModels
{
    /// <summary>
    /// Main window model with binding properties
    /// </summary>
    public class MainWindowModel : ViewModelBase
    {
        private double _mapHeight = 0;
        public double MapHeight
        {
            get => _mapHeight;
            set => SetProperty(ref _mapHeight, value);
        }

        private double _mapWidth = 0;
        public double MapWidth
        {
            get => _mapWidth;
            set => SetProperty(ref _mapWidth, value);
        }

        public MainWindowModel()
        {

        }
    }
}
