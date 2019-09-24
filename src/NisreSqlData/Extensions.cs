namespace NisrePgData
{
    public static class Extensions
    {
        public static string ToJson(this object data)
        {
            return Utf8Json.JsonSerializer.ToJsonString(data);
        }

        public static T FromJson<T>(this string data)
        {
            return Utf8Json.JsonSerializer.Deserialize<T>(data);
        }
    }
}
