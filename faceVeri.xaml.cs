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
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System.IO;

namespace face
{
    /// <summary>
    /// Interaction logic for faceVeri.xaml
    /// </summary>
    public partial class faceVeri : Window
    {
        private readonly IFaceServiceClient facesc = new FaceServiceClient("98efc23bfd4048279fccac2a079f4451");
        private string leftfaceid="";
        private string rightfaceid="";
        public faceVeri()
        {
            InitializeComponent();
        }
        private async void LeftImagePicker_Click(object sender, RoutedEventArgs e)
        {
            var openDlg = new Microsoft.Win32.OpenFileDialog();

            openDlg.Filter = "JPEG Image(*.jpg)|*.jpg";
            bool? result = openDlg.ShowDialog();

            if (!(bool)result)
            {
                return;
            }
            string filePath = openDlg.FileName;
            Uri fileUri = new Uri(filePath);
            BitmapImage bitmapSource = new BitmapImage();

            bitmapSource.BeginInit();
            bitmapSource.CacheOption = BitmapCacheOption.None;
            bitmapSource.UriSource = fileUri;
            bitmapSource.EndInit();
            LeftImageDisplay.Source = bitmapSource;
            Stream imageFileStream = File.OpenRead(filePath);
            this.Title = String.Format("Detecting....");
            var faces = await facesc.DetectAsync(imageFileStream);
            this.Title = String.Format("Detection Finished");
            //FaceRectangle[] faceRects = faces.Select(face => face.FaceRectangle).ToArray();            
            if (faces.Length == 1)
            {
                foreach (var face in faces)
                {
                    leftfaceid = face.FaceId.ToString();
                }
                FaceRectangle[] faceRects = faces.Select(face => face.FaceRectangle).ToArray();
                DrawingVisual visual = new DrawingVisual();
                DrawingContext drawingContext = visual.RenderOpen();
                drawingContext.DrawImage(bitmapSource,
                    new Rect(0, 0, bitmapSource.Width, bitmapSource.Height));
                double dpi = bitmapSource.DpiX;
                double resizeFactor = 96 / dpi;
                foreach (var faceRect in faceRects)
                {

                    drawingContext.DrawRectangle(
                        Brushes.Transparent,
                        new Pen(Brushes.Red, 2),
                        new Rect(
                            faceRect.Left * resizeFactor,
                            faceRect.Top * resizeFactor,
                            faceRect.Width * resizeFactor,
                            faceRect.Height * resizeFactor
                            ));
                }
                drawingContext.Close();
                RenderTargetBitmap faceWithRectBitmap = new RenderTargetBitmap(
                    (int)(bitmapSource.PixelWidth * resizeFactor),
                    (int)(bitmapSource.PixelHeight * resizeFactor),
                    96,
                    96,
                    PixelFormats.Pbgra32);
                faceWithRectBitmap.Render(visual);
                LeftImageDisplay.Source = faceWithRectBitmap;
            }
            else
                MessageBox.Show("Verification accepts two faces as input, please pick image with only one detectable face in it.", "Warning", MessageBoxButton.OK);
        }
        private async void RightImagePicker_Click(object sender, RoutedEventArgs e)
        {
            var openDlg = new Microsoft.Win32.OpenFileDialog();

            openDlg.Filter = "JPEG Image(*.jpg)|*.jpg";
            bool? result = openDlg.ShowDialog();

            if (!(bool)result)
            {
                return;
            }
            string filePath = openDlg.FileName;
            Uri fileUri = new Uri(filePath);
            BitmapImage bitmapSource = new BitmapImage();

            bitmapSource.BeginInit();
            bitmapSource.CacheOption = BitmapCacheOption.None;
            bitmapSource.UriSource = fileUri;
            bitmapSource.EndInit();
            RightImageDisplay.Source = bitmapSource;
            Stream imageFileStream = File.OpenRead(filePath);
            this.Title = String.Format("Detecting....");
            var faces = await facesc.DetectAsync(imageFileStream);
            this.Title = String.Format("Detection Finished");
            //FaceRectangle[] faceRects = faces.Select(face => face.FaceRectangle).ToArray();            
            if (faces.Length == 1)
            {
                foreach (var face in faces)
                {
                    rightfaceid = face.FaceId.ToString();
                }
                FaceRectangle[] faceRects = faces.Select(face => face.FaceRectangle).ToArray();
                DrawingVisual visual = new DrawingVisual();
                DrawingContext drawingContext = visual.RenderOpen();
                drawingContext.DrawImage(bitmapSource,
                    new Rect(0, 0, bitmapSource.Width, bitmapSource.Height));
                double dpi = bitmapSource.DpiX;
                double resizeFactor = 96 / dpi;
                foreach (var faceRect in faceRects)
                {

                    drawingContext.DrawRectangle(
                        Brushes.Transparent,
                        new Pen(Brushes.Red, 2),
                        new Rect(
                            faceRect.Left * resizeFactor,
                            faceRect.Top * resizeFactor,
                            faceRect.Width * resizeFactor,
                            faceRect.Height * resizeFactor
                            ));
                }
                drawingContext.Close();
                RenderTargetBitmap faceWithRectBitmap = new RenderTargetBitmap(
                    (int)(bitmapSource.PixelWidth * resizeFactor),
                    (int)(bitmapSource.PixelHeight * resizeFactor),
                    96,
                    96,
                    PixelFormats.Pbgra32);
                faceWithRectBitmap.Render(visual);
                RightImageDisplay.Source = faceWithRectBitmap;
            }
            else
            {
                MessageBox.Show("Verification accepts two faces as input, please pick image with only one detectable face in it.", "Warning", MessageBoxButton.OK);
            }
        }
        private async void Verification_Click(object sender, RoutedEventArgs e)
        {

            if (leftfaceid.Length > 0 && rightfaceid.Length > 0)
            {
                var faceId1 = leftfaceid;
                var faceId2 = rightfaceid;
                // Call verify REST API with two face id
                try
                {
                    this.Title = string.Format("verifying...");
                    var res = await facesc.VerifyAsync(Guid.Parse(faceId1), Guid.Parse(faceId2));
                    this.Title = string.Format("Verify finished");
                    result.Text = string.Format("{0} ({1:0.0})", res.IsIdentical ? "Equals" : "Does not equal", res.Confidence);
                    result1.Text = string.Format(res.IsIdentical ? "belong" : "not belong");
                }
                catch (FaceAPIException ex)
                {
                    //MainWindow.Log("Response: {0}. {1}", ex.ErrorCode, ex.ErrorMessage);

                    return;
                }
            }
            else
            {
                MessageBox.Show("please choose your images", "Warning", MessageBoxButton.OK);
            }
        }
    }
}
