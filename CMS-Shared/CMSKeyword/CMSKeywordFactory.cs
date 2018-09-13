using CMS_DTO.CMSCrawler;
using CMS_DTO.CMSKeyword;
using CMS_Entity;
using CMS_Entity.Entity;
using CMS_Shared.CMSEmployees;
using CMS_Shared.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CMS_Shared.Keyword
{
    public class CMSKeywordFactory
    {

        private static Semaphore m_Semaphore = new Semaphore(1, 1); /* semaphore for create key, craw data */
        private static Semaphore m_SemaphoreCrawlAll = new Semaphore(1, 1); /* semaphore for create key, craw data */

        public List<CMS_KeywordModels> GetList(string groupID = "")
        {
            try
            {
                using (var _db = new CMS_Context())
                {
                    /* get all key word */
                    var data = _db.CMS_KeyWord.Where(o => o.Status == (byte)Commons.EStatus.Active).Select(o => new CMS_KeywordModels()
                    {
                        Id = o.ID,
                        Sequence = o.Sequence,
                        KeySearch = o.KeyWord,
                        UpdatedDate = o.UpdatedDate,
                        CreatedDate = o.CreatedDate
                    }).ToList();

                    if (!string.IsNullOrEmpty(groupID)) /* filter by group ID */
                    {
                        var listKeyID = _db.CMS_R_GroupKey_KeyWord.Where(o => o.GroupKeyID == groupID && o.Status != (byte)Commons.EStatus.Deleted).Select(o => o.KeyWordID).ToList();
                        data = data.Where(o => listKeyID.Contains(o.Id)).ToList();
                    }

                    /* update quantity */
                    var listCount = _db.CMS_R_KeyWord_Pin
                        .GroupBy(o => o.KeyWordID)
                        .Select(o => new CMS_KeywordModels()
                        {
                            Id = o.Key,
                            Quantity = o.Count(),
                        }).ToList();

                    data.ForEach(o =>
                    {
                        o.Quantity = listCount.Where(c => c.Id == o.Id).Select(c => c.Quantity).FirstOrDefault();
                        o.StrLastUpdate = CommonHelper.GetDurationFromNow(o.UpdatedDate);
                    });
                    return data;
                }
            }
            catch (Exception ex) { }
            return null;
        }

        public bool CreateOrUpdate(CMS_KeywordModels model, ref string msg)
        {
            var result = true;
            using (var _db = new CMS_Context())
            {
                using (var trans = _db.Database.BeginTransaction())
                {
                    m_Semaphore.WaitOne();
                    try
                    {
                        if (string.IsNullOrEmpty(model.Id))
                        {
                            /* check dup old key */
                            var key = model.KeySearch;
                            var key2 = "";
                            if (key[key.Length - 1] == '/')
                                key2 = key.Substring(0, key.Length - 1);
                            else
                                key2 = key + "/";
                            var checkDup = _db.CMS_KeyWord.Where(o => o.KeyWord == key || o.KeyWord == key2).FirstOrDefault();

                            if (checkDup == null)
                            {
                                /* get current seq */
                                var curSeq = _db.CMS_KeyWord.OrderByDescending(o => o.Sequence).Select(o => o.Sequence).FirstOrDefault();

                                /* add new record */
                                var dateTimeNow = DateTime.Now;
                                var Id = Guid.NewGuid().ToString();
                                var newKey = new CMS_KeyWord()
                                {
                                    ID = Id,
                                    KeyWord = model.KeySearch,
                                    Status = (byte)Commons.EStatus.Active,
                                    CreatedBy = model.CreatedBy,
                                    CreatedDate = dateTimeNow,
                                    UpdatedBy = model.CreatedBy,
                                    UpdatedDate = dateTimeNow,
                                    Sequence = ++curSeq,
                                };
                                _db.CMS_KeyWord.Add(newKey);
                                var newGroupKey = new CMS_R_GroupKey_KeyWord()
                                {
                                    ID = Guid.NewGuid().ToString(),
                                    GroupKeyID = model.GroupID,
                                    KeyWordID = Id,
                                    Status = (byte)Commons.EStatus.Active,
                                    CreatedDate = DateTime.Now,
                                    UpdatedDate = DateTime.Now,
                                };
                                _db.CMS_R_GroupKey_KeyWord.Add(newGroupKey);
                            }
                            else if (checkDup.Status != (byte)Commons.EStatus.Active) /* re-active old key */
                            {
                                checkDup.Status = (byte)Commons.EStatus.Active;
                                checkDup.UpdatedBy = model.CreatedBy;
                                checkDup.UpdatedDate = DateTime.Now;
                            }
                            else /* duplicate key word */
                            {
                                result = false;
                                msg = "Duplicate key word.";
                            }

                            _db.SaveChanges();
                            trans.Commit();
                        }
                        else
                        {
                            result = false;
                            msg = "Unable to edit key word.";
                        }
                    }
                    catch (Exception ex)
                    {
                        msg = "Check connection, please!";
                        result = false;
                        trans.Rollback();
                    }
                    finally
                    {
                        _db.Dispose();
                        m_Semaphore.Release();
                    }
                }
            }
            return result;
        }

        public bool Delete(string Id, string createdBy, ref string msg)
        {
            var result = true;
            try
            {
                using (var _db = new CMS_Context())
                {
                    var key = _db.CMS_KeyWord.Where(o => o.ID == Id).FirstOrDefault();

                    key.Status = (byte)Commons.EStatus.Deleted;
                    key.UpdatedDate = DateTime.Now;
                    key.UpdatedBy = createdBy;

                    /* delete group key */
                    var listGroupKey = _db.CMS_R_GroupKey_KeyWord.Where(o => o.KeyWordID == Id).ToList();
                    listGroupKey.ForEach(o =>
                    {
                        o.Status = (byte)Commons.EStatus.Deleted;
                        o.UpdatedDate = DateTime.Now;
                        o.UpdatedBy = createdBy;
                    });

                    _db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                msg = "Can't delete this key words.";
                result = false;
            }
            return result;
        }

        public bool DeleteAll(string createdBy, ref string msg)
        {
            var result = true;
            try
            {
                using (var _db = new CMS_Context())
                {
                    var keys = _db.CMS_KeyWord.Where(o => o.Status == (byte)Commons.EStatus.Active).ToList();
                    var keyIDs = keys.Select(o => o.ID).ToList();
                    var keyGroupKeyDB = _db.CMS_R_GroupKey_KeyWord.Where(o => keyIDs.Contains(o.KeyWordID)).ToList();

                    /* delete key */
                    keys.ForEach(key =>
                    {
                        key.Status = (byte)Commons.EStatus.Deleted;
                        key.UpdatedDate = DateTime.Now;
                        key.UpdatedBy = createdBy;
                    });

                    /* delete group key */
                    keyGroupKeyDB.ForEach(o =>
                    {
                        o.Status = (byte)Commons.EStatus.Deleted;
                        o.UpdatedDate = DateTime.Now;
                        o.UpdatedBy = createdBy;
                    });

                    _db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                msg = "Can't delete this key words.";
                result = false;
            }
            return result;
        }

        public bool DeleteAndRemoveDB(string Id, ref string msg)
        {
            var result = true;
            try
            {
                using (var _db = new CMS_Context())
                {
                    _db.Database.CommandTimeout = 500;

                    /* remove list group key*/
                    var listGroupKey = _db.CMS_R_GroupKey_KeyWord.Where(o => o.KeyWordID == Id).ToList();

                    /* remove list Key pin */
                    var listKeyPin = _db.CMS_R_KeyWord_Pin.Where(o => o.KeyWordID == Id).ToList();

                    _db.CMS_R_GroupKey_KeyWord.RemoveRange(listGroupKey);
                    _db.CMS_R_KeyWord_Pin.RemoveRange(listKeyPin);
                    _db.SaveChanges();

                    /* remove list pin */
                    var listPinID = _db.CMS_R_KeyWord_Pin.Select(o => o.PinID).ToList();
                    var listPin = _db.CMS_Pin.Where(o => !listPinID.Contains(o.ID)).ToList();

                    /* remove key */
                    var key = _db.CMS_KeyWord.Where(o => o.ID == Id).FirstOrDefault();

                    _db.CMS_Pin.RemoveRange(listPin);
                    _db.CMS_KeyWord.Remove(key);
                    _db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                msg = "Can't delete this key words.";
                result = false;
            }
            return result;
        }

        public bool DeleteAndRemoveDBCommand(string Id, ref string msg)
        {
            var result = true;
            try
            {
                using (var _db = new CMS_Context())
                {
                    _db.Database.CommandTimeout = 500;

                    /* delete keyword_pin */
                    _db.Database.ExecuteSqlCommand(
                        "delete CMS_R_KeyWord_Pin where  KeyWordID = \'" + Id + "\'"
                        );

                    /* delete pin */
                    _db.Database.ExecuteSqlCommand(
                        "delete CMS_Pin where ID not in (select PinID from CMS_R_KeyWord_Pin)"
                        );

                    /* delete group_key */
                    _db.Database.ExecuteSqlCommand(
                        "delete CMS_R_GroupKey_KeyWord where KeyWordID = \'" + Id + "\'"
                        );

                    /* delete key */
                    _db.Database.ExecuteSqlCommand(
                        "delete CMS_KeyWord where ID = \'" + Id + "\'"
                        );
                }
            }
            catch (Exception ex)
            {
                msg = "Can't delete this key words.";
                result = false;
            }
            return result;
        }

        public bool DeleteAndRemoveDBAll(ref string msg)
        {
            var result = true;
            try
            {
                using (var _db = new CMS_Context())
                {
                    var keys = _db.CMS_KeyWord.Select(o => o.ID).ToList();
                    foreach (var keyID in keys)
                    {
                        DeleteAndRemoveDBCommand(keyID, ref msg);
                    }
                }
            }
            catch (Exception ex)
            {
                msg = "Can't delete data.";
                result = false;
            }
            return result;
        }


        public bool AddKeyToGroup(string KeyId, string GroupKeyID, ref string msg)
        {
            var result = true;
            using (var _db = new CMS_Context())
            {
                using (var trans = _db.Database.BeginTransaction())
                {
                    try
                    {
                        /* add new record */
                        var checkExist = _db.CMS_R_GroupKey_KeyWord.Where(o => o.KeyWordID == KeyId && o.GroupKeyID == GroupKeyID).FirstOrDefault();
                        if (checkExist != null)
                        {
                            if (checkExist.Status != (byte)Commons.EStatus.Active)
                            {
                                checkExist.Status = (byte)Commons.EStatus.Active;
                                checkExist.UpdatedDate = DateTime.Now;
                            }
                        }
                        else /* add new */
                        {
                            var newGroupKey = new CMS_R_GroupKey_KeyWord()
                            {
                                ID = Guid.NewGuid().ToString(),
                                GroupKeyID = GroupKeyID,
                                KeyWordID = KeyId,
                                Status = (byte)Commons.EStatus.Active,
                                CreatedDate = DateTime.Now,
                                UpdatedDate = DateTime.Now,
                            };
                            _db.CMS_R_GroupKey_KeyWord.Add(newGroupKey);
                        }

                        _db.SaveChanges();
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        msg = "Check connection, please!";
                        result = false;
                        trans.Rollback();
                    }
                    finally
                    {
                        _db.Dispose();
                    }
                }
            }
            return result;
        }

        public bool RemoveKeyFromGroup(string KeyId, string GroupKeyID, ref string msg)
        {
            var result = true;
            using (var _db = new CMS_Context())
            {
                using (var trans = _db.Database.BeginTransaction())
                {
                    try
                    {
                        /* add new record */
                        var checkRemove = _db.CMS_R_GroupKey_KeyWord.Where(o => o.KeyWordID == KeyId && o.GroupKeyID == GroupKeyID).FirstOrDefault();
                        checkRemove.Status = (byte)Commons.EStatus.Deleted;
                        checkRemove.UpdatedDate = DateTime.Now;
                        _db.SaveChanges();
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        msg = "Check connection, please!";
                        result = false;
                        trans.Rollback();
                    }
                    finally
                    {
                        _db.Dispose();
                    }
                }
            }
            return result;
        }

        public bool CrawlData(string Id, string createdBy, ref string msg)
        {
            NSLog.Logger.Info("CrawlData: " + Id);
            var model = new CMS_CrawlerModels();
            var sequence = 0;
            var key = "";

            var result = true;
            try
            {
                using (var _db = new CMS_Context())
                {
                    /* get key by ID */
                    var keyWord = _db.CMS_KeyWord.Where(o => o.ID == Id).FirstOrDefault();
                    if (keyWord != null)
                    {
                        sequence = keyWord.Sequence;
                        key = keyWord.KeyWord;
                        /* check time span crawl */
                        var timeSpanCrawl = DateTime.Now - keyWord.UpdatedDate;
                        if (timeSpanCrawl.Value.TotalMinutes > 5 || keyWord.UpdatedDate == keyWord.CreatedDate) /* 5min to crawl data again */
                        {
                            /* update crawer date */
                            var bkTime = keyWord.UpdatedDate;
                            keyWord.UpdatedDate = DateTime.Now;
                            keyWord.UpdatedBy = createdBy;
                            _db.SaveChanges();

                            /* call drawler api to crawl data */
                            CMSPinFactory _fac = new CMSPinFactory();

                            var listAcc = _db.CMS_Account.Where(o => o.Status == (byte)Commons.EStatus.Active && o.IsActive && !string.IsNullOrEmpty(o.Cookies)).ToList();
                            var listCookie = listAcc.Select(x => x.Cookies).ToList();
                            var _cookie = CommonHelper.RamdomCookie(listCookie);
                            /* crawler tab post */
                            string q = "stories-public(stories-keyword("+keyWord.KeyWord+"))";
                            string ref_path = "/search/str/"+keyWord.KeyWord+"/stories-keyword/stories-public";
                            CrawlerFBToolHelpers.CrawlerNow(q, ref_path, "list", (byte)Commons.EType.Post,_cookie, ref model);
                            /* crawler tab people */
                            q = "stories-opinion(stories-keyword("+keyWord.KeyWord+"))";
                            ref_path = "/search/str/"+keyWord.KeyWord+"/stories-keyword/stories-opinion";
                            CrawlerFBToolHelpers.CrawlerNow(q, ref_path, "list", (byte)Commons.EType.People,_cookie, ref model);
                            var res = false;
                            if (model.Pins.Count > 0)
                            {
                                var options = new ParallelOptions { MaxDegreeOfParallelism = 5 };
                                Parallel.ForEach(model.Pins, options , pin =>
                                 {
                                     CrawlerFBToolHelpers.CrawlerDetail(pin.PhotoID, ref pin);
                                 });
                                NSLog.Logger.Info("done crawler : ", model.Pins.Count);
                                res = _fac.CreateOrUpdate(model.Pins, keyWord.ID, createdBy, ref msg);
                            }

                            if (res == false)
                            {
                                /* back to last crawl data */
                                //keyWord.UpdatedDate = bkTime;
                                //_db.SaveChanges();
                                result = false;
                            }
                            else
                            {
                                keyWord.UpdatedDate = DateTime.Now;
                                _db.SaveChanges();
                            }
                        }
                    }
                }

                LogHelper.WriteLogs(sequence.ToString() + " " + key, "Num post: " + model.Pins.Count().ToString());
                NSLog.Logger.Info("ResponseCrawlData", result.ToString());
            }
            catch (Exception ex)
            {
                msg = "Crawl data is unsuccessfully.";
                result = false;

                LogHelper.WriteLogs("ErrorCrawlData: " + Id, JsonConvert.SerializeObject(ex));
                NSLog.Logger.Error("ErrorCrawlData: " + Id, ex);
            }

            return result;
        }

        public bool CrawlAllKeyWords(string createdBy, ref string msg)
        {
            LogHelper.WriteLogs("CrawlAllKeyWords", "");
            NSLog.Logger.Info("CrawlAllKeyWords");
            var result = true;
            try
            {
                m_SemaphoreCrawlAll.WaitOne();
                using (var _db = new CMS_Context())
                {
                    var keyWords = _db.CMS_KeyWord.Where(o => o.Status == (byte)Commons.EStatus.Active).OrderBy(o => o.Sequence).ToList();
                    if (keyWords != null)
                    {
                        foreach (var key in keyWords)
                        {
                            var _msg = "";
                            LogHelper.WriteLogs(key.Sequence.ToString() + " " + key.KeyWord, key.ID);
                            CrawlData(key.ID, createdBy, ref _msg);
                        }
                    }
                }
                LogHelper.WriteLogs("ResponseCrawlAllKeyWords", result.ToString());
                NSLog.Logger.Info("ResponseCrawlAllKeyWords", result.ToString());
            }
            catch (Exception ex)
            {
                msg = "Crawl data is unsuccessfully.";
                result = false;
                LogHelper.WriteLogs("ErrorCrawlAllKeyWords:", JsonConvert.SerializeObject(ex));
                NSLog.Logger.Error("ErrorCrawlAllKeyWords:", ex);
            }
            finally
            {
                m_SemaphoreCrawlAll.Release();
            };
            return result;
        }

    }
}
