using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace backoffice
{
    public static class common
    {
        public static bool IsStatusCodeSuccess(HttpStatusCode code)
        {
            bool retval = false;

            if (((int)code > 199) & ((int)code < 300))
                retval = true;

            return retval;
        }

    }
}
