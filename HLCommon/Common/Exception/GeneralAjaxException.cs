using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HLCommon
{
    public class GeneralAjaxException : ApplicationException
    {
        public GeneralAjaxException(string message) : base(message) { }
    }
}
