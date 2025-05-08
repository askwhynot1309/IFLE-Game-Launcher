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
using System.Windows.Shapes;

namespace IFLEGameLauncher
{
    /// <summary>
    /// Interaction logic for CameraAdjustWindow.xaml
    /// </summary>
    public partial class CameraAdjustWindow : Window
    {
        public CameraAdjustWindow()
        {
            InitializeComponent();
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            float parsedFloorLength, parsedFloorWidth, parsedCameraToFloor;

            if (float.TryParse(FloorLength.Text, out parsedFloorLength) &&
                float.TryParse(FloorWidth.Text, out parsedFloorWidth) &&
                float.TryParse(CameraToFloor.Text, out parsedCameraToFloor))
            {
                App.FloorLength = parsedFloorLength;
                App.FloorWidth = parsedFloorWidth;
                App.CameraToFloor = parsedCameraToFloor;
                var c = App.FloorLength;
                MessageBox.Show(c.ToString());
            }
            else
            {
                MessageBox.Show("Invalid input. Please enter valid float numbers.");
            }
            this.Close();
        }
    }
}
