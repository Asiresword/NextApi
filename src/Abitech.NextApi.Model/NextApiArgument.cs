namespace Abitech.NextApi.Model
{
    /// <summary>
    /// Basic interface for NextApi service method argument
    /// </summary>
    public interface INextApiArgument
    {
    }

    /// <summary>
    /// NextApi service method argument
    /// </summary>
    public class NextApiArgument : INextApiArgument
    {
        /// <summary>
        /// Argument name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Argument value
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// NextApi service method argument
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Parameter value</param>
        public NextApiArgument(string name, object value)
        {
            Name = name;
            Value = value;
        }

        /// <inheritdoc />
        public NextApiArgument()
        {
        }
    }

    /// <summary>
    /// NextApi service method file argument
    /// </summary>
    public class NextApiFileArgument : INextApiArgument
    {
        /// <summary>
        /// Path to file
        /// </summary>
        public string FilePath { get; set; }
    }
}
