using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageVersionChecker
{
    /// <summary>
    /// <see cref="DotNetVersions"/> クラスは、<see cref="DotNetVersion"/> をリスト表示するためのクラスです。
    /// </summary>
    public class DotNetVersions
    {
        public Version Version { get; private set; }

        public string Sp { get; private set; }

        public bool HasSp => !string.IsNullOrEmpty(Sp);

        public string Detail => $"{Version.ToString()}";

        public DotNetVersions(Tuple<Version, string> version)
        {
            Version = version.Item1;
            Sp = version.Item2;
        }
    }
}
