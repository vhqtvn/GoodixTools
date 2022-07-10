using HidLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GoodixTools
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        HidDevice device;

        private void button1_Click(object sender, EventArgs e)
        {
            if (device == null)
            {
                foreach (HidDevice hd in HidDevices.Enumerate())
                {
                    if (hd.ToString().Contains("gxt"))
                    {
                        Console.WriteLine(hd.ToString());
                    }
                }
                foreach (HidDevice hd in HidDevices.Enumerate(0x27C6, 0x0113))
                {
                    if (hd.ToString().Contains("col03")) device = hd;
                }
            }
            device.OpenDevice(DeviceMode.NonOverlapped, DeviceMode.NonOverlapped, ShareMode.ShareRead | ShareMode.ShareWrite);
            if (!device.IsOpen)
            {
                Console.WriteLine("Not open");
            }
            Thread.Sleep(100);
            byte[] buf = new byte[512];
            for (int offset = 0x8050, xoffset = 0; xoffset < 500; offset += 50, xoffset += 50)
            {
                byte[] data = new byte[65];
                int di = 0;
                data[di++] = 0x0e;
                data[di++] = 0x20;
                data[di++] = 0x00;
                data[di++] = 0x00;

                data[di++] = 0x05;
                data[di++] = 0x01;

                data[di++] = (byte)(offset / 256);
                data[di++] = (byte)(offset % 256);

                data[di++] = 50;
                data[di++] = 50;

                Console.WriteLine("Send request");
                HidReport wreport = new HidReport(65, new HidDeviceData(data, HidDeviceData.ReadStatus.Success));
                for (int i = 0; i < 10; i++)
                    if (!device.WriteReportSync(wreport))
                    {
                        Console.WriteLine("Failed");
                    }
                    else
                    {
                        Console.WriteLine("Ok");
                        break;
                    }
                for (int i = 0; i < 10; i++)
                {
                    Console.WriteLine(" Read request " + i + ": " + device.Capabilities.InputReportByteLength);
                    HidReport report = device.ReadReportSync(0xe);
                    if (report.ReadStatus == HidDeviceData.ReadStatus.NoDataRead) continue;
                    Console.WriteLine("" + report.ReadStatus + " : " + BitConverter.ToString(report.Data));
                    report.Data.Skip(4).Take(50).ToArray().CopyTo(buf, xoffset);
                    break;
                }
            }
            device.CloseDevice();
            txtHex.Text = "0x" + BitConverter.ToString(buf.Take(444).ToArray()).Replace("-", ",0x");
        }
    }
}
