using System;
namespace FilterIcap.Net
{
    /// <summary>
    /// Used by the Encapsulation class to help the programmer determine what type of encapsulated data was in the request.
    /// </summary>
    public enum EncapsulationType
    {
        RequestHeader,

        ResponseHeader,

        RequestBody,

        ResponseBody,

        NullBody,

        OptionBody
    }
}
