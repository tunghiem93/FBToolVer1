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

namespace CMS_Shared.Utilities
{
    public class CrawlerFBToolHelpers
    {
        private const string api_url = "https://www.facebook.com/ajax/pagelet/generic.php/BrowseScrollingSetPagelet";
        public static void CrawlerNow(string q,string ref_path,string view,byte type)
        {
            try
            {
                String timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssffff");
                var obj = _search_keyword_payload(q,ref_path,view);
                var url = "https://www.facebook.com/ajax/pagelet/generic.php/BrowseScrollingSetPagelet?dpr=1&data="+obj+"&__user=100003727776485&__a=1&__be=1&__pc=PHASED:DEFAULT&__spin_t="+ timeStamp + "";
                Uri uri = new Uri(url);
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
                httpWebRequest.Accept = "*/*";
                httpWebRequest.Headers["Cache-Control"] = "no-cache";
                httpWebRequest.Headers["Origin"] = "https://www.facebook.com";
                httpWebRequest.Referer = "https://www.facebook.com";
                httpWebRequest.Headers["upgrade-insecure-requests"] = "1";
                httpWebRequest.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Ubuntu Chromium/62.0.3202.94 Chrome/62.0.3202.94 Safari/537.36";
                httpWebRequest.Headers["Cookie"] = "fr=0g932KaBNIHkPNSHd.AWUalQQ9AxXL7JpoqimmeZTMmkg.BbMcZu.uD.AAA.0.0.Bbky_F.AWW4mfhc; sb=NmI3W-ffluEtyFHleEWSjhBl; datr=NmI3WwtbosYtTwDtslqJtXZd; wd=1920x944; c_user=100003727776485; xs=38%3AjT_REhmug5Jgrg%3A2%3A1536224023%3A6091%3A726; pl=n; spin=r.4289044_b.trunk_t.1536328620_s.1_v.2_; presence=EDvF3EtimeF1536372678EuserFA21B03727776485A2EstateFDutF1536372678743CEchFDp_5f1B03727776485F2CC; act=1536372716538%2F11";
                httpWebRequest.Timeout = 90000;
                using (HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var data = streamReader.ReadToEnd();
                        if(!string.IsNullOrEmpty(data))
                        {
                            data = data.Replace("for (;;);", "");
                            var json = ParseJson(data);
                            if(json != null)
                            {
                                if(string.IsNullOrEmpty(json.payload))
                                {
                                    NSLog.Logger.Info("response-data-error-account-suppend");
                                }
                                else
                                {
                                    /* parse html */
                                    var html = json.payload;
                                    if(type == (byte)Commons.EType.Photo)
                                    {
                                        ParseHtmlPhoto(html);
                                    }

                                    if(type == (byte)Commons.EType.Post)
                                    {
                                        ParseHtmlPostAndPeople(html);
                                    }

                                    if(type == (byte)Commons.EType.People)
                                    {
                                        ParseHtmlPostAndPeople(html);
                                    }
                                    
                                    if(type == (byte)Commons.EType.Video)
                                    {
                                        ParseHtmlVideo(html);
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


        public static void ParseHtmlVideo(string html)
        {
            try
            {
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
                        var strIds = mItem.GetAttributeValue("data-bt", "");
                        strIds = System.Web.HttpUtility.HtmlDecode(strIds);
                        if (!string.IsNullOrEmpty(strIds))
                        {
                            var _jsonId = JsonConvert.DeserializeObject<JsonData>(strIds);
                            if (_jsonId != null)
                            {
                                var Id = _jsonId.id;
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
                                }
                            }
                        }

                        /* get owner name */
                        var objOwnerName = mItem.Descendants().Where(o => o.Name == "div" && o.Attributes["class"] != null
                                                                    && o.Attributes["class"].Value.Contains("_4ovi")).FirstOrDefault();
                        if(objOwnerName != null)
                        {
                            var OwnerName = objOwnerName.InnerText;
                        }

                        /* get description */
                        var objDes = mItem.Descendants().Where(o => o.Name == "div" && o.Attributes["class"] != null
                                                                    && o.Attributes["class"].Value.Contains("_4ovj _4ovl")).FirstOrDefault();
                        if (objDes != null)
                        {
                            var Des = objDes.InnerText;
                            Des = System.Web.HttpUtility.HtmlDecode(Des);
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
        public static void ParseHtmlPostAndPeople(string html)
        {
            try
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                var _mHtml = doc.DocumentNode.Descendants()
                                             .Where(x => (x.Name == "div" && x.Attributes["class"] != null &&
                                                   x.Attributes["class"].Value.Contains("_5bl2 _3u1 _41je _440e"))).ToList();

                if(_mHtml != null && _mHtml.Any())
                {
                    foreach(var mItem in _mHtml)
                    {
                        /* get id */
                        var strIds = mItem.GetAttributeValue("data-bt", "");
                        strIds = System.Web.HttpUtility.HtmlDecode(strIds);
                        if (!string.IsNullOrEmpty(strIds))
                        {
                            var _jsonId = JsonConvert.DeserializeObject<JsonData>(strIds);
                            if (_jsonId != null)
                            {
                                var Id = _jsonId.id;
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
                        }

                        /* get description */
                        var objDes = mItem.Descendants().Where(o => o.Name == "a" && o.Attributes["class"] != null &&
                                                                     o.Attributes["class"].Value.Contains("_3084")).FirstOrDefault();
                        if(objDes != null)
                        {
                            var Des = objDes.InnerText;
                        }

                        /* get Image and link video */
                        var objImgVideo = mItem.Descendants().Where(o => o.Name == "div" && o.Attributes["class"] != null &&
                                                                     o.Attributes["class"].Value.Contains("_5-0i")).FirstOrDefault();
                        if(objImgVideo != null)
                        {
                            var objVideo = objImgVideo.Descendants("a").FirstOrDefault();
                            if(objVideo != null)
                            {
                                var VideoLink = objVideo.GetAttributeValue("href", "");
                                if(!string.IsNullOrEmpty(VideoLink))
                                {
                                    VideoLink = "https://www.facebook.com"+VideoLink;
                                }
                            }

                            var objImg = objImgVideo.Descendants().Where(o => o.Name == "img" && o.Attributes["class"] != null &&
                                                                     o.Attributes["class"].Value.Contains("scaledImageFitWidth")).FirstOrDefault();
                            if(objImg != null)
                            {
                                var ImageUrl = objImg.GetAttributeValue("src", "");
                                if (!string.IsNullOrEmpty(ImageUrl))
                                {
                                    ImageUrl = ImageUrl.Replace("amp;", "");
                                }
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
        public static void ParseHtmlPhoto(string html)
        {
            try
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                var _mHtml = doc.DocumentNode.Descendants()
                                             .Where(x => (x.Name == "div" && x.Attributes["class"] != null &&
                                                   x.Attributes["class"].Value.Contains("_4313"))).ToList();
                if(_mHtml != null && _mHtml.Any())
                {
                    foreach(var mItem in _mHtml)
                    {
                        /* start get id post */
                        var strIds = mItem.GetAttributeValue("data-bt", "");
                        strIds = System.Web.HttpUtility.HtmlDecode(strIds);
                        if(!string.IsNullOrEmpty(strIds))
                        {
                            var _jsonId = JsonConvert.DeserializeObject<JsonData>(strIds);
                            if(_jsonId != null)
                            {
                                var Id = _jsonId.id;
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
                    }
                }
            }
            catch(Exception ex)
            {
                NSLog.Logger.Error("ParseHtmlPhoto:", ex);
            }
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
        public static string _search_keyword_payload(string bqf, string ref_path,string view)
        {
            try
            {
                var sub_query = new sub_query
                {
                   // bqf = "photos-keyword(Awesome+T-shirt)",
                    bqf = bqf,
                    browse_sid = "0823f876bbf49659dfbb8ae86e172d62",
                    typeahead_sid = null,
                    vertical = "content",
                    post_search_vertical = null,
                    intent_data = null,
                    requestParams = new List<string>(),
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
                    high_confidence_argument = null,
                    logging_unit_id = "browse_serp:6706cad3-e6df-6703-faf1-e4c36f7ed42d",
                    query_title = null,
                    query_source = null
                };

                var enc_q = new enc_q
                {
                    view = view,
                    encoded_query = JsonConvert.SerializeObject(sub_query),
                    encoded_title = "WyJBd2Vzb21lK1Qtc2hpcnQiXQ",
                    logger_source = "www_main",
                    typeahead_sid = "",
                    tl_log = false,
                    impression_id = "0hWqPapNXiSUGrXig",
                    experience_type = "grammar",
                    ref_path = ref_path,
                    //ref_path = "/search/str/Awesome+T-shirt/photos-keyword",
                    exclude_ids = null,
                    browse_location = "browse_location:browse",
                    trending_source = null,
                    reaction_surface = null,
                    reaction_session_id = null,
                    topic_id = null,
                    place_id = null,
                    has_top_pagelet = true,
                    display_params = new List<string>(),
                    em = false,
                    tr = null,
                    is_trending = false,
                    callsite = "browse_ui:init_result_set",
                    page_number = 2,
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
    }

    public class JsonData
    {
        public string payload { get; set; }
        public string id { get; set; }
    }
}


