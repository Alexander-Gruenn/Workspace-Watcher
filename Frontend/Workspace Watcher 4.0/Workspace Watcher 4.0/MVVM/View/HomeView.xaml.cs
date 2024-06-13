using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
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
using Workspace_Watcher_4._0;
using System.Diagnostics;
using WpfAnimatedGif;
using System.Windows.Media.Effects;
using System.Drawing;
using Brushes = System.Windows.Media.Brushes;

namespace Workspace_Watcher_4._0.MVVM.View
{
    /// <summary>
    /// Interaktionslogik für HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        static MainWindow mainWindow;
        static WriteableBitmap _writableBitmap;
        public static VideoCapture videoCapture;
        Mat mat1;
        static Timer timer;
        static Timer timerForFaceDetection;

        public static int DetectedFaces { get; set; } = 0;
        private const int MIN_FACE_SIZE = 85;

        System.Drawing.Rectangle[] markrect;
        System.Drawing.Rectangle[] markrect2;
        IEnumerable<System.Drawing.Rectangle> concatinatedMarkRects;

        private const int FPS = 30;
        private const int FACE_DETECTION_INTERVAL = 3000;

        private static bool isTimerActive = false;
        private static bool cameraStarted = false;
        private static bool homeViewIsLoaded = false;

        public HomeView()
        {

            InitializeComponent();
            //SetAnimatedLogo();

            //currentHomeView = this;

            App.SetHomeView(this);

            RecordButton.Content = cameraStarted ? "Stop" : "Start";

            ImageDisplay.Stretch = Stretch.Uniform;
            ImageDisplay.StretchDirection = StretchDirection.Both;

            if (!isTimerActive)
            {
                timer = new Timer();
                timer.Interval = 1000 / FPS;
                timer.AutoReset = true;
                timer.Elapsed += OnElapsed;

                timerForFaceDetection = new Timer();
                timerForFaceDetection.Interval = FACE_DETECTION_INTERVAL;
                timerForFaceDetection.AutoReset = true;
                timerForFaceDetection.Elapsed += OnTimerForFaceDetectionElapsed;
                isTimerActive = true;
            }
        }

        private void SetAnimatedLogo()
        {
            BitmapImage animatedLogo = new BitmapImage();
            animatedLogo.BeginInit();
            animatedLogo.UriSource = new Uri("../../Animations/Workspace-Watcher-logo-idle-animation.gif", UriKind.Relative);
            animatedLogo.EndInit();
            
            //ImageBehavior.SetAnimatedSource(ImageDisplay, animatedLogo);
        }

        private void OnTimerForFaceDetectionElapsed(object sender, ElapsedEventArgs e)
        {
            GetFaceFrames();
        }

        private void OnElapsed(object sender, ElapsedEventArgs e)
        {
            mat1 = videoCapture.QueryFrame();

            CameraOnFrame(this, mat1.ToImage<Bgr, byte>());
        }
        
        private void GetFaceFrames()
        {
            Image<Bgr, byte> emguImg = mat1.ToImage<Bgr, byte>();

            CascadeClassifier classifier = new CascadeClassifier("haarcascade_frontalface_default.xml");
            CascadeClassifier classifier2 = new CascadeClassifier("haarcascade_profileface.xml");
            
            markrect = classifier.DetectMultiScale(emguImg);
            markrect2 = classifier2.DetectMultiScale(emguImg);

            DetectedFaces = 0;

            concatinatedMarkRects = markrect.Concat<System.Drawing.Rectangle>(markrect2);


            //Ignore faces that are too far away
            foreach (var face in concatinatedMarkRects) //Go through all detected faces
                if (face.Size.Width >= MIN_FACE_SIZE) //Only count faces that are big enough
                    DetectedFaces++;

            if (DetectedFaces > 0)
            {
                if (!Dispatcher.CheckAccess())
                {
                    Dispatcher.Invoke(() =>
                    {
                        mainWindow.RecordIndicator.Background = Brushes.Green;
                        mainWindow.MyShadowEffect.Color = Colors.Green;
                        mainWindow.faceDetectedText.Text = "Face Detected!";
                    });

                }
            }
            else
            {
                if (!Dispatcher.CheckAccess())
                {
                    Dispatcher.Invoke(() =>
                    {
                        mainWindow.RecordIndicator.Background = Brushes.Red;
                        mainWindow.MyShadowEffect.Color = Colors.Red;
                        mainWindow.faceDetectedText.Text = "No Face Detected!";
                    });

                }
            }
        }

        public static void GetMainWindow(MainWindow main)
        {
            mainWindow = main;
        }

        private void StartRecording()
        {
            //ImageBehavior.SetAnimatedSource(ImageDisplay, null);

            if (!cameraStarted)
            {
                _writableBitmap = new WriteableBitmap(videoCapture.Width, videoCapture.Height, 96, 96, PixelFormats.Bgr24, null);
                ImageDisplay.Source = _writableBitmap;

                FlipVideoStream();


                timer.Start();
                timerForFaceDetection.Start();

                cameraStarted = true;
            }
            else
            {
                timer.Start();
                timerForFaceDetection.Start();
            }
        }

        public static void StopRecording()
        {
            if (isTimerActive)
            {
                timer.Stop();
                timerForFaceDetection.Stop();
            }
            mainWindow.RecordIndicator.Background = Brushes.Red;
            mainWindow.MyShadowEffect.Color = Colors.Red;
            DetectedFaces = 0;
            //SetAnimatedLogo();
        }

        private void CameraOnFrame(object sender, Image<Bgr, byte> img)
        {
            if (homeViewIsLoaded)
            {
                if (!Dispatcher.CheckAccess())
                {
                    Dispatcher.Invoke(() => CameraOnFrame(sender, img));
                    return;
                }

                _writableBitmap.WritePixels(new Int32Rect(0, 0, img.Width, img.Height), img.MIplImage.ImageData, (img.MIplImage.Height * img.MIplImage.WidthStep), img.MIplImage.WidthStep);

                
            }
        }

        private void RecordButton_Checked(object sender, RoutedEventArgs e)
        {
            if (videoCapture != null)
            {
                RecordButton.Content = "Stop";
                StartRecording();
            }
            else
            {
                MessageBox.Show("Es wurde keine Kamera im System entdeckt", "Achtung!", MessageBoxButton.OK, MessageBoxImage.Error);
                RecordButton.IsEnabled = false;
                App.GetVideoCapture();

            }
        }

        private void RecordButton_Unchecked(object sender, RoutedEventArgs e)
        {
            RecordButton.Content = "Start";
            StopRecording();
            concatinatedMarkRects = null;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            homeViewIsLoaded = false;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (cameraStarted)
            {
                _writableBitmap = new WriteableBitmap(videoCapture.Width, videoCapture.Height, 96, 96, PixelFormats.Bgr24, null);
                ImageDisplay.Source = _writableBitmap;

                FlipVideoStream();
            }

            homeViewIsLoaded = true;
        }

        private void FlipVideoStream()
        {
            ImageDisplay.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            ScaleTransform flipTrans = new();
            flipTrans.ScaleX = -1;
            ImageDisplay.RenderTransform = flipTrans;
        }
    }
}
