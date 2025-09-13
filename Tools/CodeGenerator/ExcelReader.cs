using System.Collections.Generic;
using ClosedXML.Excel;

namespace CodeGenerator
{
    public class EntityProperty
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }

    public class EntityDefinition
    {
        public string Name { get; set; }
        public List<EntityProperty> Properties { get; set; } = new List<EntityProperty>();
    }

    public static class ExcelReader
    {
        public static List<string> GetEntities(string excelPath)
        {
            var entities = new HashSet<string>();
            using (var workbook = new XLWorkbook(excelPath))
            {
                var worksheet = workbook.Worksheet(1); // Assumes first worksheet
                foreach (var row in worksheet.RowsUsed())
                {
                    var entityName = row.Cell(1).GetString(); // Assumes entity name is in first column
                    if (!string.IsNullOrWhiteSpace(entityName))
                        entities.Add(entityName);
                }
            }
            return new List<string>(entities);
        }

        public static EntityDefinition GetEntityDefinition(string excelPath, string entityName)
        {
            var entity = new EntityDefinition { Name = entityName };
            using (var workbook = new XLWorkbook(excelPath))
            {
                var worksheet = workbook.Worksheet(1);
                foreach (var row in worksheet.RowsUsed())
                {
                    var name = row.Cell(1).GetString();
                    if (name == entityName)
                    {
                        var propName = row.Cell(2).GetString();
                        var propType = row.Cell(3).GetString();
                        if (!string.IsNullOrWhiteSpace(propName) && !string.IsNullOrWhiteSpace(propType))
                        {
                            entity.Properties.Add(new EntityProperty { Name = propName, Type = propType });
                        }
                    }
                }
            }
            return entity;
        }
    }
}
