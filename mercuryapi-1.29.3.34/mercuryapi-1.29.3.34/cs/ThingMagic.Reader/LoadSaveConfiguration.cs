/*
 * Copyright (c) 2014 ThingMagic, Inc.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace ThingMagic
{
    /// <summary>
    /// LoadSaveConfiguration
    /// </summary>
    class LoadSaveConfiguration
    {
        public List<string> ReadOnlyPararameters = new List<string>();

        #region ParseValue
        /// <summary>
        /// Parse string representing a parameter value.
        /// </summary>
        /// <param name="name">Name of parameter</param>
        /// <param name="valstr">String to be parsed into a parameter value</param>
        private Object ParseValue(string name, string valstr)
        {
            Object value = ParseValue(valstr);
            switch (name.ToLower())
            {
                case "/reader/antenna/portswitchgpos":
                case "/reader/region/hoptable":
                    value = ((ArrayList)value).ToArray(typeof(int));
                    break;
                case "/reader/gpio/inputlist":
                case "/reader/read/trigger/gpi":
                case "/reader/probebaudrates":
                    value = ((ArrayList)value).ToArray(typeof(int));
                    break;
                case "/reader/gpio/outputlist":
                    value = ((ArrayList)value).ToArray(typeof(int));
                    break;
                case "/reader/antenna/settlingtimelist":
                case "/reader/antenna/txrxmap":
                case "/reader/radio/portreadpowerlist":
                case "/reader/radio/portwritepowerlist":
                    value = ArrayListToInt2Array((ArrayList)value);
                    break;
                case "/reader/region/lbt/enable":
                case "/reader/antenna/checkport":
                case "/reader/tagreaddata/recordhighestrssi":
                case "/reader/tagreaddata/uniquebyantenna":
                case "/reader/tagreaddata/uniquebydata":
                case "/reader/tagreaddata/reportrssiindbm":
                case "/reader/radio/enablepowersave":
                case "/reader/status/antennaenable":
                case "/reader/status/frequencyenable":
                case "/reader/status/temperatureenable":
                case "/reader/tagreaddata/reportrssiIndbm":
                case "/reader/tagreaddata/uniquebyprotocol":
                case "/reader/tagreaddata/enablereadfilter":
                case "/reader/radio/enablesjc":
                case "/reader/gen2/writeearlyexit":
                case "/reader/extendedepc":
                    value = ParseBool(valstr);
                    break;
                case "/reader/read/plan":
                    if (valstr.StartsWith("SimpleReadPlan"))
                    {
                        value = LoadSimpleReadPlan(valstr);
                    }
                    else
                    {
                        string str = string.Empty;
                        List<ReadPlan> RdPlans = new List<ReadPlan>();
                        str = valstr.Remove(0, 14);
                        //Remove leading and trailing square brackets
                        string remove = Regex.Replace(str, @"]$|^\[", "");
                        int CurrentIndex = 0,PreviousIndex = 1;
                        List<string> Plans = new List<string>();
                        while (CurrentIndex != -1)
                        {
                            CurrentIndex = str.IndexOf("SimpleReadPlan:", CurrentIndex);
                            string st = string.Empty;
                            if (CurrentIndex != -1)
                            {
                                st = str.Substring(PreviousIndex, CurrentIndex - PreviousIndex);
                                PreviousIndex = CurrentIndex;
                                CurrentIndex += 1;
                            }
                            else
                            {
                                st = str.Substring(PreviousIndex);
                            }
                            if (st != string.Empty)
                            {
                                st = st.Remove(st.Length - 1,1);
                                Plans.Add(st);
                            }
                        }
                        foreach (string plan in Plans)
                        {
                            RdPlans.Add(LoadSimpleReadPlan(plan));
                        }
                        MultiReadPlan mrp = new MultiReadPlan(RdPlans);
                        value = mrp;
                    }
                    break;
                case "/reader/region/id":
                    value = Enum.Parse(typeof(Reader.Region), (string)value, true);
                    break;
                case "/reader/powermode":
                    value = Enum.Parse(typeof(Reader.PowerMode), (string)value, true);
                    break;
                case "/reader/tagop/protocol":
                    if (value is string)
                    {
                        value = Enum.Parse(typeof(TagProtocol), (string)value, true);
                    }
                    break;
                case "/reader/gen2/accesspassword":
                    value = new Gen2.Password(Convert.ToUInt32(valstr,16));
                    break;
                case "/reader/gen2/session":
                   value = (Gen2.Session)Enum.Parse(typeof(Gen2.Session), valstr, true);
                    break;
                case "/reader/gen2/blf":
                    value = (Gen2.LinkFrequency)Enum.Parse(typeof(Gen2.LinkFrequency), valstr, true);
                    break;
                case "/reader/gen2/tagencoding":
                    value = (Gen2.TagEncoding)Enum.Parse(typeof(Gen2.TagEncoding), valstr, true);
                    break;
                case "/reader/iso180006b/blf":
                      value = (Iso180006b.LinkFrequency)Enum.Parse(typeof(Iso180006b.LinkFrequency), valstr, true);
                    break;
                case "/reader/gen2/target":
                   value = (Gen2.Target)Enum.Parse(typeof(Gen2.Target), valstr, true);
                   break;
                case "/reader/gen2/tari":
                   value = (Gen2.Tari)Enum.Parse(typeof(Gen2.Tari), valstr, true);
                   break;
               case "/reader/gen2/protocolextension":
                   value = (Gen2.Tari)Enum.Parse(typeof(Gen2.ProtocolExtension), valstr, true);
                   break;
                case "/reader/usermode":
                     value = (SerialReader.UserMode)Enum.Parse(typeof(SerialReader.UserMode), (string)valstr, true);
                break;
                case "/reader/stats/enable":
                    valstr = valstr.Trim(new char[] {'[', ']'});
                    value = valstr != string.Empty
                        ? (Reader.Stat.StatsFlag) Enum.Parse(typeof (Reader.Stat.StatsFlag), valstr, true)
                        : (Reader.Stat.StatsFlag) Enum.Parse(typeof (Reader.Stat.StatsFlag), "NONE", true);
                    break;
                case "/reader/gen2/writemode":
                    value = (Gen2.WriteMode)Enum.Parse(typeof(Gen2.WriteMode), valstr, true);
                    break;
                case "/reader/iso180006b/delimiter":
                    value = (Iso180006b.Delimiter)Enum.Parse(typeof(Iso180006b.Delimiter), valstr, true);
                    break;
                case "/reader/iso180006b/modulationdepth":
                    value = (Iso180006b.ModulationDepth)Enum.Parse(typeof(Iso180006b.ModulationDepth), valstr, true);
                    break;
                case "/reader/gen2/q":
                    Gen2.Q setQ=null;
                    if (-1 != valstr.IndexOf("DynamicQ"))
                    {
                        setQ = new Gen2.DynamicQ();
                    }
                    else
                    {
                       string resultString = Regex.Match(valstr, @"\d+").Value;
                        int q=Int32.Parse(resultString);
                        setQ = new Gen2.StaticQ((byte)q);
                    }
                    value = setQ;
                    break;
                case "/reader/gen2/bap":
                    MatchCollection mc = Regex.Matches(valstr, @"\d+");
                    Gen2.BAPParameters bap = new Gen2.BAPParameters();
                    bap.POWERUPDELAY = Convert.ToInt32(mc[0].ToString());
                    bap.FREQUENCYHOPOFFTIME = Convert.ToInt32(mc[1].ToString());
                    value = bap;
                    break;
                case "/reader/metadata":
                    string[] usermeta = valstr.Split(',');
                    SerialReader.TagMetadataFlag val = 0x0000;
                    foreach (string meta in usermeta)
                    {
                        switch (meta.Trim())
                        {
                            case "NONE":
                                val |= SerialReader.TagMetadataFlag.NONE;
                                break;
                            case "ANTENNAID":
                                val  |= SerialReader.TagMetadataFlag.ANTENNAID;
                                break;
                            case "DATA":
                                val |= SerialReader.TagMetadataFlag.DATA;
                                break;
                            case "FREQUENCY":
                                val |= SerialReader.TagMetadataFlag.FREQUENCY;
                                break;
                            case "GPIO":
                                val |= SerialReader.TagMetadataFlag.GPIO;
                                break;
                            case "PHASE":
                                val |= SerialReader.TagMetadataFlag.PHASE;
                                break;
                            case "PROTOCOL":
                                val |= SerialReader.TagMetadataFlag.PROTOCOL;
                                break;
                            case "READCOUNT":
                                val |= SerialReader.TagMetadataFlag.READCOUNT;
                                break;
                            case "RSSI":
                                val |= SerialReader.TagMetadataFlag.RSSI;
                                break;
                            case "TIMESTAMP":
                                val |= SerialReader.TagMetadataFlag.TIMESTAMP;
                                break;
                            default:
                            case "ALL":
                                val |= SerialReader.TagMetadataFlag.ALL;
                                break;
                        }
                    }
                    value = val;
                    break;
                default:
                    break;
            }
            return value;
        }

        public ReadPlan LoadSimpleReadPlan(string valstr)
        {
            Object value = ParseValue(valstr);
            string str = string.Empty;
            //Reamoves leading string for ex: SimpleReadPlan:
            str = valstr.Remove(0, 15);
            SimpleReadPlan srp = new SimpleReadPlan();
            //Regular expression to remove leading and trailing square brackets
            string remove = Regex.Replace(str, @"]$|^\[", "");
            //Regular expression to split the string
            string[] lines = Regex.Split(remove, @",(?![^\[\]]*\])");
            TagFilter tf = null;
            TagOp op = null;
            foreach (string line in lines)
            {
                if (-1 != line.IndexOf("Antennas"))
                {
                    ArrayList list = new ArrayList();
                    int[] antList = null;
                    object value1 = ParseValue(line.Split('=')[1]);
                    if (value1 != null)
                    {
                        antList = (int[])((ArrayList)value1).ToArray(typeof(int));
                    }
                    srp.Antennas = antList;
                }
                else if (-1 != line.IndexOf("Protocol"))
                {
                    srp.Protocol = (TagProtocol)Enum.Parse(typeof(TagProtocol), line.Split('=')[1], true);
                }
                else if (-1 != line.IndexOf("Filter"))
                {
                    string filterData = line.Split('=')[1];
                    if (-1 != filterData.IndexOf("Gen2.Select"))
                    {
                        str = line.Remove(0, 19);
                        //Regular expression to remove leading and trailing square brackets
                        str = Regex.Replace(str, @"]$|^\[", "");
                        //Regular expression to split the string 
                        string[] select = Regex.Split(str, @"[ ,;]+");
                        bool Invert = false;
                        Gen2.Bank bank = Gen2.Bank.EPC;
                        uint BitPointer = 0;
                        ushort BitLength = 0;
                        byte[] mask = null;
                        if (select.Length != 5)
                        {
                            throw new Exception("Invalid number of arguments for ReadPlan filter");
                        }
                        foreach (string  arg in select)
                        {
                            if(-1!= arg.IndexOf("Invert"))
                            {
                                Invert = Convert.ToBoolean(arg.Split('=')[1]);
                            }
                            else if(-1!= arg.IndexOf("Bank"))
                            {
                                bank = (Gen2.Bank)Enum.Parse(typeof(Gen2.Bank), arg.Split('=')[1], true);
                            }
                            else if(-1!= arg.IndexOf("BitPointer"))
                            {
                                BitPointer = Convert.ToUInt32(arg.Split('=')[1]);
                            }
                            else if(-1!= arg.IndexOf("BitLength"))
                            {
                                BitLength = Convert.ToUInt16(arg.Split('=')[1]);
                            }
                            else if(-1!= arg.IndexOf("Mask"))
                            {
                                mask = StringToByteArray(arg.Split('=')[1]);
                            }
                            else
                            {
                                throw new Exception("Invalid Argument in ReadPlan");
                            }
                        }
                        tf = new Gen2.Select(Invert,bank,BitPointer,BitLength,mask);
                    }
                    else if (-1 != filterData.IndexOf("EPC"))
                    {
                        str = line.Remove(0, 15);
                        str = Regex.Replace(str, @"]$|^\[", "");
                        tf = new TagData(StringToByteArray((str.Split('=')[1])));
                    }
                    else
                    {
                        if(!filterData.Equals("null"))
                        throw new Exception("Invalid Argument in ReadPlan");
                    }
                }
                else if (-1 != line.IndexOf("Op"))
                {
                    string tagOpData = line.Split('=')[1];
                    if (tagOpData != null)
                    {
                        if (-1 != tagOpData.IndexOf("ReadData"))
                        {
                            str = line.Remove(0, 12);
                            //Regular expression to remove leading and trailing square brackets
                            str = Regex.Replace(str, @"]$|^\[", "");
                            //Regular expression to split the string
                            string[] select = Regex.Split(str, @"[ ,;]+");
                            Gen2.Bank bank = Gen2.Bank.EPC;
                            uint wordAddress = 0;
                            byte length = 0;
                            foreach (string arg in select)
                            {
                                if (-1 != arg.IndexOf("Bank"))
                                {
                                    bank = (Gen2.Bank)Enum.Parse(typeof(Gen2.Bank), arg.Split('=')[1], true);
                                }
                                else if (-1 != arg.IndexOf("WordAddress"))
                                {
                                   wordAddress = Convert.ToUInt32(arg.Split('=')[1]);
                                }
                                else if (-1 != arg.IndexOf("Len"))
                                {
                                   length = Convert.ToByte(arg.Split('=')[1]);
                                }
                                else
                                {
                                    throw new Exception("Invalid Argument in ReadPlan TagOp");
                                }
                            }
                            op = new Gen2.ReadData(bank,wordAddress,length);
                        }
                        else
                        {
                            if(!tagOpData.Equals("null"))
                            throw new Exception("Invalid Argument in ReadPlan");
                        }
                    }
                }
                else if (-1 != line.IndexOf("UseFastSearch"))
                {
                    srp.UseFastSearch= Convert.ToBoolean(line.Split('=')[1]);
                }
                else if (-1 != line.IndexOf("Weight"))
                {
                    srp.Weight = Convert.ToInt32(lines[5].Split('=')[1]);
                }
                else
                {
                    throw new Exception("Invalid Argument in ReadPlan");
                }
            }
            srp.Filter = tf;
            srp.Op = op;
            return srp;
        }

        /// <summary>
        /// Convert string to byte array.
        /// </summary>
        /// <param name="hex">Hex string</param>
        /// <returns>Byte array</returns>
        public static byte[] StringToByteArray(String hex)
        {
            hex = hex.Remove(0, 2);
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
        private static Object ParseValue(string str)
        {
            // null
            if ("null" == str.ToLower())
            {
                return null;
            }
            // Array
            if (str.StartsWith("[") && str.EndsWith("]"))
            {
                try
                {
                    ArrayList list = new ArrayList();
                    // Array of arrays
                    if ('[' == str[1])
                    {
                        int open = 1;
                        while (-1 != open)
                        {
                            int close = str.IndexOf(']', open);
                            list.Add(ParseValue(str.Substring(open, (close - open + 1))));
                            open = str.IndexOf('[', close + 1);
                        }
                    }
                    // Array of scalars
                    else
                    {
                        foreach (string eltstr in str.Substring(1, str.Length - 2).Split(new char[] { ',' }))
                        {
                            if (0 < eltstr.Length) { list.Add(ParseValue(eltstr)); }
                        }
                    }
                    return list;
                }
                catch (Exception) { }
            }
            // Integer (Decimal)
            try { return int.Parse(str); }
            catch (Exception) { }
            return str;
        }

        /// <summary>
        /// Convert ArrayList to array for ArrayList containing ArrayLists of ints.
        /// </summary>
        /// <param name="list">ArrayList</param>
        /// <returns>Array version of input list</returns>
        private static int[][] ArrayListToInt2Array(ArrayList list)
        {
            List<int[]> buildlist = new List<int[]>();
            foreach (ArrayList innerlist in list)
            {
                buildlist.Add((int[])innerlist.ToArray(typeof(int)));
            }
            return buildlist.ToArray();
        }
        /// <summary>
        /// Parse string to boolean value
        /// </summary>
        /// <param name="boolString">String representing a boolean value</param>
        /// <returns>False if input is "False", "Low", or "0".  True if input is "True", "High", or "1".  Case-insensitive.</returns>
        private static bool ParseBool(string boolString)
        {
            switch (boolString.ToLower())
            {
                case "true":
                case "high":
                case "1":
                    return true;
                case "false":
                case "low":
                case "0":
                    return false;
                default:
                    throw new FormatException();
            }
        }
        #endregion

        #region LoadConfiguration
        /// <summary>
        /// Load the configuration data to module
        /// </summary>
        /// <param name="filePath">Configuration file path</param>
        /// <param name="r">Reader instance</param>
        /// <param name="isRollBack">if flag is low LoadConfiguration method is not called from RollbackConfigData()</param>
        public void LoadConfiguration(String filePath, Reader r, bool isRollBack)
        {
            string tempFilePath = string.Empty;
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException("Unable to find the configuration properties file in" + filePath);
                }
                Dictionary<string, string> loadConfigProperties = new Dictionary<string, string>();
                loadConfigProperties = GetProperties(filePath);
                tempFilePath = Path.Combine(Path.GetDirectoryName(filePath), "DeviceConfig." + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                r.SaveConfig(tempFilePath);
                foreach (KeyValuePair<string, string> item in loadConfigProperties)
                {
                    if (!item.Key.StartsWith("/reader"))
                    {
                        r.notifyExceptionListeners(new ReaderException("\"" + item.Key + "\" is not a valid parameter "));
                        continue;
                    }
                    try
                    {
                        r.ParamSet(item.Key, ParseValue(item.Key, item.Value));
                    }
                    catch (Exception ex)
                    {
                        if (ex is ArgumentException)
                        {
                            if ((-1 != ex.Message.IndexOf("Parameter not found: \"" + item.Key + "\"")) ||
                                (-1 != ex.Message.IndexOf("Parameter \"" + item.Key + "\" is read-only.")) ||
                                (-1 != ex.Message.IndexOf("Parameter is read only."))
                                )
                            {
                                r.notifyExceptionListeners(new ReaderException("\"" + item.Key + "\" is either read only or not supported by reader. Skipping this param"));
                            }
                            else if(-1 != ex.Message.IndexOf("Illegal set of GPI for trigger read"))
                            {
                                r.notifyExceptionListeners(new ReaderException("Invalid value " + item.Value + " for " + item.Key + " " + ex.Message+". Skipping this param"));
                            }
                            else
                            {
                                if (!isRollBack)
                                {
                                    r.notifyExceptionListeners(new ReaderException("Invalid value " + item.Value +
                                            " for " + item.Key + " " + ex.Message));
                                    RollbackConfigData(r, tempFilePath);
                                    break;
                                }
                                r.notifyExceptionListeners(new ReaderException("Invalid value " + item.Value +
                                    " for " + item.Key + " " + ex.Message));
                            }
                        }
                        else if (ex is FeatureNotSupportedException || ex is FAULT_UNIMPLEMENTED_FEATURE_Exception)
                        {
                            if (item.Key.Equals("/reader/tagReadData/reportRssiInDbm"))
                                r.notifyExceptionListeners(new ReaderException(ex.Message + ". Skipping this param"));
                            else
                            r.notifyExceptionListeners(new ReaderException(item.Key + " is " +ex.Message+". Skipping this param"));
                        }
                        else if (ex is ReaderCommException)
                        {
                            if (-1 != ex.Message.IndexOf("The operation has timed out."))
                            {
                                throw ex;
                            }
                        }
                        else
                        {
                            if (!isRollBack)
                            {
                                r.notifyExceptionListeners(new ReaderException("Invalid value " + item.Value +
                                    " for " + item.Key + " " + ex.Message));
                                RollbackConfigData(r, tempFilePath);
                                break;
                            }
                            r.notifyExceptionListeners(new ReaderException("Invalid value " + item.Value + 
                                " for " + item.Key + " " + ex.Message));
                        }
                    }
                }
                File.Delete(tempFilePath);
            }
            catch (Exception ex)
            {
                if(tempFilePath!=string.Empty)
                File.Delete(tempFilePath);
                throw new ReaderException(ex.Message);
            }
        }
        #endregion

        #region RollbackConfigData
        /// <summary>
        /// Roll back configuration data to module upon getting exception
        /// </summary>
        /// <param name="r">Reader instance</param>
        /// <param name="tempFilePath">Configuration file path</param>
        public void RollbackConfigData(Reader r, string tempFilePath)
        {
            r.notifyExceptionListeners(new ReaderException("Rolling back the configuration data"));
            LoadConfiguration(tempFilePath, r, true);
            File.Delete(tempFilePath);
            return;
        }
        #endregion

        #region GetProperties
        /// <summary>
        /// Read the properties from TestConfiguration.properties file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetProperties(string path)
        {
            Dictionary<string, string> Properties = new Dictionary<string, string>();
            using (StreamReader sr = File.OpenText(path))
            {
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    if ((!string.IsNullOrEmpty(s)) &&
                        (!s.StartsWith(";")) &&
                        (!s.StartsWith("#")) &&
                        (!s.StartsWith("'")) &&
                        (!s.StartsWith("//")) &&
                        (!s.StartsWith("*")) &&
                        (s.IndexOf('=') != -1))
                    {
                        int index = s.IndexOf('=');
                        string keyConfig = s.Substring(0, index).Trim();
                        string valueConfig = s.Substring(index + 1).Trim();

                        if ((valueConfig.StartsWith("\"") && valueConfig.EndsWith("\"")) ||
                            (valueConfig.StartsWith("'") && valueConfig.EndsWith("'")))
                        {
                            valueConfig = valueConfig.Substring(1, valueConfig.Length - 2);
                        }

                        Properties.Add(keyConfig, valueConfig);
                    }
                }
            }
            return Properties;
        }
        #endregion

        #region SaveConfiguration
        /// <summary>
        /// Save the configuration data to file
        /// </summary>
        /// <param name="filePath">Configuration file path</param>
        /// <param name="r">Reader instance</param>
        public void SaveConfiguration(String filePath, Reader r)
        {
            Dictionary<string, string> saveConfigurationList = new Dictionary<string, string>();
            saveConfigurationList = GetParametersToSave(r);
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.AutoFlush = true;
                foreach (KeyValuePair<string, string> item in saveConfigurationList)
                {
                    writer.WriteLine(item.Key + "=" + item.Value);
                }
                writer.Close();
            }
        }
        #endregion

        #region GetParametersToSave
        /// <summary>
        /// Get the parameters to be saved in the configuration file
        /// </summary>
        /// <returns>List of parameters to be saved int the configuration file</returns>
        private Dictionary<string, string> GetParametersToSave(Reader r)
        {
            Dictionary<string, string> saveConfigurationList = new Dictionary<string, string>();
            string[] names;
            names = r.ParamList();
            
            foreach (string name in names)
            {
                if(!ReadOnlyPararameters.Contains(name))
                AddParameterToList(name, r, ref saveConfigurationList);
            }
            return saveConfigurationList;
        }
        #endregion

        #region AddParameterToList
        private void AddParameterToList(string name, Reader rdr, ref Dictionary<string, string> saveConfigurationList)
        {
            try
            {
                Object value = rdr.ParamGet(name);
                string repr;
                switch (name)
                {
                    case "/reader/read/plan":
                        repr = SaveReadPlan(value);
                        break;
                    case "/reader/stats/enable":
                        repr = FormatValue(value);
                        repr = "[" + repr + "]";
                        break;
                    default:
                        repr = FormatValue(value);
                        break;
                }
                saveConfigurationList.Add(name, repr);
            }
            catch (Exception ex)
            {
                if (ex is ReaderCommException)
                {
                    if (-1 != ex.Message.IndexOf("The operation has timed out."))
                    {
                        throw ex;
                    }
                }
                else
                {
                    //Skip adding parameter to configuration file.
                }
            }
        }
        #endregion

        #region SaveReadPlan
        public static string SaveReadPlan(Object value)
        {
            string readPlan = string.Empty;
            ReadPlan rp = (ReadPlan)value;
            if (rp is SimpleReadPlan)
            {
                return SaveSimpleReadPlan(value);
            }
            else
            {
                MultiReadPlan mrp = (MultiReadPlan)rp;
                List<ReadPlan> MRP = new List<ReadPlan>(mrp.Plans);
                readPlan += "MultiReadPlan:[";
                foreach (ReadPlan rap in MRP)
                {
                    readPlan += SaveSimpleReadPlan(rap) + ",";
                }
                readPlan = readPlan.Remove(readPlan.Length - 1, 1);
                readPlan += "]";
                return readPlan;
            }
        }
        #endregion

        #region SaveSimpleReadPlan
        public static string SaveSimpleReadPlan(Object value)
        {
            string readPlan = string.Empty;
            ReadPlan rp = (ReadPlan)value;
            readPlan += "SimpleReadPlan:[";
            SimpleReadPlan srp = (SimpleReadPlan)rp;
            readPlan += "Antennas="+ArrayToString(srp.Antennas);
            readPlan += "," + "Protocol=" + srp.Protocol.ToString();
            if (srp.Filter != null)
            {
                if(srp.Filter is Gen2.Select)
                {
                    Gen2.Select sf =(Gen2.Select)srp.Filter;
                    readPlan+= ","+string.Format("Filter=Gen2.Select:[Invert={0},Bank={1},BitPointer={2},BitLength={3},Mask={4}]",
                        (sf.Invert?"true" : "false"),sf.Bank,sf.BitPointer,sf.BitLength,ByteFormat.ToHex(sf.Mask));
                }
                else
                {
                    Gen2.TagData td = (Gen2.TagData)srp.Filter;
                    readPlan += "," + string.Format("Filter=TagData:[EPC={0}]", td.EpcString);
                }
            }
            else
            {
                readPlan += ",Filter=null";
            }
            if (srp.Op != null)
            {
                if (srp.Op is Gen2.ReadData)
                {
                    Gen2.ReadData rd = (Gen2.ReadData)srp.Op;
                    readPlan += "," + string.Format("Op=ReadData:[Bank={0},WordAddress={1},Len={2}]", rd.Bank, rd.WordAddress, rd.Len);
                }
                else
                {
                    readPlan += ",Op=null";
                }
            }
            else
            {
                readPlan += ",Op=null";
            }
            readPlan += ","+ "UseFastSearch=" + srp.UseFastSearch.ToString();
            readPlan += ","+ "Weight=" + srp.Weight.ToString() + "]";
            return readPlan;
        }
        #endregion

        #region ArrayToString
        private static string ArrayToString(Array array)
        {
            if (null == array)
                return "null";

            List<string> words = new List<string>();

            foreach (Object elt in array)
                words.Add(elt.ToString());

            return String.Format("[{0}]", String.Join(",", words.ToArray()));
        }
        #endregion

        #region FormatValue
        private static string FormatValue(Object val)
        {
            if (null == val)
            {
                return "null";
            }
            else if (val.GetType().IsArray)
            {
                Array arr = (Array)val;
                string[] valstrings = new string[arr.Length];
                for (int i = 0; i < arr.Length; i++)
                {
                    valstrings[i] = FormatValue(arr.GetValue(i));
                }

                StringBuilder sb = new StringBuilder();
                sb.Append("[");
                sb.Append(String.Join(",", valstrings));
                sb.Append("]");
                return sb.ToString();
            }
            else if (val is byte)
            {
                return ((byte)val).ToString("D");
            }
            else if ((val is ushort) || (val is UInt16))
            {
                return ((ushort)val).ToString("D");
            }
            else
            {
                return val.ToString();
            }
        }
        #endregion
    }
}
