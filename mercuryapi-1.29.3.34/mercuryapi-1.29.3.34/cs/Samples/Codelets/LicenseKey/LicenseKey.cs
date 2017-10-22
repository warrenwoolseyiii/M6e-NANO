using System;
using System.Collections.Generic;
using System.Text;

// Reference the API
using ThingMagic;

namespace LicenseKey
{
    class LicenseKey
    {
        static void Usage()
        {
            Console.WriteLine(String.Join("\r\n", new string[] {
                    "Please provide reader URL, such as:",
                    "tmr:///com4 --key <licence key>",
                    "tmr://my-reader.example.com --key <licence key>",
                    "Example: tmr:///com4 --key AB CD",
                }));
            Environment.Exit(1);
        }
        static StringBuilder licenseKey = new StringBuilder();
        static void Main(string[] args)
        {
            List<string> licenseList = new List<string>();
            List<byte> licensebyteList = new List<byte>();
            // Program setup
            if (3 > args.Length)
            {
                Usage();
            }
            else
            {
                try
                {
                    if (!args[1].Equals("--KEY", StringComparison.CurrentCultureIgnoreCase))
                    {
                        Console.WriteLine("Argument 1:\"{1}\" is not recognized", args[1]);
                        Usage();
                    }
                    for (int nextarg = 2; nextarg < args.Length; nextarg++)
                    {
                        licenseKey.Append(args[nextarg]);
                    }

                    licenseList.AddRange(licenseKey.ToString().Trim().Split(' '));
                    string licenseKeyString = string.Join("", licenseList.ToArray());
                    for (int i = 0; i < licenseKey.Length; i += 2)
                    {
                        licensebyteList.Add(Convert.ToByte(licenseKeyString.Substring(i, 2), 16));
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Incorrect License Key.");
                    Usage();
                }
            }

            try
            {
                // Create Reader object, connecting to physical device.
                // Wrap reader in a "using" block to get automatic
                // reader shutdown (using IDisposable interface).
                using (Reader r = Reader.Create(args[0]))
                {
                    //Uncomment this line to add default transport listener.
                    //r.Transport += r.SimpleTransportListener;

                    r.Connect();
                    if (Reader.Region.UNSPEC == (Reader.Region)r.ParamGet("/reader/region/id"))
                    {
                        Reader.Region[] supportedRegions = (Reader.Region[])r.ParamGet("/reader/region/supportedRegions");
                        if (supportedRegions.Length < 1)
                        {
                            throw new FAULT_INVALID_REGION_Exception();
                        }
                        r.ParamSet("/reader/region/id", supportedRegions[0]);
                    }

                    Console.WriteLine("License Update Started...");
                    r.ParamSet("/reader/licensekey", licensebyteList.ToArray());
                    Console.WriteLine("License Update Successfull...");

                    // Priting Supported Porotocol
                    TagProtocol[] protocolList = (TagProtocol[])r.ParamGet("/reader/version/supportedProtocols");
                    Console.WriteLine("Supported Protocols : ");
                    foreach (TagProtocol tp in protocolList)
                    {
                        Console.WriteLine(tp.ToString());
                    }
                    Console.WriteLine("");
                }
            }
            catch (ReaderException re)
            {
                Console.WriteLine("Error: " + re.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}
