using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestAzureStorage
{
    public class Program
    {
        static void Main(string[] args)
        {


            Console.WriteLine("Hello Azure Storage :) \r\n");

            Console.WriteLine("1) Blob - Create Blob Container");
            Console.WriteLine("2) Blob - Create Directory and Add File TEST/sampleText.jsonon");
            Console.WriteLine("3) Blob - Create Directory and Add File TEST2/TEST21/sampleText1~10.json");
            Console.WriteLine("4) Blob - Is File Existed");
            Console.WriteLine("5) Blob - Delete TEST2/TEST21/sampleText8.json");
            Console.WriteLine("6) Blob - GetFile Content TEST2/TEST21/sampleText9.json");

            Console.WriteLine("\r\nBlob Advanced ------\r\n");
            Console.WriteLine("7) Blob - Create File Read Permission Signature Uri");
            Console.WriteLine("8) Blob - Upload Image");
            Console.WriteLine("9) Blob - List All Files TEST2/TEST21/");
            Console.WriteLine("10) Blob - List All Directories TEST2/TEST21/");
            Console.WriteLine("11) Blob - Get File Info");

            Console.WriteLine("12) Blob - TEST2/TEST21/sampleText3.json create Snapshot");
            Console.WriteLine("12-1) Blob - TEST2/TEST21/sampleText3.json Modify Data");
            Console.WriteLine("12-2) Blob - TEST2/TEST21/sampleText3.json Recovery");
            Console.WriteLine("12-3) Blob - TEST2/TEST21/sampleText3.json Now Content");
            Console.WriteLine("e) Exit Test.");
            ShowMenu();

        }




        static void ShowMenu()
        {

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("");
            Console.Write("Choose Command \\>");

            var res = Console.ReadLine();

            switch (res.Trim())
            {
                case "1":
                    CreateBlobContainer();
                    CreateFileOnRoot();
                    break;
                case "2":
                    CreateBlobDirectory();
                    break;
                case "3":
                    CreateBlobDirectory2();
                    break;
                case "4":
                    CheckFileExisted();
                    break;
                case "5":
                    DeleteFile();
                    break;
                case "6":
                    ReadFileContent();
                    break;
                case "7":
                    GetFileUri();
                    break;
                case "8":
                    UploadImage();
                    break;
                case "9":
                    ListAllFilesFromDirectory();
                    break;
                case "10":
                    ListAllDirsFromDirectory();
                    break;
                case "11":
                    GetFileInfo();
                    break;
                case "12":
                    CreateSnapshot();
                    break;
                case "12-1":
                    ModifyDataJson3();
                    break;
                case "12-2":
                    RecovertDataJson3();
                    break;
                case "12-3":
                    CheckDataJson3Content();
                    break;
                case "e":
                    Environment.Exit(-1);
                    break;
                default:
                    Console.WriteLine("\r\nPlease Select Command..\r\n");
                    break;
            }


            ShowMenu();


        }

        public static string connsctionString = "your_connection_string";


        public static void CheckDataJson3Content()
        {
            var cloudStorage = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(connsctionString);

            var cloudBlobClient = cloudStorage.CreateCloudBlobClient();
            var cloudBlobContainer = cloudBlobClient.GetContainerReference("donmablogsample");
            var resultCreateContainer = cloudBlobContainer.CreateIfNotExistsAsync().Result;

            if (resultCreateContainer)
            {
                Console.WriteLine("donmablogsample create already.");
            }


            Microsoft.WindowsAzure.Storage.Blob.CloudBlobDirectory cloudBlobDirectory =
                cloudBlobContainer.GetDirectoryReference("TEST2/TEST21/");

            Console.WriteLine("Get File Content : TEST2/TEST21/sampleText3.json");

            var result = cloudBlobDirectory.GetBlockBlobReference("sampleText3.json").DownloadTextAsync().Result;
            Console.WriteLine("TEST2/TEST21/sampleText3.json content =>" + result);

            Console.WriteLine("IsSnapShot =>" + cloudBlobDirectory.GetBlockBlobReference("sampleText3.json").IsSnapshot);

        }

        public static void RecovertDataJson3()
        {
            var cloudStorage = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(connsctionString);

            var cloudBlobClient = cloudStorage.CreateCloudBlobClient();
            var cloudBlobContainer = cloudBlobClient.GetContainerReference("donmablogsample");

            //建立如果不存在的話
            var resultCreateContainer = cloudBlobContainer.CreateIfNotExistsAsync().Result;
            Console.WriteLine("donmablogsample create already.");
            Microsoft.WindowsAzure.Storage.Blob.CloudBlobDirectory cloudBlobDirectory =
               cloudBlobContainer.GetDirectoryReference("TEST2/TEST21/");
           
            var bbReference = cloudBlobDirectory.GetBlockBlobReference("sampleText3.json");


            var res = new List<CloudBlockBlob>();
            Microsoft.WindowsAzure.Storage.Blob.BlobContinuationToken continuationToken = null;
            do
            {
                //只看 TEST2/TEST21/sampleText3.json 的快照
                var listingResult = cloudBlobContainer.ListBlobsSegmentedAsync("TEST2/TEST21/sampleText3.json", true, Microsoft.WindowsAzure.Storage.Blob.BlobListingDetails.Snapshots, 100, continuationToken, null, new Microsoft.WindowsAzure.Storage.OperationContext
                {

                }).Result;
                continuationToken = listingResult.ContinuationToken;
                res.AddRange(listingResult.Results.Select(x => (x as Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob)).ToList());
            }
            while (continuationToken != null);
            
            foreach (var r in res)
            {
                r.FetchAttributesAsync().GetAwaiter().GetResult();
                Console.WriteLine("Meta-tag:" + (r.Metadata.ContainsKey("tag") ? (r.Metadata["tag"]) : "") + "," + r.DownloadTextAsync().Result + ";" + " IsSnapShot:" + r.IsSnapshot);

            }

            //Recovery from tag:9
            //還原 tag 9 的快照
            var bFileInfo = cloudBlobDirectory.GetBlockBlobReference("sampleText3.json");
            foreach (var r in res)
            {
                if (r.IsSnapshot && r.Metadata.ContainsKey("tag"))
                {
                    if (r.Metadata["tag"] == "9")
                    {
                        var copyResult = bFileInfo.StartCopyAsync(r).Result;

                        Console.WriteLine("Copy Result:" + copyResult);
                    }
                }
            }


        }


        public static void ModifyDataJson3()
        {
            var cloudStorage = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(connsctionString);

            var cloudBlobClient = cloudStorage.CreateCloudBlobClient();
            var cloudBlobContainer = cloudBlobClient.GetContainerReference("donmablogsample");
            var resultCreateContainer = cloudBlobContainer.CreateIfNotExistsAsync().Result;

            if (resultCreateContainer)
            {
                Console.WriteLine("donmablogsample create already.");
            }


            Microsoft.WindowsAzure.Storage.Blob.CloudBlobDirectory cloudBlobDirectory =
                cloudBlobContainer.GetDirectoryReference("TEST2/TEST21/");



            var bbReference = cloudBlobDirectory.GetBlockBlobReference("sampleText3.json");

            var bFileInfo = cloudBlobDirectory.GetBlockBlobReference("sampleText3.json");
            bFileInfo.Properties.ContentType = "application/json; charset=utf-8";
            bFileInfo.UploadTextAsync("{\"data\":\"測試資料快照後修改\"}").GetAwaiter().GetResult();


            Console.Write("Success Change Data.");

        }

        public static void CreateSnapshot()
        {
            var cloudStorage = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(connsctionString);

            var cloudBlobClient = cloudStorage.CreateCloudBlobClient();
            var cloudBlobContainer = cloudBlobClient.GetContainerReference("donmablogsample");
            var resultCreateContainer = cloudBlobContainer.CreateIfNotExistsAsync().Result;

            if (resultCreateContainer)
            {
                Console.WriteLine("donmablogsample create already.");
            }


            Microsoft.WindowsAzure.Storage.Blob.CloudBlobDirectory cloudBlobDirectory =
                cloudBlobContainer.GetDirectoryReference("TEST2/TEST21/");


            for (var i = 1; i <= 10; i++)
            {

                var bFileInfo = cloudBlobDirectory.GetBlockBlobReference("sampleText3.json");

                bFileInfo.Properties.ContentType = "application/json; charset=utf-8";
                bFileInfo.UploadTextAsync("{\"data\":\"測試資料修改" + i + "\"}").GetAwaiter().GetResult();
                var meta = new Dictionary<string, string>();
                meta.Add("tag", i.ToString());
                var result = bFileInfo.CreateSnapshotAsync(meta, null, null, null).Result;
            }

            Console.Write("Success");

        }

        public static void GetFileInfo()
        {

            var cloudStorage = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(connsctionString);

            var cloudBlobClient = cloudStorage.CreateCloudBlobClient();
            var cloudBlobContainer = cloudBlobClient.GetContainerReference("donmablogsample");
            var resultCreateContainer = cloudBlobContainer.CreateIfNotExistsAsync().Result;


            Console.WriteLine("donmablogsample create already.");



            Microsoft.WindowsAzure.Storage.Blob.CloudBlobDirectory cloudBlobDirectory =
                cloudBlobContainer.GetDirectoryReference("TEST2/TEST21/IMAGES/");

            Console.WriteLine("Get File Content : TEST2/TEST21/IMAGES/hamimelon.jpg");

            var blockReference = cloudBlobDirectory.GetBlockBlobReference("hamimelon.jpg");
            blockReference.FetchAttributesAsync().GetAwaiter().GetResult();

            Console.WriteLine(" TEST2/TEST21/IMAGES/hamimelon.jpg Size =>" + blockReference.Properties.Length.ToString("#,##0") + "bytes");
            Console.WriteLine(" TEST2/TEST21/IMAGES/hamimelon.jpg ContentType =>" + blockReference.Properties.ContentType);
            Console.WriteLine(" TEST2/TEST21/IMAGES/hamimelon.jpg Created =>" + blockReference.Properties.Created.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            Console.WriteLine(" TEST2/TEST21/IMAGES/hamimelon.jpg LastModified =>" + blockReference.Properties.LastModified.Value.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        public static void ListAllDirsFromDirectory()
        {

            var cloudStorage = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(connsctionString);

            var cloudBlobClient = cloudStorage.CreateCloudBlobClient();
            var cloudBlobContainer = cloudBlobClient.GetContainerReference("donmablogsample");
            var resultCreateContainer = cloudBlobContainer.CreateIfNotExistsAsync().Result;
            Console.WriteLine("donmablogsample create already.");


            Microsoft.WindowsAzure.Storage.Blob.CloudBlobDirectory cloudBlobDirectory =
               cloudBlobContainer.GetDirectoryReference("TEST2/TEST21/");

            var res = new List<string>();

            Microsoft.WindowsAzure.Storage.Blob.BlobContinuationToken continuationToken = null;
            do
            {

                var listingResult = cloudBlobDirectory.ListBlobsSegmentedAsync(continuationToken).Result;
                continuationToken = listingResult.ContinuationToken;
                res.AddRange(listingResult.Results.Where(x => x as Microsoft.WindowsAzure.Storage.Blob.CloudBlobDirectory != null).Select(x => x.Uri.Segments.Last()).ToList());
            }
            while (continuationToken != null);

            Console.WriteLine("Result: " + string.Join("\r\n", res));
        }

        public static void ListAllFilesFromDirectory()
        {

            var cloudStorage = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(connsctionString);

            var cloudBlobClient = cloudStorage.CreateCloudBlobClient();
            var cloudBlobContainer = cloudBlobClient.GetContainerReference("donmablogsample");
            var resultCreateContainer = cloudBlobContainer.CreateIfNotExistsAsync().Result;
            Console.WriteLine("donmablogsample create already.");


            Microsoft.WindowsAzure.Storage.Blob.CloudBlobDirectory cloudBlobDirectory =
               cloudBlobContainer.GetDirectoryReference("TEST2/TEST21/");

            var res = new List<string>();

            //這種方法在 Azure Storge 包括 table 很常使用
            Microsoft.WindowsAzure.Storage.Blob.BlobContinuationToken continuationToken = null;
            do
            {

                var listingResult = cloudBlobDirectory.ListBlobsSegmentedAsync(continuationToken).Result;
                continuationToken = listingResult.ContinuationToken;
                res.AddRange(listingResult.Results.Select(x => System.IO.Path.GetFileName(x.Uri.AbsolutePath)).ToList());
            }
            while (continuationToken != null);

            Console.WriteLine("Result: " + string.Join("\r\n", res));
        }

        public static void UploadImage()
        {
            var cloudStorage = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(connsctionString);

            var cloudBlobClient = cloudStorage.CreateCloudBlobClient();
            var cloudBlobContainer = cloudBlobClient.GetContainerReference("donmablogsample");
            var resultCreateContainer = cloudBlobContainer.CreateIfNotExistsAsync().Result;

            Console.WriteLine("donmablogsample create already.");



            Microsoft.WindowsAzure.Storage.Blob.CloudBlobDirectory cloudBlobDirectory =
                cloudBlobContainer.GetDirectoryReference("TEST2/TEST21/IMAGES/");

            cloudBlobDirectory.GetBlockBlobReference("hamimelon.jpg").UploadFromFileAsync(AppDomain.CurrentDomain.BaseDirectory + "sample.jpg").GetAwaiter().GetResult();

            Console.WriteLine("Upload Success");

            //開啟連結
            Console.WriteLine();

            var sharedPolicy = new Microsoft.WindowsAzure.Storage.Blob.SharedAccessBlobPolicy()
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddSeconds(60),
                Permissions = Microsoft.WindowsAzure.Storage.Blob.SharedAccessBlobPermissions.Read,

            };

            var sasContainerToken = cloudBlobContainer.GetSharedAccessSignature(sharedPolicy, null);
            var uri = cloudBlobDirectory.GetBlockBlobReference("hamimelon.jpg").Uri;
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("cmd", $"/c start " + uri + sasContainerToken.Replace("&", "^&")) { CreateNoWindow = true });
        }

        public static void GetFileUri()
        {
            var cloudStorage = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(connsctionString);

            var cloudBlobClient = cloudStorage.CreateCloudBlobClient();
            var cloudBlobContainer = cloudBlobClient.GetContainerReference("donmablogsample");
            var resultCreateContainer = cloudBlobContainer.CreateIfNotExistsAsync().Result;

            Console.WriteLine("donmablogsample create already.");

            // 設定可以開啟 60 秒，並且此簽章只能夠讀取
            var sharedPolicy = new Microsoft.WindowsAzure.Storage.Blob.SharedAccessBlobPolicy()
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddSeconds(60),
                Permissions = Microsoft.WindowsAzure.Storage.Blob.SharedAccessBlobPermissions.Read,

            };


            var sasContainerToken = cloudBlobContainer.GetSharedAccessSignature(sharedPolicy, null);

            Microsoft.WindowsAzure.Storage.Blob.CloudBlobDirectory cloudBlobDirectory =
               cloudBlobContainer.GetDirectoryReference("TEST2/TEST21/");
            var uri = cloudBlobDirectory.GetBlockBlobReference("sampleText9.json").Uri;


            Console.WriteLine("TEST2/TEST21/sampleText9.json Download Uri =>" + uri + sasContainerToken);

            //這一行非必要 ，呼叫系統瀏覽器開始網址而已
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("cmd", $"/c start " + uri + sasContainerToken.Replace("&", "^&")) { CreateNoWindow = true });
        }

        public static void ReadFileContent()
        {


            var cloudStorage = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(connsctionString);

            //Create Container
            var cloudBlobClient = cloudStorage.CreateCloudBlobClient();
            var cloudBlobContainer = cloudBlobClient.GetContainerReference("donmablogsample");
            var resultCreateContainer = cloudBlobContainer.CreateIfNotExistsAsync().Result;
            Console.WriteLine("donmablogsample create already.");


            //Get Dir
            Microsoft.WindowsAzure.Storage.Blob.CloudBlobDirectory cloudBlobDirectory =
                cloudBlobContainer.GetDirectoryReference("TEST2/TEST21/");

            Console.WriteLine("Get File Content : TEST2/TEST21/sampleText9.json");

            //Download Text
            var result = cloudBlobDirectory.GetBlockBlobReference("sampleText9.json").DownloadTextAsync().Result;

            Console.WriteLine("TEST2/TEST21/sampleText9.json content =>" + result);

        }

        public static void DeleteFile()
        {

            
            var cloudStorage = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(connsctionString);

            var cloudBlobClient = cloudStorage.CreateCloudBlobClient();
            var cloudBlobContainer = cloudBlobClient.GetContainerReference("donmablogsample");
            var resultCreateContainer = cloudBlobContainer.CreateIfNotExistsAsync().Result;

            Console.WriteLine("donmablogsample create already.");


            //取得 TEST2/TEST21/ 目錄
            Microsoft.WindowsAzure.Storage.Blob.CloudBlobDirectory cloudBlobDirectory =
                cloudBlobContainer.GetDirectoryReference("TEST2/TEST21/");

            Console.WriteLine("Delete File : TEST2/TEST21/sampleText8.json");
            // 如果存在就刪除，如果只是單純使用 DeleteAsync 如果不存在 會　Exception
            // 如果檔案存在被刪除， result 才會為  true , 如果檔案不存在 則 result 為  false
            var result = cloudBlobDirectory.GetBlockBlobReference("sampleText8.json").DeleteIfExistsAsync().GetAwaiter().GetResult();

            Console.WriteLine(result);

        }

        public static void CheckFileExisted()
        {

            var cloudStorage = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(connsctionString);

            var cloudBlobClient = cloudStorage.CreateCloudBlobClient();
            var cloudBlobContainer = cloudBlobClient.GetContainerReference("donmablogsample");
            var resultCreateContainer = cloudBlobContainer.CreateIfNotExistsAsync().Result;

            Console.WriteLine("donmablogsample create already.");


            Microsoft.WindowsAzure.Storage.Blob.CloudBlobDirectory cloudBlobDirectory =
                cloudBlobContainer.GetDirectoryReference("TEST2/TEST21/");

            //Existed : true
            Console.WriteLine("TEST2/TEST21/sampleText1.json is Existed =>" + cloudBlobDirectory.GetBlockBlobReference("sampleText1.json").ExistsAsync().Result);

            //Existed : false
            Console.WriteLine("TEST2/TEST21/FileNotExisted.json is Existed =>" + cloudBlobDirectory.GetBlockBlobReference("FileNotExisted.json").ExistsAsync().Result);



        }
        public static void CreateBlobDirectory2()
        {

            var cloudStorage = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(connsctionString);

            var cloudBlobClient = cloudStorage.CreateCloudBlobClient();
            var cloudBlobContainer = cloudBlobClient.GetContainerReference("donmablogsample");
            var resultCreateContainer = cloudBlobContainer.CreateIfNotExistsAsync().Result;

            Console.WriteLine("donmablogsample create already.");


            Microsoft.WindowsAzure.Storage.Blob.CloudBlobDirectory cloudBlobDirectory =
                cloudBlobContainer.GetDirectoryReference("TEST2/TEST21/");

            for (var i = 1; i <= 10; i++)
            {
                var bFileInfo = cloudBlobDirectory.GetBlockBlobReference("sampleText" + i + ".json");
                bFileInfo.Properties.ContentType = "application/json; charset=utf-8";
                bFileInfo.UploadTextAsync("{\"data\":\"測試資料" + i + "\"}").GetAwaiter().GetResult();
                Console.WriteLine("Success Add File TEST2/TEST21/sampleText" + i + ".json");
            }


        }
        public static void CreateBlobDirectory()
        {

            var cloudStorage = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(connsctionString);

            //Check Contain is Existed.
            var cloudBlobClient = cloudStorage.CreateCloudBlobClient();
            var cloudBlobContainer = cloudBlobClient.GetContainerReference("donmablogsample");
            var resultCreateContainer = cloudBlobContainer.CreateIfNotExistsAsync().Result;

            //取得該檔案夾 TEST
            Microsoft.WindowsAzure.Storage.Blob.CloudBlobDirectory cloudBlobDirectory =
                cloudBlobContainer.GetDirectoryReference("TEST");

            //在該檔案夾下面建立檔案
            var bFileInfo = cloudBlobDirectory.GetBlockBlobReference("sampleText.json");
            bFileInfo.Properties.ContentType = "application/json; charset=utf-8";
            bFileInfo.UploadTextAsync("{\"data\":\"測試資料\"}").GetAwaiter().GetResult();

            Console.WriteLine("Success Update File TEST/sampleText.json");
        }

        public static void CreateFileOnRoot()
        {


            var cloudStorage = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(connsctionString);

            var cloudBlobClient = cloudStorage.CreateCloudBlobClient();
            var cloudBlobContainer = cloudBlobClient.GetContainerReference("donmablogsample");
            //建立如果不存在的話
            var result = cloudBlobContainer.CreateIfNotExistsAsync().Result;

            //Create File 
            var bFileInfo = cloudBlobContainer.GetBlockBlobReference("sampleText.json");
            bFileInfo.Properties.ContentType = "application/json; charset=utf-8";
            bFileInfo.UploadTextAsync("{\"data\":\"測試資料\"}").GetAwaiter().GetResult();

        }

        public static void CreateBlobContainer()
        {


            var cloudStorage = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(connsctionString);

            var cloudBlobClient = cloudStorage.CreateCloudBlobClient();
            var cloudBlobContainer = cloudBlobClient.GetContainerReference("donmablogsample");
            //建立如果不存在的話
            var result = cloudBlobContainer.CreateIfNotExistsAsync().Result;

            //如果已經建立過了，也會回傳 false 
            Console.WriteLine("Create Container donmablogsample result : " + result);
        }
    }
}
