﻿using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using RawCMS.Library.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace RawCMS.Plugins.Auth.Lambdas
{
    public class UserInfoLambda : RestLambda
    {
        public override string Name => "UserInfo";

        public override string Description => throw new NotImplementedException();

        public override JObject Rest(JObject input)
        {
            var jj= new JObject();
            jj["username"] = this.Request.User.Identity.Name;
            jj["parameters"] = this.Request.User.Identity.IsAuthenticated;
            return jj;
        }
    }
}
