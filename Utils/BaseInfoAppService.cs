using Abp;
using Abp.Application.Services;
using Abp.Authorization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abp.Utils.BaseInfo
{
    public interface IBaseInfoAppService : IApplicationService {
        IEnumerable<KeyValuePair<int, string>> EnumItems(string enumType);
    }
    [AbpAuthorize]
    public class BaseInfoAppService : ApplicationService, IBaseInfoAppService
    {
        public BaseInfoAppService()
        {

        }
        public static Type GetEnumType(string enumName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(enumName);
                if (type == null)
                    continue;
                if (type.IsEnum)
                    return type;
            }
            return null;
        }

        public IEnumerable<KeyValuePair<int, string>> EnumItems(string enumType)
        {
            var values = Enum.GetValues(GetEnumType(enumType));
            var res = new List<KeyValuePair<int, string>>();
            foreach (Enum item in values)
            {
                res.Add(new KeyValuePair<int, string>(Convert.ToInt32(item), EnumHelper.GetDescription(item)));
            }
            return res;
        }
    }
}
