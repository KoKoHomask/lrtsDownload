using AngleSharp.Parser.Html;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace lrtsDownload
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        string __cookie = "";
        public MainPage()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            this.InitializeComponent();
        }
        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            tbSearch.Width = this.ActualWidth / 3 * 2;//保证不同分辨率下搜索框能正确显示
        }
        List<lrtsModel> lrts;
        private async void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            prWait.IsActive = true;
            TopRowDefinition.Height = new GridLength(0.3, GridUnitType.Star);
            var result=await DoWork(tbSearch.Text ?? "");
            prWait.IsActive = false;
            spResult.Children.Clear();
            if (result.Count == 0)
            {
                spResult.Children.Add(new TextBlock()
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 26,
                    Text = "该链接未检测到有效音频文件",
                });
            }
            else
            {
                lrts = result;
                spResult.Children.Add(new TextBlock()
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Text = "下载链接有时效,如文件无法下载可尝试重新获取",
                });
                var plxz = new HyperlinkButton()
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Content = "批量下载",
                };
                var xzjd = new TextBlock()
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Text = "下载进度:0/0",
                    Visibility = Visibility.Collapsed,
                };
                plxz.Click += new RoutedEventHandler(async (h_sender, h_e) =>
                {
                    HttpDownloads httpDownloads = new HttpDownloads(FolderName);
                    var dResult=await httpDownloads.DownloadFileList(lrts,(x)=> {
                        xzjd.Text = "下载进度:" + (x.SuccessCount + x.FailCount).ToString() + "/" + x.AllCount;
                        xzjd.Visibility = Visibility.Visible;
                    });
                    xzjd.Text = "下载完毕,总任务数量" + dResult.AllCount.ToString()
                                + ",成功" + dResult.SuccessCount.ToString()
                                + ",失败" + dResult.FailCount.ToString();
                });
                spResult.Children.Add(plxz);
                spResult.Children.Add(xzjd);
                foreach (lrtsModel tmp in result)
                {
                    spResult.Children.Add(new HyperlinkButton()
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Content=tmp.FileName,
                        NavigateUri=new Uri( tmp.Url),
                    });
                }
                
            }
        }

        private void Plxz_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        async Task<List<lrtsModel>> DoWork(string url)
        {
            List<lrtsModel> lst = new List<lrtsModel>();
            if(url.IndexOf("lrts")>=0)
            {
                var bookInfo = await GetBookInfo(url);
                List<lrtsFileModel> fileList = new List<lrtsFileModel>();
                var isGetOk=await GetPlayList(fileList,bookInfo);
                if (isGetOk)
                    lst = await Finally(fileList);
            }
            return lst;
        }
        async Task<List<lrtsModel>>Finally(List<lrtsFileModel> lrtsFilesLst)
        {
            List<lrtsModel> lst = new List<lrtsModel>();
            string url = "http://www.lrts.me/ajax/path/";//http://www.lrts.me/ajax/path/4/39068/215321538
            foreach(lrtsFileModel tmp in lrtsFilesLst)
            {
                lrtsModel lrts = new lrtsModel() { FileName=tmp.player_r_name};
                if (tmp.source!=null&&tmp.source!=string.Empty)
                {
                    lrts.Url = tmp.source;
                    lst.Add(lrts);
                }
                else
                {
                    string tmpUrl= url + tmp.share_entityType + "/" + tmp.share_fatherEntityId + "/" + tmp.sectionid;
                    var result = await GetHtml(new HttpItem() { URL = tmpUrl, Cookie = __cookie });
                    __cookie = result.Cookie;
                    int start = result.Html.ToLower().IndexOf("<data>");
                    int end= result.Html.ToLower().IndexOf("</data>");
                    string src = result.Html.Substring(
                        start, end-start);
                    lrts.Url = src.Replace("<data>", "");
                    lst.Add(lrts);
                }
            }
            return lst;
        }
        async Task<bool> GetPlayList(List<lrtsFileModel> lst, lrtsBookInfo lrtsBookInfo, int number = 1)
        {
            string url = "http://www.lrts.me/ajax/playlist/" + lrtsBookInfo.booktype + "/" + lrtsBookInfo.bookid + "/" + number.ToString();
            var result = await GetHtml(new HttpItem() { URL = url, Cookie = __cookie });
            __cookie = result.Cookie;
            string html = HttpUtility.HtmlDecode(result.Html);
            HtmlParser htmlParser = new HtmlParser();
            var _lst = htmlParser.Parse(html).QuerySelectorAll("div.column1").Select(t => new lrtsFileModel()
            {
                player_r_name = t.QuerySelectorAll("input")[4].GetAttribute("value"),
                sectionid = t.QuerySelectorAll("input")[3].GetAttribute("value"),
                source = t.QuerySelectorAll("input")[0].GetAttribute("value"),
                share_entityType = t.QuerySelectorAll("input")[7].GetAttribute("value"),
                share_fatherEntityId = t.QuerySelectorAll("input")[6].GetAttribute("value"),
            });
            lst.AddRange(_lst);
            if (_lst.Count() >= 10)
            { await GetPlayList(lst, lrtsBookInfo, (number + 10)); }
            return true;
        }

        async Task<lrtsBookInfo>GetBookInfo(string url)
        {
            lrtsBookInfo lrtsBookInfo = new lrtsBookInfo();
            var result = await GetHtml(new HttpItem() { URL = url, Cookie = __cookie });
            __cookie = result.Cookie;
            HtmlParser htmlParser = new HtmlParser();
            string tmp = null;
            try { tmp = htmlParser.Parse(result.Html).QuerySelectorAll("ul#pul").FirstOrDefault().QuerySelectorAll("a").FirstOrDefault().GetAttribute("player-info"); } catch { }
            if (tmp != null)
            {
                var array = tmp.Split('&');
                foreach (string str in array)
                {
                    if (str.IndexOf("type") >= 0)
                    { lrtsBookInfo.booktype =str.Replace("type=", ""); }
                    if (str.IndexOf("resourcesid") >= 0)
                    { lrtsBookInfo.bookid = str.Replace("resourcesid=", ""); }
                }
            }
            return lrtsBookInfo;
        }
        async Task<HttpResult> GetHtml(HttpItem item)
        {
            item.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36";
            item.Referer = "http://www.lrts.me/playlist";
            //item.Encoding = Encoding.GetEncoding("GBK");
            var result=await Task.Run(() => {
                HttpHelper http = new HttpHelper();
                return http.GetHtml(item);
            });
            return result;
        }
        const string FolderName = "lrts";
        private async void btnOpenFloader_Click(object sender, RoutedEventArgs e)
        {
            //FolderPicker p = new FolderPicker();

            StorageFolder savePath;
            var myMusic = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Music);
            var root = myMusic.SaveFolder;
            var list = await root.GetFoldersAsync();
            if (list.Where(x => x.Name == FolderName).FirstOrDefault() != null)
                savePath = await root.GetFolderAsync(FolderName);
            else
                savePath = await root.CreateFolderAsync(FolderName);
            var t = new FolderLauncherOptions();
            await Launcher.LaunchFolderAsync(savePath, t);

            //await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync("lrts");
        }
    }
}
