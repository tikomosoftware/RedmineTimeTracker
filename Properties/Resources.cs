using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace redmineSupTool.Properties
{
    public static class Resources
    {
        private static ResourceManager _resourceManager = null!;
        private static CultureInfo _resourceCulture = null!;

        public static ResourceManager ResourceManager
        {
            get
            {
                if (_resourceManager == null)
                {
                    _resourceManager = new ResourceManager("redmineSupTool.Properties.Resources", Assembly.GetExecutingAssembly());
                }
                return _resourceManager;
            }
        }

        public static CultureInfo Culture
        {
            get { return _resourceCulture; }
            set { _resourceCulture = value; }
        }

        public static string WorkAttendanceDay
        {
            get { return ResourceManager.GetString("WorkAttendanceDay", _resourceCulture)!; }
        }

        public static string SetWorkDay
        {
            get { return ResourceManager.GetString("SetWorkDay", _resourceCulture)!; }
        }

        public static string UnsetWorkDay
        {
            get { return ResourceManager.GetString("UnsetWorkDay", _resourceCulture)!; }
        }

        public static string SetWorkDayMessage
        {
            get { return ResourceManager.GetString("SetWorkDayMessage", _resourceCulture)!; }
        }

        public static string UnsetWorkDayMessage
        {
            get { return ResourceManager.GetString("UnsetWorkDayMessage", _resourceCulture)!; }
        }
    }
}
