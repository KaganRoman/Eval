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
using MonoTouch.MessageUI;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System;

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

        UIActivityIndicatorView _activitySpinner;

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
            takePicButton.SetTitle("Take Picture", UIControlState.Normal);
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

            var sendButton = new UIButton(UIButtonType.System);
            sendButton.Frame = new RectangleF(10, y, 300, 40);
            sendButton.SetTitle("Send Result", UIControlState.Normal);
            sendButton.TouchDown += HandleSend;
            View.AddSubview(sendButton);

            y += h + 5;

            var picturesLabel = new UILabel(new RectangleF(10, y, 300, h));
            View.AddSubview(picturesLabel);

            y += h + 5;

            var qrLabel = new UILabel(new RectangleF(10, y, 300, h));
            View.AddSubview(qrLabel);

            y += h + 5;

            _activitySpinner = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.Gray);
            _activitySpinner.Frame = new RectangleF(UIScreen.MainScreen.Bounds.Width / 2 - _activitySpinner.Frame.Width/2, y, _activitySpinner.Frame.Width, _activitySpinner.Frame.Height);
            //_activitySpinner.AutoresizingMask = UIViewAutoresizing.FlexibleMargins;
            View.AddSubview(_activitySpinner);
            y += _activitySpinner.Frame.Height + 5;

            w = UIScreen.MainScreen.Bounds.Width - 20;
            h = w * 1.5f;
            var uiImageView = new UIImageView(new RectangleF(10, y, w, h));
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

		async void HandleCombine (object sender, System.EventArgs e)
        {
			// if (ViewModel.Images.Count == 0)
			//  return;

            _activitySpinner.StartAnimating();
			await CombineTask();
			_activitySpinner.StopAnimating ();
        }

		async Task CombineTask()
        {
			try
			{
				var webClient = new HttpClient ();
				var response = await webClient.GetAsync (@"http://upload.wikimedia.org/wikipedia/commons/2/22/Turkish_Van_Cat.jpg");
				var certImage = await response.Content.ReadAsByteArrayAsync();

				Image<Bgr, byte> combinedImage = Image<Bgr, byte>.FromRawImageData(certImage);
				var images = new List<Image<Bgr, byte>>();
				foreach (var scannedImage in ViewModel.Images)
					images.Add(Image<Bgr, byte>.FromRawImageData(scannedImage));

				ViewModel.Bytes = combinedImage.ToJpegData();
			}
			catch(Exception e) {
			}
        }

        void OnReadBarcode(BarCodeResult barcodeResult)
        {
            if(barcodeResult.Success)
                ViewModel.BarcodeResult = barcodeResult.Code;
        }

        void HandleSend(object sender, System.EventArgs eventArgs)
        {
            var mail = new MFMailComposeViewController();
            var body = string.Format("QR Code: {0}\r\nImages", ViewModel.BarcodeResult);
            mail.SetMessageBody(body, false);
            mail.SetSubject("Results");
            mail.SetToRecipients(new[] {"roman@rumble.me"});
            int i = 0;
            foreach(var im in ViewModel.Images)
                mail.AddAttachmentData(NSData.FromArray(im), "image/jpg", string.Format("im{0}.jpg", ++i));
            if(ViewModel.Bytes != null)
                mail.AddAttachmentData(NSData.FromArray(ViewModel.Bytes), "image/jpg", "combined.jpg");
            mail.Finished += (s, e) => (s as UIViewController).DismissViewController(true, () => { });
            PresentViewController(mail, true, null);
        }
    }
}