using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageVersionChecker
{
    /// <summary>
    /// <see cref="DotNetVersion"/> クラスは、.NET のバージョンを確認するためのクラスです。
    /// </summary>
    public class DotNetVersion
    {
        /// <summary>
        /// どの .NET Framework バージョンがインストールされているか登録情報のコレクションを取得します。
        /// </summary>
        /// <returns>登録情報の組のコレクション (<see cref="Version"/>, SP)。登録情報が存在しないとき null を返却します。</returns>
        public static IEnumerable<Tuple<Version, string>> GetVersions()
        {
            var versions = new List<Tuple<Version, string>>();

             var v1 = GetVersion1To45FromRegistry();
             var v2 = GetVersionOver45FromRegistry();

            if (v1 != null)
            {
                versions.AddRange(v1);
            }

            if (v2 != null)
            {
                versions.Add(v2);
            }

            return versions.Count > 0 ? versions : null;
        }

        #region Public Methods

        /// <summary>
        /// .NET Framework 1 - 4.0 までのバージョンがインストールされているかどうかを示す登録情報を取得します。
        /// </summary>
        /// <returns>登録情報の組のコレクション（バージョン、SP）。登録情報が存在しないとき null を返却します。</returns>
        public static IEnumerable<Tuple<Version, string>> GetVersion1To45FromRegistry()
        {
            const string reg = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\";

            var netVersions = new List<Tuple<Version, string>>();

            using (var key = Registry.LocalMachine.OpenSubKey(reg))
            {
                foreach (var keyName in key.GetSubKeyNames())
                {
                    var version = "";
                    var sp = "";
                    var install = "";

                    if (keyName == "v4") continue;
                    if (!keyName.StartsWith("v")) continue;

                    var versionKey = key.OpenSubKey(keyName);

                    version = versionKey.GetValue("Version", "").ToString();
                    sp = versionKey.GetValue("SP", "").ToString();
                    install = versionKey.GetValue("Install", "").ToString();

                    // Version 2-3.5 対応
                    if (!string.IsNullOrEmpty(version))
                    {
                        if (!(string.IsNullOrEmpty(sp)) && install == "1")
                        {
                            netVersions.Add(Tuple.Create(new Version(version), sp));
                        }
                        else
                        {
                            netVersions.Add(Tuple.Create(new Version(version), ""));
                        }

                        continue;
                    }

                    // Version 4.0 対応
                    foreach (var subKeyName in versionKey.GetSubKeyNames())
                    {
                        var subKey = versionKey.OpenSubKey(subKeyName);

                        version = subKey.GetValue("Version", "").ToString();

                        if (!string.IsNullOrEmpty(version))
                        {
                            sp = subKey.GetValue("SP", "").ToString();
                        }

                        install = subKey.GetValue("Install").ToString();

                        if (string.IsNullOrEmpty(install))
                        {
                            netVersions.Add(Tuple.Create(new Version(version), ""));
                        }
                        else
                        {
                            if (!(string.IsNullOrEmpty(sp)) && install == "1")
                            {
                                netVersions.Add(Tuple.Create(new Version(version), sp));
                            }
                            else
                            {
                                netVersions.Add(Tuple.Create(new Version(version), ""));
                            }
                        }
                    }

                }

                return netVersions.Count > 0 ? netVersions : null;
            }

        }

        /// <summary>
        /// .NET Framework 1 - 4.0 までのバージョンがインストールされているかどうかを示す登録情報を取得します。
        /// </summary>
        /// <returns>登録情報の組（バージョン、SP）。登録情報が存在しないとき null を返却します。</returns>
        public static Tuple<Version, string> GetVersionOver45FromRegistry()
        {
            const string reg = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";

            using (RegistryKey ndpKey = Registry.LocalMachine.OpenSubKey(reg))
            {
                if (ndpKey == null) return null;

                var version = ndpKey.GetValue("Version")?.ToString() ?? "";

                if (!string.IsNullOrEmpty(version))
                {
                    return Tuple.Create(new Version(version), "");
                }
                else
                {
                    var result = int.TryParse(ndpKey.GetValue("Release")?.ToString() ?? "", out int release);

                    if (result)
                    {
                        var versionText = ToVersion(release);

                        if (!string.IsNullOrEmpty(versionText))
                        {
                            return Tuple.Create(new Version(versionText), "");
                        }
                    }
                }
            }

            return null;
        }

        #endregion

        /// <summary>
        /// リリースキーの番号からバージョンの番号を取得します。
        /// </summary>
        /// <param name="release">リリースキーの番号。</param>
        /// <returns>バージョンの番号。変換に失敗したときは "" を返却します。</returns>
        private static string ToVersion(int release)
        {
            if (release >= 528040) return "4.8";
            if (release >= 461808) return "4.7.2";
            if (release >= 461308) return "4.7.1";
            if (release >= 460798) return "4.7";
            if (release >= 394802) return "4.6.2";
            if (release >= 394254) return "4.6.1";
            if (release >= 393295) return "4.6";
            if (release >= 379893) return "4.5.2";
            if (release >= 378675) return "4.5.1";
            if (release >= 378389) return "4.5";

            return "";
        }
    }
}
