using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Plugins.PictureChooser;
using System.IO;
using System;
using System.Collections.Generic;
using Cirrious.CrossCore;

namespace Eval.Core.ViewModels
{
    public class FirstViewModel 
		: MvxViewModel
    {
        private IMvxCommand _clearCommand;
        public IMvxCommand ClearCommand
        { 
            get { return _clearCommand = _clearCommand ?? new MvxCommand(Clear); }
        }

        private IMvxCommand _takePicture;
        public IMvxCommand TakePictureCommand
        { 
            get { return _takePicture = _takePicture ?? new MvxCommand(TakePicture); }
        }

		private byte[] _downloadedImage;
		public byte[] DownloadedImage
		{
			get { return _downloadedImage; }
			set { _downloadedImage = value; RaisePropertyChanged(() => DownloadedImage); }
		}

        private byte[] _bytes;
        public byte[] Bytes
        {
            get { return _bytes; }
            set { _bytes = value; RaisePropertyChanged(() => Bytes); }
        }
            
        private string _picturesStatus;
        public string PicturesStatus
        {
            get { return _picturesStatus; }
            set { _picturesStatus = value; RaisePropertyChanged(() => PicturesStatus); }
        }

        private string _barcodeResult;
        public string BarcodeResult
        {
            get
            {
                return _barcodeResult;
            }
            set { _barcodeResult = value; RaisePropertyChanged(() => BarcodeResult); } 
        }

        private List<byte[]> _images;
        public List<byte[]> Images
        {
            get
            {
                return _images;
            }
            set { _images = value; RaisePropertyChanged(() => Images); } 
        }

        private IMvxPictureChooserTask _pictureChooserTask;

        private readonly int PICTURES_N = 3;

        public FirstViewModel(IMvxPictureChooserTask pictureChooserTask)
        {
            _pictureChooserTask = pictureChooserTask;
            _images = new List<byte[]>();

            Clear();
        }    

        void Clear()
        {
            _images.Clear();
            PicturesStatus = string.Format("{0} pictures of {1} taken", _images.Count, PICTURES_N);
            BarcodeResult = "";
        }

        void TakePicture()
        {
            try
            {
				_pictureChooserTask.TakePicture(800, 100, OnPicture, () => { });
            }
            catch(Exception)
            {
            }
        }

        void OnPicture(System.IO.Stream pictureStream)
        {
            try
            {
                var memoryStream = new MemoryStream();
                pictureStream.CopyTo(memoryStream);
                var bytes = memoryStream.ToArray();
                _images.Add(bytes);
                Bytes = bytes;
            }
            catch(Exception)
            {
            }
            PicturesStatus = string.Format("{0} pictures of {1} taken", _images.Count, PICTURES_N);
        }
    }
}
