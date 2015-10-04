using SimpleDataAccessLayer.Common.config.models;

namespace SimpleDataAccessLayer.Common.codegen
{
#if DEBUG 
    public class Common
#else 
    internal class Common
#endif
    {
        private readonly DalConfig _config;

        public Common(DalConfig config)
        {
            _config = config;
        }

        public string GetCode()
        {
            if (_config.Enums.Count == 0 && _config.Procedures.Count == 0)
                return "/* nothing */";

            return string.Format(
                @"namespace {1} {{
    public class ExecutionScope : global::System.IDisposable
    {{
        internal static global::System.Collections.Generic.List<int> RetryableErrors = new global::System.Collections.Generic.List<int>
        {{
			53, 601, 615, 913, 921, 922, 923, 924, 926, 927, 941, 955, 956, 983, 976, 978, 979, 982, 983, 1204, 1205, 1214, 1222, 1428, 35201
		}};
	    public global::System.Data.SqlClient.SqlTransaction Transaction {{ get; private set; }}
        private readonly global::System.Data.SqlClient.SqlConnection _connection;
        public ExecutionScope()
		{{
			this._connection = new global::System.Data.SqlClient.SqlConnection(ConnectionString);
			this._connection.Open();
			this.Transaction = _connection.BeginTransaction();
		}}
        public void Commit()
		{{
			if (this.Transaction != null)
				this.Transaction.Commit();
		}}
        public void Rollback()
		{{
			if (this.Transaction != null)
				this.Transaction.Rollback();
		}}
		public void Dispose()
		{{
			if (this.Transaction != null)
			{{
				this.Transaction.Dispose();
			}}
			if (this._connection != null && this._connection.State != System.Data.ConnectionState.Closed)
			{{
				this._connection.Close();
				this._connection.Dispose();
			}}
		}}
		private static global::System.String _connectionString;
		public static global::System.String ConnectionString
		{{
			get
			{{
				global::System.Threading.LazyInitializer.EnsureInitialized(
					ref _connectionString,
					() => global::System.Configuration.ConfigurationManager.ConnectionStrings[""{0}""].ConnectionString
				);
				return _connectionString;
			}}
			set
			{{
				_connectionString = value;
			}}
		}}
	}}
}}",
                _config.RuntimeConnectionStringName,
                _config.Namespace);
        }

    }
}
