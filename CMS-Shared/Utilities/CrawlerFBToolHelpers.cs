using CMS_DTO.CMSCrawler;
using HtmlAgilityPack;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace CMS_Shared.Utilities
{
    public class CrawlerFBToolHelpers
    {
        private static int PageSize = Commons.PageSize;
        private const string api_url = "https://www.facebook.com/ajax/pagelet/generic.php/BrowseScrollingSetPagelet";
        public static void CrawlerNow(string q,string ref_path,string view,byte type,string cookie, ref CMS_CrawlerModels pins)
        {
            try
            {
                string cursor = "";
                for (var i = 1;i < PageSize; i++)
                {
                    String timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssffff");
                    var obj = _search_keyword_payload(q, ref_path, view,i,cursor);
                    var userID = GetUserIDFromCookies(cookie);
                    var url = "https://www.facebook.com/ajax/pagelet/generic.php/BrowseScrollingSetPagelet?dpr=1&data=" + obj + "&__user="+ userID + "&__a=1&__be=1&__pc=PHASED:DEFAULT&__spin_t=" + timeStamp + "";
                    Uri uri = new Uri(url);
                    var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
                    httpWebRequest.Accept = "*/*";
                    httpWebRequest.Headers["Cache-Control"] = "no-cache";
                    httpWebRequest.Headers["Origin"] = "https://www.facebook.com";
                    httpWebRequest.Referer = "https://www.facebook.com";
                    httpWebRequest.Headers["upgrade-insecure-requests"] = "1";
                    httpWebRequest.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Ubuntu Chromium/62.0.3202.94 Chrome/62.0.3202.94 Safari/537.36";
                    httpWebRequest.Headers["Cookie"] = cookie; //"fr=0g932KaBNIHkPNSHd.AWUalQQ9AxXL7JpoqimmeZTMmkg.BbMcZu.uD.AAA.0.0.Bbky_F.AWW4mfhc; sb=NmI3W-ffluEtyFHleEWSjhBl; datr=NmI3WwtbosYtTwDtslqJtXZd; wd=1920x944; c_user=100003727776485; xs=38%3AjT_REhmug5Jgrg%3A2%3A1536224023%3A6091%3A726; pl=n; spin=r.4289044_b.trunk_t.1536328620_s.1_v.2_; presence=EDvF3EtimeF1536372678EuserFA21B03727776485A2EstateFDutF1536372678743CEchFDp_5f1B03727776485F2CC; act=1536372716538%2F11";
                    httpWebRequest.Timeout = 90000;
                    using (HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                    {
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            var data = streamReader.ReadToEnd();
                            streamReader.Dispose();
                            httpResponse.Dispose();

                            if (!string.IsNullOrEmpty(data))
                            {
                                data = data.Replace("for (;;);", "");
                                var json = ParseJson(data);
                                if (json != null)
                                {
                                    if (string.IsNullOrEmpty(json.payload))
                                    {
                                        NSLog.Logger.Info("response-data-error-account-suppend");
                                    }
                                    else
                                    {
                                        /* get cursor */
                                        cursor = GetCursor(json.jsmods);
                                        /* parse html */
                                        var html = json.payload;
                                        
                                        if (type == (byte)Commons.EType.Photo)
                                        {
                                            ParseHtmlPhoto(json, ref pins);
                                        }

                                        if (type == (byte)Commons.EType.Post)
                                        {
                                            ParseHtmlPostAndPeople(json, (int)Commons.EType.Post, ref pins);
                                        }

                                        if (type == (byte)Commons.EType.People)
                                        {
                                            ParseHtmlPostAndPeople(json, (int)Commons.EType.People, ref pins);
                                        }

                                        if (type == (byte)Commons.EType.Video)
                                        {
                                            ParseHtmlVideo(json, ref pins);
                                        }

                                        if (string.IsNullOrEmpty(cursor))
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                NSLog.Logger.Error("CrawlerNow : ", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsmods"></param>
        /// <param name="pin"></param>
        public static void GetPhotoID(jsmods jsmods,string fbId, ref PinsModels pin)
        {
            try
            {
                if (jsmods != null)
                {
                    var require = jsmods.require.ToList();
                    var data = require.Where(o => o.ToString().Contains("UFIController")).ToList();
                    foreach (var item in data)
                    {
                        var s = item.ToString();
                        var serializer = new JavaScriptSerializer();
                        dynamic objs = serializer.Deserialize(s, typeof(object));
                        if (objs != null)
                        {
                            var lastPin = objs[3][2];
                            if (lastPin != null)
                            {
                                var feedbacktarget = lastPin["feedbacktarget"];
                                var json = new JavaScriptSerializer().Serialize(feedbacktarget);
                                feedbacktarget objPin = JsonConvert.DeserializeObject<feedbacktarget>(json);
                                if (objPin != null)
                                {
                                    if (objPin.entidentifier.Equals(fbId))
                                    {
                                        var last = objs[3][1];
                                        if (last != null)
                                        {
                                            var conversationGuideParams = last["conversationGuideParams"];
                                            if (conversationGuideParams != null)
                                            {
                                                var strtrackingID = conversationGuideParams["trackingID"];
                                                //var json = new JavaScriptSerializer().Serialize(strtrackingID);
                                                conversationGuideParams objPhoto = JsonConvert.DeserializeObject<conversationGuideParams>(strtrackingID);
                                                if (objPhoto != null)
                                                {
                                                    if (!string.IsNullOrEmpty(objPhoto.photo_id))
                                                        pin.PhotoID = objPhoto.photo_id;

                                                    if (objPhoto.photo_attachments_list != null && objPhoto.photo_attachments_list.Any())
                                                    {
                                                        pin.PhotoID = objPhoto.photo_attachments_list.FirstOrDefault();
                                                    }
                                                }
                                            }

                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NSLog.Logger.Error("GetPhotoID:", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsmods"></param>
        public static void GetPin(jsmods jsmods,string fbId,ref PinsModels pin)
        {
            try
            {
                if(jsmods != null)
                {
                    var require = jsmods.require.ToList();
                    var data =  require.Where(o => o.ToString().Contains("UFIController")).ToList();
                    foreach(var item in data)
                    {
                        var s = item.ToString();
                        var serializer = new JavaScriptSerializer();
                        dynamic objs = serializer.Deserialize(s, typeof(object));
                        if(objs != null)
                        {
                            var last = objs[3][2];
                            if(last != null)
                            {
                                var feedbacktarget = last["feedbacktarget"];
                                var json = new JavaScriptSerializer().Serialize(feedbacktarget);
                                feedbacktarget objPin = JsonConvert.DeserializeObject<feedbacktarget>(json);
                                if(objPin != null)
                                {
                                    if(objPin.entidentifier.Equals(fbId))
                                    {
                                        pin.reactioncount = objPin.reactioncount;
                                        pin.commentTotalCount = objPin.commentcount;
                                        pin.sharecount = objPin.sharecount;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                NSLog.Logger.Error("GetPin:", ex);
            }
        }

        /* get user ID from cookies */
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns></returns>
        private static string GetUserIDFromCookies(string cookie)
        {
            var ret = "";
            if (!string.IsNullOrEmpty(cookie))
            {
                var start = cookie.IndexOf("c_user=");
                var end = cookie.Length;
                start = cookie.IndexOf("=", start) + 1;
                for (int i = start; i < cookie.Length; i++)
                {
                    char key = cookie[i];
                    if (key == ';')
                    {
                        end = i;
                        break;
                    }
                }
                ret = cookie.Substring(start, (end - start));
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsmods"></param>
        /// <returns></returns>
        public static string GetCursor(jsmods jsmods)
        {
            var cursor = "";
            try
            {
                /* get cursor */
                if (jsmods != null)
                {
                    var dataJsmode = jsmods.require[1].ToString();
                    var serializer = new JavaScriptSerializer();
                    dynamic objs = serializer.Deserialize(dataJsmode, typeof(object));
                    if (objs != null)
                    {
                        var objCursor = objs[3][0] as Dictionary<string, dynamic>;
                        if (objCursor != null)
                        {
                            cursor = objCursor["cursor"];
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                NSLog.Logger.Error("GetCursor:", ex);
            }
            return cursor;
        }

        /// <summary>
        /// parse html tab video
        /// </summary>
        /// <param name="html"></param>
        public static void ParseHtmlVideo(JsonData json, ref CMS_CrawlerModels pins)
        {
            try
            {
                string html = json.payload;
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                var _mHtml = doc.DocumentNode.Descendants()
                                             .Where(x => (x.Name == "div" && x.Attributes["class"] != null &&
                                                   x.Attributes["class"].Value.Contains("_4ou6"))).ToList();
                if(_mHtml != null && _mHtml.Any())
                {
                    foreach(var mItem in _mHtml)
                    {
                        /* get id */
                        var pin = new PinsModels();
                        var strIds = mItem.GetAttributeValue("data-bt", "");
                        strIds = System.Web.HttpUtility.HtmlDecode(strIds);
                        if (!string.IsNullOrEmpty(strIds))
                        {
                            var _jsonId = JsonConvert.DeserializeObject<JsonData>(strIds);
                            if (_jsonId != null)
                            {
                                var Id = _jsonId.id;
                                pin.ID = Id;
                            }
                        }

                        /* get link video */
                        var objLink = mItem.Descendants().Where(o => o.Name == "div" && o.Attributes["class"] != null
                                                                && o.Attributes["class"].Value.Contains("_4ovv")).FirstOrDefault();
                        if(objLink != null)
                        {
                            var mLink = objLink.Descendants().Where(o => o.Name == "a" && o.Attributes["class"] != null
                                                                    && o.Attributes["class"].Value.Contains("async_saving _400z _2-40 _3k0i")).FirstOrDefault();
                            if(mLink != null)
                            {
                                var Link = mLink.GetAttributeValue("href", "");
                                if(!string.IsNullOrEmpty(Link))
                                {
                                    Link = "https://www.facebook.com" + Link;
                                    pin.LinkVideo = Link;
                                }
                            }
                        }

                        /* get owner name */
                        var objOwnerName = mItem.Descendants().Where(o => o.Name == "div" && o.Attributes["class"] != null
                                                                    && o.Attributes["class"].Value.Contains("_4ovi")).FirstOrDefault();
                        if(objOwnerName != null)
                        {
                            var OwnerName = objOwnerName.InnerText;
                            pin.OwnerName = OwnerName;
                        }

                        /* get description */
                        var objDes = mItem.Descendants().Where(o => o.Name == "div" && o.Attributes["class"] != null
                                                                    && o.Attributes["class"].Value.Contains("_4ovj _4ovl")).FirstOrDefault();
                        if (objDes != null)
                        {
                            var Des = objDes.InnerText;
                            Des = System.Web.HttpUtility.HtmlDecode(Des);
                            pin.Description = Des;
                        }

                        /* get facebook fan page link */
                        var objFanPage = mItem.Descendants().Where(o => o.Name == "div" && o.Attributes["class"] != null
                                                                    && o.Attributes["class"].Value.Contains("_42bz _4ovs")).FirstOrDefault();
                        if(objFanPage != null)
                        {
                            var FanPage = objFanPage.Descendants("a").FirstOrDefault();
                            if(FanPage != null)
                            {
                                var LinkFanPage = FanPage.GetAttributeValue("href", "");
                                var DesFanPage = FanPage.InnerText;
                                
                            }
                        }

                        /* get time */
                        var objTime = mItem.Descendants().Where(o => o.Name == "div" && o.Attributes["class"] != null &&
                                                                     o.Attributes["class"].Value.Contains("_42b-")).FirstOrDefault();
                        if (objTime != null)
                        {
                            var abbr = objTime.Descendants("abbr").FirstOrDefault();
                            var timeStamp = abbr.GetAttributeValue("data-utime", "");
                            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                            var Created_At = dtDateTime.AddSeconds(double.Parse(timeStamp)).ToLocalTime();
                            pin.Created_At = Created_At;
                        }

                        if(!string.IsNullOrEmpty(pin.ID))
                        {
                            GetPin(json.jsmods, pin.ID, ref pin);
                            pins.Pins.Add(pin);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                NSLog.Logger.Error("ParseHtmlVideo:", ex);
            }
        }


        /// <summary>
        /// parse html tab post
        /// </summary>
        /// <param name="html"></param>
        public static void ParseHtmlPostAndPeople(JsonData json, int type, ref CMS_CrawlerModels pins)
        {
            try
            {
                string html = json.payload;
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                var _mHtml = doc.DocumentNode.Descendants()
                                             .Where(x => (x.Name == "div" && x.Attributes["class"] != null &&
                                                   x.Attributes["class"].Value.Contains("_5bl2 _3u1 _41je _440e"))).ToList();

                //var _mHtml = doc.DocumentNode.Descendants()
                //                             .Where(x => (x.Name == "div" && x.Attributes["class"] != null &&
                //                                   x.Attributes["class"].Value.Contains("_401d"))).ToList();

                if (_mHtml != null && _mHtml.Any())
                {
                    foreach(var mItem in _mHtml)
                    {
                        var pin = new PinsModels();
                        /* get parent id */
                        #region "get parrent id "
                        //var ParentNode = mItem.ParentNode.OuterHtml;
                        //if(ParentNode != null)
                        //{
                        //    var docParrent = new HtmlDocument();
                        //    docParrent.LoadHtml(ParentNode);
                        //    var _mParent = docParrent.DocumentNode.Descendants()
                        //                                            .Where(o => o.Name == "div" && o.Attributes["class"] != null &&
                        //                                                    o.Attributes["class"].Value.Contains("_401d")).FirstOrDefault();
                        //    if(_mParent != null)
                        //    {
                        //        var strParentIds = _mParent.GetAttributeValue("data-gt", "");
                        //        strParentIds = System.Web.HttpUtility.HtmlDecode(strParentIds);
                        //        var serializer = new JavaScriptSerializer();
                        //        dynamic objs = serializer.Deserialize(strParentIds, typeof(object));
                        //        if(objs != null)
                        //        {
                        //            string strXt = objs["xt"];
                        //            if (!string.IsNullOrEmpty(strXt))
                        //            {
                        //                var firstIndex = strXt.IndexOf('{');
                        //                strXt = strXt.Substring(firstIndex);
                        //                if(!string.IsNullOrEmpty(strXt))
                        //                {
                        //                    var _jsonId = JsonConvert.DeserializeObject<JsonData>(strXt);
                        //                    if (_jsonId != null)
                        //                    {
                        //                        var parentId = _jsonId.unit_id_result_id;
                        //                        pin.ParentID = parentId;
                        //                    }
                        //                }
                        //            }
                        //        }
                        //    }
                        //}
#endregion
                        /* get id */
                        var strIds = mItem.GetAttributeValue("data-bt", "");
                        strIds = System.Web.HttpUtility.HtmlDecode(strIds);
                        if (!string.IsNullOrEmpty(strIds))
                        {
                            var _jsonId = JsonConvert.DeserializeObject<JsonData>(strIds);
                            if (_jsonId != null)
                            {
                                var Id = _jsonId.id;
                                pin.ID = Id;
                            }
                        }

                        /* get owner name */
                        var objOwnerName = mItem.Descendants().Where(o => o.Name == "a" && o.Attributes["class"] != null &&
                                                                        o.Attributes["class"].Value.Contains("_vwp")).FirstOrDefault();
                        if(objOwnerName != null)
                        {
                            var OwnerName = objOwnerName.InnerText;
                            if(!string.IsNullOrEmpty(OwnerName))
                            {
                                OwnerName = System.Web.HttpUtility.HtmlDecode(OwnerName);
                                pin.OwnerName = OwnerName;
                            }
                            var OwnerHref = objOwnerName.GetAttributeValue("href", "");
                        }

                        /* get time */
                        var objTime = mItem.Descendants().Where(o => o.Name == "a" && o.Attributes["class"] != null &&
                                                                     o.Attributes["class"].Value.Contains("_lie")).FirstOrDefault();
                        if(objTime != null)
                        {
                            var abbr = objTime.Descendants("abbr").FirstOrDefault();
                            var timeStamp = abbr.GetAttributeValue("data-utime", "");
                            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                            var Created_At = dtDateTime.AddSeconds(double.Parse(timeStamp)).ToLocalTime();
                            pin.Created_At = Created_At;
                        }

                        /* get description */
                        var objDes = mItem.Descendants().Where(o => o.Name == "a" && o.Attributes["class"] != null &&
                                                                     o.Attributes["class"].Value.Contains("_3084")).FirstOrDefault();
                        if(objDes != null)
                        {
                            var Des = objDes.InnerText;
                            pin.Description = Des;
                        }

                        /* get Image and link video */
                        var objImgVideo = mItem.Descendants().Where(o => o.Name == "div" && o.Attributes["class"] != null &&
                                                                     o.Attributes["class"].Value.Contains("_5-0i")).FirstOrDefault();
                        if(objImgVideo != null)
                        {
                            /* check video or image */
                            var IsCheck = objImgVideo.GetAttributeValue("aria-label", "").Equals("Video Link");
                            if(IsCheck)
                            {
                                var objVideo = objImgVideo.Descendants("a").FirstOrDefault();
                                if (objVideo != null)
                                {
                                    var VideoLink = objVideo.GetAttributeValue("href", "");
                                    if (!string.IsNullOrEmpty(VideoLink))
                                    {
                                        VideoLink = "https://www.facebook.com" + VideoLink;
                                    }
                                    // link api detail
                                    var LinkApi = VideoLink;
                                    
                                }
                            }
                            

                            var objImg = objImgVideo.Descendants().Where(o => o.Name == "img" && o.Attributes["class"] != null &&
                                                                     o.Attributes["class"].Value.Contains("img")).FirstOrDefault();
                            if(objImg != null)
                            {
                                var ImageUrl = objImg.GetAttributeValue("src", "");
                                if (!string.IsNullOrEmpty(ImageUrl))
                                {
                                    ImageUrl = ImageUrl.Replace("amp;", "");
                                    pin.ImageURL = ImageUrl;
                                }
                            }
                            if (!string.IsNullOrEmpty(pin.ID))
                            {
                                // CrawlerDetail(pin.ID, ref pin);
                                // GetPin(json.jsmods, pin.ID, ref pin);
                                GetPhotoID(json.jsmods,pin.ID, ref pin);
                                GetPin(json.jsmods, pin.ID, ref pin);
                                pin.Type = type;
                                pins.Pins.Add(pin);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                NSLog.Logger.Error("ParseHtmlPost:", ex);
            }
        }

        /// <summary>
        /// parse html tab photo
        /// </summary>
        /// <param name="html"></param>
        public static void ParseHtmlPhoto(JsonData json, ref CMS_CrawlerModels pins)
        {
            try
            {
                string html = json.payload;
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                var _mHtml = doc.DocumentNode.Descendants()
                                             .Where(x => (x.Name == "div" && x.Attributes["class"] != null &&
                                                   x.Attributes["class"].Value.Contains("_4313"))).ToList();
                if(_mHtml != null && _mHtml.Any())
                {
                    foreach(var mItem in _mHtml)
                    {
                        var pin = new PinsModels();
                        /* start get id post */
                        var strIds = mItem.GetAttributeValue("data-bt", "");
                        strIds = System.Web.HttpUtility.HtmlDecode(strIds);
                        if(!string.IsNullOrEmpty(strIds))
                        {
                            var _jsonId = JsonConvert.DeserializeObject<JsonData>(strIds);
                            if(_jsonId != null)
                            {
                                var Id = _jsonId.id;
                                pin.ID = Id;
                            }
                        }
                        /* end get id post */

                        /*start get image url */
                        var _mImg = mItem.Descendants("img").Where(o => o.Attributes["class"] != null && o.Attributes["class"].Value.Contains("scaledImageFitHeight")).ToList();
                        if(_mImg != null && _mImg.Any())
                        {
                            var ImageUrl = _mImg[0].GetAttributeValue("src", "");
                            if(!string.IsNullOrEmpty(ImageUrl))
                            {
                                ImageUrl = ImageUrl.Replace("amp;", "");
                                pin.ImageURL = ImageUrl;
                            }
                        }
                        /* end get image url */

                        /* start get by */
                        var _mBys = mItem.Descendants("div").Where(o => o.Attributes["class"] != null && o.Attributes["class"].Value.Contains("_23p")).ToList();
                        if(_mBys != null && _mBys.Any())
                        {
                            var _mBy = _mBys.LastOrDefault();
                            var By = _mBy.InnerText;
                            var objHref = _mBy.Descendants("a").FirstOrDefault();
                            if(objHref != null)
                            {
                                var href = objHref.GetAttributeValue("href", "");
                            }
                        }
                        /* end get by */
                        if(!string.IsNullOrEmpty(pin.ID))
                        {
                            GetPin(json.jsmods, pin.ID, ref pin);
                            pins.Pins.Add(pin);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                NSLog.Logger.Error("ParseHtmlPhoto:", ex);
            }
        }

        public static void CrawlerDetail(string fbId,ref PinsModels pin)
        {
            try
            {
                var Url = "https://www.facebook.com/ajax/pagelet/generic.php/PhotoViewerInitPagelet?dpr=1&fb_dtsg_ag=AdwP2CkmxqbthyUwtuNGyntNwoZnRqeWS0c-ybaoDCyvqg%3AAdxaAcHQyTfiHpCrGBxNeQxTkMKM_AQsNq70bpE_Vat-gQ&ajaxpipe=1&ajaxpipe_token=AXhBRNxR61W2G51W&no_script_path=1&data={\"fbid\":"+fbId+",\"set\":\"p."+fbId+"\",\"type\":\"1\",\"theater\":null,\"firstLoad\":true,\"ssid\":1536763892151}&__user=100003727776485&__a=1&__dyn=7AgNe-4amaxx2u6Xolg9obHGiqEW8xdLFwxx-6EeAq2i5U4e2C3-7WyUcWwIKaxeUW3Kag4idwJx64e3W9xicwJwpUiwBx61zwzwno8o2fDBw9-6rGUogc8rwFwgE467Uy2adwRwGxO4p8gy85Ofy946e4oC2bixK8Cgepo-cGcBK2C6U4W10Gu15ghyEgx2UTwEwFyFE-3e4U9ogwJw&__req=jsonp_2&__be=1&__pc=PHASED%3ADEFAULT&__rev=4303580&__spin_r=4303580&__spin_b=trunk&__spin_t=1536763736&__adt=2";
                Uri uri = new Uri(Url);
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
                httpWebRequest.Headers["Cookie"] = "fr=0g932KaBNIHkPNSHd.AWUalQQ9AxXL7JpoqimmeZTMmkg.BbMcZu.uD.AAA.0.0.Bbky_F.AWW4mfhc; sb=NmI3W-ffluEtyFHleEWSjhBl; datr=NmI3WwtbosYtTwDtslqJtXZd; wd=1920x944; c_user=100003727776485; xs=38%3AjT_REhmug5Jgrg%3A2%3A1536224023%3A6091%3A726; pl=n; spin=r.4289044_b.trunk_t.1536328620_s.1_v.2_; presence=EDvF3EtimeF1536372678EuserFA21B03727776485A2EstateFDutF1536372678743CEchFDp_5f1B03727776485F2CC; act=1536372716538%2F11";
                httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:61.0) Gecko/20100101 Firefox/61.0";
                httpWebRequest.Accept = "*/*";
                httpWebRequest.Headers["Cache-Control"] = "no-cache";
                httpWebRequest.Headers["Origin"] = "https://www.facebook.com";
                httpWebRequest.Referer = "https://www.facebook.com";
                httpWebRequest.Headers["upgrade-insecure-requests"] = "1";
                httpWebRequest.Timeout = 9000000;
                using (HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var html = streamReader.ReadToEnd();
                        streamReader.Dispose();
                        httpResponse.Dispose();

                        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(html);
                        /* FIND FEEDBACK_TARGET */
                        var script = doc.DocumentNode.Descendants().Where(n => n.Name == "script").ToList();
                        //var innerScript = script.Where(o => !string.IsNullOrEmpty(o.InnerText) && o.InnerText.Contains("require(\"TimeSlice\").guard(function() {require(\"ServerJSDefine\")")).Select(o => o.InnerText).FirstOrDefault();
                        var innerScript = script.Where(o => !string.IsNullOrEmpty(o.InnerText) && o.InnerText.Contains("if (self != top)")).Select(o => o.InnerText).ToList();
                        if(innerScript != null && innerScript.Any())
                        {
                            foreach(var item in innerScript)
                            {
                                findNode(item, "feedbacktarget", 0, fbId, ref pin);
                            }
                            
                        }
                        
                    }
                }
            }
            catch(Exception ex)
            {
                NSLog.Logger.Error("CrawlerDetail:", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="fb_id"></param>
        /// <param name="pin"></param>
        /// <returns></returns>
        public static bool findNode(string input, string key, int start, string fb_id, ref PinsModels pin)
        {
            var jsonfeedbacktarget = findElement(input, "feedbacktarget", 0);
            if (string.IsNullOrEmpty(jsonfeedbacktarget))
                return false;
            try
            {
                var dobj = JsonConvert.DeserializeObject<JsonObject_v2>(jsonfeedbacktarget);
                if (dobj != null)
                {
                    pin.commentTotalCount = dobj.commentcount;
                    pin.sharecount = dobj.sharecount;
                    pin.reactioncount = dobj.reactioncount;
                    pin.ID = dobj.entidentifier;
                    return true;
                    //if (fb_id.Equals(dobj.entidentifier))
                    //{
                    //    pin.commentTotalCount = dobj.commentcount;
                    //    pin.sharecount = dobj.sharecount;
                    //    pin.reactioncount = dobj.reactioncount;
                    //    pin.ID = dobj.entidentifier;
                    //    return true;
                    //}
                    //else
                    //{
                    //    jsonfeedbacktarget = "\"feedbacktarget\":" + jsonfeedbacktarget;
                    //    input = input.Replace(jsonfeedbacktarget, "");
                    //    return findNode(input, key, start, fb_id, ref pin);
                    //}
                }
            }
            catch (Exception ex) {
                NSLog.Logger.Error("findNode:", ex);
            }
            return false;
        }

        /// <summary>
        /// Find node json have like , shared , comment
        /// </summary>
        /// <param name="_input"></param>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public static string findElement(string _input, string key, int start)
        {
            var ret = "";
            try
            {
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
            }
            catch (Exception ex) {
                NSLog.Logger.Error("findElement:", ex);
            };
            return ret;
        }

        /// <summary>
        /// parse data json from string 
        /// </summary>
        /// <param name="strData"></param>
        /// <returns></returns>
        public static JsonData ParseJson(string strData)
        {
            try
            {
                var _json = JsonConvert.DeserializeObject<JsonData>(strData);
                return _json;
            }
            catch(Exception ex)
            {
                NSLog.Logger.Error("ParseJson:", ex);
            }
            return null;
        }
        /// <summary>
        /// param data request
        /// </summary>
        /// <returns></returns>
        public static string _search_keyword_payload(string bqf, string ref_path,string view,int page,string cursor="")
        {
            try
            {
                var sub_query = new sub_query
                {
                   // bqf = "photos-keyword(Awesome+T-shirt)",
                    bqf = bqf,
                    //browse_sid = "0823f876bbf49659dfbb8ae86e172d62",
                    //typeahead_sid = null,
                    vertical = "content",
                    post_search_vertical = null,
                    //intent_data = null,
                    //requestParams = new List<string>(),
                    has_chrono_sort = false,
                    query_analysis = null,
                    subrequest_disabled = false,
                    token_role = "NONE",
                    preloaded_story_ids = new List<string>(),
                    extra_data = null,
                    disable_main_browse_unicorn = false,
                    entry_point_scope = null,
                    entry_point_surface = null,
                    squashed_ent_ids = new List<string>(),
                    source_session_id = null,
                    preloaded_entity_ids = new List<string>(),
                    preloaded_entity_type = null,
                    //high_confidence_argument = null,
                    //logging_unit_id = "browse_serp:6706cad3-e6df-6703-faf1-e4c36f7ed42d",
                    //query_title = null,
                    //query_source = null
                };

                var enc_q = new enc_q
                {
                    view = view,
                    encoded_query = JsonConvert.SerializeObject(sub_query),
                    //encoded_title = "WyJBd2Vzb21lK1Qtc2hpcnQiXQ",
                    encoded_title ="",
                    //logger_source = "www_main",
                    //typeahead_sid = "",
                    //tl_log = false,
                    //impression_id = "0hWqPapNXiSUGrXig",
                    experience_type = "grammar",
                    //ref_path = ref_path,
                    //ref_path = "/search/str/Awesome+T-shirt/photos-keyword",
                    exclude_ids = null,
                    browse_location = "browse_location:browse",
                    //trending_source = null,
                    //reaction_surface = null,
                    //reaction_session_id = null,
                    //topic_id = null,
                    //place_id = null,
                    //has_top_pagelet = true,
                    //display_params = new List<string>(),
                    //em = false,
                    //tr = null,
                    //is_trending = false,
                    callsite = "browse_ui:init_result_set",
                    page_number = page,
                    cursor = cursor
                };
                return JsonConvert.SerializeObject(enc_q);
            }
            catch(Exception ex)
            {
                NSLog.Logger.Error("_search_keyword_payload : ", ex);
            }
            return null;
        }
    }

    public class sub_query
    {
        public string bqf { get; set; }
        public string browse_sid { get; set; }
        public string typeahead_sid { get; set; }
        public string query_analysis { get; set; }
        public string vertical { get; set; }
        public string post_search_vertical { get; set; }
        public string intent_data { get; set; }
        public List<string> requestParams { get; set; }
        public bool has_chrono_sort { get; set; }
        public bool subrequest_disabled { get; set; }
        public string token_role { get; set; }
        public List<string> preloaded_story_ids { get; set; }
        public string extra_data { get; set; }
        public bool disable_main_browse_unicorn { get; set; }
        public string entry_point_scope { get; set; }
        public string entry_point_surface { get; set; }
        public List<string> squashed_ent_ids { get; set; }
        public string source_session_id { get; set; }
        public List<string> preloaded_entity_ids { get; set; }
        public string preloaded_entity_type { get; set; }
        public string high_confidence_argument { get; set; }
        public string logging_unit_id { get; set; }
        public string query_title { get; set; }
        public string query_source { get; set; }
    }

    public class enc_q
    {
        public string typeahead_sid { get; set; }
        public bool tl_log { get; set; }
        public string impression_id { get; set; }
        public string experience_type { get; set; }
        public string ref_path { get; set; }
        public string exclude_ids { get; set; }
        public string browse_location { get; set; }
        public object trending_source { get; set; }
        public object reaction_surface { get; set; }
        public object reaction_session_id { get; set; }
        public string place_id { get; set; }
        public string topic_id { get; set; }
        public bool has_top_pagelet { get; set; }
        public List<string> display_params { get; set; }
        public bool em { get; set; }
        public string tr { get; set; }
        public bool is_trending { get; set; }
        public string callsite { get; set; }
        public int page_number { get; set; }

        public string view { get; set; }
        public string encoded_query { get; set; }
        public string encoded_title { get; set; }
        public string logger_source { get; set; }
        public string cursor { get; set; }
    }

    public class JsonData
    {
        public string payload { get; set; }
        public jsmods jsmods { get; set; }
        public string id { get; set; }
        public string unit_id_result_id { get; set; }
    }

    public class jsmods
    {
        public List<object> require { get; set; }
    }

    public class feedbacktarget
    {
        public string entidentifier { get; set; }
        public int reactioncount { get; set; }
        public int sharecount { get; set; }
        public int commentcount { get; set; }
    }

    public class conversationGuideParams
    {
        public List<string> photo_attachments_list { get; set; }
        public string photo_id { get; set; }

        public conversationGuideParams()
        {
            photo_attachments_list = new List<string>();
        }
    }
}


