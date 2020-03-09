using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HugsLib;
using HugsLib.Utils;

namespace LessArbitrarySurgery
{
    public class PDCBase : ModBase
    {
        private static ModLogger StoredLogger;

        public override string ModIdentifier
        {
            get
            {
                return "LessArbitrarySurgery";
            }
        }

        public static ModLogger PDCLog
        {
            get
            {
                return StoredLogger;
            }
        }

        public override void Initialize()
        {
            StoredLogger = Logger;
        }
    }
}
