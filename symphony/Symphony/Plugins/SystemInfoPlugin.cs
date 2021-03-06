﻿using System;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Paragon.Plugins;
using Symphony.Win32;

namespace Symphony.Plugins
{
    [JavaScriptPlugin(Name = "symphony.systeminfo", IsBrowserSide = true)]
    public class SystemInfoPlugin: IParagonPlugin
    {
        private string windowDpi;

        public IApplication Application { get; private set; }

        public void Initialize(IApplication application)
        {
            Application = application;
        }

        public void Shutdown()
        {
            Application = null;
        }

        [JavaScriptPluginMember]
        public JObject GetSystemInfo()
        {
            JObject json = new JObject();

            var ip = Dns
                .GetHostAddresses(Dns.GetHostName())
                .FirstOrDefault(address => address.AddressFamily == AddressFamily.InterNetwork);

            if (ip != null)
            {
                json["ip"] = ip.ToString();
            }

            json["version"] = "LiveCurrentClient_" + Assembly
                .GetExecutingAssembly()
                .GetName()
                .Version;

            if (Application != null && Application.Metadata != null)
            {
                var package = Application.Metadata.GetApplicationPackage();

                if (package != null && package.Manifest != null)
                {
                    json["appVersion"] = package.Manifest.Version;
                }
            }

            json["windowDpi"] = this.GetWindowDpi();

            return json;
        }

        private string GetWindowDpi()
        {
            if (string.IsNullOrEmpty(this.windowDpi))
            {
                int xDpi;
                using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                {
                    IntPtr desktop = g.GetHdc();
                    xDpi = NativeMethods.GetDeviceCaps(desktop, NativeMethods.LOGPIXELSX);
                }

                if (xDpi < 120)
                {
                    this.windowDpi = "Smaller";
                }
                else if (xDpi < 144)
                {
                    this.windowDpi = "Medium";
                }
                else
                {
                    this.windowDpi = "Larger";
                }
            }

            return this.windowDpi;
        }
    }
}
