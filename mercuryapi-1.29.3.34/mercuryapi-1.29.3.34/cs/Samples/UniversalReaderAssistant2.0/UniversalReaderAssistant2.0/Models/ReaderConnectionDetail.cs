using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace ThingMagic.URA2.Models
{
    public class ReaderConnectionDetail
    {

        private static string readername;
        public static string ReaderName
        {
            get { return readername; }
            set { readername = value; }
        }

        private static string readermodel;
        public static string ReaderModel
        {
            get { return readermodel; }
            set { readermodel = value; }
        }

        private static string baudrate;
        public static string BaudRate
        {
            get { return baudrate; }
            set { baudrate = value; }
        }

        private static string readerType;
        public static string ReaderType
        {
            get { return readerType; }
            set { readerType = value; }
        }

        private static string region;
        public static string Region
        {
            get { return region; }
            set { region = value; }
        }

        private static string antenna;
        public static string Antenna
        {
            get { return antenna; }
            set { antenna = value; }
        }

        private static string protocol;
        public static string Protocol
        {
            get { return protocol; }
            set { protocol = value; }
        }

        static ReaderConnectionDetail()
        { }

        public ReaderConnectionDetail()
        { }

        public static void Display()
        {
            MessageBox.Show(ReaderName + " " + BaudRate + " " + ReaderType + " " + Region);
        }
    }
}
