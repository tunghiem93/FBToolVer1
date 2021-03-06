﻿using CMS_DTO.CMSCrawler;
using CMS_DTO.CMSKeyword;
using CMS_Shared;
using CMS_Shared.CMSEmployees;
using CMS_Shared.Keyword;
using CMS_Shared.Utilities;
using CMS_Web.Web.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace CMS_Web.Areas.Admin.Controllers
{
    [NuAuth]
    public class CMSKeywordsController : BaseController
    {
        // GET: Admin/GroupSearchs
        private CMSKeywordFactory _factory;
        private List<string> ListItem = null;
        public CMSKeywordsController()
        {
            _factory = new CMSKeywordFactory();
            ListItem = new List<string>();
            //ListItem = _factory.GetList().Select(o=>o.KeySearch).ToList();
            var lstItem = _factory.GetList();
            if (lstItem != null)
                ListItem = lstItem.Select(o=>o.KeySearch).ToList();

            ViewBag.ListGroupKey = getListGroupKeyword();
        }

        public ActionResult Index()
        {
            CMS_KeywordModels model = new CMS_KeywordModels();
            return View(model);
        }

        public ActionResult LoadGrid(CMS_KeywordModels item)
        {
            try
            {
                var msg = "";
                bool isCheck = true;
                if (item.KeySearch != null && item.KeySearch.Length > 0)
                {
                    var temp = ListItem.Where(o => o.Trim() == item.KeySearch.Trim()).FirstOrDefault();
                    if (temp == null)
                    {
                        var result = _factory.CreateOrUpdate(item, ref msg);
                        if (!result)
                        {
                            if (!string.IsNullOrEmpty(msg))
                                ViewBag.DuplicateKeyword = msg;
                            else
                                isCheck = false;
                        }                        
                    }
                    else
                    {
                        ViewBag.DuplicateKeyword = "Duplicate Keyword!";
                    }
                }
                if (isCheck)
                {
                    var model = _factory.GetList();
                    return PartialView("_ListData", model);
                }
                else
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                //_logger.Error("Keyword_Search: " + e);
                return new HttpStatusCodeResult(400, e.Message);
            }            
        }

        public ActionResult AddKeyToGroup(string KeyID, string GroupKeyID)
        {
            var msg = "";
            var result = _factory.AddKeyToGroup(KeyID, GroupKeyID, ref msg);
            if (result)
            {
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        public ActionResult RemoveKeyFromGroup(string KeyID, string GroupKeyID)
        {
            var msg = "";
            var result = _factory.RemoveKeyFromGroup(KeyID, GroupKeyID, ref msg);
            if (result)
            {
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        #region For row
        public ActionResult CrawlerKeyword(string ID, string Key)
        {
            var msg = "";
            new Thread(() => { _factory.CrawlData(ID, "Admin", ref msg); }).Start();
            var result = true;
            if (result)
            {
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        public ActionResult Delete(string ID)
        {
            var msg = "";
            var result = _factory.Delete(ID, "Admin", ref msg);
            if (result)
            {
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        public ActionResult DeleteAndClearPost(string ID)
        {
            var msg = "";
            //var result = _factory.DeleteAndRemoveDB(ID, ref msg);
            var result = _factory.DeleteAndRemoveDBCommand(ID, ref msg);

            if (result)
            {
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        #endregion

        #region For All
        public ActionResult KeywordCrawlAll(string ID, string Key)
        {
            var msg = "";

            //_factory.CrawlAllKeyWords("Admin", ref msg);
            new Thread(() => {_factory.CrawlAllKeyWords("Admin", ref msg);

            }).Start();

            var result = true;
            if (result)
            {
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        public ActionResult DeleteAll()
        {
            var msg = "";
            var result = _factory.DeleteAll("Admin", ref msg);
            if (result)
            {
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        public ActionResult DeleteAndClearPostAll()
        {
            var msg = "";
            var result = _factory.DeleteAndRemoveDBAll(ref msg);

            if (result)
            {
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        #endregion
    }
}