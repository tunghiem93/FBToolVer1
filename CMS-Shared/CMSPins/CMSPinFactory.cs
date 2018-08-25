using CMS_DTO.CMSCrawler;
using CMS_DTO.CMSEmployee;
using CMS_Entity;
using CMS_Entity.Entity;
using CMS_Shared.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CMS_Shared.CMSEmployees
{
    public class CMSPinFactory
    {
        private static Semaphore m_Semaphore = new Semaphore(1, 1); /* semaphore for create pin data */

        public bool CreateOrUpdate(List<PinsModels> lstPin, string KeyWordID, string createdBy, ref string msg)
        {
            NSLog.Logger.Info("CreateOrUpdatePin: " + KeyWordID, lstPin);
            var result = true;

            m_Semaphore.WaitOne();
            try
            {
                using (var _db = new CMS_Context())
                {
                    _db.Database.CommandTimeout = 500;
                    lstPin = lstPin.GroupBy(x => x.ID).Select(x => x.First()).ToList();
                    lstPin = lstPin.Where(x => !string.IsNullOrEmpty(x.ID) && x.ID.Length <= 60).ToList();
                    var lstPinID = lstPin.Select(o => o.ID).ToList();
                    var lstPinUpdate = _db.CMS_Pin.Where(o => lstPinID.Contains(o.ID)).ToList();
                    var lstPinUpdateID = lstPinUpdate.Select(o => o.ID).ToList();
                    var lstPinInsert = lstPin.Where(o => !lstPinUpdateID.Contains(o.ID)).ToList();

                    /* update pin */
                    foreach (var uPin in lstPinUpdate)
                    {
                        var checkPin = lstPin.Where(o => o.ID == uPin.ID).FirstOrDefault();
                        if (checkPin != null)
                        {
                            /* update days count */
                            if (uPin.UpdatedDate.Value.Day != DateTime.Now.Day)
                                uPin.DayCount++;

                            /* update other info */
                            uPin.Created_At = checkPin.Created_At < Commons.MinDate ? uPin.Created_At : checkPin.Created_At;
                            uPin.Repin_count = checkPin.Repin_count;
                            uPin.ReactionCount = checkPin.reactioncount;
                            uPin.ShareCount = checkPin.sharecount;
                            uPin.CommentCount = checkPin.commentTotalCount;
                            uPin.Description = checkPin.Description;
                            uPin.UpdatedBy = createdBy;
                            uPin.UpdatedDate = DateTime.Now;
                        }
                    }

                    /* insert new pin */
                    var listInsertDB = new List<CMS_Pin>();
                    foreach (var pin in lstPinInsert)
                    {
                        listInsertDB.Add(new CMS_Pin()
                        {
                            ID = pin.ID,
                            Link = pin.Link,
                            Repin_count = pin.Repin_count,
                            ReactionCount = pin.reactioncount,
                            ShareCount = pin.sharecount,
                            CommentCount = pin.commentTotalCount,
                            OwnerId = pin.OwnerID,
                            OwnerName = pin.OwnerName,
                            Description = pin.Description,
                            ImageUrl = pin.ImageURL,
                            Created_At = pin.Created_At < Commons.MinDate ? Commons.MinDate : pin.Created_At,
                            Domain = pin.Domain,
                            Status = (byte)Commons.EStatus.Active,
                            CreatedBy = createdBy,
                            CreatedDate = DateTime.Now,
                            UpdatedBy = createdBy,
                            UpdatedDate = DateTime.Now,
                            DayCount = 1,
                        });

                    }
                    if (listInsertDB.Count > 0)
                        _db.CMS_Pin.AddRange(listInsertDB);

                    //_db.SaveChanges();
                    /* TABLE KEYWORD_PIN */
                    var lstKeyWrd_Pin_Exist = _db.CMS_R_KeyWord_Pin.Where(o => o.KeyWordID == KeyWordID && lstPinID.Contains(o.PinID)).Select(o => o.PinID).ToList();
                    var lstKeyWrd_Pin_New = lstPinID.Where(o => !lstKeyWrd_Pin_Exist.Contains(o)).ToList();
                    var lstKeyWrd_Pin_InsertBD = lstKeyWrd_Pin_New.Select(o => new CMS_R_KeyWord_Pin()
                    {
                        ID = Guid.NewGuid().ToString(),
                        KeyWordID = KeyWordID,
                        PinID = o,
                        Status = (byte)Commons.EStatus.Active,
                        CreatedBy = createdBy,
                        CreatedDate = DateTime.Now,
                        UpdatedBy = createdBy,
                        UpdatedDate = DateTime.Now,
                    }).ToList();

                    if (lstKeyWrd_Pin_InsertBD.Count > 0)
                        _db.CMS_R_KeyWord_Pin.AddRange(lstKeyWrd_Pin_InsertBD);
                    // save db 
                    _db.SaveChanges();
                }

                NSLog.Logger.Info("ResponseCreateOrUpdatePin: " + KeyWordID, result);

            }
            catch (Exception ex)
            {
                msg = "CreateOrUpdate Pin with exception.";
                result = false;
                LogHelper.WriteLogs("ErrorCreateOrUpdatePin: ", JsonConvert.SerializeObject(ex));
                NSLog.Logger.Error("ErrorCreateOrUpdatePin: ", ex);
            }
            finally
            {
                m_Semaphore.Release();
            }

            return result;
        }

        public bool GetPin(ref List<PinsModels> lstPins, ref int totalPin, PinFilterDTO filter, ref string msg)
        {
            var result = true;
            lstPins = new List<PinsModels>();

            try
            {
                using (var _db = new CMS_Context())
                {
                    var query = _db.CMS_Pin.Where(o => o.Status != (byte)Commons.EStatus.Deleted);

                    if (filter != null)
                    {
                        /* filter by list GROUP key words */
                        if (filter.LstGroupID != null)
                        {
                            if (filter.LstGroupID.Count > 0)
                            {
                                var lstKeyID = _db.CMS_R_GroupKey_KeyWord.Where(o => filter.LstGroupID.Contains(o.GroupKeyID) && o.Status != (byte)Commons.EStatus.Deleted).Select(o => o.KeyWordID).ToList();
                                if (lstKeyID.Count > 0)
                                {
                                    if (filter.LstKeyWordID == null)
                                        filter.LstKeyWordID = new List<string>();
                                    filter.LstKeyWordID.AddRange(lstKeyID);
                                }
                            }
                        }

                        /* filter by list key words */
                        if (filter.LstKeyWordID != null)
                        {
                            if (filter.LstKeyWordID.Count > 0)
                            {
                                var lstPinID = _db.CMS_R_KeyWord_Pin.Where(o => filter.LstKeyWordID.Contains(o.KeyWordID)).Select(o => o.PinID).ToList();
                                query = query.Where(o => lstPinID.Contains(o.ID));
                            }
                        }

                        /* filter by create date */
                        if (filter.CreatedDateFrom != filter.CreatedDateTo && filter.CreatedDateTo != null)
                        {
                            query = query.Where(o => DbFunctions.TruncateTime(o.CreatedDate) >= DbFunctions.TruncateTime(filter.CreatedDateFrom)
                                                && DbFunctions.TruncateTime(o.CreatedDate) <= DbFunctions.TruncateTime(filter.CreatedDateTo));
                        }

                        /* filter by create at */
                        if (filter.CreatedAtFrom != null && filter.CreatedAtTo != null)
                        {
                            query = query.Where(o => DbFunctions.TruncateTime(o.Created_At) >= DbFunctions.TruncateTime(filter.CreatedAtFrom)
                                                && DbFunctions.TruncateTime(o.Created_At) <= DbFunctions.TruncateTime(filter.CreatedAtTo));
                        }

                        /* filter by pin count */
                        if (filter.PinCountFrom != null)
                        {
                            query = query.Where(o => o.Repin_count >= filter.PinCountFrom);
                        }

                        if (filter.PinCountTo != null)
                        {
                            query = query.Where(o => o.Repin_count <= filter.PinCountTo);
                        }

                        /* get total pin */
                        totalPin = query.Count();

                        /* order data */
                        //if (filter.TypeTime.Equals(Commons.ETimeType.TimeReduce.ToString("d")))
                        //{
                        //    query = query.OrderByDescending(x => x.Created_At).ThenBy(o=> o.ID);
                        //}
                        //else if (filter.TypeTime.Equals(Commons.ETimeType.TimeIncrease.ToString("d")))
                        //{
                        //    query = query.OrderBy(x => x.Created_At).ThenBy(o => o.ID);
                        //}
                        //else if (filter.TypeTime.Equals(Commons.ETimeType.PinReduce.ToString("d")))
                        //{
                        //    query = query.OrderByDescending(x => x.Repin_count).ThenBy(o => o.ID);
                        //}
                        //else if (filter.TypeTime.Equals(Commons.ETimeType.PinIncrease.ToString("d")))
                        //{
                        //    query = query.OrderBy(x => x.Repin_count).ThenBy(o => o.ID);
                        //}
                        //else if (filter.TypeTime.Equals(Commons.ETimeType.ToolReduce.ToString("d")))
                        //{
                        //    query = query.OrderByDescending(x => x.CreatedDate).ThenBy(o => o.ID);
                        //}
                        //else if (filter.TypeTime.Equals(Commons.ETimeType.ToolIncrease.ToString("d")))
                        //{
                        //    query = query.OrderBy(x => x.CreatedDate).ThenBy(o => o.ID);
                        //}

                        SortPinData(ref query, filter.Sort1, filter.Sort2);

                        /* get by page size - page index */
                        query = query.Skip((filter.PageIndex - 1) * filter.PageSize).Take(filter.PageSize);
                    }

                    lstPins = query.Select(o => new PinsModels()
                    {
                        ID = o.ID,
                        Link = o.Link,
                        Domain = o.Domain,
                        Repin_count = o.Repin_count,
                        Images = new List<ImageModels>()
                                    {
                                        new ImageModels()
                                        {
                                            url = o.ImageUrl,
                                        },
                                    },
                        Created_At = o.Created_At,
                        CreatedDate = o.CreatedDate ?? DateTime.MinValue,
                        UpdateDate = o.UpdatedDate ?? DateTime.MinValue,
                        commentTotalCount = o.CommentCount,
                        reactioncount = o.ReactionCount,
                        sharecount = o.ShareCount,
                        Description = o.Description,
                        DayCount = o.DayCount,
                        OwnerName = o.OwnerName
                        //LastTime = CommonHelper.GetDurationFromNow(o.UpdatedDate),
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                msg = "CreateOrUpdate Pin with exception.";
                result = false;
            }
            finally
            {
            }
            return result;
        }

        private void SortPinData(ref IQueryable<CMS_Pin> query, int sort1, int sort2)
        {
            try
            {
                switch (sort1)
                {
                    case (byte)Commons.ESortType1.TimeCreatedAtIncrease: /* time created_at increase */
                        {
                            switch (sort2)
                            {
                                case (byte)Commons.ESortType2.ReactionIncrease: /* reaction increase */
                                    query = query.OrderBy(o =>DbFunctions.TruncateTime(o.Created_At)).ThenBy(o => o.ReactionCount).ThenBy(o=> o.ID);
                                    break;

                                case (byte)Commons.ESortType2.ReactionDecrease:/* reaction decrease */
                                    query = query.OrderBy(o => DbFunctions.TruncateTime(o.Created_At)).ThenByDescending(o => o.ReactionCount).ThenBy(o => o.ID);
                                    break;

                                case (byte)Commons.ESortType2.ShareIncrease: /* ShareCount increase */
                                    query = query.OrderBy(o => DbFunctions.TruncateTime(o.Created_At)).ThenBy(o => o.ShareCount).ThenBy(o => o.ID);
                                    break;

                                case (byte)Commons.ESortType2.ShareDecrease: /* ShareCount decrease */
                                    query = query.OrderBy(o => DbFunctions.TruncateTime(o.Created_At)).ThenByDescending(o => o.ShareCount).ThenBy(o => o.ID);
                                    break;

                                case (byte)Commons.ESortType2.CommentIncrease: /* CommentCount increase */
                                    query = query.OrderBy(o => DbFunctions.TruncateTime(o.Created_At)).ThenBy(o => o.CommentCount).ThenBy(o => o.ID);
                                    break;

                                case (byte)Commons.ESortType2.CommentDecrease: /* CommentCount decrease */
                                    query = query.OrderBy(o => DbFunctions.TruncateTime(o.Created_At)).ThenByDescending(o => o.CommentCount).ThenBy(o => o.ID);
                                    break;

                                default:
                                    query = query.OrderBy(o => o.Created_At).ThenByDescending(o => o.CommentCount).ThenBy(o => o.ID);
                                    break;
                            }
                        }
                        break;

                    case (byte)Commons.ESortType1.TimeCreatedAtDecrease: /* time created_at Decrease */
                        {
                            switch (sort2)
                            {
                                case (byte)Commons.ESortType2.ReactionIncrease: /* reaction increase */
                                    query = query.OrderByDescending(o => DbFunctions.TruncateTime(o.Created_At)).ThenBy(o => o.ReactionCount).ThenBy(o => o.ID);
                                    break;

                                case (byte)Commons.ESortType2.ReactionDecrease:/* reaction decrease */
                                    query = query.OrderByDescending(o => DbFunctions.TruncateTime(o.Created_At)).ThenByDescending(o => o.ReactionCount).ThenBy(o => o.ID);
                                    break;

                                case (byte)Commons.ESortType2.ShareIncrease: /* ShareCount increase */
                                    query = query.OrderByDescending(o => DbFunctions.TruncateTime(o.Created_At)).ThenBy(o => o.ShareCount).ThenBy(o => o.ID);
                                    break;

                                case (byte)Commons.ESortType2.ShareDecrease: /* ShareCount decrease */
                                    query = query.OrderByDescending(o => DbFunctions.TruncateTime(o.Created_At)).ThenByDescending(o => o.ShareCount).ThenBy(o => o.ID);
                                    break;

                                case (byte)Commons.ESortType2.CommentIncrease: /* CommentCount increase */
                                    query = query.OrderByDescending(o => DbFunctions.TruncateTime(o.Created_At)).ThenBy(o => o.CommentCount).ThenBy(o => o.ID);
                                    break;

                                case (byte)Commons.ESortType2.CommentDecrease: /* CommentCount decrease */
                                    query = query.OrderByDescending(o => DbFunctions.TruncateTime(o.Created_At)).ThenByDescending(o => o.CommentCount).ThenBy(o => o.ID);
                                    break;

                                default:
                                    query = query.OrderByDescending(o => o.Created_At).ThenByDescending(o => o.CommentCount).ThenBy(o => o.ID);
                                    break;
                            }
                        }
                        break;

                    case (byte)Commons.ESortType1.TimeOnToolIncrease: /* time on tool increase */
                        {
                            switch (sort2)
                            {
                                case (byte)Commons.ESortType2.ReactionIncrease: /* reaction increase */
                                    query = query.OrderBy(o => DbFunctions.TruncateTime(o.CreatedDate)).ThenBy(o => o.ReactionCount).ThenBy(o => o.ID);
                                    break;

                                case (byte)Commons.ESortType2.ReactionDecrease:/* reaction decrease */
                                    query = query.OrderBy(o => DbFunctions.TruncateTime(o.CreatedDate)).ThenByDescending(o => o.ReactionCount).ThenBy(o => o.ID);
                                    break;

                                case (byte)Commons.ESortType2.ShareIncrease: /* ShareCount increase */
                                    query = query.OrderBy(o => DbFunctions.TruncateTime(o.CreatedDate)).ThenBy(o => o.ShareCount).ThenBy(o => o.ID);
                                    break;

                                case (byte)Commons.ESortType2.ShareDecrease: /* ShareCount decrease */
                                    query = query.OrderBy(o => DbFunctions.TruncateTime(o.CreatedDate)).ThenByDescending(o => o.ShareCount).ThenBy(o => o.ID);
                                    break;

                                case (byte)Commons.ESortType2.CommentIncrease: /* CommentCount increase */
                                    query = query.OrderBy(o => DbFunctions.TruncateTime(o.CreatedDate)).ThenBy(o => o.CommentCount).ThenBy(o => o.ID);
                                    break;

                                case (byte)Commons.ESortType2.CommentDecrease: /* CommentCount decrease */
                                    query = query.OrderBy(o => DbFunctions.TruncateTime(o.CreatedDate)).ThenByDescending(o => o.CommentCount).ThenBy(o => o.ID);
                                    break;

                                default:
                                    query = query.OrderBy(o => o.CreatedDate).ThenByDescending(o => o.CommentCount).ThenBy(o => o.ID);
                                    break;
                            }
                        }
                        break;

                    case (byte)Commons.ESortType1.TimeOnToolDecrease: /* time created_at Decrease */
                        {
                            switch (sort2)
                            {
                                case (byte)Commons.ESortType2.ReactionIncrease: /* reaction increase */
                                    query = query.OrderByDescending(o => DbFunctions.TruncateTime(o.CreatedDate)).ThenBy(o => o.ReactionCount).ThenBy(o => o.ID);
                                    break;

                                case (byte)Commons.ESortType2.ReactionDecrease:/* reaction decrease */
                                    query = query.OrderByDescending(o => DbFunctions.TruncateTime(o.CreatedDate)).ThenByDescending(o => o.ReactionCount).ThenBy(o => o.ID);
                                    break;

                                case (byte)Commons.ESortType2.ShareIncrease: /* ShareCount increase */
                                    query = query.OrderByDescending(o => DbFunctions.TruncateTime(o.CreatedDate)).ThenBy(o => o.ShareCount).ThenBy(o => o.ID);
                                    break;

                                case (byte)Commons.ESortType2.ShareDecrease: /* ShareCount decrease */
                                    query = query.OrderByDescending(o => DbFunctions.TruncateTime(o.CreatedDate)).ThenByDescending(o => o.ShareCount).ThenBy(o => o.ID);
                                    break;

                                case (byte)Commons.ESortType2.CommentIncrease: /* CommentCount increase */
                                    query = query.OrderByDescending(o => DbFunctions.TruncateTime(o.CreatedDate)).ThenBy(o => o.CommentCount).ThenBy(o => o.ID);
                                    break;

                                case (byte)Commons.ESortType2.CommentDecrease: /* CommentCount decrease */
                                    query = query.OrderByDescending(o => DbFunctions.TruncateTime(o.CreatedDate)).ThenByDescending(o => o.CommentCount).ThenBy(o => o.ID);
                                    break;

                                default:
                                    query = query.OrderByDescending(o => o.CreatedDate).ThenByDescending(o => o.CommentCount).ThenBy(o => o.ID);
                                    break;
                            }
                        }
                        break;
                    default: /* don't set sort 1 */
                        {
                            switch (sort2)
                            {
                                case (byte)Commons.ESortType2.ReactionIncrease: /* reaction increase */
                                    query = query.OrderBy(o => o.ReactionCount).ThenBy(o => o.ID);
                                    break;

                                case (byte)Commons.ESortType2.ReactionDecrease:/* reaction decrease */
                                    query = query.OrderByDescending(o => o.ReactionCount).ThenBy(o => o.ID);
                                    break;

                                case (byte)Commons.ESortType2.ShareIncrease: /* ShareCount increase */
                                    query = query.OrderBy(o => o.ShareCount).ThenBy(o => o.ID);
                                    break;

                                case (byte)Commons.ESortType2.ShareDecrease: /* ShareCount decrease */
                                    query = query.OrderByDescending(o => o.ShareCount).ThenBy(o => o.ID);
                                    break;

                                case (byte)Commons.ESortType2.CommentIncrease: /* CommentCount increase */
                                    query = query.OrderBy(o => o.CommentCount).ThenBy(o => o.ID);
                                    break;

                                case (byte)Commons.ESortType2.CommentDecrease: /* CommentCount decrease */
                                    query = query.OrderByDescending(o => o.CommentCount).ThenBy(o => o.ID);
                                    break;

                                default:
                                    query = query.OrderByDescending(o => o.CommentCount).ThenBy(o => o.ID);
                                    break;
                            }
                        }
                        break;

                }
            }
            catch (Exception ex) { }
        }
    }
}
