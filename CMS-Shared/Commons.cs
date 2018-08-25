using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS_Shared
{
    public static class Commons
    {
        public const string Image100_50 = "http://placehold.it/100x50";
        public const string Image200_100 = "http://placehold.it/200x100";
        public const string Image272_259 = "http://placehold.it/272x259";

        #region EnumWeb
        public enum EQuantityType
        {
            Done = 0,
            ZeroToOne = 5,
            OneToTwo = 10,
            TwoToThree = 15,
            ThreeToFour = 20,
            FourToFive = 25,
            MoreFive = 30,
        }
        public enum ETimeType
        {
            TimeIncrease = 0,
            TimeReduce = 1,
            TimeCustom = 2,
            PinIncrease = 3,
            PinReduce = 4,
            ToolIncrease = 5,
            ToolReduce = 6,
        }

        public enum ESortType1
        {
            TimeCreatedAtIncrease = 1,
            TimeCreatedAtDecrease = 2,
            TimeOnToolIncrease = 3,
            TimeOnToolDecrease = 4,
        }

        public enum ESortType2
        {
            ReactionIncrease = 1,
            ReactionDecrease = 2,
            ShareIncrease = 3,
            ShareDecrease = 4,
            CommentIncrease = 5,
            CommentDecrease = 6,
        }

        public enum EPinType
        {
            PinIncrease = 0,
            PinReduce = 1,
        }

        public enum EStatus
        {
            Active = 1,
            Deleted = 9,
        }

        public enum EIndex
        {
            LikeReduce = 1,
            CommentReduce = 2,
            ShareReduce = 3,
        }

        public enum EErrorStatus
        {
            Exception = 0,
            AccPending = 2,
            AccBlocked = 3,
        }
        #endregion

        public static int WidthProduct = Convert.ToInt16(ConfigurationManager.AppSettings["WidthProduct"]);
        public static int HeightProduct = Convert.ToInt16(ConfigurationManager.AppSettings["HeightProduct"]);

        public static int WidthImageNews = Convert.ToInt16(ConfigurationManager.AppSettings["WidthImageNews"]);
        public static int HeightImageNews = Convert.ToInt16(ConfigurationManager.AppSettings["HeightImageNews"]);
        public static int WidthImageSilder = Convert.ToInt16(ConfigurationManager.AppSettings["WidthImageSilder"]);
        public static int HeightImageSilder = Convert.ToInt16(ConfigurationManager.AppSettings["HeightImageSilder"]);
        public static string Phone1 = ConfigurationManager.AppSettings["Phone1"];
        public static string Phone2 = ConfigurationManager.AppSettings["Phone2"];
        public static string Email1 = ConfigurationManager.AppSettings["Email1"];
        public static string Email2 = ConfigurationManager.AppSettings["Email2"];
        public static string AddressCompany = ConfigurationManager.AppSettings["AddressCompnay"];
        public static string CompanyTitle = ConfigurationManager.AppSettings["CompanyTitle"];
        public static string HostImage = ConfigurationManager.AppSettings["HostImage"];
        public static string _PublicImages = string.IsNullOrEmpty(ConfigurationManager.AppSettings["PublicImages"]) ? "" : ConfigurationManager.AppSettings["PublicImages"];

        public static string HostApi = ConfigurationManager.AppSettings["HostApi"];
        public static string HostApiOrtherPin = ConfigurationManager.AppSettings["HostApiOrtherPin"];
        public static string HostApiPinDetail = ConfigurationManager.AppSettings["HostApiPinDetail"];
        public static string HostApiHomePin = ConfigurationManager.AppSettings["HostApiHomePin"];
        public static int PinDefault = Convert.ToInt16(ConfigurationManager.AppSettings["PinDefault"]);
        public static int PinOrtherDefault = Convert.ToInt16(ConfigurationManager.AppSettings["PinOrtherDefault"]);
        public static int PageIndex = Convert.ToInt16(ConfigurationManager.AppSettings["PageIndex"]);
        public static int PageSize = Convert.ToInt16(ConfigurationManager.AppSettings["PageSize"]);
        public static int TimerInterval = Convert.ToInt32(ConfigurationManager.AppSettings["TimerInterval"]);
        public static double TimerStartAt = Convert.ToDouble(ConfigurationManager.AppSettings["TimerStartAt"]);

        public static DateTime MinDate = new DateTime(1900, 01, 01, 00, 00, 00, DateTimeKind.Unspecified);
        public static DateTime MaxDate = new DateTime(9999, 12, 31, 23, 59, 59, DateTimeKind.Unspecified);

        public static List<string> Proxys = new List<string>
        {
            "104.140.210.56:3128",
            "173.234.181.200:3128",
            "94.229.71.26:3128",
            "94.229.71.10:3128",
            "94.229.71.19:3128",
            "94.229.71.79:3128",
            "192.126.164.150:3128",
            "173.234.181.61:3128",
            "173.208.91.247:3128",
            "173.208.91.94:3128",
            "192.126.164.226:3128",
            "192.126.164.159:3128",
            "104.140.210.236:3128",
            "173.208.46.44:3128",
            "173.208.46.227:3128",
            "173.208.46.163:3128",
            "104.140.210.6:3128",
            "173.208.46.195:3128",
            "173.208.91.178:3128",
            "173.234.181.196:3128",
            "192.126.164.14:3128",
            "173.208.91.194:3128",
            "173.234.181.101:3128",
            "104.140.210.231:3128",
            "173.234.181.217:3128"
        };
    }
}
