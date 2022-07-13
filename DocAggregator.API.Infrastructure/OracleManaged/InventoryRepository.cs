using DocAggregator.API.Core;
using DocAggregator.API.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAggregator.API.Infrastructure.OracleManaged
{
    public class InventoryRepository : IInventoryRepository
    {
        public Inventory GetInventory(int ID)
        {
            string[] values = new[]
            {
                "120320",
                "«20» ноября 2022",
                "специалист, Костин Анатолий Константинович",
                "Костин Анатолий Константинович",
                "отдел баз данных, специалист",
                "",
                "",
                "ул. Сормовская, д. 4, кв 11",
                "",
                "True",
                "False",
                "",
                "True",
                "False",
                "Костин Анатолий Константинович",
                "Костин Анатолий Константинович",
            };
            return new Inventory()
            {
                Template = @"D:\Users\akkostin\Documents\Templates\daaa\Акт приема-передачи ТМЦ_в пользование.docx",
                InventoryFields = values.Select((val, num) => new ClaimField() { NumeralID = num + 1, Value = val }),
            };
        }
    }
}
