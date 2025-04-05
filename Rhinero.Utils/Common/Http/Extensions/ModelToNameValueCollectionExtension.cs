using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Rhinero.Utils.Common.Http.Extensions
{
    public static class ModelToNameValueCollectionExtension
    {
       public static NameValueCollection ToNameValueCollection(this object input, bool includeNulls = false)
        {
            NameValueCollection data = new NameValueCollection();
            input.GetType().GetProperties().ToList().ForEach(pi => 
            {
                var val = pi.GetValue(input, null);
                if (val is not null || includeNulls)
                {
                    data.Add(pi.Name, val.ToString());
                }                                    
            });

            return data;
        }
    }
}
