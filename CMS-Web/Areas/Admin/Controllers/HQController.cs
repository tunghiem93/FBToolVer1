using CMS_DTO.CMSBase;
using CMS_DTO.CMSSession;
using CMS_Shared;
using CMS_Shared.CMSGroupKeywords;
using CMS_Shared.Keyword;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CMS_Web.Areas.Admin.Controllers
{
    public class HQController : Controller
    {
        // GET: Administration/HQ
        public UserSession CurrentUser
        {
            get
            {
                if (System.Web.HttpContext.Current.Session["User"] != null)
                    return (UserSession)System.Web.HttpContext.Current.Session["User"];
                else
                    return new UserSession();
            }
        }
        public List<SelectListItem> getListTime()
        {
            var _lstTime = new List<SelectListItem>() {
                //new SelectListItem() {Text="Latest on Post",Value=Commons.ETimeType.TimeReduce.ToString("d") },
                //new SelectListItem() {Text="Latest on tool",Value=Commons.ETimeType.ToolReduce.ToString("d") },
                //new SelectListItem() {Text="Oldest on Post",Value=Commons.ETimeType.TimeIncrease.ToString("d")},
                //new SelectListItem() {Text="Oldest on tool",Value=Commons.ETimeType.ToolIncrease.ToString("d")},

                //new SelectListItem() {Text="Repin reduce",Value=Commons.ETimeType.PinReduce.ToString("d") },
                //new SelectListItem() {Text="Repin increase",Value=Commons.ETimeType.PinIncrease.ToString("d")},
                //new SelectListItem() {Text="Custom",Value=Commons.ETimeType.TimeCustom.ToString("d")},

                new SelectListItem() {Text="Latest on Post",Value=Commons.ESortType1.TimeCreatedAtDecrease.ToString("d") },
                new SelectListItem() {Text="Latest on tool",Value=Commons.ESortType1.TimeOnToolDecrease.ToString("d") },
                new SelectListItem() {Text="Oldest on Post",Value=Commons.ESortType1.TimeCreatedAtIncrease.ToString("d")},
                new SelectListItem() {Text="Oldest on tool",Value=Commons.ESortType1.TimeOnToolIncrease.ToString("d")},
            };

            return _lstTime;
        }

        public List<SelectListItem> getListRepinCount()
        {
            var _lstTime = new List<SelectListItem>() {
                new SelectListItem() {Text="Pin reduce",Value=Commons.EPinType.PinReduce.ToString("d") },
                new SelectListItem() {Text="Pin increase",Value=Commons.EPinType.PinIncrease.ToString("d")},
                //new SelectListItem() {Text="Custom",Value=Commons.ETimeType.TimeCustom.ToString("d")},
            };

            return _lstTime;
        }

        public List<SelectListItem> getListQuantity()
        {
            var _lstQuantity = new List<SelectListItem>() {
                new SelectListItem() { Text = "0 - 100", Value = Commons.EQuantityType.ZeroToOne.ToString("d") },
                new SelectListItem() { Text = "100 - 200", Value = Commons.EQuantityType.OneToTwo.ToString("d") },
                new SelectListItem() { Text = "200 - 300", Value = Commons.EQuantityType.TwoToThree.ToString("d") },
                new SelectListItem() { Text = "300 - 400", Value = Commons.EQuantityType.ThreeToFour.ToString("d") },
                new SelectListItem() { Text = "400 - 500", Value = Commons.EQuantityType.FourToFive.ToString("d") },
                new SelectListItem() { Text = "> 500", Value = Commons.EQuantityType.MoreFive.ToString("d") },
            };
            return _lstQuantity;
        }

        public List<SelectListItem> getListIndex()
        {
            var _lstIndex = new List<SelectListItem>() {
                new SelectListItem() { Text = "Latest on Like", Value = Commons.EIndex.LikeReduce.ToString("d") },
                new SelectListItem() { Text = "Latest on Comment", Value = Commons.EIndex.CommentReduce.ToString("d") },
                new SelectListItem() { Text = "Latest on Share", Value = Commons.EIndex.ShareReduce.ToString("d") },
            };
            return _lstIndex;
        }

        public List<SelectListItem> getListSort2()
        {
            var _lstIndex = new List<SelectListItem>() {
                new SelectListItem() { Text = "Reaction Decrease", Value = Commons.ESortType2.ReactionDecrease.ToString("d") },
                new SelectListItem() { Text = "Share Decrease", Value = Commons.ESortType2.ShareDecrease.ToString("d") },
                new SelectListItem() { Text = "Comment Decrease", Value = Commons.ESortType2.CommentDecrease.ToString("d") },
                new SelectListItem() { Text = "Reaction Increase", Value = Commons.ESortType2.ReactionIncrease.ToString("d") },
                new SelectListItem() { Text = "Share Increase", Value = Commons.ESortType2.ShareIncrease.ToString("d") },
                new SelectListItem() { Text = "Comment Increase", Value = Commons.ESortType2.CommentIncrease.ToString("d") },
            };
            return _lstIndex;
        }

        public List<SelectListItem> getListKeyword()
        {
            var _fac = new CMSKeywordFactory();
            var _facGroup = new CMSGroupKeywordsFactory();
            var data = _fac.GetList();
            var lstKeyword = new List<SelectListItem>();
            if(data != null && data.Any())
            {
                foreach(var item in data)
                {
                    lstKeyword.Add(new SelectListItem
                    {
                        Value = item.Id,
                        Text = item.KeySearch
                    });
                }
            }
            return lstKeyword;
        }

        public List<SelectListItem> getListGroupKeyword()
        {
            var _fac = new CMSGroupKeywordsFactory();
            var data = _fac.GetList();
            var lstGroupKeyword = new List<SelectListItem>();
            if (data != null && data.Any())
            {
                foreach (var item in data)
                {
                    lstGroupKeyword.Add(new SelectListItem
                    {
                        Value = item.Id,
                        Text = item.Name
                    });
                }
            }
            return lstGroupKeyword;
        }

        public List<string> getListKeyWordByGroup(string GroupId)
        {
            var _fac = new CMSKeywordFactory();
            var data = _fac.GetList(GroupId);
            var lstString = new List<string>();
            if(data != null && data.Any())
            {
                foreach(var item in data)
                {
                    lstString.Add(item.Id);
                }
            }
            return lstString;
        }

        public List<SelectListItem> getListTypePost()
        {
            var _lstTypePost = new List<SelectListItem>() {
                new SelectListItem() { Text = "All", Value = Commons.ETypePost.All.ToString("d") },
                new SelectListItem() { Text = "Photos", Value = Commons.ETypePost.Photo.ToString("d") },
                new SelectListItem() { Text = "No Photos", Value = Commons.ETypePost.NoPhoto.ToString("d") },
                new SelectListItem() { Text = "Videos", Value = Commons.ETypePost.Video.ToString("d") },
                new SelectListItem() { Text = "No Videos", Value = Commons.ETypePost.NoVideo.ToString("d") },
            };
            return _lstTypePost;
        }
    }
}