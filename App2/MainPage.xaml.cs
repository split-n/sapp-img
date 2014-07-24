using Windows.Devices.Input;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using App2.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 基本ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234237 を参照してください

namespace App2
{
    /// <summary>
    /// 多くのアプリケーションに共通の特性を指定する基本ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private NavigationHelper navigationHelper;
        /// <summary>
        /// NavigationHelper は、ナビゲーションおよびプロセス継続時間管理を
        /// 支援するために、各ページで使用します。
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        private Point prevPoint;


        private EventHandler<object> layoutChanged;
        private uint paintingPointerId;
        private Windows.UI.Xaml.Shapes.Path path;

        public MainPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;

            layoutChanged = (s, ee) => imageAndCanvasSizeAdjust();
        }

        /// <summary>
        /// このページには、移動中に渡されるコンテンツを設定します。前のセッションからページを
        /// 再作成する場合は、保存状態も指定されます。
        /// </summary>
        /// <param name="sender">
        /// イベントのソース (通常、<see cref="NavigationHelper"/>)>
        /// </param>
        /// <param name="e">このページが最初に要求されたときに
        /// <see cref="Frame.Navigate(Type, Object)"/> に渡されたナビゲーション パラメーターと、
        /// 前のセッションでこのページによって保存された状態の辞書を提供する
        /// セッション。ページに初めてアクセスするとき、状態は null になります。</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            image.Source =
                new BitmapImage(
                    new Uri(
                        "http://kyotonotabi.c.blog.so-net.ne.jp/_images/blog/_d02/kyotonotabi/E4BC8FE8A68BE7A8B2E88DB7E5A4A7E7A4BE20E58D83E69CACE9B3A5E5B185.JPG?c=a3"));
            LayoutUpdated += layoutChanged;
        }

        /// <summary>
        /// アプリケーションが中断される場合、またはページがナビゲーション キャッシュから破棄される場合、
        /// このページに関連付けられた状態を保存します。値は、
        /// <see cref="SuspensionManager.SessionState"/> のシリアル化の要件に準拠する必要があります。
        /// </summary>
        /// <param name="sender">イベントのソース (通常、<see cref="NavigationHelper"/>)</param>
        /// <param name="e">シリアル化可能な状態で作成される空のディクショナリを提供するイベント データ
        ///。</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            LayoutUpdated -= layoutChanged;

        }

        #region NavigationHelper の登録

        /// このセクションに示したメソッドは、NavigationHelper がページの
        /// ナビゲーション メソッドに応答できるようにするためにのみ使用します。
        /// 
        /// ページ固有のロジックは、
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// および <see cref="GridCS.Common.NavigationHelper.SaveState"/> のイベント ハンドラーに配置する必要があります。
        /// LoadState メソッドでは、前のセッションで保存されたページの状態に加え、
        /// ナビゲーション パラメーターを使用できます。

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void InkCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {

            var pt = e.GetCurrentPoint(InkCanvas);
            if (pt.Properties.IsRightButtonPressed) return;
            prevPoint = pt.Position;
            paintingPointerId = pt.PointerId;
            path = new Windows.UI.Xaml.Shapes.Path() {
                Data = new PathGeometry(),
                Stroke = new SolidColorBrush(Windows.UI.Colors.Blue),
                StrokeStartLineCap = PenLineCap.Round,
                StrokeThickness = 8.5
                
            };
            ((PathGeometry)path.Data).Figures.Add(new PathFigure{StartPoint = pt.Position});
            InkCanvas.Children.Add(path);
            e.Handled = true;

        }

        private void image_ImageOpened(object sender, RoutedEventArgs e)
        {
            imageAndCanvasSizeAdjust();
        }

        private void imageAndCanvasSizeAdjust()
        {
            var imgSrc = (BitmapImage)image.Source;
            var c = container;
            var hfactor = container.ActualHeight / imgSrc.PixelHeight;
            var wfactor = container.ActualWidth / imgSrc.PixelWidth;

            var minfactor = Math.Min(hfactor, wfactor);
            InkCanvas.Height = image.Height = imgSrc.PixelHeight * minfactor;
            InkCanvas.Width = image.Width = imgSrc.PixelWidth * minfactor;
        }



        private void InkCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var pt = e.GetCurrentPoint(InkCanvas);
            if (pt.PointerId == paintingPointerId)
            {
                var currPoint = pt.Position;
                if (prevPoint != currPoint)
                {
                    var geo = (PathGeometry) path.Data;
                    geo.Figures.First().Segments.Add(new LineSegment{Point = currPoint});

                    prevPoint = currPoint;

                    e.Handled = true;
                }
            }
        }

        private void InkCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            paintingPointerId = 0;
            e.Handled = true;
        }

        private async void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var imgSrc = (BitmapImage)image.Source;
            InkCanvas.Width = imgSrc.PixelWidth*5;
            InkCanvas.Height = imgSrc.PixelHeight*5;

            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(this.InkCanvas);
            
            var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();

            var savePicker = new FileSavePicker();
            savePicker.DefaultFileExtension = ".png";
            savePicker.FileTypeChoices.Add(".png", new List<string> { ".png" });
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.SuggestedFileName = "snapshot.png";

            // Prompt the user to select a file
            var saveFile = await savePicker.PickSaveFileAsync();

            // Verify the user selected a file
            if (saveFile == null)
                return;




            // Encode the image to the selected file on disk
            using (var fileStream = await saveFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, fileStream);

                encoder.SetPixelData(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Premultiplied, // http://compojigoku.blog.fc2.com/blog-entry-5.html
                    (uint)renderTargetBitmap.PixelWidth,
                    (uint)renderTargetBitmap.PixelHeight,
                    DisplayInformation.GetForCurrentView().LogicalDpi,
                    DisplayInformation.GetForCurrentView().LogicalDpi,
                    pixelBuffer.ToArray()
                    );

                await encoder.FlushAsync();
            }

            imageAndCanvasSizeAdjust();
        }
    }
}
