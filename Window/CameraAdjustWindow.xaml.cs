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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FloorLength.Text = App.FloorLength.ToString();
            FloorWidth.Text = App.FloorWidth.ToString();
            CameraToFloor.Text = App.CameraToFloor.ToString();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            float parsedFloorLength;
            float parsedFloorWidth;
            float parsedCameraToFloor;

            if (string.IsNullOrWhiteSpace(FloorLength.Text) ||
                !float.TryParse(FloorLength.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out parsedFloorLength))
            {
                MessageBox.Show("Chiều dài sàn không hợp lệ.");
                FloorLength.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(FloorWidth.Text) ||
                !float.TryParse(FloorWidth.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out parsedFloorWidth))
            {
                MessageBox.Show("Chiều rộng sàn không hợp lệ.");
                FloorWidth.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(CameraToFloor.Text) ||
                !float.TryParse(CameraToFloor.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out parsedCameraToFloor))
            {
                MessageBox.Show("Chiều cao camera không hợp lệ.");
                CameraToFloor.Focus();
                return;
            }

            if (parsedCameraToFloor < 1.8f || parsedCameraToFloor > 2.7f)
            {
                MessageBox.Show("Khoảng cách từ sàn đến camera phải nằm trong khoảng từ 1.8 đến 2.7 mét.");
                CameraToFloor.Focus();
                return;
            }

            if (parsedCameraToFloor + parsedFloorLength > 3.7f)
            {
                MessageBox.Show("Tổng chiều dài sàn và khoảng cách từ camera đến sàn không được vượt quá 3.7 mét.");
                FloorLength.Focus();
                return;
            }

            if (parsedFloorWidth > 1.8f)
            {
                MessageBox.Show("Chiều rộng sàn không được lớn hơn 1.8 mét.");
                FloorWidth.Focus();
                return;
            }



            App.FloorLength = parsedFloorLength;
            App.FloorWidth = parsedFloorWidth;
            App.CameraToFloor = parsedCameraToFloor;

            this.Close();
        }

    }
}


