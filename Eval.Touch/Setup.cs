using MonoTouch.UIKit;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Touch.Platform;
using MonoTouch.TestFlight;
using System;
using System.Runtime.InteropServices;

namespace Eval.Touch
{
	public class Setup : MvxTouchSetup
	{
		[DllImport ("libc")]
		private static extern int sigaction (Signal sig, IntPtr act, IntPtr oact);

		enum Signal {
			SIGBUS = 10,
			SIGSEGV = 11
		}

		public Setup(MvxApplicationDelegate applicationDelegate, UIWindow window)
            : base(applicationDelegate, window)
		{
		}

		protected override IMvxApplication CreateApp ()
		{
			CheckCrashes ();
			return new Core.App();
		}
		
        protected override IMvxTrace CreateDebugTrace()
        {
            return new DebugTrace();
        }

		void CheckCrashes()
		{
			IntPtr sigbus = Marshal.AllocHGlobal (512);
			IntPtr sigsegv = Marshal.AllocHGlobal (512);

			// Store Mono SIGSEGV and SIGBUS handlers
			sigaction (Signal.SIGBUS, IntPtr.Zero, sigbus);
			sigaction (Signal.SIGSEGV, IntPtr.Zero, sigsegv);

			// Enable crash reporting libraries
			RegisterTestFlight();

			// Restore Mono SIGSEGV and SIGBUS handlers            
			sigaction (Signal.SIGBUS, sigbus, IntPtr.Zero);
			sigaction (Signal.SIGSEGV, sigsegv, IntPtr.Zero);

			Marshal.FreeHGlobal (sigbus);
			Marshal.FreeHGlobal (sigsegv);

		}

		void RegisterTestFlight ()
		{
			string testflightKey = "9a624e6a-5140-4ede-a56c-10c8b9a29344";
			if (!string.IsNullOrEmpty(testflightKey))
			{
				TestFlight.TakeOffThreadSafe(testflightKey);
				TestFlight.PassCheckpoint("Starting app");
			}
		}

	}
}