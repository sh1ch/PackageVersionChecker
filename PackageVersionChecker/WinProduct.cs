using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageVersionChecker
{
    /// <summary>
    /// <see cref="WinProduct"/> クラスは、Win32_Product のデータを表現するクラスです。
    /// </summary>
    public class WinProduct
    {
        public int No { get; set; }

        public string IdentifyingNumber { get; set; }

        public string Name { get; set; }
    }
}
