using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace lrtsDownload
{
    public class HttpDownloads
    {
        public string FolderName { get; }
        public HttpDownloads(string folderName)
        {
            FolderName = folderName;
        }
        public delegate void OnSingleOverCallBack(DownloadResult downloadResult);
        public async Task<DownloadResult> DownloadFileList(List<lrtsModel> lrts, OnSingleOverCallBack onSingleOverCallBack)
        {
            DownloadResult processResult = new DownloadResult() { FailCount = 0, SuccessCount = 0 , AllCount=lrts.Count};
            onSingleOverCallBack?.Invoke(processResult);
            foreach (var single in lrts)
            {
                var result = await DownloadSingleFile(single.Url, single.FileName);
                if (result) processResult.SuccessCount++;
                else processResult.FailCount++;
                onSingleOverCallBack?.Invoke(processResult);
            }
            return processResult;
        }
        public async Task<bool> DownloadSingleFile(string url,string name)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                byte[] data = await httpClient.GetByteArrayAsync(url);
                string fileName = name;
                if (url.IndexOf("mp3") >= 0)
                    fileName += ".mp3";
                else if (url.IndexOf("m4a") >= 0)
                    fileName += "m4a";
                var saveResult = await SaveFile(fileName, data);
                return saveResult;
            }
            catch(Exception ex) { }
            return false;   
        }

        public async Task<bool> SaveFile(string fileName,byte[] data)
        {
            try
            {
                StorageFolder savePath;
                var myMusic = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music);
                var root = myMusic.SaveFolder;
                var list = await root.GetFoldersAsync();
                if (list.Where(x => x.Name == FolderName).FirstOrDefault() != null)
                    savePath = await root.GetFolderAsync(FolderName);
                else
                    savePath = await root.CreateFolderAsync(FolderName);
                var file = await savePath.CreateFileAsync(fileName);
                var fileStream = await file.OpenStreamForWriteAsync();
                fileStream.Write(data, 0, data.Length);
                fileStream.Flush();
                fileStream.Close();
                return true;
            }
            catch (Exception ex) { }
            return false;
        }
    }
}
