using System.Collections.Generic;
using System.Linq;

namespace DocAggregator.API.Core.Models
{
    public static class AccessRightFieldExtensions
    {
        public static AccessRightStatus GetWholeStatus(this IEnumerable<InformationResource> fields)
        {
            return fields.Aggregate(AccessRightStatus.NotMentioned, (ars, arf) => ars | GetWholeStatus(arf.AccessRightFields));
        }

        public static AccessRightStatus GetWholeStatus(this IEnumerable<AccessRightField> fields)
        {
            return fields.Aggregate(AccessRightStatus.NotMentioned, (ars, arf) => ars | arf.Status);
        }
    }
}
