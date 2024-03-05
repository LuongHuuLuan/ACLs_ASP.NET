using ACLAuthorization.Models;

namespace ACLAuthorization.Helper
{
    public class MethodConverter
    {
        public static Method ConvertToMethod(string value)
        {
            switch (value)
            {
                case "get":
                    return Method.get;
                case "post":
                    return Method.post;
                case "put":
                    return Method.put;
                case "patch":
                    return Method.patch;
                case "delete":
                    return Method.delete;
                default:
                    throw new NotImplementedException();
            }
        }

        public static string ConvertToString(Method method)
        {
            switch (method)
            {
                case Method.get:
                    return "get";
                case Method.post:
                    return "post";
                case Method.put:
                    return "put";
                case Method.patch:
                    return "patch";
                case Method.delete:
                    return "delete";
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
