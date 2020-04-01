using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using BEL = BusinessEntityLayer;
using BWS = BusinessWindowsServiceLayer;

namespace ContentMigration
{
    public class Program
    {
        static BEL.Credentials _credentialsObj = new BEL.Credentials();
        BEL.GlobalParameters _globalParametersObj;
        BWS.LogProfileService _logProfileServiceObj;
        BWS.AuthenticationService _authenticationServiceObj;
        BWS.CheckDriveFreeSpaceService _checkDriveFreeSpaceServiceObj;
        BWS.FileOperationService _fileOperationServiceObj;
        BWS.FileTransferService _fileTransferServiceObj;
        Program()
        {
            _globalParametersObj = new BEL.GlobalParameters();
            _logProfileServiceObj = new BWS.LogProfileService();
            _authenticationServiceObj = new BWS.AuthenticationService();
            _checkDriveFreeSpaceServiceObj = new BWS.CheckDriveFreeSpaceService();
            _fileOperationServiceObj = new BWS.FileOperationService();
            _fileTransferServiceObj = new BWS.FileTransferService();
        }
        static void Main(string[] args)
        {
            Program programObj = new Program();
            programObj.InitializeValues();
            programObj.CreateLogEntry();
            GetLoginCredentials();
            programObj.FileSegregation();
        }

        void InitializeValues()
        {
            //Reading Config.xml file
            _globalParametersObj.dateTime = "{" + DateTime.Now.ToString("dd.MM.yyyy-hh.mm.ss") + "}";
            _globalParametersObj.currentPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            _globalParametersObj.doc.Load(_globalParametersObj.currentPath + @"\Configurable\Config.xml");
            _globalParametersObj.root = _globalParametersObj.doc.DocumentElement;

            //Accessing values from Config.xml file
            _globalParametersObj.successDirectory = @"" + _globalParametersObj.root.SelectSingleNode("LogDirectory").InnerText + _globalParametersObj.dateTime + _globalParametersObj.root.SelectSingleNode("SuccessVariable").InnerText;
            _globalParametersObj.failureDirectory = @"" + _globalParametersObj.root.SelectSingleNode("LogDirectory").InnerText + _globalParametersObj.dateTime + _globalParametersObj.root.SelectSingleNode("FailureVariable").InnerText;
            _globalParametersObj.systemDirectory = @"" + _globalParametersObj.root.SelectSingleNode("LogDirectory").InnerText + _globalParametersObj.dateTime + _globalParametersObj.root.SelectSingleNode("SystemVariable").InnerText;

            //Generating the GUID for the purpose of Log Folder Creation
            _globalParametersObj.guidObj = Guid.NewGuid();
            _globalParametersObj.hostName = Dns.GetHostName(); // Retrive the Name of Host
            _globalParametersObj.ipAddress = Dns.GetHostEntry(_globalParametersObj.hostName).AddressList[0].ToString();

            Console.WriteLine("Please enter the SourcePath :");
            _globalParametersObj.sourcePathText = Console.ReadLine();
            Console.WriteLine("Please enter the TargetPath :");
            _globalParametersObj.targetPathText = Console.ReadLine();
        }
        void CreateLogEntry()
        {
            _logProfileServiceObj.CreateLogFiles(_globalParametersObj);
        }
        static void GetLoginCredentials()
        {
            Program program = new Program();
            Console.WriteLine("Please enter the UserName :");
            _credentialsObj.userID = Console.ReadLine();
            Console.WriteLine("Please enter the Password :");
            _credentialsObj.password = Console.ReadLine();
            bool Authentication = program.ValidateCredentials(_credentialsObj);
            if (Authentication)
            {
                Console.WriteLine("Authentication established");
            }
            else
            {
                Console.WriteLine("Authentication failed");
            }
            Console.WriteLine("Continue.");
        }
        bool ValidateCredentials(BEL.Credentials credentials)
        {
            return _authenticationServiceObj.CheckAuthentication(credentials);
        }
        void FileSegregation()
        {
            _fileOperationServiceObj.AddFilesintoList(_globalParametersObj);
        }
        void CheckSourceDriveFreeSpace()
        {
            if (_checkDriveFreeSpaceServiceObj.sourceDriveFreeSpace(_globalParametersObj))
            {
                CheckDestinationDriveFreeSpace();
            }
        }
        void CheckDestinationDriveFreeSpace()
        {
            if (_checkDriveFreeSpaceServiceObj.destinationDriveFreeSpace(_globalParametersObj))
            {
                FileDataintoXML();
                FileTransfer(_globalParametersObj.currentDirectory + "//" + _globalParametersObj.root.SelectSingleNode("XMLFileName").InnerText);
            }
        }
        void FileDataintoXML()
        {
            _fileOperationServiceObj.FileDataintoXML(_globalParametersObj);
        }
        bool FileTransfer(string xmlFile)
        {
            return _fileTransferServiceObj.FileTransfer(xmlFile, _globalParametersObj);
        }
    }
}
