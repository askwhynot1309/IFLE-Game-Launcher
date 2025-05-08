using System;
using System.Collections.Generic;
using System.Globalization;
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

            bool isLengthParsed = float.TryParse(FloorLength.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out parsedFloorLength);
            bool isWidthParsed = float.TryParse(FloorWidth.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out parsedFloorWidth);
            bool isCameraParsed = float.TryParse(CameraToFloor.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out parsedCameraToFloor);

            if (isLengthParsed && isWidthParsed && isCameraParsed)
            {
                App.FloorLength = parsedFloorLength;
                App.FloorWidth = parsedFloorWidth;
                App.CameraToFloor = parsedCameraToFloor;
            }
            else
            {
                MessageBox.Show("Vui lòng nhập dữ liệu hợp lệ vd: 2 2.1");
            }

            this.Close();
        }
    }
}
