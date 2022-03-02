using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using lwsc_xamarin_lora.Services;
using UIKit;
using Xamarin.Forms;

namespace lwsc_xamarin_lora.iOS
{
    public class Application
    {
        // This is the main entry point of the application.
        static void Main(string[] args)
        {
            // if you want to use a different Application Delegate class from "AppDelegate"
            // you can specify it here.
            DependencyService.Register<IMessage, MessageIOS>();
            UIApplication.Main(args, null, typeof(AppDelegate));

        }
    }
}
