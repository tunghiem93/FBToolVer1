﻿using CMS_DTO.CMSCrawler;
using CMS_DTO.CMSImage;
using CMS_DTO.CMSProduct;
using CMS_Shared;
using CMS_Shared.CMSEmployees;
using CMS_Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace CMS_Web.Areas.Admin.Controllers
{
    public class CMSProductsController : BaseController
    {
        private CMSPinFactory _fac;
        public CMSProductsController()
        {
            _fac = new CMSPinFactory();
        }
        // GET: Admin/CMSCategories
        public ActionResult Index()
        {
            CMS_ProductsModels model = new CMS_ProductsModels();
            try
            {
                var _Key = Request["keywordID"] ?? "";
                var _Group = Request["GroupID"] ?? "";
                var FilterModel = new PinFilterDTO();
                FilterModel.PageIndex = Commons.PageIndex;
                FilterModel.PageSize = Commons.PageSize;
                FilterModel.TypeTime = Commons.ESortType1.TimeCreatedAtDecrease.ToString("d");
                model.TypeTime = int.Parse(Commons.ESortType1.TimeCreatedAtDecrease.ToString("d"));
                FilterModel.Sort1 = (byte)Commons.ESortType1.TimeCreatedAtDecrease;
                model.Sort1 = (byte)Commons.ESortType1.TimeCreatedAtDecrease;
                FilterModel.Sort2 = (byte)Commons.ESortType2.ReactionDecrease;
                model.Sort2 = (byte)Commons.ESortType2.ReactionDecrease;
                model.Index = int.Parse(Commons.EIndex.LikeReduce.ToString("d"));
                model.TypePost = int.Parse(Commons.ETypePost.All.ToString("d"));
                #region "Comment"
                //if (Request.Cookies["FromDate"] != null)
                //{
                //    var _FromDate = Convert.ToDateTime(Request.Cookies["FromDate"].Value);
                //    FilterModel.CreatedAtFrom = _FromDate;
                //}
                //if(Request.Cookies["ToDate"] != null)
                //{
                //    var _ToDate = Convert.ToDateTime(Request.Cookies["ToDate"].Value);
                //    FilterModel.CreatedAtTo = _ToDate;
                //}
                //string TypeTime = "", TypePin = "";
                //if (Request.Cookies["TypeTime"] != null)
                //{
                //    TypeTime = Request.Cookies["TypeTime"].Value;
                //    model.TypeTime = Convert.ToInt16(TypeTime);
                //}
                //if (Request.Cookies["TypePin"] != null)
                //{
                //    TypePin = Request.Cookies["TypePin"].Value;
                //    model.TypePin = Convert.ToInt16(TypePin);
                //}

                //if (Request.Cookies["TypeQuantity"] != null)
                //{
                //    var RepinCount = Request.Cookies["TypeQuantity"].Value.ToString();
                //    if(RepinCount == Commons.EQuantityType.ZeroToOne.ToString("d"))
                //    {
                //        FilterModel.PinCountFrom = 0;
                //        FilterModel.PinCountTo = 100;
                //    }

                //    if (RepinCount == Commons.EQuantityType.OneToTwo.ToString("d"))
                //    {
                //        FilterModel.PinCountFrom = 100;
                //        FilterModel.PinCountTo = 200;
                //    }
                //    if (RepinCount == Commons.EQuantityType.TwoToThree.ToString("d"))
                //    {
                //        FilterModel.PinCountFrom = 200;
                //        FilterModel.PinCountTo = 300;
                //    }
                //    if (RepinCount == Commons.EQuantityType.ThreeToFour.ToString("d"))
                //    {
                //        FilterModel.PinCountFrom = 300;
                //        FilterModel.PinCountTo = 400;
                //    }
                //    if (RepinCount == Commons.EQuantityType.FourToFive.ToString("d"))
                //    {
                //        FilterModel.PinCountFrom = 400;
                //        FilterModel.PinCountTo = 500;
                //    }
                //    if (RepinCount == Commons.EQuantityType.MoreFive.ToString("d"))
                //    {
                //        FilterModel.PinCountFrom = 500;
                //    }
                //    model.TypeQuantity = Convert.ToInt16(RepinCount);
                //}

                //if(Request.Cookies["Keywords"] != null)
                //{
                //    var Keywords = Request.Cookies["Keywords"].Value;
                //    char[] separator = new char[] { ',' };
                //    var ListKeyword = CommonHelper.ParseStringToList(Keywords, separator);
                //    FilterModel.lstKeyWordID = ListKeyword;
                //    model.listKeywords = ListKeyword;
                //}
                #endregion
                model.ListTime = getListTime();
                model.ListQuantity = getListQuantity();
                model.ListIndex = getListIndex();
                model.ListSort2 = getListSort2();
               // model.ListRePin = getListRepinCount();
                ViewBag.Keywords = getListKeyword();
                ViewBag.ListGroupFanpage = getListGroupKeyword();
                ViewBag.ListTypePost = getListTypePost();
                if (!string.IsNullOrEmpty(_Key))
                {
                    FilterModel.LstKeyWordID.Add(_Key);
                    model.listKeywords.Add(_Key);
                }
                if (!string.IsNullOrEmpty(_Group))
                {
                    FilterModel.LstGroupID.Add(_Group);
                    
                    var _lstKeywords = getListKeyWordByGroup(_Group);
                    model.listKeywords.AddRange(_lstKeywords);
                }

                var _pinModels = new List<PinsModels>();
                var msg = "";
                int totalPin = 0;
                var result = _fac.GetPin(ref _pinModels, ref totalPin, FilterModel, ref msg);
                if(result)
                {
                    model.Crawler.Pins = _pinModels.OrderBy(x => x.Created_At).ToList();
                    #region "Comment"
                    //if (TypeTime.Equals(Commons.ETimeType.TimeReduce.ToString("d")))
                    //{
                    //    model.Crawler.Pins = model.Crawler.Pins.OrderByDescending(x => x.Created_At).ToList();
                    //}
                    //else if (TypeTime.Equals(Commons.ETimeType.TimeIncrease.ToString("d")))
                    //{
                    //    model.Crawler.Pins = model.Crawler.Pins.OrderBy(x => x.Created_At).ToList();
                    //}
                    //else if (TypePin.Equals(Commons.ETimeType.PinReduce.ToString("d")))
                    //{
                    //    model.Crawler.Pins = model.Crawler.Pins.OrderByDescending(x => x.Repin_count).ToList();
                    //}
                    //else if (TypePin.Equals(Commons.ETimeType.PinIncrease.ToString("d")))
                    //{
                    //    model.Crawler.Pins = model.Crawler.Pins.OrderBy(x => x.Repin_count).ToList();
                    //}
#endregion
                } 
            }
            catch(Exception ex)
            {
            }
            return View(model);
        }

        public ActionResult Search()
        {
            try
            {
                var FilterModel = new PinFilterDTO();
                FilterModel.PageIndex = Commons.PageIndex;
                FilterModel.PageSize = Commons.PageSize;
                FilterModel.CreatedDateFrom = null;
                FilterModel.CreatedDateTo = null;
                // var _Key = Request["Key"] ?? "";
                var TypeTime = Request["TypeTime"] ?? "2";
                //var Sort1 = Request["Sort1"] ?? "";
                var Sort2 = Request["Sort2"] ?? "2";
                var TypePost = Request["TypePost"] ?? "0";
                //  var TypePin = Request["TypePin"] ?? "";
                var _TypeQuantity = Request["TypeQuantity"];
                int TypeQuantity = -1;
                if (!string.IsNullOrEmpty(_TypeQuantity))
                {
                    TypeQuantity = Convert.ToInt16(_TypeQuantity);
                }
                var Keywords = Request["listKeywords"] ?? null;
                char[] separator = new char[] { ',' };
                var ListKeyword = CommonHelper.ParseStringToList(Keywords, separator);
                var _FromDate = Convert.ToDateTime(Request["FromDate"]);
                var _ToDate = Convert.ToDateTime(Request["ToDate"]);
                #region "comment"
                //cache data
                //Response.Cookies["TypeTime"].Value = TypeTime.ToString();
                //Response.Cookies["TypeTime"].Expires = DateTime.Now.AddYears(1); // add expiry time

                //Response.Cookies["TypePin"].Value = TypePin.ToString();
                //Response.Cookies["TypePin"].Expires = DateTime.Now.AddYears(1); // add expiry time

                //Response.Cookies["FromDate"].Value = _FromDate.ToString();
                //Response.Cookies["FromDate"].Expires = DateTime.Now.AddYears(1); // add expiry time
                //Response.Cookies["ToDate"].Value = _ToDate.ToString();
                //Response.Cookies["ToDate"].Expires = DateTime.Now.AddYears(1); // add expiry time
                //if(TypeQuantity != 0)
                //{
                //    Response.Cookies["TypeQuantity"].Value = TypeQuantity.ToString();
                //    Response.Cookies["TypeQuantity"].Expires = DateTime.Now.AddYears(1); // add expiry time
                //}
                #endregion
                FilterModel.CreatedAtFrom = _FromDate;
                FilterModel.CreatedAtTo = _ToDate;

                var _Group = Request["GroupID"] ?? "";
                if (!string.IsNullOrEmpty(_Group))
                {
                    FilterModel.LstGroupID.Add(_Group);

                    var _lstKeywords = getListKeyWordByGroup(_Group);
                    FilterModel.LstKeyWordID.AddRange(_lstKeywords);
                }

                if (TypeQuantity.ToString() == Commons.EQuantityType.ZeroToOne.ToString("d"))
                {
                    FilterModel.PinCountFrom = 0;
                    FilterModel.PinCountTo = 100;
                }
                if (TypeQuantity.ToString() == Commons.EQuantityType.OneToTwo.ToString("d"))
                {
                    FilterModel.PinCountFrom = 100;
                    FilterModel.PinCountTo = 200;
                }
                if (TypeQuantity.ToString() == Commons.EQuantityType.TwoToThree.ToString("d"))
                {
                    FilterModel.PinCountFrom = 200;
                    FilterModel.PinCountTo = 300;
                }
                if (TypeQuantity.ToString() == Commons.EQuantityType.ThreeToFour.ToString("d"))
                {
                    FilterModel.PinCountFrom = 300;
                    FilterModel.PinCountTo = 400;
                }
                if (TypeQuantity.ToString() == Commons.EQuantityType.FourToFive.ToString("d"))
                {
                    FilterModel.PinCountFrom = 400;
                    FilterModel.PinCountTo = 500;
                }
                if (TypeQuantity.ToString() == Commons.EQuantityType.MoreFive.ToString("d"))
                {
                    FilterModel.PinCountFrom = 500;
                }

                if (ListKeyword != null && ListKeyword.Count > 0)
                {
                    FilterModel.LstKeyWordID = ListKeyword;
                    // Response.Cookies["Keywords"].Value = Keywords.ToString();
                    //  Response.Cookies["Keywords"].Expires = DateTime.Now.AddYears(1); // add expiry time
                }

                FilterModel.TypeTime = TypeTime;
                var tmp = 0;
                int.TryParse(TypeTime, out tmp);
                FilterModel.Sort1 = tmp;
                int.TryParse(Sort2, out tmp);
                FilterModel.Sort2 = tmp;
                int.TryParse(TypePost, out tmp);
                FilterModel.TypePost = tmp;

                var modelCrawler = new CMS_CrawlerModels();
                var _pinModels = new List<PinsModels>();
                var msg = "";
                int totalPin = 0;
                var result = _fac.GetPin(ref _pinModels, ref totalPin, FilterModel, ref msg);
                if (result)
                {
                    modelCrawler.Pins = _pinModels;
                }
                return PartialView("_ListItem", modelCrawler);
            }
            catch (Exception ex) { }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        public ActionResult LoadGrid()
        {
            //var model = _factory.GetList();
            return null; // PartialView("_ListData", model);
        }

        public ActionResult Create()
        {
            CMS_ProductsModels model = new CMS_ProductsModels();
            return PartialView("_Create", model);
        }

        public CMS_ProductsModels GetDetail(string Id)
        {
            return null; // _factory.GetDetail(Id);
        }

        [HttpPost]
        public ActionResult Create(CMS_ProductsModels model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return PartialView("_Create", model);
                }
                byte[] photoByte = null;
                Dictionary<int, byte[]> lstImgByte = new Dictionary<int, byte[]>();
                var data = new List<CMS_ImagesModels>();
                if (model.PictureUpload.Length > 0 && model.PictureUpload.Any() && model.PictureUpload[0] != null)
                {
                    
                }
                
                var msg = "";
                var result = true; // _factory.CreateOrUpdate(model, ref msg);
                if (result)
                {
                    foreach (var item in data)
                    {
                        if (!string.IsNullOrEmpty(item.ImageURL) && item.PictureByte != null)
                        {
                            if (System.IO.File.Exists(Server.MapPath("~/Uploads/Products/" + item.TempImageURL)))
                            {
                                ImageHelper.Me.TryDeleteImageUpdated(Server.MapPath("~/Uploads/Products/" + item.TempImageURL));
                            }

                            var path = Server.MapPath("~/Uploads/Products/" + item.ImageURL);
                            MemoryStream ms = new MemoryStream(lstImgByte[item.OffSet], 0, lstImgByte[item.OffSet].Length);
                            ms.Write(lstImgByte[item.OffSet], 0, lstImgByte[item.OffSet].Length);
                            System.Drawing.Image imageTmp = System.Drawing.Image.FromStream(ms, true);

                            ImageHelper.Me.SaveCroppedImage(imageTmp, path, item.ImageURL, ref photoByte,400,Commons.WidthProduct,Commons.HeightProduct);
                            model.PictureByte = photoByte;
                        }
                    }
                    return RedirectToAction("Index");
                }
                    
                ModelState.AddModelError("ProductCode", msg);
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView("_Create", model);
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView("_Create", model);
            }
        }        

        [HttpPost]
        public PartialViewResult AddImageItem(int OffSet, int Length)
        {

            List<CMS_ImagesModels> model = new List<CMS_ImagesModels>();
            var _OffSet = OffSet;
            for (int i = 0; i < Length; i++)
            {
                model.Add(new CMS_ImagesModels
                {
                    OffSet = _OffSet,
                    IsDeleted = false
                });
                _OffSet = _OffSet + 1;
            }
            return PartialView("_ListItem", model);
        }

        [HttpPost]
        public ActionResult DeleteImage(string Id,string ProductId)
        {
            try
            {
                string msg = "";
                var result = true; // _factory.DeleteImage(Id,ref msg);
                if (!result)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return new HttpStatusCodeResult(400, "Have an error when you delete a Payment Method");
                }
                // delete image for folder
                if (System.IO.File.Exists(Server.MapPath("~/Uploads/Products/" + msg)))
                {
                    ImageHelper.Me.TryDeleteImageUpdated(Server.MapPath("~/Uploads/Products/" + msg));
                }
                var model = new CMS_ProductsModels();
               
                var _OffSet = 0;
                
                return PartialView("_ListItem", model);
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        public ActionResult ProductDetail(string id,string Key)
        {
            var modelCrawler = new CMS_CrawlerModels();
            try
            {
                var model = new PinsModels();
                CrawlerHelper.Get_Tagged_PinsDetail(ref model, id);
                CrawlerHelper.Get_Tagged_OrtherPins(ref modelCrawler, Key, Commons.PinOrtherDefault, "", 1, id);
                modelCrawler.Pin = model;
            }
            catch (Exception) { }
            return View(modelCrawler);
        }

        public ActionResult LoadScroll(PinFilterDTO pinFilter)
        {
            try
            {
                if(!string.IsNullOrEmpty(pinFilter.Url))
                {
                    NameValueCollection QueryString = CommonHelper.GetQueryParameters(pinFilter.Url);
                    var _Key = QueryString["keywordID"] ?? "";
                    var _Group = QueryString["GroupID"] ?? "";

                    if (!string.IsNullOrEmpty(_Key))
                    {
                        pinFilter.LstKeyWordID.Add(_Key);
                    }
                    if (!string.IsNullOrEmpty(_Group))
                    {
                        pinFilter.LstGroupID.Add(_Group);

                        var _lstKeywords = getListKeyWordByGroup(_Group);
                        pinFilter.LstKeyWordID.AddRange(_lstKeywords);
                    }
                }

                if (pinFilter.LstKeyWordID != null && pinFilter.LstKeyWordID.Count > 0)
                {
                    if (string.IsNullOrEmpty(pinFilter.LstKeyWordID[0]))
                        pinFilter.LstKeyWordID = null;
                }
                var modelCrawler = new CMS_CrawlerModels();
                var _pinModels = new List<PinsModels>();
                var msg = "";
                pinFilter.PageSize = Commons.PageSize;
                int totalPin = 0;
                var result = _fac.GetPin(ref _pinModels, ref totalPin, pinFilter, ref msg);
                if (result)
                {
                    modelCrawler.Pins = _pinModels;
                }
                return PartialView("_ListItem", modelCrawler);
            }
            catch (Exception) { }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        
        public ActionResult DeletePin(string ID)
        {
            var msg = "";
            var result = _fac.HidePin(ID, "Admin", ref msg);
            if (result)
            {
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
    }
}