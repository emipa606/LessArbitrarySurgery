using HugsLib;
using HugsLib.Utils;

namespace LessArbitrarySurgery
{
    public class PDCBase : ModBase
    {
        public override string ModIdentifier => "LessArbitrarySurgery";

        public static ModLogger PDCLog { get; private set; }

        public override void Initialize()
        {
            PDCLog = Logger;
        }
    }
}