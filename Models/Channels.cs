using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace IM_Api.Models
{
    public static class Channels
    {
       

        public static Dictionary<string, Channel<string>> DataChannels = new Dictionary<string, Channel<string>>();


    }
}
