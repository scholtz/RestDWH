namespace RestDWH.Elastic.Model.Config
{
    public class Elastic
    {
        /// <summary>
        /// Elastic hostname
        /// </summary>
        public string Host { get; set; } = "";
        /// <summary>
        /// API key
        /// </summary>
        public string ApiKey { get; set; } = "";
        /// <summary>
        /// Index prefix is text which will be used before the entity name
        /// 
        /// Use lowercase chars and dash.
        /// </summary>
        public string IndexPrefix { get; set; } = "restdwh-";
        /// <summary>
        /// Index suffix main is text which will be used after the entity name for main index
        /// 
        /// Use lowercase chars and dash.
        /// </summary>
        public string IndexSuffixMain { get; set; } = "-main";
        /// <summary>
        /// Index suffix log is text which will be used after the entity name for main index
        /// 
        /// Use lowercase chars and dash.
        /// </summary>
        public string IndexSuffixLog { get; set; } = "-log";
        /// <summary>
        /// Ensure that schema is created
        /// </summary>
        public bool EnsureCreated { get; set; } = true;
        /// <summary>
        /// Ensure that migrations are applied
        /// </summary>
        public bool ApplyMigration { get; set; } = false;
    }
}
