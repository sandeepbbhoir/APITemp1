using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace CodeGenerator
{
    public class MainForm : Form
    {
        private Button btnGenerate;
        private ComboBox cmbEntities;
        private Label lblSelectEntity;

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Code Generator";
            this.Width = 400;
            this.Height = 200;

            lblSelectEntity = new Label { Text = "Select Entity:", Left = 20, Top = 30, Width = 100 };
            cmbEntities = new ComboBox { Left = 130, Top = 25, Width = 200 };
            btnGenerate = new Button { Text = "Generate", Left = 130, Top = 70, Width = 100 };

            btnGenerate.Click += BtnGenerate_Click;

            this.Controls.Add(lblSelectEntity);
            this.Controls.Add(cmbEntities);
            this.Controls.Add(btnGenerate);
        }

        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);
            LoadEntities();
        }

        private void LoadEntities()
        {
            string excelPath = @"E:\RepoFinal\Asp.Net-Core-Inventory-Order-Management-System\Entity.xlsx";
            try
            {
                var entities = ExcelReader.GetEntities(excelPath);
                cmbEntities.Items.Clear();
                cmbEntities.Items.AddRange(entities.ToArray());
                if (cmbEntities.Items.Count > 0)
                    cmbEntities.SelectedIndex = 0;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error reading Excel: {ex.Message}");
            }
        }

        private void BtnGenerate_Click(object sender, System.EventArgs e)
        {
            if (cmbEntities.SelectedItem == null)
            {
                MessageBox.Show("Please select an entity.");
                return;
            }
            string entityName = cmbEntities.SelectedItem.ToString();
            string excelPath = @"E:\RepoFinal\Asp.Net-Core-Inventory-Order-Management-System\Entity.xlsx";
            var entityDef = ExcelReader.GetEntityDefinition(excelPath, entityName);

            // 1. Domain: Entity class
            string domainPath = $"..\\..\\..\\Core\\Domain\\Entities\\{entityName}.cs";
            File.WriteAllText(domainPath, GenerateEntityClass(entityDef));

            // 2. Application: Manager class in Features
            string appManagerPath = $"..\\..\\..\\Core\\Application\\Features\\{entityName}Manager.cs";
            File.WriteAllText(appManagerPath, GenerateManagerClass(entityDef));

            // 3. Infrastructure: Configuration class
            string infraConfigPath = $"..\\..\\..\\Infrastructure\\Infrastructure\\DataAccessManager\\EFCore\\Configurations\\{entityName}Configuration.cs";
            File.WriteAllText(infraConfigPath, GenerateConfigurationClass(entityDef));

            // 4. Infrastructure: Add DbSet and ApplyConfiguration to DataContext
            string dataContextPath = "..\\..\\..\\Infrastructure\\Infrastructure\\DataAccessManager\\EFCore\\Contexts\\DataContext.cs";
            UpdateDataContext(dataContextPath, entityDef);

            // 5. BackEndApi: Controller
            string controllerPath = $"..\\..\\..\\Presentation\\BackEndApi\\BackEnd\\Controllers\\{entityName}Controller.cs";
            File.WriteAllText(controllerPath, GenerateControllerClass(entityDef));

            MessageBox.Show($"Code generated for: {entityName}");
        }

        private string GenerateEntityClass(CodeGenerator.EntityDefinition entity)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine($"namespace Domain.Entities");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {entity.Name}");
            sb.AppendLine("    {");
            foreach (var prop in entity.Properties)
                sb.AppendLine($"        public {prop.Type} {prop.Name} {{ get; set; }}");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private string GenerateManagerClass(CodeGenerator.EntityDefinition entity)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using Domain.Entities;");
            sb.AppendLine($"namespace Application.Features");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {entity.Name}Manager");
            sb.AppendLine("    {");
            sb.AppendLine($"        // Add manager logic for {entity.Name}");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private string GenerateConfigurationClass(CodeGenerator.EntityDefinition entity)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using Domain.Entities;");
            sb.AppendLine("using Microsoft.EntityFrameworkCore;");
            sb.AppendLine("using Microsoft.EntityFrameworkCore.Metadata.Builders;");
            sb.AppendLine($"namespace Infrastructure.DataAccessManager.EFCore.Configurations");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {entity.Name}Configuration : IEntityTypeConfiguration<{entity.Name}>");
            sb.AppendLine("    {");
            sb.AppendLine($"        public void Configure(EntityTypeBuilder<{entity.Name}> builder)");
            sb.AppendLine("        {");
            sb.AppendLine("            // Add configuration logic here");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private void UpdateDataContext(string dataContextPath, CodeGenerator.EntityDefinition entity)
        {
            // Read, update, and write DataContext.cs
            var lines = File.ReadAllLines(dataContextPath);
            var sb = new StringBuilder();
            bool dbSetAdded = false;
            bool configAdded = false;
            foreach (var line in lines)
            {
                sb.AppendLine(line);
                if (!dbSetAdded && line.Contains("protected override void OnModelCreating"))
                {
                    sb.AppendLine($"        public DbSet<{entity.Name}> {entity.Name} {{ get; set; }}");
                    dbSetAdded = true;
                }
                if (!configAdded && line.Contains("modelBuilder.ApplyConfiguration"))
                {
                    sb.AppendLine($"        modelBuilder.ApplyConfiguration(new {entity.Name}Configuration());");
                    configAdded = true;
                }
            }
            File.WriteAllText(dataContextPath, sb.ToString());
        }

        private string GenerateControllerClass(CodeGenerator.EntityDefinition entity)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using Domain.Entities;");
            sb.AppendLine("using Microsoft.AspNetCore.Mvc;");
            sb.AppendLine($"namespace BackEnd.Controllers");
            sb.AppendLine("{");
            sb.AppendLine($"    [ApiController]");
            sb.AppendLine($"    [Route(\"api/[controller]\")]");
            sb.AppendLine($"    public class {entity.Name}Controller : ControllerBase");
            sb.AppendLine("    {");
            sb.AppendLine($"        // Add controller logic for {entity.Name}");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}
