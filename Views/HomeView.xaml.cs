using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using System.Threading;
using HalconDotNet;

namespace CompactNavigationMenu.Views
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    enum enumStatus
    {
        pclSuccess = 0,

        //Base 
        pclParameterSetupFailed,

        //Device
        pclNoDevice,
        pclDeviceOpenErr,
        pclImageSizeErr,
        pclImageLoadErr,
        pclProjectorSizeErr,
        pclpreviousSetupFailed,
        pclImageBuffersEmpty,
        pclGrabTimeout,
        pclDeviceAlreadyConnected,

        //Core
        pclCoreProcessErr,

        //Calibration
        pclCalloadErr,
        pclCalProcessErr,
        pclUnknownErr,
    };

    //base
    [StructLayout(LayoutKind.Sequential)]
    unsafe public struct basePara
    {
        public bool SoftwareSimulation; //use h/w

        //screen 
        public int screenSizeX;        //px
        public int screenSizeY;

        public float screenPixelSizeX; //mm
        public float screenPixelSizeY;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string dir_imagesave; //이미지 저장 경로

        public CoreCoordinate coordinate_system;

        bool saveImage;
        bool useItyTable;
    }

    //sync
    [StructLayout(LayoutKind.Sequential)]
    unsafe public struct syncPara
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string local_ip;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string remote_ip;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string port;

        //camera
        public int maxcamera;
        public int ImageSizeX;
        public int ImageSizeY;

        //projector
        public int projSizeX;
        public int projSizeY;

        //command
        public float exposure_time;
        public float high_time;

        public int capture_count;
    }

    //core
    //enum 
    public enum DetectionAlgorithm
    {
        Structuredlight = 1,
        ActiveStereo = 2,
    }

    public enum ScanType
    {
        Inspection = 0,
        Calibration = 1,
        Continuous = 2,
        Intensity_Calibration = 3,
    }
    public enum CoreCoordinate
    {
        Camera = 0,
        World = 1,
    };

    [StructLayout(LayoutKind.Sequential)]
    unsafe public struct corePara
    {
        int bucket;
        int threshold;
        int max_distance;
        int amplitudethreshold;

        public DetectionAlgorithm detectionAlgo;
        public ScanType scanType;
    }

    //calibration
    [StructLayout(LayoutKind.Sequential)]
    unsafe public struct calPara
    {
        bool fit_plane;
    }


    //point cloud
    [StructLayout(LayoutKind.Sequential)]
    unsafe public struct pointcloud
    {
        public IntPtr x;
        public IntPtr y;
        public IntPtr z;
        public long dataSize;
    }
    public partial class HomeView : UserControl
    {    
        [DllImport("Library.PointCloudOptics.dll")]
        public unsafe static extern int pclUploadParameter(basePara para1, syncPara para2, corePara para3, calPara para4);

        [DllImport("Library.PointCloudOptics.dll")]
        public unsafe static extern int pclDownloadParameter(ref basePara para1, ref syncPara para2, ref corePara para3, ref calPara para4);

        [DllImport("Library.PointCloudOptics.dll")]
        public unsafe static extern int pclSetup();

        [DllImport("Library.PointCloudOptics.dll")]
        public unsafe static extern int pclCapture();

        [DllImport("Library.PointCloudOptics.dll")]
        public unsafe static extern int pclManualRun(string dir, pointcloud* data);

        [DllImport("Library.PointCloudOptics.dll")]
        public unsafe static extern int pclRun(byte** pImages, pointcloud* data);

        [DllImport("Library.PointCloudOptics.dll")]
        public unsafe static extern int pclScan(pointcloud* data);

        basePara para1 = new basePara();
        syncPara para2 = new syncPara();
        corePara para3 = new corePara();
        calPara para4 = new calPara();

        Rectangle rectHalcon;
        HWindow hWindow;

        public HomeView()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(HalconWPFWindow_Loaded);
            //this.SizeChanged += HalconWPFWindow_SizeChanged;
        }

        private void HomeView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            double width = HalconWPFWindow.Width;
            rectHalcon.Width = (int)width;
            double height = HalconWPFWindow.Height;
            rectHalcon.Height = (int)height;

            hWindow = HalconWPFWindow.HalconWindow;

            if (hWindow == null)
            {
                return;
            }

            Thread thread = new Thread(new ThreadStart(ThreadRun));
            thread.Start();            

            unsafe
            {
                enumStatus status = (enumStatus)pclSetup();

                status = (enumStatus)pclDownloadParameter(ref para1, ref para2, ref para3, ref para4);

                txtBoxResult.Text = status.ToString();                
            }
        }

        void ThreadRun()
        {
            HDevelopExport HD = new HDevelopExport(rectHalcon, hWindow);
        }

        private void HalconWPFWindow_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            HalconWPFWindow.Width = e.NewSize.Width;
            HalconWPFWindow.Height = e.NewSize.Height;
        }

        private void HalconWPFWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (double.IsNaN(this.Width)==true) return;

            HalconWPFWindow.Width = this.Width;
            HalconWPFWindow.Height = this.Height;
            
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            
        }
    }
}
