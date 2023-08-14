////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// common methods are used by the the "smartcard" sdk samples
////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Text;

namespace CaasId.src.Domain.Core.SDKLibs
{
    public class SmartcardCommon
    {

        /// <summary>
        /// optional routine
        /// </summary>
        /// <param name="protocol"></param>
        public static void DisplayProtocols(uint protocol)
        {
            var protocols = SCard.StringsFromProtocol(protocol);
            Console.Write("   protocol[s]: ");
            for (int index = 0; index < protocols.Length; index++)
            {
                Console.Write(protocols[index] + " ");
            }
            Console.WriteLine();
        }

        public static void DisplayByteVectorAsHex(string title, byte[] receivedBytes)
        {
            Console.Write("{0}:", title);
            for (int index = 0; index < receivedBytes.Length; index++)
            {
                //std::cout << std::setw(2) << std::setfill('0') << std::uppercase << std::hex << int(bytes[index]) << " ";
                Console.Write("{0:X02} ", receivedBytes[index]);
            }
            Console.WriteLine();
        }

        public static void ParkCard(BidiSplWrap bidiSpl, bool parkBack)
        {
            var parkCommand = parkBack ? strings.SMARTCARD_PARK_BACK : strings.SMARTCARD_PARK;
            string printerStatusXML = bidiSpl.SetPrinterData(parkCommand);
            PrinterStatusValues printerStatusValues = Util.ParsePrinterStatusXML(printerStatusXML);
            if (0 != printerStatusValues._errorCode)
            {
                throw new Exception("SmartcardPark(): " + printerStatusValues._errorString);
            }
        }


        public static void ResumeCard(BidiSplWrap bidiSpl, int printerJobID, int errorCode)
        {
            string xmlFormat = strings.PRINTER_ACTION_XML;
            string input = string.Format(xmlFormat, (int)Actions.Resume, printerJobID, errorCode);
            Console.WriteLine("issuing Resume after smartcard:");
            bidiSpl.SetPrinterData(strings.PRINTER_ACTION, input);
        }



        ////////////////////////////////////////////////////////////////////////////////
        // SCardResultMessage()
        // format a message for display.
        // SCARD errors are declared in WinError.H
        ////////////////////////////////////////////////////////////////////////////////
        public static string SCardResultMessage(long scardResult, string message)
        {
            var sb = new StringBuilder();
            sb.Append(message);
            sb.Append(" result: ");
            sb.Append(scardResult.ToString());
            sb.Append("; ");
            sb.Append(Util.Win32ErrorString((int)scardResult));
            return sb.ToString();
        }


        /// <summary>
        /// issue the SCardStatus() call. this is important, because the SCardConnect()
        /// call always succeeds - as long as the card is staged.
        ///
        /// the SCardConnect() call returns the Answer-To-Reset bytes (ATR).
        /// </summary>
        public static void GetSCardStatus(SCard scard)
        {
            Console.WriteLine();
            Console.WriteLine("GetSCardStatus()");

            var states = new int[0];
            var protocol = (uint)scard_protocol.SCARD_PROTOCOL_UNDEFINED;
            var ATRBytes = new byte[0];

            var scardResult = scard.SCardStatus(ref states, ref protocol, ref ATRBytes);

            Console.WriteLine("SCardStatus result: {0}: {1}", scardResult, Util.Win32ErrorString((int)scardResult));
            if (scard_error.SCARD_S_SUCCESS != (scard_error)scardResult)
            {
                var msg = SCardResultMessage(scardResult, "");
                throw new Exception("SCardStatus() fail: " + msg);
            }

            // display all the 'states' returned. if ANY of the states is SCARD_ABSENT -
            // we are done with this card.
            bool cardAbsent = false;
            Console.Write("SCardStatus() states: ");
            for (var index = 0; index < states.Length; index++)
            {
                Console.Write(states[index]);
                Console.Write(" ");
                Console.Write(SCard.StringFromState(states[index]));
                Console.Write(" ");

                if (scard_state.SCARD_ABSENT == (scard_state)states[index])
                    cardAbsent = true;
            }
            Console.WriteLine();

            if (cardAbsent)
            {
                throw new Exception("one of the states is SCARD_ABSENT.");
            }

            Console.Write("SCardStatus() ATR: ");
            Console.WriteLine(Bytes_to_HEX_String(ATRBytes));
            Console.WriteLine();
        }



        public static string Bytes_to_HEX_String(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                sb.Append(string.Format("{0:X02} ", b));
            }
            return sb.ToString();
        }


        ////////////////////////////////////////////////////////////////////////////////
        // ReadVendorInfo()
        //
        ////////////////////////////////////////////////////////////////////////////////
        public static bool ReadVendorInfo(SCard scard)
        {

            var attributesToTry = new scard_attr[] {
                scard_attr.SCARD_ATTR_VENDOR_NAME,
                scard_attr.SCARD_ATTR_VENDOR_IFD_SERIAL_NO,
                scard_attr.SCARD_ATTR_VENDOR_IFD_TYPE,
                scard_attr.SCARD_ATTR_VENDOR_IFD_VERSION
            };

            for (var attribIndex = 0; attribIndex < attributesToTry.Length; attribIndex++)
            {
                byte[] scardAttributeBytes = new byte[0];

                var scardResult = scard.SCardGetAttrib(attributesToTry[attribIndex], ref scardAttributeBytes);

                if (scard_error.SCARD_S_SUCCESS != (scard_error)scardResult)
                    continue;

                string msg = string.Format("SCardGetAttrib({0})", SCard.StringFromAttr((int)attributesToTry[attribIndex]));
                Console.WriteLine(SCardResultMessage(scardResult, msg));

                switch (attributesToTry[attribIndex])
                {
                    case scard_attr.SCARD_ATTR_VENDOR_IFD_VERSION:
                        // vendor-supplied interface device version: DWORD in the form 0xMMmmbbbb where
                        //      MM = major version;
                        //      mm = minor version; and
                        //      bbbb = build number:
                        var byteCount = scardAttributeBytes.Length;
                        var minorVersion = byteCount > 0 ? scardAttributeBytes[0] : 0;
                        var majorVersion = byteCount > 1 ? scardAttributeBytes[1] : 0;
                        var buildNumber = 0;
                        if (byteCount > 3)
                        {
                            buildNumber = (scardAttributeBytes[2] << 8) + scardAttributeBytes[3];
                        }

                        Console.WriteLine("  SCARD_ATTR_VENDOR_IFD_VERSION:");
                        Console.Write("    major: " + majorVersion.ToString());
                        Console.Write("    minor: " + minorVersion.ToString());
                        Console.WriteLine("    build: " + buildNumber.ToString());
                        break;

                    default:
                        // null-terminate the returned byte for string display.
                        var bytesList = new List<byte>(scardAttributeBytes);
                        bytesList.Add(0);
                        scardAttributeBytes = bytesList.ToArray();
                        Console.WriteLine(Encoding.UTF8.GetString(scardAttributeBytes));
                        break;
                }
            }

            return true;
        }
    }
} // end of namespace