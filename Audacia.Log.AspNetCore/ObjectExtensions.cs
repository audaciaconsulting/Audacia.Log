namespace Audacia.Log.AspNetCore
{
    internal static class ObjectExtensions
    {
        /// <summary>
        /// Returns true if object is a class.
        /// </summary>
        public static bool IsClass(this object data)
        {
            return data.GetType().IsClass && !(data is string);
        }

        /// <summary>
        /// Returns true if object is a struct.
        /// </summary>
        public static bool IsStruct(this object data)
        {
            var type = data.GetType();
            return type.IsValueType && !type.IsPrimitive && !type.IsEnum;
        }
    }
}