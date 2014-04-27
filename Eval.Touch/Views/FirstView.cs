using System.Drawing;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Touch.Views;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using Cirrious.CrossCore;
using Acr.MvvmCross.Plugins.BarCodeScanner;
using Eval.Core.ViewModels;
using Emgu.CV;
using Emgu.CV.Structure;

namespace Eval.Touch.Views
{
    [Register("FirstView")]
    public class FirstView : MvxViewController
    {
        new FirstViewModel ViewModel
        {
            get
            { 
                return base.ViewModel as FirstViewModel;
            }
        }

        public override void ViewDidLoad()
        {
            var view = new UIScrollView(){ BackgroundColor = UIColor.White};
            view.ContentSize = new SizeF(UIScreen.MainScreen.Bounds.Width, 1.5f * UIScreen.MainScreen.Bounds.Height);
            View = view;

            base.ViewDidLoad();

			// ios7 layout
            if (RespondsToSelector(new Selector("edgesForExtendedLayout")))
               EdgesForExtendedLayout = UIRectEdge.None;
			   
            float x = 10, y = 10, w = 300, h = 40;

            var resetButton = new UIButton(UIButtonType.System);
            resetButton.Frame = new RectangleF(10, y, 300, 40);
            resetButton.SetTitle("Reset", UIControlState.Normal);
            View.AddSubview(resetButton);

            y += h + 5;

            var takePicButton = new UIButton(UIButtonType.System);
            takePicButton.Frame = new RectangleF(10, y, 300, 40);
            takePicButton.SetTitle("Take Pictures", UIControlState.Normal);
            View.AddSubview(takePicButton);

            y += h + 5;

            var qr = new UIButton(UIButtonType.System);
            qr.Frame = new RectangleF(10, y, 300, h);
            qr.SetTitle("Scan QR", UIControlState.Normal);
            qr.TouchDown += HandleReadBarcode;
            View.AddSubview(qr);

            y += h + 5;

            var combineButton = new UIButton(UIButtonType.System);
            combineButton.Frame = new RectangleF(10, y, 300, h);
            combineButton.SetTitle("Combine", UIControlState.Normal);
            combineButton.TouchDown += HandleCombine;
            View.AddSubview(combineButton);

            y += h + 5;

            var picturesLabel = new UILabel(new RectangleF(10, y, 300, h));
            View.AddSubview(picturesLabel);

            y += h + 5;

            var qrLabel = new UILabel(new RectangleF(10, y, 300, h));
            View.AddSubview(qrLabel);

            y += h + 5;

            w = UIScreen.MainScreen.Bounds.Width - 2 * x;
            h = UIScreen.MainScreen.Bounds.Height - 2 * x;
            var uiImageView = new UIImageView(new RectangleF(x, y, w, h));
            View.AddSubview(uiImageView);

            var set = this.CreateBindingSet<FirstView, Core.ViewModels.FirstViewModel>();
            set.Bind(qrLabel).To(vm => vm.BarcodeResult);
            set.Bind(picturesLabel).To(vm => vm.PicturesStatus);
            set.Bind(takePicButton).To(vm => vm.TakePictureCommand);
            set.Bind(resetButton).To(vm => vm.ClearCommand);
            set.Bind(uiImageView).To(vm => vm.Bytes).WithConversion("InMemoryImage");

            set.Apply();
        }

        void HandleReadBarcode (object sender, System.EventArgs e)
        {
            Mvx.Resolve<IBarCodeScanner>().Read(OnReadBarcode);
        }

        void HandleCombine (object sender, System.EventArgs e)
        {
            if (ViewModel.Images.Count == 0)
                return;

            Image<Bgr, byte> combinedImage = null;
            foreach (var scannedImage in ViewModel.Images)
            {
                var im = Image<Bgr, byte>.FromRawImageData(scannedImage);
                if (null == combinedImage)
                    combinedImage = new Image<Bgr, byte>(im.Width, im.Height);
                for (int i = 0; i < im.Width; ++i)
                    for (int j = 0; j < im.Height; ++j)
                    {
                        var val = im[j, i];
                        val.Red /= 2;
                        val.Blue /= 2;
                        combinedImage[j,i] = val;
                    }
            }
            ViewModel.Bytes = combinedImage.ToJpegData();
        }

        void OnReadBarcode(BarCodeResult barcodeResult)
        {
            if(barcodeResult.Success)
                ViewModel.BarcodeResult = barcodeResult.Code;
        }
    }
}