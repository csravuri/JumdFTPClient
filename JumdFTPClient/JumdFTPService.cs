using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JumdFTPClient
{
    internal class JumdFTPService
    {
        private string FTPAddress;
        private string FTPUserName;
        private string FTPPassword;
        private string currentFtpDirectory;
        private string currentLocalDirectory;

        internal JumdFTPService(string address, string userName, string password, string localPath)
        {
            this.FTPAddress = address;
            this.FTPUserName = userName;
            this.FTPPassword = password;
            this.currentFtpDirectory = address;
            this.currentLocalDirectory = localPath;
        }

        internal void EstablishFTPConnection()
        {

        }

        /// <summary>
        /// List of files and folders
        /// </summary>
        /// <returns>List<FileName, isFolder></returns>
        internal List<Tuple<string, bool>> GetListofItemsInFtpFolder(string fullFtpFolderPath)
        {
            FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create(fullFtpFolderPath);

            ftpWebRequest.UseBinary = true;
            ftpWebRequest.Credentials = new NetworkCredential(FTPUserName, FTPPassword);


            ftpWebRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;


            FtpWebResponse ftpWebResponse = (FtpWebResponse)ftpWebRequest.GetResponse();


            Stream stream = ftpWebResponse.GetResponseStream();
            StreamReader streamReader = new StreamReader(stream);

            string line = streamReader.ReadLine();
            List<Tuple<string, bool>> listItems = new List<Tuple<string, bool>>();

            while (line != null)
            {
                listItems.Add(GetFileFolderProperties(line));
                line = streamReader.ReadLine();
            }

            streamReader.Close();
            stream.Close();

            return listItems;

        }

        internal List<Tuple<string, bool>> GetListofItemsInLocalFolder(string fullLocalFolderPath)
        {
            List<string> fileList = Directory.GetFiles(fullLocalFolderPath).ToList();
            List<string> folderList = Directory.GetDirectories(fullLocalFolderPath).ToList();

            List<Tuple<string, bool>> allItems = new List<Tuple<string, bool>>();
            allItems.AddRange(fileList.Select(x => Tuple.Create(x, false)));
            allItems.AddRange(folderList.Select(x => Tuple.Create(x, true)));

            return allItems;
        }
        private Tuple<string, bool> GetFileFolderProperties(string fileName)
        {
            bool isFolder = fileName.Contains("<DIR>");
            if (isFolder)
            {
                int indx = fileName.IndexOf("<DIR>") + 5;
                fileName = fileName.Substring(indx).Trim();
            }
            else
            {
                int indx = fileName.IndexOf(':');
                fileName = fileName.Substring(indx);
                for (int i = 0; i < 3; i++)
                {
                    indx = fileName.IndexOf(' ');
                    fileName = fileName.Substring(indx);
                }
                fileName = fileName.Trim();
                indx = fileName.IndexOf(' ');
                fileName = fileName.Substring(indx).Trim();

            }

            return Tuple.Create(fileName, isFolder);

        }

        internal void PullItemsToLocal(List<Tuple<string, bool>> itemsList, string localPath)
        {
            foreach (Tuple<string, bool> eachFile in itemsList)
            {
                PullItemToLocal(eachFile, localPath);
            }

        }

        private void PullItemToLocal(Tuple<string, bool> file, string localpath)
        {

            Uri serverUri = new Uri($"{currentFtpDirectory}/{file.Item1}");

            if (serverUri.Scheme != Uri.UriSchemeFtp)
            {
                return;
            }

            if (file.Item2)
            {
                string tempCurrentDir = currentFtpDirectory;
                string tempLocalDir = currentLocalDirectory;

                currentFtpDirectory += $"/{file.Item1}";
                currentLocalDirectory += $"\\{file.Item1}";

                Directory.CreateDirectory($"{localpath}\\{file.Item1}");
                PullItemsToLocal(GetListofItemsInFtpFolder(currentFtpDirectory), currentLocalDirectory);

                currentFtpDirectory = tempCurrentDir;
                currentLocalDirectory = tempLocalDir;
            }
            else
            {
                FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create($"{currentFtpDirectory}/{file.Item1}");

                ftpWebRequest.UseBinary = true;
                ftpWebRequest.Credentials = new NetworkCredential(FTPUserName, FTPPassword);

                ftpWebRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                FtpWebResponse ftpWebResponse = (FtpWebResponse)ftpWebRequest.GetResponse();

                Stream downloadStream = ftpWebResponse.GetResponseStream();
                FileStream writeStream = new FileStream($"{localpath}\\{file.Item1}", FileMode.Create);

                int length = 2048;
                Byte[] buffer = new Byte[length];
                int byteRead = downloadStream.Read(buffer, 0, length);

                while (byteRead > 0)
                {
                    writeStream.Write(buffer, 0, length);
                    byteRead = downloadStream.Read(buffer, 0, length);
                }

                writeStream.Close();
                downloadStream.Close();
            }
        }

        internal void PushItemsToServer(List<Tuple<string, bool>> itemsList, string serverPath)
        {
            foreach (Tuple<string, bool> eachFile in itemsList)
            {
                PushItemToServer(eachFile, serverPath);
            }
        }

        internal void PushItemToServer(Tuple<string, bool> file, string serverPath)
        {

            Uri serverUri = new Uri($"{currentFtpDirectory}/{file.Item1}");

            if (serverUri.Scheme != Uri.UriSchemeFtp)
            {
                return;
            }

            if (file.Item2)
            {
                string tempCurrentDir = currentFtpDirectory;
                string tempLocalDir = currentLocalDirectory;

                currentFtpDirectory += $"/{Path.GetFileName(file.Item1)}";
                currentLocalDirectory += $"\\{Path.GetFileName(file.Item1)}";

                FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create(currentFtpDirectory);

                ftpWebRequest.UseBinary = true;
                ftpWebRequest.Credentials = new NetworkCredential(FTPUserName, FTPPassword);

                ftpWebRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
                ftpWebRequest.GetResponse();

                PushItemsToServer(GetListofItemsInLocalFolder(currentLocalDirectory), currentFtpDirectory);

                currentFtpDirectory = tempCurrentDir;
                currentLocalDirectory = tempLocalDir;
            }
            else
            {
                FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create($"{currentFtpDirectory}/{Path.GetFileName(file.Item1)}");

                ftpWebRequest.UseBinary = true;
                ftpWebRequest.Credentials = new NetworkCredential(FTPUserName, FTPPassword);

                ftpWebRequest.Method = WebRequestMethods.Ftp.UploadFile;

                FileStream writeStream = File.OpenRead(file.Item1);

                Stream uploadStream = ftpWebRequest.GetRequestStream();

                byte[] buffer = new byte[writeStream.Length];
                uploadStream.Write(buffer, 0, buffer.Length);
                uploadStream.Flush();
                writeStream.Close();
                uploadStream.Close();
            }
        }

        internal void CreateFolderInFTP(string itemName)
        {
            FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create($"{currentFtpDirectory}/{itemName}");

            ftpWebRequest.UseBinary = true;
            ftpWebRequest.Credentials = new NetworkCredential(FTPUserName, FTPPassword);

            ftpWebRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
            ftpWebRequest.GetResponse();
        }

        internal void CopyItemsInServer(List<string> itemsList, string sourcePath, string destinationPath)
        {

        }

        internal void MoveItemsInServer(List<string> itemsList, string sourcePath, string destinationPath)
        {

        }

        internal void DeleteItemInFTP(Tuple<string, bool> file)
        {
            if(file.Item2)
            {
                string tempCurrentDir = currentFtpDirectory;
                string tempLocalDir = currentLocalDirectory;

                currentFtpDirectory += $"/{Path.GetFileName(file.Item1)}";
                currentLocalDirectory += $"\\{Path.GetFileName(file.Item1)}";

                DeleteItemsInFTP(GetListofItemsInFtpFolder(currentFtpDirectory));

                currentFtpDirectory = tempCurrentDir;
                currentLocalDirectory = tempLocalDir;

                FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create($"{currentFtpDirectory}/{file.Item1}");

                ftpWebRequest.UseBinary = true;
                ftpWebRequest.Credentials = new NetworkCredential(FTPUserName, FTPPassword);

                ftpWebRequest.Method = WebRequestMethods.Ftp.RemoveDirectory;
                ftpWebRequest.GetResponse();

            }
            else
            {
                FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create($"{currentFtpDirectory}/{file.Item1}");

                ftpWebRequest.UseBinary = true;
                ftpWebRequest.Credentials = new NetworkCredential(FTPUserName, FTPPassword);

                ftpWebRequest.Method = WebRequestMethods.Ftp.DeleteFile;
                ftpWebRequest.GetResponse();
            }
        }
        internal void DeleteItemsInFTP(List<Tuple<string, bool>> itemsList)
        {
            foreach(Tuple<string, bool> eachItem in itemsList)
            {
                DeleteItemInFTP(eachItem);
            }
        }

    }
}
