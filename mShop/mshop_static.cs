using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace mShop
{
    public static class mshop_common
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
