using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumdFTPClient
{
    class Program
    {
        static void Main()
        {
            JumdFTPService jumdFTPServiceObj = new JumdFTPService("ftp://localhost", "PUBG", "1", "G:\\ftp_local");


            List<Tuple<string, bool>> itemListFTP = jumdFTPServiceObj.GetListofItemsInFtpFolder("ftp://localhost");

            foreach (Tuple<string, bool> eachItem in itemListFTP)
            {
                Console.WriteLine(eachItem);
            }


            //jumdFTPServiceObj.PullItemsToLocal(itemListFTP, "G:\\ftp_local");

            List<Tuple<string, bool>> itemListLocal = jumdFTPServiceObj.GetListofItemsInLocalFolder("G:\\ftp_local");
            //jumdFTPServiceObj.PushItemsToServer(itemListLocal, "ftp://localhost");

            //jumdFTPServiceObj.CreateFolderInFTP("abc");
            //jumdFTPServiceObj.CreateFileFolder("abc.txt", true);

            jumdFTPServiceObj.DeleteItemsInFTP(itemListFTP);



        }
    }
}
