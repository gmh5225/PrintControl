////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// c# wrapper class for IBidiSpl COM interfaces.
////////////////////////////////////////////////////////////////////////////////
using System;
using System.Runtime.InteropServices;

namespace CaasId.src.Domain.Core.SDKLibs
{

    public class BidiSplWrap
    {

        // static extern methods for ibidispl through interop
        [ComImport, Guid("B9162A23-45F9-47cc-80F5-FE0FE9B9E1A2")]
        public class BidiRequest { }

        [ComImport, Guid("FC5B8A24-DB05-4a01-8388-22EDF6C2BBBA")]
        public class BidiRequestContainer { }

        [ComImport, Guid("2A614240-A4C5-4c33-BD87-1BC709331639")]
        public class BidiSpl { }

        [Guid("D580DC0E-DE39-4649-BAA8-BF0B85A03A97")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IBidiSpl
        {

            int BindDevice([In] string prnName, [In] int access);

            int UnbindDevice();

            int SendRecv([In] string action, [In, MarshalAs(UnmanagedType.Interface)] IBidiRequest req);

            int MultiSendRecv([In] string action, [In, MarshalAs(UnmanagedType.Interface)] IBidiRequestContainer reqCont);
        }

        [Guid("8F348BD7-4B47-4755-8A9D-0F422DF3DC89")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IBidiRequest
        {

            int SetSchema([In] string schema);

            int SetInputData([In] uint type, [In] nint pData, [In] uint size);

            int GetResult([Out] nint pHRes);

            int GetOutputData(
                [In] int index,
                [Out] string schema,
                [Out] nint type,
                [Out] nint ppData,
                [Out] nint size);

            int GetEnumCount([Out] nint pTotal);
        }

        [Guid("D752F6C0-94A8-4275-A77D-8F1D1A1121AE")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IBidiRequestContainer
        {

            int AddRequest([In, MarshalAs(UnmanagedType.Interface)] IBidiRequest req);

            int GetEnumObject([Out, MarshalAs(UnmanagedType.Interface)] IEnumUnknown enumBidiReqCont);

            int GetRequestCount([Out] uint count);
        }

        [Guid("00000100-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IEnumUnknown
        {

            int Next([In, MarshalAs(UnmanagedType.U4)] uint celt, [Out, MarshalAs(UnmanagedType.U4)] nint rgelt,
                [Out, MarshalAs(UnmanagedType.U4)] uint pceltFetched);

            int Skip([In, MarshalAs(UnmanagedType.U4)] uint celt);

            int Reset();

            int Clone([Out, MarshalAs(UnmanagedType.LPStruct)] IEnumUnknown enumUnk);
        }

        // IBidiSpl strings:
        private const string BIDI_ACTION_GET = "Get";

        private const string BIDI_ACTION_SET = "Set";

        private const int BIDI_ACCESS_ADMINISTRATOR = 0x1;
        private const int BIDI_ACCESS_USER = 0x2;

        public enum BIDI_TYPE
        {
            BIDI_NULL = 0,
            BIDI_INT = 1,
            BIDI_FLOAT = 2,
            BIDI_BOOL = 3,
            BIDI_STRING = 4,
            BIDI_TEXT = 5,
            BIDI_ENUM = 6,
            BIDI_BLOB = 7
        }

        // retain this COM pointer instance:
        private IBidiSpl _iBidiSpl = null;

        public void BindDevice(string printerName)
        {
            try
            {
                BidiSpl bidi = (BidiSpl)Activator.CreateInstance(typeof(BidiSpl), true);
                _iBidiSpl = (IBidiSpl)bidi;
                _iBidiSpl.BindDevice(printerName, BIDI_ACCESS_USER);
            }
            catch (SystemException e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(-1);
            }
        }

        public void UnbindDevice()
        {
            _iBidiSpl.UnbindDevice();
        }

        public string SendRecv(string action, string actionType, string data)
        {
            nint dataPointer = default;
            nint dataTypePointer = default;
            nint sizePointer = default;
            nint pointerToDataPointer = default;
            nint tempResultPointer = default;
            nint countPointer = default;
            nint resultPointer = default;

            string xml = string.Empty;

            try
            {
                BidiRequest bidiRequest = (BidiRequest)Activator.CreateInstance(typeof(BidiRequest), true);
                IBidiRequest iBidiRequest = (IBidiRequest)bidiRequest;

                iBidiRequest.SetSchema(action);

                // Set the input data for the request, if any
                if (data.Length != 0)
                {
                    dataPointer = Marshal.AllocCoTaskMem(data.Length * sizeof(char));
                    var chars = data.ToCharArray();
                    Marshal.Copy(chars, 0, dataPointer, data.Length);
                    iBidiRequest.SetInputData((uint)BIDI_TYPE.BIDI_BLOB, dataPointer, (uint)data.Length * sizeof(char));
                }

                // Send request to driver
                var val = _iBidiSpl.SendRecv(actionType, iBidiRequest);

                // Check if request was a success
                resultPointer = Marshal.AllocCoTaskMem(sizeof(int));

                // Call the method
                iBidiRequest.GetResult(resultPointer);

                // Get the value
                int result = (int)Marshal.PtrToStructure(resultPointer, typeof(int));

                const int resultSuccess = 0;
                if (result == resultSuccess)
                {
                    // Check if any data was returned. Note: dxp01sdk.strings.ENDJOB
                    // and dxp01sdk.strings.PRINTER_ACTION do not return any values.

                    // First allocate memory
                    countPointer = Marshal.AllocCoTaskMem(sizeof(int));

                    // Call the method
                    iBidiRequest.GetEnumCount(countPointer);

                    // Get the value
                    int count = (int)Marshal.PtrToStructure(countPointer, typeof(int));
                    if (count != 0)
                    {
                        // Driver sent some data. Now retrieve the data sent by the driver

                        // First allocate memory for type and size
                        dataTypePointer = Marshal.AllocCoTaskMem(sizeof(int));
                        sizePointer = Marshal.AllocCoTaskMem(sizeof(int));

                        // Now allocate memory for data. Also add level of indirection.
                        nint ptrTemp = default;
                        pointerToDataPointer = Marshal.AllocCoTaskMem(sizeof(int));
                        Marshal.StructureToPtr(ptrTemp, pointerToDataPointer, false);

                        // Finally, retrieve the data sent by the driver
                        string schema = string.Empty;
                        iBidiRequest.GetOutputData(0, schema, dataTypePointer, pointerToDataPointer, sizePointer);

                        // Get the size of the data in bytes returned from SendRecv method.
                        int size = (int)Marshal.PtrToStructure(sizePointer, typeof(int));

                        // Get the data itself. First remove level of indirection
                        tempResultPointer = Marshal.ReadIntPtr(pointerToDataPointer);

                        // Get Unicode string from pointer. Unicode characters are 16-bit characters.
                        // size contains the number of bytes returned by SendRecv method
                        xml = Marshal.PtrToStringUni(tempResultPointer, size / sizeof(char));
                    }
                }
            }
            finally
            {
                Marshal.FreeCoTaskMem(dataPointer);
                Marshal.FreeCoTaskMem(sizePointer);
                Marshal.FreeCoTaskMem(dataTypePointer);
                Marshal.FreeCoTaskMem(pointerToDataPointer);
                Marshal.FreeCoTaskMem(tempResultPointer);
                Marshal.FreeCoTaskMem(countPointer);
                Marshal.FreeCoTaskMem(resultPointer);
            }
            return xml;
        }
        public string SendRecv(string action, string actionType, byte[] data)
        {
            nint dataPointer = default;
            nint dataTypePointer = default;
            nint sizePointer = default;
            nint pointerToDataPointer = default;
            nint tempResultPointer = default;
            nint countPointer = default;
            nint resultPointer = default;

            string xml = string.Empty;

            try
            {
                BidiRequest bidiRequest = (BidiRequest)Activator.CreateInstance(typeof(BidiRequest), true);
                IBidiRequest iBidiRequest = (IBidiRequest)bidiRequest;

                iBidiRequest.SetSchema(action);

                // Set the input data for the request, if any
                if (data.Length != 0)
                {
                    int size = Marshal.SizeOf(data[0]) * data.Length;
                    dataPointer = Marshal.AllocCoTaskMem(size);
                    Marshal.Copy(data, 0, dataPointer, data.Length);
                    iBidiRequest.SetInputData((uint)BIDI_TYPE.BIDI_BLOB, dataPointer, (uint)size);
                }

                // Send request to driver
                var val = _iBidiSpl.SendRecv(actionType, iBidiRequest);

                // Check if request was a success
                resultPointer = Marshal.AllocCoTaskMem(sizeof(int));

                // Call the method
                iBidiRequest.GetResult(resultPointer);

                // Get the value
                int result = (int)Marshal.PtrToStructure(resultPointer, typeof(int));

                const int resultSuccess = 0;
                if (result == resultSuccess)
                {
                    // Check if any data was returned. Note: dxp01sdk.strings.ENDJOB
                    // and dxp01sdk.strings.PRINTER_ACTION do not return any values.

                    // First allocate memory
                    countPointer = Marshal.AllocCoTaskMem(sizeof(int));

                    // Call the method
                    iBidiRequest.GetEnumCount(countPointer);

                    // Get the value
                    int count = (int)Marshal.PtrToStructure(countPointer, typeof(int));
                    if (count != 0)
                    {
                        // Driver sent some data. Now retrieve the data sent by the driver

                        // First allocate memory for type and size
                        dataTypePointer = Marshal.AllocCoTaskMem(sizeof(int));
                        sizePointer = Marshal.AllocCoTaskMem(sizeof(int));

                        // Now allocate memory for data. Also add level of indirection.
                        nint ptrTemp = default;
                        pointerToDataPointer = Marshal.AllocCoTaskMem(sizeof(int));
                        Marshal.StructureToPtr(ptrTemp, pointerToDataPointer, false);

                        // Finally, retrieve the data sent by the driver
                        string schema = string.Empty;
                        iBidiRequest.GetOutputData(0, schema, dataTypePointer, pointerToDataPointer, sizePointer);

                        // Get the size of the data in bytes returned from SendRecv method.
                        int size = (int)Marshal.PtrToStructure(sizePointer, typeof(int));

                        // Get the data itself. First remove level of indirection
                        tempResultPointer = Marshal.ReadIntPtr(pointerToDataPointer);

                        // Get Unicode string from pointer. Unicode characters are 16-bit characters. 
                        // size contains the number of bytes returned by SendRecv method
                        xml = Marshal.PtrToStringUni(tempResultPointer, size / sizeof(char));
                    }   // if count != 0
                }  // if (result == resultSuccess)
            }
            finally
            {
                Marshal.FreeCoTaskMem(dataPointer);
                Marshal.FreeCoTaskMem(sizePointer);
                Marshal.FreeCoTaskMem(dataTypePointer);
                Marshal.FreeCoTaskMem(pointerToDataPointer);
                Marshal.FreeCoTaskMem(tempResultPointer);
                Marshal.FreeCoTaskMem(countPointer);
                Marshal.FreeCoTaskMem(resultPointer);
            }
            return xml;
        }  // SendRecv

        /// <summary>
        /// Use the method to read data from the printer. The action parameter
        /// specifies the data to be read from the printer. This method returns
        /// the xml data back.
        ///
        /// Use the method below for reading magstripe data
        ///     (dxp01sdk.strings.MAGSTRIPE_READ),
        /// getting printer messages
        ///     (dxp01sdk.strings.PRINTER_MESSAGES) and
        /// for getting printer supplies status
        ///     (dxp01sdk.strings.PRINTER_OPTIONS2)
        ///
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public string GetPrinterData(string action)
        {
            string result = SendRecv(action, BIDI_ACTION_GET, "");
            return result;
        }

        /// <summary>
        /// Use the method to read data from the printer. The action parameter specifies
        /// the data to be read from the printer.  The user also has to specify the
        /// input data that is required.
        /// This method returns the xml data back.
        ///
        /// Use the method below for getting job status for a given
        /// printer job id (dxp01sdk.strings.JOB_STATUS)
        /// </summary>
        /// <param name="iSpl"></param>
        /// <param name="action"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public string GetPrinterData(string action, string input)
        {
            return SendRecv(action, BIDI_ACTION_GET, input);
        }

        public string GetPrinterData(string action, byte[] input)
        {
            return SendRecv(action, BIDI_ACTION_GET, input);
        }

        /// <summary>
        /// Use the method to send data to printer. The user has to specify the action
        /// it wants to take in action parameter.
        /// This method returns the xml data back.
        ///
        /// Use the method below for starting a job (dxp01sdk.strings.STARTJOB),
        /// ending a job (dxp01sdk.strings.ENDJOB) and for parking the card
        /// at smartcard station (dxp01sdk.strings.SMARTCARD_PARK)
        /// </summary>
        /// <param name="action"></param>
        /// <returns>an xml 'printerstatus' response like this:
        ///
        ///     <!-- Printer status xml file.  -->
        ///     <PrinterStatus>
        ///       <ClientID>myMachineName_{901F168C-B593-42B8-8602-4C715D8D658C}</ClientID>
        ///       <WindowsJobID>0</WindowsJobID>
        ///       <PrinterJobID>9219</PrinterJobID>
        ///       <ErrorCode>112</ErrorCode>
        ///       <ErrorSeverity>4</ErrorSeverity>
        ///       <ErrorString>Message 112: Card hopper empty.</ErrorString>
        ///       <DataFromPrinter>
        ///       <![CDATA[     ]]>
        ///       </DataFromPrinter
        ///     </PrinterStatus>
        ///
        ///</returns>
        public string SetPrinterData(string action)
        {
            string result = SendRecv(action, BIDI_ACTION_SET, "");
            return result;
        }

        /// <summary>
        /// Use the method to send data to printer. The user has to specify the action
        /// it wants to take in action parameter. The also user has to specify the data
        /// it wants to send in input parameter.
        /// This method returns the xml data back.
        ///
        /// Use the method below for encoding magstripe(dxp01sdk.strings.MAGSTRIPE_ENCODE)
        /// and for sending actions to printer like resume, cancel or
        /// restart (dxp01sdk.strings.PRINTER_ACTION)
        /// </summary>
        /// <param name="action"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public string SetPrinterData(string action, string input)
        {
            string result = SendRecv(action, BIDI_ACTION_SET, input);
            return result;
        }

        public string SetPrinterData(string action, byte[] input)
        {
            string result = SendRecv(action, BIDI_ACTION_SET, input);
            return result;
        }

    }
}