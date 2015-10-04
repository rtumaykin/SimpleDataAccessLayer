using System.Linq;

namespace SimpleDataAccessLayer.Common.codegen
{
    public class TableTypeColumn
    {
        public string ColumnName { get; private set; }

        public string ClrTypeName { get; private set; }

        public TableTypeColumn(string columnName, string clrTypeName)
        {
            ColumnName = columnName;
            if ("System.Int64 System.Boolean System.DateTime System.DateTimeOffset System.Decimal System.Double Microsoft.SqlServer.Types.SqlHierarchyId System.Int32 System.Single System.Int16 System.TimeSpan System.Byte System.Guid".Split(' ').Contains(clrTypeName))
            {
                clrTypeName += "?";
            }
            ClrTypeName = clrTypeName;
        }
    }
}
