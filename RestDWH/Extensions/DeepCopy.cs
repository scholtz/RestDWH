using Newtonsoft.Json;

namespace RestDWH.Extensions
{
    public static class ExtensionMethods
    {
        public static T DeepCopy<T>(this T self)
        {
            var serialized = JsonConvert.SerializeObject(self);
            if (serialized == null) { throw new Exception("Unable to serialize T"); }
            var ret = JsonConvert.DeserializeObject<T>(serialized);
            if (ret == null) { throw new Exception("Unable to deserialize T"); }
            return ret;
        }
    }
}
