using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Entities.Helpers
{
    internal static class JsonElementExtensions
    {
        public static int? GetIntOrNull(this Dictionary<string, JsonElement> dict, string key) =>
            dict.TryGetValue(key, out var el) && el.ValueKind == JsonValueKind.Number
                ? el.GetInt32() : null;

        public static decimal? GetDecimalOrNull(this Dictionary<string, JsonElement> dict, string key) =>
            dict.TryGetValue(key, out var el) && el.ValueKind == JsonValueKind.Number
                ? el.GetDecimal() : null;
    }

}
