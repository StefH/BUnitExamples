using System;
using System.Reflection;

namespace ConsoleApp1
{
    public static class Ext
    {
        public static FieldInfo SafeResolveField(this Module m, int token)
        {
            FieldInfo fi;
            m.TryResolveField(token, out fi);
            return fi;
        }
        public static bool TryResolveField(this Module m, int token, out FieldInfo fi)
        {
            var ok = false;
            try { fi = m.ResolveField(token); ok = true; }
            catch { fi = null; }
            return ok;
        }
        public static MethodBase SafeResolveMethod(this Module m, int token)
        {
            MethodBase fi;
            m.TryResolveMethod(token, out fi);
            return fi;
        }
        public static bool TryResolveMethod(this Module m, int token, out MethodBase fi)
        {
            var ok = false;
            try { fi = m.ResolveMethod(token); ok = true; }
            catch { fi = null; }
            return ok;
        }
        public static string SafeResolveString(this Module m, int token)
        {
            string fi;
            m.TryResolveString(token, out fi);
            return fi;
        }
        public static bool TryResolveString(this Module m, int token, out string fi)
        {
            var ok = false;
            try { fi = m.ResolveString(token); ok = true; }
            catch { fi = null; }
            return ok;
        }
        public static byte[] SafeResolveSignature(this Module m, int token)
        {
            byte[] fi;
            m.TryResolveSignature(token, out fi);
            return fi;
        }
        public static bool TryResolveSignature(this Module m, int token, out byte[] fi)
        {
            var ok = false;
            try { fi = m.ResolveSignature(token); ok = true; }
            catch { fi = null; }
            return ok;
        }
        public static Type SafeResolveType(this Module m, int token)
        {
            Type fi;
            m.TryResolveType(token, out fi);
            return fi;
        }

        public static bool TryResolveType(this Module m, int token, out Type fi)
        {
            var ok = false;
            try { fi = m.ResolveType(token); ok = true; }
            catch { fi = null; }
            return ok;
        }
    }
}
