using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ThingMagic;
using ThingMagic.URA2.BL;
using AEITagDecoder;

namespace ThingMagic.URA2
{
    /// <summary>
    /// Interaction logic for ucTagInspectorxaml.xaml
    /// </summary>
    public partial class ucTagInspector : UserControl
    {
        Reader objReader;
        uint startAddress = 0;
        int antenna = 0;
        Gen2.Bank selectMemBank;
        TagReadRecord tempSelectedTag;

        // Constant XPC_1 offset 
        const int XPC1OFFSET = 42;

        // Constant XPC_2 offset
        const int XPC2OFFSET = 44;

        string model = string.Empty;

        public ucTagInspector()
        {
            InitializeComponent();
        }

        public void LoadTagInspector(Reader reader, string readerModel)
        {
            objReader = reader;
            model = readerModel;
            if (btnRead.Content.ToString() == "Read")
                rbtngen2.Visibility = rbtnata.Visibility = Visibility.Visible;

            InitializeRadioButton();
            if (rbtnata.IsChecked == false && rbtngen2.IsChecked == false)
            {
                rbtngen2.IsChecked = true;
            }
        }

        public void LoadTagInspector(Reader reader, uint address, Gen2.Bank selectedBank, TagReadRecord selectedTagRed, string readerModel)
        {
            try
            {
                InitializeRadioButton();
                objReader = reader;
                startAddress = address;
                model = readerModel;
                rbFirstTagIns.IsEnabled = true;
                rbSelectedTagIns.IsChecked = true;
                rbSelectedTagIns.IsEnabled = true;
                rbEPCAscii.IsEnabled = true;
                rbEPCBase36.IsEnabled = true;
                btnRead.Content = "Refresh";
                antenna = selectedTagRed.Antenna;
                tempSelectedTag = selectedTagRed;
                selectMemBank = selectedBank;
                //txtEPCData.Text = selectedTagRed.EPC;
                string[] stringData = selectedTagRed.Data.Split(' ');
                txtEpc.Text = selectedTagRed.EPC;
                currentEPC = txtEpc.Text;
                txtData.Text = string.Join("", stringData);
                Window mainWindow = App.Current.MainWindow;
                ucTagResults tagResults = (ucTagResults)mainWindow.FindName("TagResults");
                if (selectedTagRed.Protocol == TagProtocol.GEN2)
                {
                    rbtngen2.IsChecked = true;
                    rbtnata.Visibility = Visibility.Collapsed;
                    switch (selectedBank)
                    {
                        case Gen2.Bank.EPC:
                            if (tagResults.txtSelectedCell.Text == "Data")
                            {
                                lblSelectFilter.Content = "Showing tag: EPC data at decimal address " + address.ToString() + "  = " + txtData.Text;
                            }
                            else
                            {
                                lblSelectFilter.Content = "Showing tag: EPC ID = " + selectedTagRed.EPC;
                            }
                            break;
                        case Gen2.Bank.TID:
                            if (tagResults.txtSelectedCell.Text == "Data")
                            {
                                lblSelectFilter.Content = "Showing tag: TID data at decimal address " + address.ToString() + " = " + txtData.Text;
                            }
                            else
                            {
                                lblSelectFilter.Content = "Showing tag: EPC ID = " + selectedTagRed.EPC;
                            }
                            break;
                        case Gen2.Bank.USER:
                            if (tagResults.txtSelectedCell.Text == "Data")
                            {
                                lblSelectFilter.Content = "Showing tag: User data at decimal address " + address.ToString() + " = " + txtData.Text;
                            }
                            else
                            {
                                lblSelectFilter.Content = "Showing tag: EPC ID = " + selectedTagRed.EPC;
                            }
                            break;
                    }
                    PopulateData();
                }
                else if (selectedTagRed.Protocol == TagProtocol.ATA)
                {
                    rbtnata.IsChecked = true;
                    rbtngen2.Visibility = Visibility.Collapsed;
                    DecodeATATag(selectedTagRed.EPC);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error : Universal Reader Assistant", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeRadioButton()
        {
            CheckBox cbxgen2 = (CheckBox)App.Current.MainWindow.FindName("gen2CheckBox");
            CheckBox cbxata = (CheckBox)App.Current.MainWindow.FindName("ataCheckBox");
            if (cbxata.Visibility != Visibility.Visible)
            {
                rbtnata.Visibility = Visibility.Collapsed;
                lblNote.Content = "Note : Tag Inspector Operation supports only GEN2 Tags. Rest of the protocols will be ignored";
                stkSelectProtocol.Visibility = Visibility.Collapsed;
            }
            //if (cbxgen2.IsChecked == true && (cbxata.IsChecked == true && cbxata.Visibility == Visibility.Visible))
            //{
            //    rbtngen2.IsChecked = true;
            //}
            //else
            //{
            //    if (cbxgen2.IsChecked == true)
            //        rbtngen2.IsChecked = true;
            //    else if (cbxata.IsChecked == true && cbxata.Visibility == Visibility.Visible)
            //        rbtnata.IsChecked = true;
            //    else
            //        rbtngen2.IsChecked = true;
            //}
        }

        string currentEPC = string.Empty;

        private void btnRead_Click(object sender, RoutedEventArgs e)
        {
            Mouse.SetCursor(Cursors.Wait);
            Window mainWindow = App.Current.MainWindow;
            lbltagnotfound.Visibility = Visibility.Collapsed;
            ComboBox CheckRegionCombobx = (ComboBox)mainWindow.FindName("regioncombo");
            if (CheckRegionCombobx.SelectedValue.ToString() == "Select")
            {
                MessageBox.Show("Please select region", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            TagReadData[] tagReads = null;
            try
            {
                rbEPCAscii.IsEnabled = true;
                rbEPCBase36.IsEnabled = true;
                if (btnRead.Content.Equals("Read"))
                {
                    if ((bool)rbtngen2.IsChecked)
                    {
                        SimpleReadPlan srp = new SimpleReadPlan(((null != GetSelectedAntennaList()) ? (new int[] { GetSelectedAntennaList()[0] }) : null), TagProtocol.GEN2, null, 0);
                        objReader.ParamSet("/reader/read/plan", srp);
                        tagReads = objReader.Read(500);
                        if ((null != tagReads) && (tagReads.Length > 0))
                        {
                            currentEPC = tagReads[0].EpcString;
                            if ((bool)rbEPCAscii.IsChecked)
                            {
                                txtEpc.Text = Utilities.HexStringToAsciiString(tagReads[0].EpcString);
                            }
                            else if ((bool)rbEPCBase36.IsChecked)
                            {
                                txtEpc.Text = Utilities.ConvertHexToBase36(tagReads[0].EpcString);
                            }
                            else
                            {
                                txtEpc.Text = tagReads[0].EpcString;
                            }
                            lblSelectFilter.Content = "Showing tag: EPC ID = " + tagReads[0].EpcString;
                            if (tagReads.Length > 1)
                            {
                                lblTagInspectorError.Content = "Warning: More than one tag responded";
                                lblTagInspectorError.Visibility = System.Windows.Visibility.Visible;
                            }
                            else
                            {
                                lblTagInspectorError.Visibility = System.Windows.Visibility.Collapsed;
                            }
                        }
                        else
                        {
                            txtEpc.Text = "";
                            currentEPC = string.Empty;
                            MessageBox.Show("No tags found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    else if ((bool)rbtnata.IsChecked)
                    {
                        SimpleReadPlan srp = new SimpleReadPlan(((null != GetSelectedAntennaList()) ? (new int[] { GetSelectedAntennaList()[0] }) : null), TagProtocol.ATA, null, 0);
                        objReader.ParamSet("/reader/read/plan", srp);
                        tagReads = objReader.Read(500);
                        if ((null != tagReads) && (tagReads.Length > 0))
                        {
                            DecodeATATag(tagReads[0].EpcString);
                        }
                        else
                        {
                            txtEpc.Text = "";
                            currentEPC = string.Empty;
                            MessageBox.Show("No tags found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please select a protocol.", "Universal Reader Assistant", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else
                {
                    rbSelectedTagIns.IsChecked = true;
                }
                if (!(bool)rbtnata.IsChecked)
                    PopulateData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.SetCursor(Cursors.Arrow);
            }
        }


        private void DecodeATATag(string epcdata)
        {
            try
            {
                lblSelectFilter.Content = "Showing tag: EPC ID = " + epcdata;
                AeiTag decodedTag = new AeiTag(epcdata);
                txtATAEPCData.Text = epcdata;
                ResetATATextControl();
                if ((AeiTag.DataFormat)decodedTag.getDataFormat == (AeiTag.DataFormat.SIX_BIT_ASCII) && !decodedTag.IsHalfFrameTag)
                {
                    tblASCIIBinaryFormat.Visibility = lblASCIIBinaryFormat.Visibility = tblASCIIFormat.Visibility = lblASCIIFormat.Visibility = Visibility.Visible;
                    lblEquipmentGroup.Visibility = txtEquipmentGroup.Visibility = lblEquipmentInitial.Visibility = txtEquipmentInitial.Visibility = txtTagType.Visibility = lblTagType.Visibility = Visibility.Collapsed;
                    txtDataFormat.Foreground = Brushes.Gray;
                    txtDataFormat.Text = ((AeiTag.DataFormat)decodedTag.getDataFormat).ToString();
                    string binstrvalue = BinaryStringToHexString(epcdata);
                    StringBuilder asciiValue = new StringBuilder();
                    tblASCIIBinaryFormat.Text = binstrvalue;
                    List<int> finalList = AeiTag.FromString(binstrvalue);
                    foreach (int temp in finalList)
                        asciiValue.Append(AeiTag.convertDecToSixBitAscii(temp).ToString());
                    tblASCIIFormat.Text = asciiValue.ToString();
                }
                else
                {
                    tblASCIIBinaryFormat.Visibility = lblASCIIBinaryFormat.Visibility = tblASCIIFormat.Visibility = lblASCIIFormat.Visibility = Visibility.Collapsed;
                    lblEquipmentGroup.Visibility = txtEquipmentGroup.Visibility = lblEquipmentInitial.Visibility = txtEquipmentInitial.Visibility = txtTagType.Visibility = lblTagType.Visibility = Visibility.Visible;
                    if (decodedTag.IsFieldValid[AeiTag.TagField.EQUIPMENT_GROUP])
                    {

                        txtEquipmentGroup.Foreground = txtCarNumber.Foreground = txtSideIndicator.Foreground = txtLength.Foreground = txtNumberofAxles.Foreground = txtBearingType.Foreground = txtPlatformID.Foreground = Brushes.Gray;
                        txtEquipmentGroup.Text = ((AeiTag.EquipmentGroup)decodedTag.getEquipmentGroup).ToString();

                        if (((AeiTag.EquipmentGroup)decodedTag.getEquipmentGroup) == AeiTag.EquipmentGroup.RAILCAR)
                        {
                            gridRailcarLocomotiveMultiModalPassiveAlarm.Visibility = Visibility.Visible;
                            gridTrailer.Visibility = gridRailcarCover.Visibility = gridIntermodal.Visibility = gridChassis.Visibility = gridEndofTrain.Visibility = gridGeneratorSet.Visibility = gridErrorMessage.Visibility = Visibility.Collapsed;
                            lblPlatformID.Visibility = txtPlatformID.Visibility = Visibility.Visible;
                            lblAlarm.Visibility = txtAlarm.Visibility = Visibility.Collapsed;
                            lblTypeDetails.Visibility = txtTypeDetails.Visibility = Visibility.Collapsed;

                            if (decodedTag.IsHalfFrameTag)
                                txtNumberofAxles.Visibility = lblNumberofAxles.Visibility = txtBearingType.Visibility = lblBearingType.Visibility = txtPlatformID.Visibility = lblPlatformID.Visibility = lblSpare.Visibility = txtSpare.Visibility = Visibility.Collapsed;
                            else
                                txtNumberofAxles.Visibility = lblNumberofAxles.Visibility = txtBearingType.Visibility = lblBearingType.Visibility = txtPlatformID.Visibility = lblPlatformID.Visibility = lblSpare.Visibility = txtSpare.Visibility = Visibility.Visible;

                            txtCarNumber.Text = decodedTag.getCarNumber.ToString();
                            txtSideIndicator.Text = ((AeiTag.SideIndicator)decodedTag.getSide).ToString();
                            txtLength.Text = decodedTag.getLengthInDecimeters.ToString();
                            txtNumberofAxles.Text = decodedTag.getNumberOfAxles.ToString();
                            txtBearingType.Text = ((AeiTag.BearingType)decodedTag.getBearing).ToString();
                            txtPlatformID.Text = ((AeiTag.PlatformId)decodedTag.getPlatformId).ToString();
                            txtSpare.Text = decodedTag.getSpare.ToString();
                        }
                        else if (((AeiTag.EquipmentGroup)decodedTag.getEquipmentGroup) == AeiTag.EquipmentGroup.END_OF_TRAIN_DEVICE)
                        {
                            gridEndofTrain.Visibility = Visibility.Visible;
                            gridTrailer.Visibility = gridRailcarCover.Visibility = gridIntermodal.Visibility = gridChassis.Visibility = gridRailcarLocomotiveMultiModalPassiveAlarm.Visibility = gridGeneratorSet.Visibility = gridErrorMessage.Visibility = Visibility.Collapsed;

                            if (decodedTag.IsHalfFrameTag)
                                txtEOTSpare.Visibility = lblEOTSpare.Visibility = Visibility.Collapsed;
                            else
                                txtEOTSpare.Visibility = lblEOTSpare.Visibility = Visibility.Visible;

                            txtEOTCarNumber.Text = decodedTag.getCarNumber.ToString();
                            txtEOTType.Text = decodedTag.getEOTType.ToString();
                            txtEOTSideIndicator.Text = ((AeiTag.SideIndicator)decodedTag.getSide).ToString();
                            txtEOTSpare.Text = decodedTag.getEOTType.ToString();
                        }
                        else if (((AeiTag.EquipmentGroup)decodedTag.getEquipmentGroup) == AeiTag.EquipmentGroup.LOCOMOTIVE)
                        {
                            gridRailcarLocomotiveMultiModalPassiveAlarm.Visibility = Visibility.Visible;
                            gridTrailer.Visibility = gridRailcarCover.Visibility = gridIntermodal.Visibility = gridChassis.Visibility = gridEndofTrain.Visibility = gridGeneratorSet.Visibility = gridErrorMessage.Visibility = Visibility.Collapsed;

                            lblPlatformID.Visibility = txtPlatformID.Visibility = Visibility.Collapsed;
                            lblAlarm.Visibility = txtAlarm.Visibility = Visibility.Collapsed;
                            lblTypeDetails.Visibility = txtTypeDetails.Visibility = Visibility.Collapsed;

                            if (decodedTag.IsHalfFrameTag)
                                txtNumberofAxles.Visibility = lblNumberofAxles.Visibility = txtBearingType.Visibility = lblBearingType.Visibility = lblSpare.Visibility = txtSpare.Visibility = Visibility.Collapsed;
                            else
                                txtNumberofAxles.Visibility = lblNumberofAxles.Visibility = txtBearingType.Visibility = lblBearingType.Visibility = lblSpare.Visibility = txtSpare.Visibility = Visibility.Visible;

                            txtCarNumber.Text = decodedTag.getCarNumber.ToString();
                            txtSideIndicator.Text = ((AeiTag.SideIndicator)decodedTag.getSide).ToString();
                            txtLength.Text = decodedTag.getLengthInDecimeters.ToString();
                            txtNumberofAxles.Text = decodedTag.getNumberOfAxles.ToString();
                            txtBearingType.Text = ((AeiTag.BearingType)decodedTag.getBearing).ToString();
                            txtSpare.Text = decodedTag.getSpare.ToString();
                        }
                        else if (((AeiTag.EquipmentGroup)decodedTag.getEquipmentGroup) == AeiTag.EquipmentGroup.INTERMODAL_CONTAINER)
                        {
                            gridIntermodal.Visibility = Visibility.Visible;
                            gridTrailer.Visibility = gridRailcarCover.Visibility = gridEndofTrain.Visibility = gridChassis.Visibility = gridRailcarLocomotiveMultiModalPassiveAlarm.Visibility = gridGeneratorSet.Visibility = gridErrorMessage.Visibility = Visibility.Collapsed;

                            if (decodedTag.IsHalfFrameTag)
                                txtIntermodalHeight.Visibility = lblIntermodalHeight.Visibility = txtIntermodalWidth.Visibility = lblIntermodalWidth.Visibility = txtIntermodalContainerType.Visibility = lblIntermodalContainerType.Visibility = txtIntermodalMaxGrossWeight.Visibility = lblIntermodalMaxGrossWeight.Visibility = txtIntermodalTareWeight.Visibility = lblIntermodalTareWeight.Visibility = txtIntermodalSpare.Visibility = lblIntermodalSpare.Visibility = Visibility.Collapsed;
                            else
                                txtIntermodalHeight.Visibility = lblIntermodalHeight.Visibility = txtIntermodalWidth.Visibility = lblIntermodalWidth.Visibility = txtIntermodalContainerType.Visibility = lblIntermodalContainerType.Visibility = txtIntermodalMaxGrossWeight.Visibility = lblIntermodalMaxGrossWeight.Visibility = txtIntermodalTareWeight.Visibility = lblIntermodalTareWeight.Visibility = txtIntermodalSpare.Visibility = lblIntermodalSpare.Visibility = Visibility.Visible;

                            txtEquipmentGroup.Text = "CONTAINER";
                            txtIntermodalCarNumber.Text = decodedTag.getCarNumber.ToString();
                            txtIntermodalCheckDigit.Text = decodedTag.getCheckDigit.ToString();
                            txtIntermodalLengthCM.Text = decodedTag.getLengthCM.ToString();
                            txtIntermodalHeight.Text = decodedTag.getHeight.ToString();
                            txtIntermodalWidth.Text = decodedTag.getWidth.ToString();
                            txtIntermodalContainerType.Text = decodedTag.getContainerType.ToString();
                            txtIntermodalMaxGrossWeight.Text = decodedTag.getMaxGrossWeight.ToString();
                            txtIntermodalTareWeight.Text = decodedTag.getTareWeight.ToString();
                            txtIntermodalSpare.Text = decodedTag.getSpare.ToString();

                        }
                        else if (((AeiTag.EquipmentGroup)decodedTag.getEquipmentGroup) == AeiTag.EquipmentGroup.TRAILER)
                        {
                            gridTrailer.Visibility = Visibility.Visible;
                            gridRailcarLocomotiveMultiModalPassiveAlarm.Visibility = gridRailcarCover.Visibility = gridIntermodal.Visibility = gridChassis.Visibility = gridEndofTrain.Visibility = gridGeneratorSet.Visibility = gridErrorMessage.Visibility = Visibility.Collapsed;
                            if (decodedTag.IsHalfFrameTag)
                                txtTrailerLengthCM.Visibility = lblTrailerLengthCM.Visibility = txtTrailerWidth.Visibility = lblTrailerWidth.Visibility = txtTrailerTandemWidth.Visibility = lblTrailerTandemWidth.Visibility = txtTypeDetails.Visibility = lblTypeDetails.Visibility = txtTrailerForwardExtension.Visibility = lblTrailerForwardExtension.Visibility = txtTrailerTareWeight.Visibility = lblTrailerTareWeight.Visibility = txtTrailerHeight.Visibility = lblTrailerHeight.Visibility = Visibility.Collapsed;
                            else
                                txtTrailerLengthCM.Visibility = lblTrailerLengthCM.Visibility = txtTrailerWidth.Visibility = lblTrailerWidth.Visibility = txtTrailerTandemWidth.Visibility = lblTrailerTandemWidth.Visibility = txtTypeDetails.Visibility = lblTypeDetails.Visibility = txtTrailerForwardExtension.Visibility = lblTrailerForwardExtension.Visibility = txtTrailerTareWeight.Visibility = lblTrailerTareWeight.Visibility = txtTrailerHeight.Visibility = lblTrailerHeight.Visibility = Visibility.Collapsed;

                            txtTrailerNumber.Text = decodedTag.getTrailerNumer.ToString();
                            txtTrailerNumberString.Text = decodedTag.getTrailerNumerString;
                            txtTrailerLengthCM.Text = decodedTag.getLengthCM.ToString();
                            txtTrailerWidth.Text = decodedTag.getWidth.ToString();
                            txtTrailerTandemWidth.Text = decodedTag.getTandemWidth.ToString();
                            txtTypeDetails.Text = decodedTag.getTypeDetail.ToString();
                            txtTrailerForwardExtension.Text = decodedTag.getForwardExtension.ToString();
                            txtTrailerTareWeight.Text = decodedTag.getTareWeight.ToString();
                            txtTrailerHeight.Text = decodedTag.getHeight.ToString();
                        }
                        else if (((AeiTag.EquipmentGroup)decodedTag.getEquipmentGroup) == AeiTag.EquipmentGroup.CHASSIS)
                        {
                            gridChassis.Visibility = Visibility.Visible;
                            gridTrailer.Visibility = gridRailcarCover.Visibility = gridIntermodal.Visibility = gridEndofTrain.Visibility = gridRailcarLocomotiveMultiModalPassiveAlarm.Visibility = gridGeneratorSet.Visibility = gridErrorMessage.Visibility = Visibility.Collapsed;

                            if (decodedTag.IsHalfFrameTag)
                                txtChassisTandemWidth.Visibility = lblChassisTandemWidth.Visibility = txtChassisForwardExtension.Visibility = lblChassisForwardExtension.Visibility = txtChassisKingPinSettin.Visibility = lblChassisKingPinSettin.Visibility = txtChassisAxleSpacing.Visibility = lblChassisAxleSpacing.Visibility = txtChassisRunningGearLoc.Visibility = lblChassisRunningGearLoc.Visibility = txtChassisNumLength.Visibility = lblChassisNumLength.Visibility = txtChassisMinLength.Visibility = lblChassisMinLength.Visibility = txtChassisSpare.Visibility = lblChassisSpare.Visibility = txtChassisMaxLength.Visibility = lblChassisMaxLength.Visibility = Visibility.Collapsed;
                            else
                                txtChassisTandemWidth.Visibility = lblChassisTandemWidth.Visibility = txtChassisForwardExtension.Visibility = lblChassisForwardExtension.Visibility = txtChassisKingPinSettin.Visibility = lblChassisKingPinSettin.Visibility = txtChassisAxleSpacing.Visibility = lblChassisAxleSpacing.Visibility = txtChassisRunningGearLoc.Visibility = lblChassisRunningGearLoc.Visibility = txtChassisNumLength.Visibility = lblChassisNumLength.Visibility = txtChassisMinLength.Visibility = lblChassisMinLength.Visibility = txtChassisSpare.Visibility = lblChassisSpare.Visibility = txtChassisMaxLength.Visibility = lblChassisMaxLength.Visibility = Visibility.Collapsed;

                            txtChassisCarNumber.Text = decodedTag.getCarNumber.ToString();
                            txtChassisTypeDetail.Text = decodedTag.getTypeDetail.ToString();
                            txtChassisTareWeight.Text = decodedTag.getTareWeight.ToString();
                            txtChassisHeight.Text = decodedTag.getHeight.ToString();
                            txtChassisTandemWidth.Text = decodedTag.getTandemWidth.ToString();
                            txtChassisForwardExtension.Text = decodedTag.getForwardExtension.ToString();
                            txtChassisKingPinSettin.Text = decodedTag.getKingPinSetting.ToString();
                            txtChassisAxleSpacing.Text = decodedTag.getAxleSpacing.ToString();
                            txtChassisRunningGearLoc.Text = decodedTag.getRunningGearLoc.ToString();
                            txtChassisNumLength.Text = decodedTag.getNumLengths.ToString();
                            txtChassisMinLength.Text = decodedTag.getMinLength.ToString();
                            txtChassisSpare.Text = decodedTag.getSpare.ToString();
                            txtChassisMaxLength.Text = decodedTag.getMaxLength.ToString();
                        }
                        else if (((AeiTag.EquipmentGroup)decodedTag.getEquipmentGroup) == AeiTag.EquipmentGroup.RAILCAR_COVER)
                        {
                            gridRailcarCover.Visibility = Visibility.Visible;
                            gridTrailer.Visibility = gridIntermodal.Visibility = gridEndofTrain.Visibility = gridChassis.Visibility = gridRailcarLocomotiveMultiModalPassiveAlarm.Visibility = gridGeneratorSet.Visibility = gridErrorMessage.Visibility = Visibility.Collapsed;
                            if (decodedTag.IsHalfFrameTag)
                                txtInsulation.Visibility = lblInsulation.Visibility = txtfitting.Visibility = lblfitting.Visibility = txtAssocRailCarInitial.Visibility = lblAssocRailCarInitial.Visibility = txtAssocRailCarInitialString.Visibility = lblAssocRailCarInitialString.Visibility = txtAssocRailCarNumber.Visibility = lblAssocRailCarNumber.Visibility = Visibility.Collapsed;
                            else
                                txtInsulation.Visibility = lblInsulation.Visibility = txtfitting.Visibility = lblfitting.Visibility = txtAssocRailCarInitial.Visibility = lblAssocRailCarInitial.Visibility = txtAssocRailCarInitialString.Visibility = lblAssocRailCarInitialString.Visibility = txtAssocRailCarNumber.Visibility = lblAssocRailCarNumber.Visibility = Visibility.Visible;

                            txtRailcarCoverCarNumber.Text = decodedTag.getCarNumber.ToString();
                            txtRailcarCoverSideIndicator.Text = ((AeiTag.SideIndicator)decodedTag.getSide).ToString();
                            txtRailcarCoverLength.Text = decodedTag.getLengthInDecimeters.ToString();
                            txtCoverType.Text = decodedTag.getCoverType.ToString();
                            txtDateBuilt.Text = decodedTag.getDateBuilt.ToString();
                            txtInsulation.Text = decodedTag.getInsulation.ToString();
                            txtfitting.Text = decodedTag.getFitting.ToString();
                            txtAssocRailCarInitial.Text = decodedTag.getAssocRailcarInitial.ToString();
                            txtAssocRailCarInitialString.Text = decodedTag.getAssocRailcarInitialString.ToString();
                            txtAssocRailCarNumber.Text = decodedTag.getAssocRailcarInitialNumber.ToString();
                        }
                        else if (((AeiTag.EquipmentGroup)decodedTag.getEquipmentGroup) == AeiTag.EquipmentGroup.PASSIVE_ALARM_TAG)
                        {
                            gridRailcarLocomotiveMultiModalPassiveAlarm.Visibility = Visibility.Visible;
                            gridTrailer.Visibility = gridRailcarCover.Visibility = gridIntermodal.Visibility = gridChassis.Visibility = gridEndofTrain.Visibility = gridGeneratorSet.Visibility = gridErrorMessage.Visibility = Visibility.Collapsed;

                            lblPlatformID.Visibility = txtPlatformID.Visibility = Visibility.Visible;
                            lblAlarm.Visibility = txtAlarm.Visibility = Visibility.Visible;
                            lblTypeDetails.Visibility = txtTypeDetails.Visibility = Visibility.Collapsed;

                            if (decodedTag.IsHalfFrameTag)
                                txtNumberofAxles.Visibility = lblNumberofAxles.Visibility = txtBearingType.Visibility = lblBearingType.Visibility = txtPlatformID.Visibility = lblPlatformID.Visibility = lblSpare.Visibility = txtSpare.Visibility = lblAlarm.Visibility = txtAlarm.Visibility = Visibility.Collapsed;
                            else
                                txtNumberofAxles.Visibility = lblNumberofAxles.Visibility = txtBearingType.Visibility = lblBearingType.Visibility = txtPlatformID.Visibility = lblPlatformID.Visibility = lblSpare.Visibility = txtSpare.Visibility = lblAlarm.Visibility = txtAlarm.Visibility = Visibility.Visible;

                            txtCarNumber.Text = decodedTag.getCarNumber.ToString();
                            txtSideIndicator.Text = ((AeiTag.SideIndicator)decodedTag.getSide).ToString();
                            txtLength.Text = decodedTag.getLengthInDecimeters.ToString();
                            txtNumberofAxles.Text = decodedTag.getNumberOfAxles.ToString();
                            txtBearingType.Text = ((AeiTag.BearingType)decodedTag.getBearing).ToString();
                            txtSpare.Text = decodedTag.getSpare.ToString();
                            txtPlatformID.Text = ((AeiTag.PlatformId)decodedTag.getPlatformId).ToString();
                            txtAlarm.Text = decodedTag.getAlarm.ToString();
                            txtTypeDetails.Text = decodedTag.getTypeDetail.ToString();
                        }
                        else if (((AeiTag.EquipmentGroup)decodedTag.getEquipmentGroup) == AeiTag.EquipmentGroup.GENERATOR_SET)
                        {
                            gridGeneratorSet.Visibility = Visibility.Visible;
                            gridTrailer.Visibility = gridRailcarCover.Visibility = gridIntermodal.Visibility = gridChassis.Visibility = gridEndofTrain.Visibility = gridRailcarLocomotiveMultiModalPassiveAlarm.Visibility = gridErrorMessage.Visibility = Visibility.Collapsed;

                            if (decodedTag.IsHalfFrameTag)
                                txtGeneratorSetTareWeight.Visibility = lblGeneratorSetTareWeight.Visibility = txtFuelCapacity.Visibility = lblFuelCapacity.Visibility = txtVoltage.Visibility = lblVoltage.Visibility = txtGeneratorSerSpare.Visibility = lblGeneratorSerSpare.Visibility = Visibility.Collapsed;
                            else
                                txtGeneratorSetTareWeight.Visibility = lblGeneratorSetTareWeight.Visibility = txtFuelCapacity.Visibility = lblFuelCapacity.Visibility = txtVoltage.Visibility = lblVoltage.Visibility = txtGeneratorSerSpare.Visibility = lblGeneratorSerSpare.Visibility = Visibility.Visible;

                            txtGeneratorSetNumber.Text = decodedTag.getGenSetNumber.ToString();
                            txtGeneratorSetNumberString.Text = decodedTag.getGenSetNumberString.ToString();
                            txtMounting.Text = decodedTag.getMouting.ToString();
                            txtGeneratorSetTareWeight.Text = decodedTag.getTareWeight.ToString();
                            txtFuelCapacity.Text = decodedTag.getFuelCapacity.ToString();
                            txtVoltage.Text = decodedTag.getVoltage.ToString();
                            txtGeneratorSerSpare.Text = decodedTag.getSpare.ToString();
                        }
                        else if (((AeiTag.EquipmentGroup)decodedTag.getEquipmentGroup) == AeiTag.EquipmentGroup.MULTIMODAL_EQUIPMENT)
                        {
                            gridRailcarLocomotiveMultiModalPassiveAlarm.Visibility = Visibility.Visible;
                            gridTrailer.Visibility = gridRailcarCover.Visibility = gridIntermodal.Visibility = gridChassis.Visibility = gridEndofTrain.Visibility = gridGeneratorSet.Visibility = gridErrorMessage.Visibility = Visibility.Collapsed;

                            lblPlatformID.Visibility = txtPlatformID.Visibility = Visibility.Visible;
                            lblAlarm.Visibility = txtAlarm.Visibility = Visibility.Collapsed;
                            lblTypeDetails.Visibility = txtTypeDetails.Visibility = Visibility.Visible;

                            if (decodedTag.IsHalfFrameTag)
                                txtNumberofAxles.Visibility = lblNumberofAxles.Visibility = txtBearingType.Visibility = lblBearingType.Visibility = txtPlatformID.Visibility = lblPlatformID.Visibility = lblSpare.Visibility = txtSpare.Visibility = txtTypeDetails.Visibility = lblTypeDetails.Visibility = Visibility.Collapsed;
                            else
                                txtNumberofAxles.Visibility = lblNumberofAxles.Visibility = txtBearingType.Visibility = lblBearingType.Visibility = txtPlatformID.Visibility = lblPlatformID.Visibility = lblSpare.Visibility = txtSpare.Visibility = txtTypeDetails.Visibility = lblTypeDetails.Visibility = Visibility.Visible;

                            txtCarNumber.Text = decodedTag.getCarNumber.ToString();
                            txtSideIndicator.Text = ((AeiTag.SideIndicator)decodedTag.getSide).ToString();
                            txtLength.Text = decodedTag.getLengthInDecimeters.ToString();
                            txtNumberofAxles.Text = decodedTag.getNumberOfAxles.ToString();
                            txtBearingType.Text = ((AeiTag.BearingType)decodedTag.getBearing).ToString();
                            txtSpare.Text = decodedTag.getSpare.ToString();
                            txtPlatformID.Text = ((AeiTag.PlatformId)decodedTag.getPlatformId).ToString();
                            txtTypeDetails.Text = decodedTag.getTypeDetail.ToString();
                        }
                    }
                    else
                    {
                        gridErrorMessage.Visibility = Visibility.Visible;
                        tblErrorMessage.Text += "- Unknown/unidentified tag type, can not proceed with tag parsing\nand displaying the raw ASCII data.\n";
                        txtEquipmentGroup.Text = Convert.ToString(decodedTag.getEquipmentGroup, 2).PadLeft(5, '0');
                        tblBinaryFormat.Text = BinaryStringToHexString(epcdata);
                    }

                    txtEquipmentInitial.Text = decodedTag.getEquipmentInitialString;

                    if (decodedTag.IsFieldValid[AeiTag.TagField.TAG_TYPE])
                    {
                        txtTagType.Foreground = Brushes.Gray;
                        txtTagType.Text = ((AeiTag.TagType)decodedTag.getTagType).ToString();
                    }
                    else
                    {
                        txtTagType.Foreground = Brushes.Red;
                        txtTagType.Text = Convert.ToString(decodedTag.getTagType, 2).PadLeft(2, '0');
                        tblErrorMessage.Text += "- Invalid Tag Type.\n";
                    }

                    if (decodedTag.IsFieldValid[AeiTag.TagField.DATA_FORMAT])
                    {
                        txtDataFormat.Foreground = Brushes.Gray;
                        txtDataFormat.Text = ((AeiTag.DataFormat)decodedTag.getDataFormat).ToString();
                    }
                    else
                    {
                        txtDataFormat.Foreground = Brushes.Red;
                        txtDataFormat.Text = Convert.ToString(decodedTag.getDataFormat, 2).PadLeft(6, '0');
                        tblErrorMessage.Text += "- Invalid Data Format.\n";
                    }

                    if (decodedTag.IsHalfFrameTag)
                        txtDataFormat.Visibility = lblDataFormat.Visibility = Visibility.Collapsed;
                    else
                        txtDataFormat.Visibility = lblDataFormat.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                gridErrorMessage.Visibility = Visibility.Visible;
                tblErrorMessage.Text += "- Unsupported AEI Tag.\n" + "- " + ex.Message;
                tblBinaryFormat.Text = BinaryStringToHexString(epcdata);
                throw new Exception("Unsupported AEI Tag. " + ex.Message);
            }
            finally
            {
                tblErrorMessage.Text = tblErrorMessage.Text.TrimEnd();
            }
        }

        private void ResetATATextControl()
        {
            txtCarNumber.Text = "";
            txtSideIndicator.Text = "";
            txtLength.Text = "";
            txtNumberofAxles.Text = "";
            txtBearingType.Text = "";
            txtSpare.Text = "";
            txtPlatformID.Text = "";
            txtAlarm.Text = "";
            txtTypeDetails.Text = "";

            txtTrailerNumber.Text = "";
            txtTrailerNumberString.Text = "";
            txtTrailerLengthCM.Text = "";
            txtTrailerWidth.Text = "";
            txtTrailerTandemWidth.Text = "";
            txtTypeDetails.Text = "";
            txtTrailerForwardExtension.Text = "";
            txtTrailerTareWeight.Text = "";
            txtTrailerHeight.Text = "";

            txtChassisCarNumber.Text = "";
            txtChassisTypeDetail.Text = "";
            txtChassisTareWeight.Text = "";
            txtChassisHeight.Text = "";
            txtChassisTandemWidth.Text = "";
            txtChassisForwardExtension.Text = "";
            txtChassisKingPinSettin.Text = "";
            txtChassisAxleSpacing.Text = "";
            txtChassisRunningGearLoc.Text = "";
            txtChassisNumLength.Text = "";
            txtChassisMinLength.Text = "";
            txtChassisSpare.Text = "";
            txtChassisMaxLength.Text = "";

            txtEOTCarNumber.Text = "";
            txtEOTType.Text = "";
            txtEOTSideIndicator.Text = "";
            txtEOTSpare.Text = "";

            txtIntermodalCarNumber.Text = "";
            txtIntermodalCheckDigit.Text = "";
            txtIntermodalLengthCM.Text = "";
            txtIntermodalHeight.Text = "";
            txtIntermodalWidth.Text = "";
            txtIntermodalContainerType.Text = "";
            txtIntermodalMaxGrossWeight.Text = "";
            txtIntermodalTareWeight.Text = "";
            txtIntermodalSpare.Text = "";

            txtRailcarCoverCarNumber.Text = "";
            txtRailcarCoverSideIndicator.Text = "";
            txtRailcarCoverLength.Text = "";
            txtCoverType.Text = "";
            txtDateBuilt.Text = "";
            txtInsulation.Text = "";
            txtfitting.Text = "";
            txtAssocRailCarInitial.Text = "";
            txtAssocRailCarInitialString.Text = "";
            txtAssocRailCarNumber.Text = "";

            txtGeneratorSetNumber.Text = "";
            txtGeneratorSetNumberString.Text = "";
            txtMounting.Text = "";
            txtGeneratorSetTareWeight.Text = "";
            txtFuelCapacity.Text = "";
            txtVoltage.Text = "";
            txtGeneratorSerSpare.Text = "";

            tblErrorMessage.Text = "";
            tblBinaryFormat.Text = "";
        }

        private void InitializeControlForEOT()
        {
            lblCarNumber.Text = "EOT Number : ";
            lblLength.Text = "EOT Type : ";
            lblSideIndicator.Visibility = txtSideIndicator.Visibility = Visibility.Visible;
            lblBearingType.Visibility = txtBearingType.Visibility = lblNumberofAxles.Visibility = txtNumberofAxles.Visibility = lblPlatformID.Visibility = txtPlatformID.Visibility = Visibility.Collapsed;
            //lblCheckDigit.Visibility = txtCheckDigit.Visibility = lblContainerLength.Visibility = txtContainerLength.Visibility = lblContainerHeight.Visibility = txtContainerHeight.Visibility = lblContainerGrossWeight.Visibility = txtContainerGrossWeight.Visibility = lblContainerTareWeight.Visibility = txtContainerTareWeight.Visibility = lblContainerType.Visibility = txtContainerType.Visibility = lblConainterWidth.Visibility = txtConainterWidth.Visibility = Visibility.Collapsed;
        }

        private void InitializeControlForLocomotive()
        {
            lblCarNumber.Text = "Car Number : ";
            lblLength.Text = "Length (dm) : ";
            lblBearingType.Visibility = txtBearingType.Visibility = lblNumberofAxles.Visibility = txtNumberofAxles.Visibility = Visibility.Visible;
            lblSideIndicator.Visibility = txtSideIndicator.Visibility = Visibility.Visible;
            lblPlatformID.Visibility = txtPlatformID.Visibility = Visibility.Collapsed;
            //lblCheckDigit.Visibility = txtCheckDigit.Visibility = lblContainerLength.Visibility = txtContainerLength.Visibility = lblContainerHeight.Visibility = txtContainerHeight.Visibility = lblContainerGrossWeight.Visibility = txtContainerGrossWeight.Visibility = lblContainerTareWeight.Visibility = txtContainerTareWeight.Visibility = lblContainerType.Visibility = txtContainerType.Visibility = lblConainterWidth.Visibility = txtConainterWidth.Visibility = Visibility.Collapsed;
        }

        private void InitializeControlForContainer()
        {
            lblBearingType.Visibility = txtBearingType.Visibility = lblNumberofAxles.Visibility = txtNumberofAxles.Visibility = lblPlatformID.Visibility = txtPlatformID.Visibility = lblLength.Visibility = txtLength.Visibility = lblSideIndicator.Visibility = txtSideIndicator.Visibility = Visibility.Collapsed;
            //lblCheckDigit.Visibility = txtCheckDigit.Visibility = lblContainerLength.Visibility = txtContainerLength.Visibility = lblContainerHeight.Visibility = txtContainerHeight.Visibility = lblContainerGrossWeight.Visibility = txtContainerGrossWeight.Visibility = lblContainerTareWeight.Visibility = txtContainerTareWeight.Visibility = lblContainerType.Visibility = txtContainerType.Visibility = lblConainterWidth.Visibility = txtConainterWidth.Visibility = Visibility.Visible;

        }

        string userData = string.Empty;
        string unUsedEpcData = string.Empty;
        string unadditionalMemData = string.Empty;
        private void PopulateData()
        {
            try
            {
                Mouse.SetCursor(Cursors.Wait);

                // Create the object to read tag memory
                ReadTagMemory readTagMem = new ReadTagMemory(objReader, model);

                if ((bool)rbFirstTagIns.IsChecked)
                {
                    antenna = GetSelectedAntennaList()[0];
                }

                objReader.ParamSet("/reader/tagop/antenna", antenna);
                TagFilter searchSelect = null;

                if ((bool)rbSelectedTagIns.IsChecked)
                {
                    if (lblSelectFilter.Content.ToString().Contains("EPC ID"))
                    {
                        searchSelect = new TagData(currentEPC);
                    }
                    else
                    {
                        int dataLength = 0;
                        byte[] SearchSelectData = ByteFormat.FromHex(txtData.Text);
                        if (null != SearchSelectData)
                        {
                            dataLength = SearchSelectData.Length;
                        }

                        searchSelect = new Gen2.Select(false, selectMemBank, Convert.ToUInt16(startAddress * 16), Convert.ToUInt16(dataLength * 8), SearchSelectData);
                    }
                }
                else
                {
                    searchSelect = new TagData(currentEPC);
                }

                #region ReadReservedMemory

                //Read Reserved memory bank data
                ushort[] reservedData = null;

                txtKillPassword.Text = "";
                txtAcessPassword.Text = "";
                txtReservedMemUnusedValue.Text = "";
                // Hide additional memory textboxes
                lblAdditionalReservedMem.Visibility = System.Windows.Visibility.Collapsed;
                txtReservedMemUnusedValue.Visibility = System.Windows.Visibility.Collapsed;
                lblAdditionalReservedMemAdd.Visibility = System.Windows.Visibility.Collapsed;

                try
                {
                    readTagMem.ReadTagMemoryData(Gen2.Bank.RESERVED, searchSelect, ref reservedData);
                    // Parse the response to get access pwd, kill pwd and if additional memory exists
                    ParseReservedMemData(reservedData);
                }
                catch (Exception ex)
                {
                    // Hide additional memory textboxes
                    lblAdditionalReservedMem.Visibility = System.Windows.Visibility.Collapsed;
                    txtReservedMemUnusedValue.Visibility = System.Windows.Visibility.Collapsed;
                    lblAdditionalReservedMemAdd.Visibility = System.Windows.Visibility.Collapsed;
                    txtReservedMemUnusedValue.Text = "";
                    // If either of the memory is locked get in or else throw the exception.
                    if ((ex is FAULT_PROTOCOL_BIT_DECODING_FAILED_Exception) || (ex is FAULT_GEN2_PROTOCOL_MEMORY_LOCKED_Exception))
                    {
                        try
                        {
                            ReadReservedMemData(Gen2.Bank.RESERVED, searchSelect);
                        }
                        catch (Exception e)
                        {
                            if (e.Message.ToLower().Contains("no tags found"))
                            {
                                txtKillPassword.Text = "Unable to read Kill Password";
                                txtAcessPassword.Text = "Unable to read Access Password";
                                lbltagnotfound.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                txtKillPassword.Text = txtAcessPassword.Text = e.Message;
                            }
                        }
                    }
                    else if (ex is FAULT_GEN2_PROTOCOL_MEMORY_OVERRUN_BAD_PC_Exception)
                    {
                        txtKillPassword.Text = "Read Error";
                        txtAcessPassword.Text = "Read Error";
                    }
                    else
                    {
                        if (ex.Message.ToLower().Contains("no tags found"))
                        {
                            txtKillPassword.Text = "Unable to read Kill Password";
                            txtAcessPassword.Text = "Unable to read Access Password";
                            lbltagnotfound.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            txtKillPassword.Text = txtAcessPassword.Text = ex.Message;
                        }
                    }
                }
                #endregion ReadReservedMemory

                #region ReadEPCMemory
                //Read EPC bank data
                ushort[] epcData = null;

                txtPC.Text = "";
                txtCRC.Text = "";
                txtEPCData.Text = "";
                txtEPCValue.Text = "";
                txtEPCUnused.Text = "";
                txtEPCUnusedValue.Text = "";
                txtadditionalMemValue.Text = "";

                // Hide additional memory
                spUnused.Visibility = System.Windows.Visibility.Collapsed;
                spXPC.Visibility = System.Windows.Visibility.Collapsed;
                spXPC2.Visibility = System.Windows.Visibility.Collapsed;
                spAddMemory.Visibility = System.Windows.Visibility.Collapsed;

                try
                {
                    readTagMem.ReadTagMemoryData(Gen2.Bank.EPC, searchSelect, ref epcData);
                    ParseEPCMemData(epcData, searchSelect);
                }
                catch (Exception ex)
                {
                    if (ex.Message.ToLower().Contains("no tags found"))
                    {
                        txtEPCData.Text = "Unable to read EPC Memory Bank.";
                        lbltagnotfound.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        txtEPCData.Text = ex.Message;
                    }
                    rbEPCAscii.IsEnabled = false;
                    rbEPCBase36.IsEnabled = false;
                }
                #endregion ReadEPCMemory

                #region ReadTIDMemory
                //Read TID bank data
                ushort[] tidData = null;

                txtClsID.Text = "";
                txtVendorID.Text = "";
                txtVendorValue.Text = "";
                txtModelID.Text = "";
                txtModeldIDValue.Text = "";
                txtUniqueIDValue.Text = "";

                try
                {
                    readTagMem.ReadTagMemoryData(Gen2.Bank.TID, searchSelect, ref tidData);
                    ParseTIDMemData(tidData);
                }
                catch (Exception ex)
                {
                    if (ex is FAULT_GEN2_PROTOCOL_MEMORY_OVERRUN_BAD_PC_Exception)
                    {
                        txtUniqueIDValue.Text = "Read Error";
                    }
                    else
                    {
                        if (ex.Message.ToLower().Contains("no tags found"))
                        {
                            txtUniqueIDValue.Text = "Unable to read TID Memory Bank.";
                            lbltagnotfound.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            txtUniqueIDValue.Text = ex.Message;
                        }
                    }
                }
                #endregion ReadTIDMemory

                #region ReadUserMemory
                //Read USER bank data
                ushort[] userMemData = null;

                txtUserDataValue.Text = "";
                txtUserMemData.Text = "";

                try
                {
                    readTagMem.ReadTagMemoryData(Gen2.Bank.USER, searchSelect, ref userMemData);
                    ParseUserMemData(userMemData);
                }
                catch (Exception ex)
                {
                    if (ex is FAULT_GEN2_PROTOCOL_MEMORY_OVERRUN_BAD_PC_Exception)
                    {
                        txtUserMemData.Text = "Read Error";
                    }
                    else
                    {
                        if (ex.Message.ToLower().Contains("no tags found"))
                        {
                            txtUserMemData.Text = "Unable to read User Memory Bank.";
                            lbltagnotfound.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            txtUserMemData.Text = ex.Message;
                        }
                    }
                }
                #endregion ReadUserMemory
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.SetCursor(Cursors.Arrow);
            }
        }

        /// <summary>
        /// Parse reserved memory data and populate the reserved mem textboxes
        /// </summary>
        /// <param name="reservedData">accepts read reserved memory data</param>
        private void ParseReservedMemData(ushort[] reservedData)
        {
            string reservedMemData = string.Empty;
            if (null != reservedData)
                reservedMemData = ByteFormat.ToHex(ByteConv.ConvertFromUshortArray(reservedData), "", " ");

            if (reservedMemData.Length > 0)
            {
                if (reservedMemData.Length > 11)
                {
                    // Extract kill pwd
                    txtKillPassword.Text = reservedMemData.Substring(0, 11).TrimStart(' ');
                    string tempData = reservedMemData.Substring(12).TrimStart(' ');
                    // Check if reserved memory has additional data
                    if (tempData.Length > 11)
                    {
                        // Extract access pwd
                        txtAcessPassword.Text = reservedMemData.Substring(12, 11).TrimStart(' ');
                        // Extract additional reserved memory
                        txtReservedMemUnusedValue.Text = reservedMemData.Substring(24).TrimStart(' ');

                        // Visible additional memory textboxes
                        lblAdditionalReservedMem.Visibility = System.Windows.Visibility.Visible;
                        txtReservedMemUnusedValue.Visibility = System.Windows.Visibility.Visible;
                        lblAdditionalReservedMemAdd.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        // Extract access pwd
                        txtAcessPassword.Text = reservedMemData.Substring(12).TrimStart(' ');

                        // Hide additional memory textboxes
                        lblAdditionalReservedMem.Visibility = System.Windows.Visibility.Collapsed;
                        txtReservedMemUnusedValue.Visibility = System.Windows.Visibility.Collapsed;
                        lblAdditionalReservedMemAdd.Visibility = System.Windows.Visibility.Collapsed;
                    }
                }
                else
                {
                    txtKillPassword.Text = reservedMemData.Substring(0, reservedData.Length).TrimStart(' ');
                }
            }
        }

        /// <summary>
        /// Parse epc memory data and populate the epc mem textboxes
        /// </summary>
        /// <param name="epcData">accepts read epc memory data</param>
        private void ParseEPCMemData(ushort[] epcData, TagFilter filter)
        {
            unUsedEpcData = string.Empty;
            byte[] epcBankData = null;
            if (null != epcData)
            {
                epcBankData = ByteConv.ConvertFromUshortArray(epcData);
                int readOffset = 0;
                byte[] epc, crc, pc, unusedEpc = null, additionalMemData = null;
                int lengthCounter = 2;
                crc = SubArray(epcBankData, ref readOffset, lengthCounter);
                pc = SubArray(epcBankData, ref readOffset, lengthCounter);
                lengthCounter += 2;

                // Extract the epc length from pc word
                int epclength = Convert.ToInt32(((pc[0] & 0xf8) >> 3)) * 2;

                epc = SubArray(epcBankData, ref readOffset, epclength);

                List<byte> xpc = new List<byte>();

                /* Add support for XPC bits
                    * XPC_W1 is present, when the 6th most significant bit of PC word is set
                    */
                if ((pc[0] & 0x02) == 0x02)
                {
                    /* When this bit is set, the XPC_W1 word will follow the PC word
                        * Our TMR_Gen2_TagData::pc has enough space, so copying to the same.
                        */
                    try
                    {
                        ushort[] xpcW1 = (ushort[])objReader.ExecuteTagOp(new Gen2.ReadData(Gen2.Bank.EPC, 0x21, 1), filter);
                        spXPC.Visibility = System.Windows.Visibility.Visible;
                        lblXPC1MemAddress.Content = "33";
                        xpc.AddRange(ByteConv.ConvertFromUshortArray(xpcW1));
                        lengthCounter += 2;
                        txtXPC1.Text = ByteFormat.ToHex(ByteConv.ConvertFromUshortArray(xpcW1), "", " ");
                        lblXPC1.Content = "XPC";
                    }
                    catch (Exception ex)
                    {
                        spXPC.Visibility = System.Windows.Visibility.Visible;
                        txtXPC1.Text = ex.Message;
                        lblXPC1.Content = "XPC";
                    }
                    /* If the most siginificant bit of XPC_W1 is set, then there exists
                    * XPC_W2. A total of 6  (PC + XPC_W1 + XPC_W2 bytes)
                    */
                    if ((xpc[0] & 0x80) == 0x80)
                    {
                        try
                        {
                            ushort[] xpcW2 = (ushort[])objReader.ExecuteTagOp(new Gen2.ReadData(Gen2.Bank.EPC, 0x22, 1), filter);
                            spXPC2.Visibility = System.Windows.Visibility.Visible;
                            lblXPC2MemAddress.Content = "34";
                            lengthCounter += 2;
                            txtXPC2.Text = ByteFormat.ToHex(ByteConv.ConvertFromUshortArray(xpcW2), "", " ");
                            // Change the name of XPC to XPC1
                            lblXPC1.Content = "XPC1";
                        }
                        catch (Exception ex)
                        {
                            spXPC2.Visibility = System.Windows.Visibility.Visible;
                            txtXPC2.Text = ex.Message;
                            // Change the name of XPC to XPC1
                            lblXPC1.Content = "XPC1";
                        }
                    }
                }
                // Read extended epc memory
                if (epcBankData.Length > (lengthCounter + epclength))
                {
                    lblExtdEPCMemAddress.Content = Convert.ToString(readOffset / 2);
                    bool isExtendedEPCMemover = true;
                    uint startExtdEPCMemAddress = (uint)readOffset / 2;
                    List<ushort> data = new List<ushort>();
                    try
                    {
                        while (isExtendedEPCMemover)
                        {
                            // Make sure reading of memory word by word doesn't override XPC1 data
                            if (startExtdEPCMemAddress < 33)
                            {
                                data.AddRange((ushort[])objReader.ExecuteTagOp(new Gen2.ReadData(Gen2.Bank.EPC, startExtdEPCMemAddress, 1), filter));
                                startExtdEPCMemAddress += 1;
                            }
                            else
                            {
                                // Read of memory should not exceed XPC bytes
                                isExtendedEPCMemover = false;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // If more then once the below exceptions are recieved then come out of the loop.
                        if (ex is FAULT_GEN2_PROTOCOL_MEMORY_OVERRUN_BAD_PC_Exception || (-1 != ex.Message.IndexOf("Non-specific reader error")) || (-1 != ex.Message.IndexOf("General Tag Error")) || (-1 != ex.Message.IndexOf("Tag data access failed")))
                        {
                            if (data.Count > 0)
                            {
                                // Just skip the exception and move on. So as not to lose the already read data.
                                isExtendedEPCMemover = false;
                            }
                        }
                    }

                    if (data.Count > 0)
                    {
                        unusedEpc = ByteConv.ConvertFromUshortArray(data.ToArray());
                    }
                }

                // Read additional memory
                if (epcBankData.Length > (lengthCounter + epclength))
                {
                    lblAddMemAddress.Content = "35";
                    bool isAdditionalMemover = true;
                    uint startAdditionalMemAddress = 0x23;
                    List<ushort> dataAdditionalMem = new List<ushort>();
                    try
                    {
                        while (isAdditionalMemover)
                        {
                            dataAdditionalMem.AddRange((ushort[])objReader.ExecuteTagOp(new Gen2.ReadData(Gen2.Bank.EPC, startAdditionalMemAddress, 1), filter));
                            startAdditionalMemAddress += 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        // If more then once the below exceptions are recieved then come out of the loop.
                        if (ex is FAULT_GEN2_PROTOCOL_MEMORY_OVERRUN_BAD_PC_Exception || (-1 != ex.Message.IndexOf("Non-specific reader error")) || (-1 != ex.Message.IndexOf("General Tag Error")) || (-1 != ex.Message.IndexOf("Tag data access failed")))
                        {
                            if (dataAdditionalMem.Count > 0)
                            {
                                // Just skip the exception and move on. So as not to lose the already read data.
                                isAdditionalMemover = false;
                            }
                        }
                    }
                    if (dataAdditionalMem.Count > 0)
                    {
                        additionalMemData = ByteConv.ConvertFromUshortArray(dataAdditionalMem.ToArray());
                    }
                }

                if (txtXPC1.Text != "")
                {
                    spXPC.Visibility = System.Windows.Visibility.Visible;
                }
                if (txtXPC2.Text != "")
                {
                    spXPC2.Visibility = System.Windows.Visibility.Visible;
                }

                txtCRC.Text = ByteFormat.ToHex(crc, "", " ");
                txtPC.Text = ByteFormat.ToHex(pc, "", " ");
                if (epc.Length == epclength)
                {
                    txtEPCData.Text = ByteFormat.ToHex(epc, "", " ");
                }
                else
                {
                    txtEPCData.Text = currentEPC;
                }
                if (null != unusedEpc)
                {
                    txtEPCUnused.Text = ByteFormat.ToHex(unusedEpc, "", " ");
                    unUsedEpcData = ByteFormat.ToHex(unusedEpc, "", "");
                    // Visible additional memory
                    spUnused.Visibility = System.Windows.Visibility.Visible;
                }

                if (null != additionalMemData)
                {
                    txtAdditionalMem.Text = ByteFormat.ToHex(additionalMemData, "", " ");
                    unadditionalMemData = ByteFormat.ToHex(additionalMemData, "", "");
                    // Visible additional memory
                    spAddMemory.Visibility = System.Windows.Visibility.Visible;
                }

                if ((bool)rbEPCAscii.IsChecked)
                {
                    txtEPCValue.Text = Utilities.HexStringToAsciiString(currentEPC);
                    txtEPCUnusedValue.Text = Utilities.HexStringToAsciiString(unUsedEpcData);
                    txtadditionalMemValue.Text = Utilities.HexStringToAsciiString(unadditionalMemData);
                }
                else if ((bool)rbEPCBase36.IsChecked)
                {
                    txtEPCValue.Text = Utilities.ConvertHexToBase36(currentEPC);
                    txtEPCUnusedValue.Text = Utilities.ConvertHexToBase36(unUsedEpcData);
                    txtadditionalMemValue.Text = Utilities.ConvertHexToBase36(unadditionalMemData);
                }

                #region 0 length read

                //if (model.Equals("M5e") || model.Equals("M5e EU") || model.Equals("M5e Compact"))
                //{
                //    ReadData(Gen2.Bank.EPC, searchSelect, out epcData);
                //}
                //else
                //{
                //    op = new Gen2.ReadData(Gen2.Bank.EPC, 0, 0);
                //    epcData = (ushort[])objReader.ExecuteTagOp(op, searchSelect);
                //}

                //if(null!= epcData)
                //    epcBankData = ByteFormat.ToHex(ByteConv.ConvertFromUshortArray(epcData), "", " ");

                //if (epcBankData.Length > 0)
                //{
                //    int epcLen = txtEpc.Text.Length;
                //    txtCRC.Text = epcBankData.Substring(0, 5).TrimStart(' ');
                //    txtPC.Text = epcBankData.Substring(6, 5).TrimStart(' ');
                //    int epcstringLength = epcLen+((epcLen/2)-1);
                //    txtEPCData.Text = epcBankData.Substring(11, epcstringLength).TrimStart(' ');

                //    //string epcDataString = ByteFormat.ToHex(ByteConv.ConvertFromUshortArray(epcData), "", "");
                //    txtEPCUnused.Text = epcBankData.Substring(11 + epcstringLength).TrimStart(' '); //String.Join(" ", (epcDataString.Substring(8 + epcLen)).ToArray());
                #endregion

            }
        }

        /// <summary>
        /// Parse tid memory data and populate the tid mem textboxes
        /// </summary>
        /// <param name="tidData">accepts read tid memory data</param>
        private void ParseTIDMemData(ushort[] tidData)
        {
            string tidBankData = string.Empty;
            if (null != tidData)
                tidBankData = ByteFormat.ToHex(ByteConv.ConvertFromUshortArray(tidData), "", " ");

            if (tidBankData.Length > 0)
            {
                txtClsID.Text = tidBankData.Substring(0, 2).TrimStart(' ');
                txtVendorID.Text = tidBankData.Substring(3, 4).Replace(" ", string.Empty);
                string tagModel = string.Empty;
                txtVendorValue.Text = GetVendor(txtVendorID.Text, tidBankData.Substring(7, 4).Replace(" ", string.Empty), out tagModel);
                txtModelID.Text = tidBankData.Substring(7, 4).Replace(" ", string.Empty);
                txtModeldIDValue.Text = tagModel;
                if (tidBankData.Length >= 12)
                    txtUniqueIDValue.Text = tidBankData.Substring(12).TrimStart(' ');
            }
        }

        /// <summary>
        /// Parse user memory data and populate the user mem textboxes
        /// </summary>
        /// <param name="userData">accepts read user memory data</param>
        private void ParseUserMemData(ushort[] userData)
        {
            string userMemData = string.Empty;
            if (null != userData)
            {
                userMemData = ByteFormat.ToHex(ByteConv.ConvertFromUshortArray(userData), "", "");
                txtUserDataValue.Text = ReplaceSpecialCharInAsciiData(Utilities.HexStringToAsciiString(userMemData).ToCharArray());
                txtUserMemData.Text = ByteFormat.ToHex(ByteConv.ConvertFromUshortArray(userData), "", " ");
            }
        }

        #region SubArray
        /// <summary>
        /// Extract subarray
        /// </summary>
        /// <param name="src">Source array</param>
        /// <param name="offset">Start index in source array</param>
        /// <param name="length">Number of source elements to extract</param>
        /// <returns>New array containing specified slice of source array</returns>
        private static byte[] SubArray(byte[] src, int offset, int length)
        {
            return SubArray(src, ref offset, length);
        }

        /// <summary>
        /// Extract subarray, automatically incrementing source offset
        /// </summary>
        /// <param name="src">Source array</param>
        /// <param name="offset">Start index in source array.  Automatically increments value by copied length.</param>
        /// <param name="length">Number of source elements to extract</param>
        /// <returns>New array containing specified slice of source array</returns>
        private static byte[] SubArray(byte[] src, ref int offset, int length)
        {
            byte[] dst = new byte[length];
            try
            {
                Array.Copy(src, offset, dst, 0, length);
                offset += length;
            }
            catch
            {
            }
            return dst;
        }

        #endregion

        private void ReadReservedMemData(Gen2.Bank bank, TagFilter filter)
        {
            ushort[] reservedData;
            TagOp op;
            try
            {
                try
                {
                    // Read kill password
                    op = new Gen2.ReadData(Gen2.Bank.RESERVED, 0, 2);
                    reservedData = (ushort[])objReader.ExecuteTagOp(op, filter);
                    if (null != reservedData)
                    {
                        txtKillPassword.Text = ByteFormat.ToHex(ByteConv.ConvertFromUshortArray(reservedData), "", " ");
                    }
                    else
                    {
                        txtKillPassword.Text = "";
                    }
                }
                catch (Exception ex)
                {
                    if (ex is FAULT_GEN2_PROTOCOL_MEMORY_OVERRUN_BAD_PC_Exception)
                    {
                        txtKillPassword.Text = "Read Error";
                    }
                    else
                    {
                        if (ex.Message.ToLower().Contains("no tag found"))
                        {
                            txtKillPassword.Text = "Unable to read Kill Password";
                        }
                        else
                        {
                            txtKillPassword.Text = ex.Message;
                        }
                    }
                }

                try
                {
                    // Read access password
                    reservedData = null;
                    op = new Gen2.ReadData(Gen2.Bank.RESERVED, 2, 2);
                    reservedData = (ushort[])objReader.ExecuteTagOp(op, filter);
                    if (null != reservedData)
                    {
                        txtAcessPassword.Text = ByteFormat.ToHex(ByteConv.ConvertFromUshortArray(reservedData), "", " ");
                    }
                    else
                    {
                        txtAcessPassword.Text = "";
                    }
                }
                catch (Exception ex)
                {
                    if (ex is FAULT_GEN2_PROTOCOL_MEMORY_OVERRUN_BAD_PC_Exception)
                    {
                        txtAcessPassword.Text = "Read Error";
                    }
                    else
                    {
                        if (ex.Message.ToLower().Contains("no tag found"))
                        {
                            txtAcessPassword.Text = "Unable to read Access Password";
                        }
                        else
                        {
                            txtAcessPassword.Text = ex.Message;
                        }
                    }
                }

                // Read additional memory password
                try
                {
                    reservedData = null;
                    if (model.Equals("M5e") || model.Equals("M5e EU") || model.Equals("M5e Compact") || model.Equals("M5e PRC") || model.Equals("Astra"))
                    {
                        ReadAdditionalReservedMemDataM5eVariants(Gen2.Bank.RESERVED, 4, filter, out reservedData);
                    }
                    else
                    {
                        op = new Gen2.ReadData(Gen2.Bank.RESERVED, 4, 0);
                        reservedData = (ushort[])objReader.ExecuteTagOp(op, filter);
                    }

                    if (null != reservedData)
                    {
                        txtReservedMemUnusedValue.Text = ByteFormat.ToHex(ByteConv.ConvertFromUshortArray(reservedData), "", " ");
                        // Visible additional memory textboxes
                        lblAdditionalReservedMem.Visibility = System.Windows.Visibility.Visible;
                        txtReservedMemUnusedValue.Visibility = System.Windows.Visibility.Visible;
                        lblAdditionalReservedMemAdd.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        txtReservedMemUnusedValue.Text = "";
                    }
                }
                catch
                {
                    // catch the exception and move on. Only some tags has aditional memory 
                    txtReservedMemUnusedValue.Text = "";
                    // Hide additional memory textboxes
                    lblAdditionalReservedMem.Visibility = System.Windows.Visibility.Collapsed;
                    txtReservedMemUnusedValue.Visibility = System.Windows.Visibility.Collapsed;
                    lblAdditionalReservedMemAdd.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Read additional reserved memory for m5e variants
        /// </summary>
        /// <param name="bank"></param>
        /// <param name="startAddress"></param>
        /// <param name="filter"></param>
        /// <param name="data"></param>
        private void ReadAdditionalReservedMemDataM5eVariants(Gen2.Bank bank, uint startAddress, TagFilter filter, out ushort[] data)
        {
            data = null;
            int words = 1;
            TagOp op;
            while (true)
            {
                try
                {
                    op = new Gen2.ReadData(bank, startAddress, Convert.ToByte(words));
                    data = (ushort[])objReader.ExecuteTagOp(op, filter);
                    words++;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Get selected antenna list
        /// </summary>
        /// <returns></returns>
        private List<int> GetSelectedAntennaList()
        {
            Window mainWindow = App.Current.MainWindow;
            CheckBox Ant1CheckBox = (CheckBox)mainWindow.FindName("Ant1CheckBox");
            CheckBox Ant2CheckBox = (CheckBox)mainWindow.FindName("Ant2CheckBox");
            CheckBox Ant3CheckBox = (CheckBox)mainWindow.FindName("Ant3CheckBox");
            CheckBox Ant4CheckBox = (CheckBox)mainWindow.FindName("Ant4CheckBox");
            CheckBox[] antennaBoxes = { Ant1CheckBox, Ant2CheckBox, Ant3CheckBox, Ant4CheckBox };
            List<int> ant = new List<int>();

            for (int antIdx = 0; antIdx < antennaBoxes.Length; antIdx++)
            {
                CheckBox antBox = antennaBoxes[antIdx];

                if ((bool)antBox.IsChecked)
                {
                    int antNum = antIdx + 1;
                    ant.Add(antNum);
                }
            }
            if (ant.Count > 0)
                return ant;
            else
                return null;
        }

        private string GetVendor(string vendorId, string tagId, out string model)
        {
            string vendor = string.Empty;
            model = string.Empty;

            switch (vendorId)
            {
                case Utilities.Impinj: vendor = "Impinj";
                    switch (tagId)
                    {
                        case Utilities.ImpinjOld: model = "Old"; break;
                        case Utilities.ImpinjAnchor: model = "Anchor"; break;
                        case Utilities.ImpinjMonaco: model = "Monaco"; break;
                        case Utilities.ImpinjMonza: model = "Monza"; break;
                        case Utilities.ImpinjMonza2: model = "Monza 2"; break;
                        case Utilities.ImpinjMonza2A: model = "Monza 2A"; break;
                        case Utilities.ImpinjMonzaID_3: model = "Monza ID/3"; break;
                        case Utilities.ImpinjMonzaID_3A: model = "Monza ID/3A"; break;
                        case Utilities.ImpinjMonzaID_3B: model = "Monza ID/3B"; break;
                        case Utilities.ImpinjMonzaID_3C: model = "Monza ID/3C"; break;
                        case Utilities.ImpinjMonza3: model = "Monza 3"; break;
                        case Utilities.ImpinjMonza_3A: model = "Monza 3A"; break;
                        case Utilities.ImpinjMonza_3B: model = "Monza 3B"; break;
                        case Utilities.ImpinjMonza_3C: model = "Monza 3C"; break;
                        case Utilities.ImpinjMonza_3D: model = "Monza 3D"; break;
                        default: break;
                    }
                    break;
                case Utilities.ImpinjXTID: vendor = "Impinj";
                    switch (tagId)
                    {
                        case Utilities.ImpinjMonza_4D: model = "Monza 4D"; break;
                        case Utilities.ImpinjMonza_4E: model = "Monza 4E"; break;
                        case Utilities.ImpinjMonza_4I: model = "Monza 4I"; break;
                        case Utilities.ImpinjMonza_4U: model = "Monza 4U"; break;
                        case Utilities.ImpinjMonza_4QT: model = "Monza 4QT"; break;
                        case Utilities.ImpinjMonza_5: model = "Monza 5"; break;
                        case Utilities.ImpinjMonza_X2K: model = "Monza X2K"; break;
                        case Utilities.ImpinjMonza_R6: model = "Monza R6"; break;
                        case Utilities.ImpinjMonza_R6_P: model = "Monza R6-P"; break;
                        case Utilities.ImpinjMonza_S6_C: model = "Monza S6-C"; break;
                        default: break;
                    }
                    break;
                case Utilities.TI: vendor = "TI"; break;
                case Utilities.Alien: vendor = "Alien";
                    switch (tagId)
                    {
                        case Utilities.AlienHiggs2: model = "Higgs 2"; break;
                        case Utilities.AlienHiggs3: model = "Higgs 3"; break;
                        case Utilities.AlienHiggs4: model = "Higgs 4"; break;
                        default: break;
                    }
                    break;
                case Utilities.Phillips: vendor = "Phillips"; break;
                case Utilities.NXP: vendor = "NXP";
                    switch (tagId)
                    {
                        case Utilities.NXP_G2: model = "G2"; break;
                        case Utilities.NXP_G2_XM: model = "G2 XM"; break;
                        case Utilities.NXP_G2_XL: model = "G2 XL"; break;
                        case Utilities.NXP_G2_iL_V0: model = "G2 iL V0"; break;
                        case Utilities.NXP_G2_iL_V2: model = "G2 iL V2"; break;
                        case Utilities.NXP_G2_iL_V6: model = "G2 iL V6"; break;
                        case Utilities.NXP_G2_iL_Plus_V0: model = "G2 iL+ V0"; break;
                        case Utilities.NXP_G2_iL_Plus_V2: model = "G2 iL+ V2"; break;
                        case Utilities.NXP_G2_iL_Plus_V6: model = "G2 iL+ V6"; break;
                        case Utilities.NXP_G2_IM: model = "G2 iM"; break;
                        case Utilities.NXP_G2_IM_Plus: model = "G2 iM+"; break;
                        case Utilities.NXPI2C: model = "I2C"; break;
                        default: break;
                    }
                    break;
                case Utilities.NXPXTID: vendor = "NXPXTID";
                    switch (tagId)
                    {
                        case Utilities.NXP_UCODE_7: model = "UCODE7"; break;
                        case Utilities.NXP_UCODE_7m: model = "UCODE 7m"; break;
                        case Utilities.NXPI2C4011: model = "I2C 4011"; break;
                        case Utilities.NXPI2C4021: model = "I2C 4011"; break;
                        default: break;
                    }
                    break;
                case Utilities.NXPv2: vendor = "NXP";
                    switch (tagId)
                    {
                        case Utilities.NXP_UCODE_7xm: model = "UCODE 7xm"; break;
                        case Utilities.NXP_UCODE_7xm_Plus: model = "UCODE 7xm+"; break;
                        case Utilities.NXP_UCODE_DNA: model = "UCODE DNA"; break;
                    }
                    break;
                case Utilities.STMicro: vendor = "STMicro";
                    switch (tagId)
                    {
                        case Utilities.STMicro_XRAG2: model = "XRAG2"; break;
                        default: break;
                    }
                    break;
                case Utilities.EMMICRO: vendor = "EM Micro";
                    switch (tagId)
                    {
                        case Utilities.EMMICRO_EM4126_V0: model = "EM4126 V0"; break;
                        case Utilities.EMMICRO_EM4126_V1: model = "EM4126 V1"; break;
                        case Utilities.EMMICRO_EM4126_V2: model = "EM4126 V2"; break;
                        case Utilities.EMMICRO_EM4126_V3: model = "EM4126 V3"; break;
                        case Utilities.EMMICRO_EM4126_V4: model = "EM4126 V4"; break;
                        case Utilities.EMMICRO_EM4126_V5: model = "EM4126 V5"; break;
                        case Utilities.EMMICRO_EM4126_V6: model = "EM4126 V6"; break;
                        case Utilities.EMMICRO_EM4126_V7: model = "EM4126 V7"; break;
                        case Utilities.EMMICRO_EM4124: model = "EM4124"; break;
                        case Utilities.EMMICRO_EM4124_D: model = "EM4124 D"; break;
                        case Utilities.PowerIDEmMicroWithTamper: model = "With tamper alaram"; break;
                        case Utilities.PowerIDEmMicroWithOutTamper: model = "With out tamper alaram"; break;
                        default: break;
                    }
                    break;
                case Utilities.EMMICROXTID: vendor = "EM Micro";
                    switch (tagId)
                    {
                        case Utilities.EMMICRO_EM4325_V11: model = "EM4325 V11"; break;
                        case Utilities.EMMICRO_EM4325_V21: model = "EM4325 V21"; break;
                        case Utilities.EMMICRO_EM4325_V31: model = "EM4325 V31"; break;
                        case Utilities.EMMICRO_EM4325_V41: model = "EM4325 V41"; break;
                        case Utilities.EMMICRO_EM4423_S: model = "EM4423 S"; break;
                        case Utilities.EMMICRO_EM4423_L: model = "EM4423 L"; break;
                        case Utilities.EMMICRO_EM4423_S_PLUS: model = "EM4423 S+"; break;
                        case Utilities.EMMICRO_EM4423_L_PLUS: model = "EM4423 L+"; break;
                        case Utilities.EMMICRO_EM4423T_S: model = "EM4423T S"; break;
                        case Utilities.EMMICRO_EM4423T_L: model = "EM4423T L"; break;
                        case Utilities.EMMICRO_EM4423T_S_PLUS: model = "EM4423T S+"; break;
                        case Utilities.EMMICRO_EM4423T_L_PLUS: model = "EM4423T L+"; break;
                        default: break;
                    }
                    break;
                case Utilities.Hitchi: vendor = "Hitchi"; break;
                case Utilities.Quanray: vendor = "Quanray";
                    switch (tagId)
                    {
                        case Utilities.QuanrayQstar5: model = "Qstar5"; break;
                        default: break;
                    }
                    break;
                case Utilities.FUJITSU: vendor = "Fujitsu";
                    switch (tagId)
                    {
                        case Utilities.FUJUTSU_64K: model = "64K"; break;
                        default: break;
                    }
                    break;
                case Utilities.IDS: vendor = "IDS";
                    switch (tagId)
                    {
                        case Utilities.IDSSL900A: model = "SL900A"; break;
                        default: break;
                    }
                    break;
                case Utilities.RAMTRON: vendor = "RAMTRON";
                    switch (tagId)
                    {
                        case Utilities.RAMTRON_WM72016: model = "WM72016"; break;
                        default: break;
                    }
                    break;
                case Utilities.Tego: vendor = "Tego";
                    switch (tagId)
                    {
                        case Utilities.Tego4K: model = "TegoChip 4K"; break;
                        case Utilities.Tego8K: model = "TegoChip 8K"; break;
                        case Utilities.Tego24K: model = "TegoChip 24K"; break;
                        default: break;
                    }
                    break;
                case Utilities.TegoXTID: vendor = "Tego";
                    switch (tagId)
                    {
                        case Utilities.Tego4K: model = "TegoChip 4K"; break;
                        case Utilities.Tego8K: model = "TegoChip 8K"; break;
                        case Utilities.Tego24K: model = "TegoChip 24K"; break;
                        default: break;
                    }
                    break;
                case Utilities.RFMicron: vendor = "RF Micron";
                    switch (tagId)
                    {
                        case Utilities.RFMicronMagnus: model = "Magnus"; break;
                        default: break;
                    }
                    break;
            }
            return vendor;
        }

        private void rbEPCAscii_Checked(object sender, RoutedEventArgs e)
        {
            if (null != objReader)
            {
                if (!(txtEpc.Text.Equals("")))
                {
                    txtEPCValue.Text = Utilities.HexStringToAsciiString(currentEPC);
                    txtEPCUnusedValue.Text = Utilities.HexStringToAsciiString(unUsedEpcData);
                    txtadditionalMemValue.Text = Utilities.HexStringToAsciiString(unadditionalMemData);
                }
            }
        }

        private void rbEPCBase36_Checked(object sender, RoutedEventArgs e)
        {
            if (null != objReader)
            {
                if (!(txtEpc.Text.Equals("")))
                {
                    txtEPCValue.Text = Utilities.ConvertHexToBase36(currentEPC);
                    txtEPCUnusedValue.Text = Utilities.ConvertHexToBase36(unUsedEpcData);
                    txtadditionalMemValue.Text = Utilities.ConvertHexToBase36(unadditionalMemData);
                }
            }
        }

        private void rbUserAscii_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != objReader)
                {
                    if (!(txtUserDataValue.Text.Equals(string.Empty)))
                    {
                        txtUserDataValue.Text = Utilities.HexStringToAsciiString(userData);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void rbUserBase36_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != objReader)
                {
                    if (!(txtUserDataValue.Text.Equals(string.Empty)))
                    {
                        txtUserDataValue.Text = Utilities.ConvertHexToBase36(userData);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void txtUserMemData_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //Don't accept any characters in the textbox
            e.Handled = true;
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            e.Handled = true;
        }

        private void txtEPCData_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Back) || (e.Key == Key.Delete))
            {
                e.Handled = true;
            }
        }

        private void rbFirstTagIns_Checked(object sender, RoutedEventArgs e)
        {
            if (null != objReader)
            {
                ResetTagInspectorTab();
            }
        }

        /// <summary>
        /// Replace all the special characters in the ascii string with .[DOT]
        /// </summary>
        /// <param name="asciiData"></param>
        /// <returns></returns>
        private string ReplaceSpecialCharInAsciiData(char[] asciiData)
        {
            string replacedData = string.Empty;
            foreach (char character in asciiData)
            {
                if (char.IsControl(character))
                {
                    replacedData += ".";
                }
                else
                {
                    replacedData += character;
                }
            }
            return replacedData;
        }

        /// <summary>
        /// Reset taginspector tab to default values
        /// </summary>
        public void ResetTagInspectorTab()
        {
            if (null != objReader)
            {
                //set the default values for TagInspector tab
                lblSelectFilter.Content = "Showing tag:";
                txtAcessPassword.Text = "";
                txtKillPassword.Text = "";
                txtEpc.Text = "";
                txtCRC.Text = "";
                txtPC.Text = "";
                lblTagInspectorError.Content = "";
                lblTagInspectorError.Visibility = System.Windows.Visibility.Collapsed;
                txtEPCData.Text = "";
                txtEPCValue.Text = "";

                // Hide XPC bytes textbox
                spXPC.Visibility = System.Windows.Visibility.Collapsed;
                spXPC2.Visibility = System.Windows.Visibility.Collapsed;
                lblXPC1.Content = "XPC";
                txtXPC1.Text = "";
                txtXPC2.Text = "";

                //Hide additional memory
                spAddMemory.Visibility = System.Windows.Visibility.Collapsed;
                txtAdditionalMem.Text = "";
                txtadditionalMemValue.Text = "";

                // Hide reserved additional memory textboxes
                lblAdditionalReservedMemAdd.Visibility = System.Windows.Visibility.Collapsed;
                lblAdditionalReservedMem.Visibility = System.Windows.Visibility.Collapsed;
                txtReservedMemUnusedValue.Visibility = System.Windows.Visibility.Collapsed;
                txtReservedMemUnusedValue.Text = "";

                // Hide epc additional memory textboxes
                spUnused.Visibility = System.Windows.Visibility.Collapsed;
                txtEPCUnused.Text = "";
                txtEPCUnusedValue.Text = "";

                txtClsID.Text = "";
                txtVendorID.Text = "";
                txtVendorValue.Text = "";
                txtModelID.Text = "";
                txtModeldIDValue.Text = "";
                txtUniqueIDValue.Text = "";

                txtUserMemData.Text = "";
                txtUserDataValue.Text = "";
                btnRead.Content = "Read";
                rbSelectedTagIns.IsEnabled = false;
                rbFirstTagIns.IsChecked = true;
                rbFirstTagIns.IsEnabled = true;
                rbEPCAscii.IsChecked = true;
                rbEPCAscii.IsEnabled = true;
                rbEPCBase36.IsEnabled = true;

                rbtngen2.Visibility = Visibility.Visible;
                CheckBox cbxata = (CheckBox)App.Current.MainWindow.FindName("ataCheckBox");
                if (cbxata.Visibility != Visibility.Visible)
                    rbtnata.Visibility = Visibility.Collapsed;
                else
                    rbtnata.Visibility = Visibility.Visible;

                lblSelectFilter.Content = "Showing tag:";

                gridChassis.Visibility = gridTrailer.Visibility = gridRailcarCover.Visibility = gridIntermodal.Visibility = gridEndofTrain.Visibility = gridRailcarLocomotiveMultiModalPassiveAlarm.Visibility = gridGeneratorSet.Visibility = gridErrorMessage.Visibility = Visibility.Collapsed;
                txtATAEPCData.Text = "";
                txtTagType.Text = "";
                txtDataFormat.Text = "";
                txtEquipmentGroup.Text = "";
                txtEquipmentInitial.Text = "";
                tblASCIIFormat.Text = "";
                tblASCIIBinaryFormat.Text = "";
            }
        }

        private void rbtngen2_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                //rbFirstTagIns.IsChecked = true;
                if (!(bool)rbFirstTagIns.IsChecked)
                {
                    if (tempSelectedTag.Protocol == TagProtocol.ATA)
                    {
                        if (!(bool)rbtnata.IsChecked)
                            rbFirstTagIns.IsChecked = true;
                    }
                    else if (tempSelectedTag.Protocol == TagProtocol.GEN2)
                    {
                        if (!(bool)rbtngen2.IsChecked)
                            rbFirstTagIns.IsChecked = true;
                    }
                }
                else if ((bool)rbFirstTagIns.IsChecked)
                {
                    if (lblSelectFilter.Content.ToString() != "Showing tag:")
                        ResetTagInspectorTab();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Universal Reader Assistant", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string BinaryStringToHexString(string binary)
        {
            StringBuilder result = new StringBuilder(binary.Length / 8 + 1);
            int mod4Len = binary.Length % 8;
            if (mod4Len != 0)
            {
                // pad to length multiple of 8
                binary = binary.PadLeft(((binary.Length / 8) + 1) * 8, '0');
            }

            for (int i = 0; i < binary.Length; i += 1)
            {
                string eightBits = binary.Substring(i, 1);
                result.Append(Convert.ToString(Convert.ToInt64(eightBits, 16), 2).PadLeft(4, '0'));
            }

            return result.ToString();
        }
    }
}
