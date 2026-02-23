using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public enum DocuSignAPIType
    {
        [Description("Reg")]
        Rooms = 0,

        [Description("eg")]
        ESignature = 1,

        [Description("ClickEg")]
        Click = 2,

        [Description("monitorExample")]
        Monitor = 3,

        [Description("Aeg")]
        Admin = 4,
    }

    public static class DocuSignAPITypeExtensions
    {
        public static string ToKeywordString(this DocuSignAPIType val)
        {
            DescriptionAttribute[] attributes = (DescriptionAttribute[])val
               .GetType()
               .GetField(val.ToString())
               .GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : string.Empty;
        }
    }

}
