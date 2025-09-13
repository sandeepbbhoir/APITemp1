using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;

namespace CodeGenerator
{
    public class EntityProperty
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Required { get; set; }
        public string MaxLength { get; set; }
        public string MinLength { get; set; }
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
                foreach (var worksheet in workbook.Worksheets)
                {
                    var sheetName = worksheet.Name;
                    if (!string.IsNullOrWhiteSpace(sheetName))
                        entities.Add(sheetName);
                }
            }
            return new List<string>(entities);
        }

        public static EntityDefinition GetEntityDefinition(string excelPath, string entityName)
        {
            var entity = new EntityDefinition { Name = entityName };
            using (var workbook = new XLWorkbook(excelPath))
            {
                var worksheet = workbook.Worksheet(entityName);
                foreach (var row in worksheet.RowsUsed().Skip(1))
                {
                    var propName = row.Cell(2).GetString();
                    var propType = row.Cell(3).GetString();
                    var required = row.Cell(6).GetString();
                    var maxLength = row.Cell(7).GetString();
                    var minLength = row.Cell(8).GetString();
                    if (!string.IsNullOrWhiteSpace(propName) && !string.IsNullOrWhiteSpace(propType))
                    {
                        entity.Properties.Add(new EntityProperty
                        {
                            Name = propName,
                            Type = propType,
                            Required = required,
                            MaxLength = maxLength,
                            MinLength = minLength
                        });
                    }
                }
            }
            return entity;
        }
    }
}
