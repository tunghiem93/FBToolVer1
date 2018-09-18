using CMS_Shared.Keyword;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace CMS_Shared.Utilities
{
    public sealed class AutoSingleton
    {
        private static AutoSingleton instance = null;
        private static readonly object padlock = new object();

        AutoSingleton()
        {
        }

        public static AutoSingleton Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (padlock)
                    {
                        if (instance == null)
                        {
                            instance = new AutoSingleton();

                            /* wait for next date */
                            WaitForNextDate();

                            /* auto run with timer */
                            AutoFunction();
                        }
                    }
                }
                return instance;
            }
        }
        public static void WaitForNextDate()
        {
            try
            {
                //var dateTimeNow = DateTime.UtcNow;

                /* time in VN: GMT+7*/
               // dateTimeNow = dateTimeNow.AddHours(7);

                /* get time start timer */
                //var timeStart = Commons.TimerStartAt;
               // var dateStart = dateTimeNow.Date.AddHours(timeStart);
               // if (dateStart < dateTimeNow)/* start at next date */
               // {
                   // dateStart = dateStart.AddDays(1); /* */
                //}

                /* time span to sleep */
                //var timeSpan = dateStart - dateTimeNow;
                //LogHelper.WriteLogs("WaitForNextDate", dateStart.ToString());
                Thread.Sleep(Commons.TimerInterval * 1000);
				//Thread.Sleep(60 * 1000);
            }
            catch (Exception ex) { };
        }
        public static void AutoFunction()
        {
            /* set timer event */
            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = Commons.TimerInterval * 1000; /* (second*1000) = milisecond */
		   //aTimer.Interval = 60 * 1000; //(test after 1p continue crawler)
            aTimer.Enabled = true;

            /* run first time */
            AutoRun();
        }

        // Specify what you want to happen when the Elapsed event is raised.
        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                AutoRun();
            }
            catch (Exception ex) { };

            /* log data */
            LogHelper.WriteLogs("Timer", "");
        }

        private static void AutoRun()
        {
            LogHelper.WriteLogs("AutoRun", "");
            try
            {
                /* crawl data */
                AutoCrawlAll();
            }
            catch (Exception ex) { };
        }

        private static void AutoCrawlAll()
        {
            try
            {
                var keyFac = new CMSKeywordFactory();
                var msg = "";
                keyFac.CrawlAllKeyWords("AutoCrawlAll", ref msg);
            }
            catch (Exception ex) { };
        }
    }
}
