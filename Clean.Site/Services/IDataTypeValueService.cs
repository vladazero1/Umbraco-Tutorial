using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Clean.Site.Services
{
    public interface IDataTypeValueService
    {
        IEnumerable<SelectListItem> GetItemsFromValueListDataType(string dataTypeName, string[] selectedValues);
    }
}
