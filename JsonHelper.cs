using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace HR.RequestContext.Domain.Tests
{
    public static class JsonHelper
    {
        public static bool IsNullOrEmpty(this JToken token)
        {
            return (token == null) ||
                   (token.Type == JTokenType.Array && !token.HasValues) ||
                   (token.Type == JTokenType.Object && !token.HasValues) ||
                   (token.Type == JTokenType.String && token.ToString() == String.Empty) ||
                   (token.Type == JTokenType.Null);
        }

        public static bool IsNullOrEmpty2(this JToken token)
        {
            return token == null || string.IsNullOrWhiteSpace(token.ToString());
        }
    }
}
