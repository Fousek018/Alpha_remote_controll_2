using Alpha_remote_controll.Services;
using CommunityToolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Utility.Messaging;
using Utils.Logger;
using MediatR;

namespace Alpha_remote_controll.Model
{
    public class AlphaResponseBase
    {
        public string name { get; set; }
    }
    public class alphaMethodList : AlphaResponseBase
    {
        public List<string> methodNames { get; set; }
    }

    public class alphaMethodInfo : AlphaResponseBase
    {
        public string axisDistance { get; set; }
        public string specimenThicknes { get; set; }
        public List<string> probeInfos { get; set; }
    }

    public class alphaComputedValues : AlphaResponseBase
    {
        public List<string> values { get; set; }
    }

    public class alphaRealTimeValues : AlphaResponseBase
    {
        public List<string> values { get; set; }
    }

    public class alphaApiVersion : AlphaResponseBase
    {
        public string majorVersion { get; set; }
        public string minorVersion { get; set; }
    }

    public class messageList : AlphaResponseBase
    {
        public string messageNames { get; set; }
    }

    public class status : AlphaResponseBase
    {
        public string code { get; set; }
        public string message { get; set; }
    }


}
