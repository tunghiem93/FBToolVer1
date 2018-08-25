using CMS_DTO.CMSCrawler;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace CMS_Shared.Utilities
{
    public static class CrawlerHelperFB
    {
        public static bool Get_Tagged_Pins(ref CMS_CrawlerModels model, string search_str, int limit = 1, string bookmarks_str = null, int page = 1)
        {
            var url = "https://www.facebook.com/lifewithsunshine/ads/?ref=page_internal&dpr=1&ajaxpipe=1&ajaxpipe_token=AXh9WxHKM0f06Rq9&country=1&path=%2Flifewithsunshine%2Fads%2F&__user=100003324695675&__a=1&__dyn=5V4cjLx2ByK5A9UkKHqAyqomzFE9XG8GAdyeGDirWqF1G7UnGdwIhEnUF7yWCHxCEjCyEnyo88ObGubyRUC48G5WAxamjDK7GgPwzxuFS58-ER2KdyU8p94jUXVoS48nVV8Gicx2q5o4OmayrBy8GudAx6cw_xle9xmjx2Qm3GE-qp3FK4bUCaxKh1e5pVkdxCi78SaCCy89ooKHVohxyhu9K9BmFpEBq8IHGfio8l8imEggmKbKqify4cXJ2oS3m6ogUK8GE_WUWiUd9azEKiEDyp8ymaVeaDU8fiAx2miQhxdyopBAyEN4yprypVUV1bCxe9yEgy8LzU9FWDz8a8Z112HJ7VVHAyEsyUlzF8WEKU&__req=jsonp_3&__be=1&__pc=PHASED%3ADEFAULT&__rev=4066324&__spin_r=4066324&__spin_b=trunk&__spin_t=1530514908&__adt=3";
            //var url = "https://www.facebook.com/lifewithsunshine/photos/a.1797478757133845.1073741829.1757631344451920/2017087941839591/?type=3";
            getDataPinterest(url, model, "", ref bookmarks_str);

            return false;
        }

        public static CMS_CrawlerModels getDataPinterest(string url, CMS_CrawlerModels model, string pinId, ref string bookmarks)
        {
            try
            {
                Uri uri = new Uri(url);
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);

                /* request need cookie & user agent */
                httpWebRequest.Headers["Cookie"] = "fr=0HZLfh0cIOmtmNqCq.AWXR_MW9yNog0CyLSTuvJhhdnGM.BajqQf.RE.AAA.0.0.BbOi7n.AWUh9bO8; sb=Cs05WwnlYymkzEg6Xn32mzc8; wd=1366x654; datr=Js05W_jbAaa1Ij5CurtBJmwC; locale=en_GB; c_user=100003324695675; xs=23%3AVia9gvMSQtiufw%3A2%3A1530514908%3A467%3A6165; pl=n; spin=r.4066324_b.trunk_t.1530514908_s.1_v.2_; act=1530541156947%2F6; presence=EDvF3EtimeF1530541148EuserFA21B03324695675A2EstateFDutF1530541148851CEchFDp_5f1B03324695675F4CC; x-src=%2Fpg%2Flifewithsunshine%2Fads%2F%7Ccontent_container; pnl_data2=eyJhIjoib25hZnRlcmxvYWQiLCJjIjoiWFBhZ2VzUHJvZmlsZUhvbWVDb250cm9sbGVyIiwiYiI6ZmFsc2UsImQiOiIvbGlmZXdpdGhzdW5zaGluZS9hZHMvIiwiZSI6W119";
                httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:61.0) Gecko/20100101 Firefox/61.0";

                httpWebRequest.Timeout = 100000;
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var answer = streamReader.ReadToEnd();
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(answer);

                    /* get list scripts */
                    var scripts = htmlDoc.DocumentNode.Descendants("script").ToList();

                    var listData = new List<string>();
                    int i = 0;
                    foreach (var script in scripts)
                    {
                        i++;
                        if (i == 3) /* ERROR IN 3TH SCRIPT */
                            break;

                        /* find pay load element */
                        var res = findElement(script.InnerHtml, "payload", 0);
                        if (!string.IsNullOrEmpty(res))
                        {
                            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                            dynamic dobj = jsonSerializer.Deserialize<dynamic>(res);
                            var htmlData = dobj["content"];
                            if (htmlData != null)
                            {
                                var xmlData = htmlData["content"];
                                if (xmlData != null)
                                {
                                    /* get list tag a */
                                    htmlDoc.LoadHtml(xmlData);
                                    var lstA = htmlDoc.DocumentNode.Descendants("a").Where(n => n.GetAttributeValue("rel", "") == "theater").ToList();
                                    foreach (var tagA in lstA)
                                    {
                                        /* GET DATA MODEL */
                                        var href = tagA.GetAttributeValue("href", "");
                                        var ajaxify = tagA.GetAttributeValue("ajaxify", "");
                                        var fbID = findID(ajaxify);
                                        var pin = new PinsModels()
                                        {
                                            ID = fbID,
                                            Link = href,
                                        };
                                        model.Pins.Add(pin);

                                        CrawlerFBDetail(href, fbID,ref pin);
                                    }
                                }
                            }
                        }
                    }
                    streamReader.Close();
                    streamReader.Dispose();
                }
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("ErrorgetDataPinterest" + "\n url: " + url + "\nBookmarks:" + bookmarks, ex);
            }
            return model;
        }

        public static string findElement(string _input, string key, int start)
        {
            var ret = "";
            start = _input.IndexOf(key);
            if (start > 0)
            {
                var countLeftBreak = 0;
                var iEnd = 0;
                start = _input.IndexOf('{', start);
                for (int i = start; i < _input.Length; i++)
                {
                    char ch = _input[i];
                    if (ch == '}')
                    {
                        countLeftBreak--;
                        if (countLeftBreak == 0)
                        {
                            iEnd = i;
                            break;
                        }
                    }
                    else if (ch == '{')
                        countLeftBreak++;
                }

                if (iEnd > start)
                    ret = _input.Substring(start, iEnd - start + 1);
            }

            return ret;
        }

        public static void CrawlerFBDetail(string Url, string fb_id, ref PinsModels pin)
        {
            try
            {
                var url = "https://www.facebook.com" + Url + "";
                //url = "https://www.facebook.com/lifewithsunshine/photos/a.1797478757133845.1073741829.1757631344451920/2063689897179395/?type=3&theater&fb_id=2063689920512726";
                Uri uri = new Uri(url);
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);

                httpWebRequest.Headers["Cookie"] = "fr=0g932KaBNIHkPNSHd.AWUyWBwpX4_A_YKA4NhvmupYBkk.BbMcZu.uD.Fs5.0.0.BbOtXK.AWXncoeT; sb=NmI3W-ffluEtyFHleEWSjhBl; wd=1920x943; datr=NmI3WwtbosYtTwDtslqJtXZd; c_user=100003727776485; xs=136%3Au6XG_yUasjTeFQ%3A2%3A1530356294%3A6091%3A726; pl=n; spin=r.4066192_b.trunk_t.1530495666_s.1_v.2_; act=1530582591458%2F0; presence=EDvF3EtimeF1530582595EuserFA21B03727776485A2EstateFDutF1530582595488CEchFDp_5f1B03727776485F2CC";
                httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:61.0) Gecko/20100101 Firefox/61.0";
                httpWebRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                httpWebRequest.Headers["Proxy-Authorization"] = "Digest username=\"54737357\", realm=\"anonymox.net\", nonce=\"rt86WwAAAABgQKLm21UAAAwL3yIAAAAA\", uri=\"www.facebook.com:443\", response=\"47dc76deffbdaef3fc92784579b19d65\", qop=auth, nc=0000021f, cnonce=\"d0a10718bc5ea4b9\"";
                //httpWebRequest.Headers["Remote-Address"] = "146.185.28.59:443";
                httpWebRequest.Timeout = 100000;
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var html = streamReader.ReadToEnd();
                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(html);
                    var script = doc.DocumentNode.Descendants()
                             .Where(n => n.Name == "script").ToList();
                    if (script != null && script.Count > 10)
                    {
                        var nodeJson = script[10].InnerText;
                        if (!string.IsNullOrEmpty(nodeJson))
                        {
                            findNode(nodeJson, "feedbacktarget", 0, fb_id, ref pin);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("CrawlerFB Detail", ex);
            }
        }

        public static void findNode(string input, string key, int start, string fb_id, ref PinsModels pin)
        {
            var jsonfeedbacktarget = findElement(input, "feedbacktarget", 0);
            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            dynamic dobj = jsonSerializer.Deserialize<dynamic>(jsonfeedbacktarget);
            var dictionary = dobj as Dictionary<string, dynamic>;
            if (dictionary.ContainsKey("entidentifier"))
            {
                var _fb_id = dictionary["entidentifier"];
                if (fb_id.Equals(_fb_id))
                {
                    if (dictionary.ContainsKey("commentTotalCount"))
                    {
                        var commentTotalCount = Convert.ToInt16(dictionary["commentTotalCount"]);
                        pin.commentTotalCount = commentTotalCount;
                    }
                    if (dictionary.ContainsKey("reactioncount"))
                    {
                        var reactioncount = Convert.ToInt16(dictionary["reactioncount"]);
                        pin.reactioncount = reactioncount;
                    }
                    if (dictionary.ContainsKey("sharecount"))
                    {
                        var sharecount = Convert.ToInt16(dictionary["sharecount"]);
                        pin.sharecount = sharecount;
                    }
                    return;
                }
                else
                {
                    jsonfeedbacktarget = "\"feedbacktarget\":" + jsonfeedbacktarget;
                    input = input.Replace(jsonfeedbacktarget, "");
                    findNode(input, key, start, fb_id, ref pin);
                }
            }
        }

        public static string findID(string input)
        {
            var ret = "";
            var iStart = input.IndexOf("fbid");
            var iEnd = input.IndexOf("&amp;", iStart);
            if (iEnd > iStart)
            {
                iStart += 5;
                ret = input.Substring(iStart, iEnd - iStart);
            }
            return ret;
        }

    }
}
