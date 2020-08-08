using System;

namespace UnderMineControl.Loader.Core.Models
{
    public class Version : IComparable
    {
        public int Major { get; set; } = 0;
        public int Minor { get; set; } = 0;
        public int Patch { get; set; } = 0;
        public int Revision { get; set; } = 0;
        public string Original { get; set; }

        public Version(string version)
        {
            Original = version;
            if (version.Trim().ToLower().StartsWith("v"))
                version = version.Remove(0, 1).Trim();

            string[] v = version.Split('.');

            if (v.Length > 0 && int.TryParse(v[0], out int major))
                Major = major;
            if (v.Length > 1 && int.TryParse(v[1], out int minor))
                Minor = minor;
            if (v.Length > 2 && int.TryParse(v[2], out int patch))
                Patch = patch;
            if (v.Length > 3 && int.TryParse(v[3], out int revision))
                Revision = revision;
        }

        public Version(int major, int minor = 0, int patch = 0, int revision = 0)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            Revision = revision;
            Original = Major + "." + Minor + "." + Patch + "." + Revision;
        }

        public static bool operator <(Version emp1, Version emp2)
        {
            return Compare(emp1, emp2) < 0;
        }

        public static bool operator >(Version emp1, Version emp2)
        {
            return Compare(emp1, emp2) > 0;
        }

        public static bool operator ==(Version emp1, Version emp2)
        {
            return Compare(emp1, emp2) == 0;
        }

        public static bool operator !=(Version emp1, Version emp2)
        {
            return Compare(emp1, emp2) != 0;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Version)) 
                return false;
            return this == (Version)obj;
        }

        public static bool operator <=(Version emp1, Version emp2)
        {
            return Compare(emp1, emp2) <= 0;
        }

        public static bool operator >=(Version emp1, Version emp2)
        {
            return Compare(emp1, emp2) >= 0;
        }

        public static int Compare(Version a, Version b)
        {
            if (a.Major > b.Major) return 1;
            if (a.Major < b.Major) return -1;
            if (a.Minor > b.Minor) return 1;
            if (a.Minor < b.Minor) return -1;
            if (a.Patch > b.Patch) return 1;
            if (a.Patch < b.Patch) return -1;
            if (a.Revision > b.Revision) return 1;
            if (a.Revision < b.Revision) return -1;
            return 0;
        }

        public override string ToString()
        {
            return Original;
        }

        public int CompareTo(object obj)
        {
            if (!(obj is Version)) return 1;
            return Compare(this, (Version)obj);
        }

        public override int GetHashCode()
        {
            var hashCode = -1661427959;
            hashCode = hashCode * -1521134295 + Major.GetHashCode();
            hashCode = hashCode * -1521134295 + Minor.GetHashCode();
            hashCode = hashCode * -1521134295 + Patch.GetHashCode();
            hashCode = hashCode * -1521134295 + Revision.GetHashCode();
            return hashCode;
        }
    }
}
