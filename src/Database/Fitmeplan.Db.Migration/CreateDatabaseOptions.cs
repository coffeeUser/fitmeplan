namespace Fitmeplan.Db.Migration
{
    public class CreateDatabaseOptions : DatabaseTaskOptionsBase
    {
        public string ScriptFile { get; set; }
        public bool SchemaOnly { get; set; }
    }
}
