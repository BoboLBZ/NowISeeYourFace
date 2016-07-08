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
using System.IO;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;

namespace face
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IFaceServiceClient facesc = new FaceServiceClient("98efc23bfd4048279fccac2a079f4451");
        private readonly EmotionServiceClient emotionsc =new EmotionServiceClient("1ee9a7d29d3b46f797242bf989c36871");
        public MainWindow()
        {
            InitializeComponent();
            //this.Content = new faceApi();
        }
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {faceAPI(0);  }
        private void age_Click(object sender, RoutedEventArgs e)
        { faceAPI(1); }       
        private void gender_Click(object sender, RoutedEventArgs e)
        { faceAPI(2); }
        private void facialhair_Click(object sender, RoutedEventArgs e)
        { faceAPI(3); }
        private void glasses_Click(object sender, RoutedEventArgs e)
        { faceAPI(4); }
        private void veri_Click(object sender, RoutedEventArgs e)
        {
            faceVeri f = new faceVeri();
            f.Show();
        }
        private async void emotion_Click(object sender, RoutedEventArgs e)
        {
            var openDlg = new Microsoft.Win32.OpenFileDialog();

            openDlg.Filter = "JPEG Image(*.jpg)|*.jpg";
            bool? result = openDlg.ShowDialog(this);

            if (!(bool)result)
            {
                return;
            }
            Title = "Detecting...";
            Uri fileUri = new Uri(openDlg.FileName);
            BitmapImage bitmapSource = new BitmapImage();
            bitmapSource.BeginInit();
            bitmapSource.CacheOption = BitmapCacheOption.None;
            bitmapSource.UriSource = fileUri;
            bitmapSource.EndInit();
            FacePhoto.Source = bitmapSource;
            //emotion api
            Stream imageFileStream = File.OpenRead(openDlg.FileName);
            Emotion[] emotions= await emotionsc.RecognizeAsync(imageFileStream);
            Title = String.Format("Detection Finished");
            if (emotions.Length > 0)
            {
                string display = "";
                DrawingVisual visual = new DrawingVisual();
                DrawingContext drawingContext = visual.RenderOpen();
                drawingContext.DrawImage(bitmapSource,
                    new Rect(0, 0, bitmapSource.Width, bitmapSource.Height));
                double dpi = bitmapSource.DpiX;
                double resizeFactor = 96 / dpi;
                int id = 1;
                foreach (var emotion in emotions)
                {

                    var faceRect = emotion.FaceRectangle;
                    var scores = emotion.Scores;
                    drawingContext.DrawRectangle(
                        Brushes.Transparent,
                        new Pen(Brushes.Red, 2),
                        new Rect(
                            faceRect.Left * resizeFactor,
                            faceRect.Top * resizeFactor,
                            faceRect.Width * resizeFactor,
                            faceRect.Height * resizeFactor
                            ));
                    // select the emotion
                    float[] emotionscores = {scores.Anger,scores.Contempt,scores.Disgust,scores.Fear,
                                             scores.Happiness,scores.Neutral,scores.Sadness,scores.Surprise };
                    int temp = 0;
                    for(int i=1;i<8;i++)
                    {
                        if (emotionscores[i] > emotionscores[temp])
                            temp = i;
                    }
                    switch (temp)
                    {
                        case 0:
                            display = "anger";
                            break;
                        case 1:
                            display = "contempt";
                            break;
                        case 2:
                            display = "disgust";
                            break;
                        case 3:
                            display = "fear";
                            break;
                        case 4:
                            display = "happiness";
                            break;
                        case 5:
                            display = "neutrul";
                            break;
                        case 6:
                            display = "sadness";
                            break;
                        case 7:
                            display = "surprise";
                            break;
                        default:
                            break;
                    }
                    drawingContext.DrawText(new FormattedText(display,
                                                new System.Globalization.CultureInfo("en-us"),
                                                FlowDirection.LeftToRight,
                                                new Typeface("Verdana"),
                                                32, Brushes.Red),
                        new Point(faceRect.Left * resizeFactor, (faceRect.Top+faceRect.Height) * resizeFactor));
                    id++;
                }
                drawingContext.Close();
                RenderTargetBitmap faceWithRectBitmap = new RenderTargetBitmap(
                    (int)(bitmapSource.PixelWidth * resizeFactor),
                    (int)(bitmapSource.PixelHeight * resizeFactor),
                    96,
                    96,
                    PixelFormats.Pbgra32);

                faceWithRectBitmap.Render(visual);
                FacePhoto.Source = faceWithRectBitmap;
            }
        }
        private async void faceAPI(int flag)
        {
            var openDlg = new Microsoft.Win32.OpenFileDialog();

            openDlg.Filter = "JPEG Image(*.jpg)|*.jpg";
            bool? result = openDlg.ShowDialog(this);

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
            FacePhoto.Source = bitmapSource;
            Title = "Detecting...";
            Stream imageFileStream = File.OpenRead(filePath);
            var requiredFaceAttributes = new FaceAttributeType[] {
                FaceAttributeType.Age,
                FaceAttributeType.Gender,
                FaceAttributeType.Smile,
                FaceAttributeType.FacialHair,
                FaceAttributeType.HeadPose,
                FaceAttributeType.Glasses
            };
            var faces = await facesc.DetectAsync(imageFileStream, returnFaceAttributes: requiredFaceAttributes);
            FaceRectangle[] faceRects = faces.Select(face => face.FaceRectangle).ToArray();
            Title = String.Format("Detection Finished");
            if (faceRects.Length > 0)
            {
                string display = "";
                DrawingVisual visual = new DrawingVisual();
                DrawingContext drawingContext = visual.RenderOpen();
                drawingContext.DrawImage(bitmapSource,
                    new Rect(0, 0, bitmapSource.Width, bitmapSource.Height));
                double dpi = bitmapSource.DpiX;
                double resizeFactor = 96 / dpi;
                int id = 1;
                foreach (var face in faces)
                {

                    var faceRect = face.FaceRectangle;
                    var faceAttribute = face.FaceAttributes;
                    drawingContext.DrawRectangle(
                        Brushes.Transparent,
                        new Pen(Brushes.Red, 2),
                        new Rect(
                            faceRect.Left * resizeFactor,
                            faceRect.Top * resizeFactor,
                            faceRect.Width * resizeFactor,
                            faceRect.Height * resizeFactor
                            ));
                    //不同的选择
                    if (flag == 0)
                        display = "";
                    if (flag == 1)
                        display = faceAttribute.Age.ToString();
                    if (flag == 2)
                        display = faceAttribute.Gender.ToString();
                    if (flag == 3)
                    {
                        var facialhair = faceAttribute.FacialHair;
                        if (facialhair.Beard > 0 && facialhair.Moustache > 0 && facialhair.Sideburns > 0)
                        {
                            if (facialhair.Beard > facialhair.Moustache)
                            {
                                if (facialhair.Sideburns > facialhair.Beard)
                                    display = "Sideburns";
                                else
                                    display = "Beard";
                            }
                            else
                            {
                                if (facialhair.Sideburns > facialhair.Moustache)
                                    display = "Sideburns";
                                else
                                    display = "Moustache";
                            }
                        }
                        else
                            display = "No facialhair";
                    }
                    if (flag == 4)
                        display = faceAttribute.Glasses.ToString();
                    drawingContext.DrawText(new FormattedText(display,
                                                new System.Globalization.CultureInfo("en-us"),
                                                FlowDirection.LeftToRight,
                                                new Typeface("Verdana"),
                                                32, Brushes.Red),
                        new Point(faceRect.Left * resizeFactor, (faceRect.Top + faceRect.Height) * resizeFactor));
                    id++;
                }
                drawingContext.Close();
                RenderTargetBitmap faceWithRectBitmap = new RenderTargetBitmap(
                    (int)(bitmapSource.PixelWidth * resizeFactor),
                    (int)(bitmapSource.PixelHeight * resizeFactor),
                    96,
                    96,
                    PixelFormats.Pbgra32);

                faceWithRectBitmap.Render(visual);
                FacePhoto.Source = faceWithRectBitmap;
            }
        }
    }
    
}
