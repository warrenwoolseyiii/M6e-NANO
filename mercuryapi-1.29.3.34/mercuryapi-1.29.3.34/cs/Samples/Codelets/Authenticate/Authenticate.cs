using System;
using System.Collections.Generic;
using System.Text;
using AesLib;
// Reference the API
using ThingMagic;

namespace Authenticate
{
    /// <summary>
    /// Sample program that to demonstrate the usage of Gen2v2 Authenticate.
    /// </summary>
    class Authenticate
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

                    // Create a simplereadplan which uses the antenna list created above
                    SimpleReadPlan plan = new SimpleReadPlan(antennaList, TagProtocol.GEN2, null, null, 1000);
                    // Set the created readplan
                    r.ParamSet("/reader/read/plan", plan);

                    //Use first antenna for operation
                    if (antennaList != null)
                        r.ParamSet("/reader/tagop/antenna", antennaList[0]);


                    ushort[] Key0 = new ushort[] { 0x0123, 0x4567, 0x89AB, 0xCDEF, 0x0123, 0x4567, 0x89AB, 0xCDEF };
                    ushort[] Key1 = new ushort[] { 0x1122, 0x3344, 0x5566, 0x7788, 0x1122, 0x3344, 0x5566, 0x7788 };
                    ushort[] Ichallenge = new ushort[] { 0x0123, 0x4567, 0x89AB, 0xCDEF, 0xABCD };

                    Gen2.Select filter = new Gen2.Select(false, Gen2.Bank.EPC, 32, 96,
                        new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF });

                    bool SendRawData = true;
                    Gen2.NXP.AES.Tam1Authentication tam1Auth;
                    Gen2.NXP.AES.Tam2Authentication tam2Auth;

                    // Authenticate with TAM1 with key0

                    tam1Auth = new Gen2.NXP.AES.Tam1Authentication(Gen2.NXP.AES.KeyId.KEY0, Key0, Ichallenge, SendRawData);
                    Gen2.NXP.AES.Authenticate auth = new Gen2.NXP.AES.Authenticate(tam1Auth);
                    byte[] Response = (byte[])r.ExecuteTagOp(auth, null);
                    if (SendRawData)
                    {
                        byte[] Challenge = DecryptIchallenge(Response, ByteConv.ConvertFromUshortArray(Key0));
                        Array.Copy(Challenge, 6, Challenge, 0, 10);
                        Array.Resize(ref Challenge, 10);
                        Console.WriteLine("Returned Ichallenge:" + ByteFormat.ToHex(Challenge, "", " "));
                    }
                    else
                    {
                        Console.WriteLine("Data: " + ByteFormat.ToHex(Response, "", " "));
                    }

                    //Uncomment this to enable Authenticate with TAM1 with Key1

                    //tam1Auth = new Gen2.NXP.AES.Tam1Authentication(Gen2.NXP.AES.KeyId.KEY1, Key1, Ichallenge, SendRawData);
                    //Gen2.NXP.AES.Authenticate tam1AuthKey1 = new Gen2.NXP.AES.Authenticate(tam1Auth);
                    //Response = (byte[])r.ExecuteTagOp(tam1AuthKey1, null);
                    //if (SendRawData)
                    //{
                    //    byte[] Challenge = DecryptIchallenge(Response, ByteConv.ConvertFromUshortArray(Key1));
                    //    Array.Copy(Challenge, 6, Challenge, 0, 10);
                    //    Array.Resize(ref Challenge, 10);
                    //    Console.WriteLine("Returned Ichallenge:" + ByteFormat.ToHex(Challenge, "", " "));
                    //}
                    //else
                    //{
                    //    Console.WriteLine("Data: " + ByteFormat.ToHex(Response, "", " "));
                    //}



                    //Uncomment this to enable Authenticate with TAM2 with key1

                    //ushort Offset = 0;
                    //ushort BlockCount = 1;
                    //tam2Auth = new Gen2.NXP.AES.Tam2Authentication(Gen2.NXP.AES.KeyId.KEY1, Key1, Ichallenge, Gen2.NXP.AES.Profile.EPC,
                    //   Offset, BlockCount, SendRawData);
                    //Gen2.NXP.AES.Authenticate tam2AuthKey1 = new Gen2.NXP.AES.Authenticate(tam2Auth);
                    //Response = (byte[])r.ExecuteTagOp(tam2AuthKey1, null);
                    //if (SendRawData)
                    //{
                    //    byte[] IV = new byte[16];
                    //    byte[] CipherData = new byte[16];
                    //    Array.Copy(Response, 0, IV, 0, 16);
                    //    Array.Copy(Response, 16, CipherData, 0, 16);
                    //    Console.WriteLine("Custom Data: " + DecryptCustomData(CipherData, ByteConv.ConvertFromUshortArray(Key1), (byte[])IV.Clone()));
                    //    byte[] Challenge = DecryptIchallenge(IV, ByteConv.ConvertFromUshortArray(Key1));
                    //    Array.Copy(Challenge, 6, Challenge, 0, 10);
                    //    Array.Resize(ref Challenge, 10);
                    //    Console.WriteLine("Returned Ichallenge:" + ByteFormat.ToHex(Challenge, "", " "));
                    //}
                    //else
                    //{
                    //    Console.WriteLine("Data: " + ByteFormat.ToHex(Response, "", " "));
                    //}
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

        #region DecryptIchallenge

        public static byte[] DecryptIchallenge(byte[] CipherData, byte[] Key)
        {
            byte[] decipheredText = new byte[16];
            SwapBytes(ref CipherData);
            Aes a = new Aes(Aes.KeySize.Bits128, Key);
            a.InvCipher(CipherData, decipheredText);
            return decipheredText;
        }

        #endregion

        #region  DecryptCustomData

        public static string DecryptCustomData(byte[] CipherData, byte[] key, byte[] IV)
        {
            byte[] decipheredText = new byte[16];
            decipheredText = DecryptIchallenge(CipherData, key);
            SwapBytes(ref IV);
            byte[] CustomData = new byte[16];
            for (int i = 0; i < IV.Length; i++)
            {
                CustomData[i] = (byte)(decipheredText[i] ^ IV[i]);
            }
            return ByteFormat.ToHex(CustomData, "", " ");
        }

        #endregion

        #region SwapBytes

        public static void SwapBytes(ref byte[] buf)
        {
            for (int index = 0; index < buf.Length; index += 2)
            {
                byte temp = buf[index];
                buf[index] = buf[index + 1];
                buf[index + 1] = temp;
            }
        }

        #endregion
    }
}
