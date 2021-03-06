﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Web;
using CMS_Entity.Entity;

namespace CMS_Shared.Utilities
{
    public class LogHelper
    {
        public static void WriteLogs(string description, string jsonContent)
        {
            try
            {
                using (var _db = new CMS_Context())
                {

                    description = description.Length > 98 ? description.Substring(0, 95) + "\n..." : description;
                    jsonContent = jsonContent.Length > 3999 ? jsonContent.Substring(0, 3995) + "\n..." : jsonContent;

                    var logData = new CMS_Log()
                    {
                        ID = Guid.NewGuid().ToString(),
                        Decription = description,
                        CreatedDate = DateTime.UtcNow.AddHours(7),
                        JsonContent = jsonContent,
                    };
                    _db.CMS_Log.Add(logData);
                    _db.SaveChanges();

                    _db.Database.CommandTimeout = 500;

                    /* delete log from 7 day ago */
                    _db.Database.ExecuteSqlCommand(
                        "delete CMS_Log where  CreatedDate < DATEADD(DAY,-7,getdate())"
                        );
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}
