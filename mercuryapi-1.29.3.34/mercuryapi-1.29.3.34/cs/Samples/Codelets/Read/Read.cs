using System;
using System.Collections.Generic;
using System.Text;

// Reference the API
using ThingMagic;

namespace Read
{
    /// <summary>
    /// Sample program that reads tags for a fixed period of time (500ms)
    /// and prints the tags found.
    /// </summary>
    class Program
    {
        static void Usage()
        {
            Console.WriteLine(String.Join("\r\n", new string[] {
                    "Usage: "+"Please provide valid arguments, such as:",
                    "tmr:///com4 or tmr:///com4 --ant 1,2",
                    "tmr://my-reader.example.com or tmr://my-reader.example.com --ant 1,2"
            }));
            Environment.Exit(1);
        }
        static void Main(string[] args)
        {
            // Program setup
            if (1 > args.Length)
            {
                Usage();
            }
            int[] antennaList = null;
            for (int nextarg = 1; nextarg < args.Length; nextarg++)
            {
                string arg = args[nextarg];
                if (arg.Equals("--ant"))
                {
                    if (null != antennaList)
                    {
                        Console.WriteLine("Duplicate argument: --ant specified more than once");
                        Usage();
                    }
                    antennaList = ParseAntennaList(args, nextarg);
                    nextarg++;
                }
                else
                {
                    Console.WriteLine("Argument {0}:\"{1}\" is not recognized", nextarg, arg);
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
                    string model = r.ParamGet("/reader/version/model").ToString();
                    if ((model.Equals("M6e Micro") || model.Equals("M6e Nano") || model.Equals("Sargas")) && antennaList == null)
                    {
                        Console.WriteLine("Module doesn't has antenna detection support please provide antenna list");
                        Usage();
                    }

                    // Enable printTagMetada Flags to print Metadata value
                    bool printTagMetadata = false;

                    if (r is SerialReader)
                    {
                        //SerialReader.TagMetadataFlag flagSet = SerialReader.TagMetadataFlag.ANTENNAID | SerialReader.TagMetadataFlag.FREQUENCY;
                        SerialReader.TagMetadataFlag flagSet = SerialReader.TagMetadataFlag.ALL;
                        r.ParamSet("/reader/metadata", flagSet);
                    }
                    else
                    {
                        // Configurable Metadata param is not supported for llrp readers
                        printTagMetadata = false;
                    }

                    // Create a simplereadplan which uses the antenna list created above
                    SimpleReadPlan plan = new SimpleReadPlan(antennaList, TagProtocol.GEN2, null, null, 1000);
                    // Set the created readplan
                    r.ParamSet("/reader/read/plan", plan);

                    // Read tags
                    TagReadData[] tagReads = r.Read(5000);
                    
                    // Print tag reads
                    foreach (TagReadData tr in tagReads)
                    {
                        Console.WriteLine("EPC: " + tr.EpcString);
                        if (printTagMetadata)
                        {
                            SerialReader.TagMetadataFlag flagGet = (SerialReader.TagMetadataFlag)r.ParamGet("/reader/metadata");
                            foreach (SerialReader.TagMetadataFlag flg in Enum.GetValues(typeof(SerialReader.TagMetadataFlag)))
                            {
                                if (flg != SerialReader.TagMetadataFlag.ALL)
                                {
                                    switch ((SerialReader.TagMetadataFlag)(flagGet & flg))
                                    {
                                        case SerialReader.TagMetadataFlag.ANTENNAID:
                                            Console.WriteLine("Antenna ID : " + tr.Antenna.ToString());
                                            break;
                                        case SerialReader.TagMetadataFlag.DATA:
                                            Console.WriteLine("Data : " + BitConverter.ToString(tr.Data).Replace("-", " "));
                                            break;
                                        case SerialReader.TagMetadataFlag.FREQUENCY:
                                            Console.WriteLine("Frequency : " + tr.Frequency.ToString());
                                            break;
                                        case SerialReader.TagMetadataFlag.GPIO:
                                            foreach (GpioPin pin in tr.GPIO)
                                                Console.WriteLine("GPIO Pin " + pin.Id.ToString() + ": " + (pin.High ? "High" : "Low"));
                                            break;
                                        case SerialReader.TagMetadataFlag.PHASE:
                                            Console.WriteLine("Phase : " + tr.Phase.ToString());
                                            break;
                                        case SerialReader.TagMetadataFlag.PROTOCOL:
                                            Console.WriteLine("Protocol : " + tr.Tag.Protocol.ToString());
                                            break;
                                        case SerialReader.TagMetadataFlag.READCOUNT:
                                            Console.WriteLine("Read Count : " + tr.ReadCount.ToString());
                                            break;
                                        case SerialReader.TagMetadataFlag.RSSI:
                                            Console.WriteLine("RSSI : " + tr.Rssi.ToString());
                                            break;
                                        case SerialReader.TagMetadataFlag.TIMESTAMP:
                                            Console.WriteLine("Timestamp : " + tr.Time.ToLocalTime().ToString());
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                        }
                    }
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

        #region ParseAntennaList

        private static int[] ParseAntennaList(IList<string> args, int argPosition)
        {
            int[] antennaList = null;
            try
            {
                string str = args[argPosition + 1];
                antennaList = Array.ConvertAll<string, int>(str.Split(','), int.Parse);
                if (antennaList.Length == 0)
                {
                    antennaList = null;
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine("Missing argument after args[{0:d}] \"{1}\"", argPosition, args[argPosition]);
                Usage();
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}\"{1}\"", ex.Message, args[argPosition + 1]);
                Usage();
            }
            return antennaList;
        }

        #endregion
    }
}
