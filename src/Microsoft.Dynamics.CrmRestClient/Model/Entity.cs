namespace Microsoft.Dynamics.CrmRestClient
{
	using Newtonsoft.Json.Linq;
	using System;
	using System.Collections.Generic;

    /// <summary>
    /// Represents a single record in CRM.
    /// </summary>
	public class Entity : IEntity, IEntityReference
	{
		private string entityLogicalName = string.Empty;
		private string primaryIdAttribute = string.Empty;

		private Guid entityId = Guid.Empty;

        /// <summary>
        /// Identifier for the record.
        /// </summary>
		public Guid Id
		{
			get
			{
				if (entityId.Equals(Guid.Empty))
				{
					if (!string.IsNullOrEmpty(this.primaryIdAttribute))
					{
						entityId = this.EntityJson.ReadChildAs(this.primaryIdAttribute, Guid.Empty);
					}
				}
				return entityId;
			}
		}

        /// <summary>
        /// Logical name of the record.
        /// </summary>
		public string LogicalName
		{
			get
			{
				return this.entityLogicalName;
			}
		}

        /// <summary>
        /// JSON representing the record.
        /// </summary>
		protected JToken EntityJson { get; private set; }

        /// <summary>
        /// Represents a single record in CRM.
        /// </summary>
        /// <param name="entityJson">JSON to form entity.</param>
        /// <param name="entityLogicalName">Logical name of the record.</param>
        /// <param name="primaryIdAttribute">Id of the record.</param>
		public Entity(JToken entityJson, string entityLogicalName, string primaryIdAttribute)
		{
			this.EntityJson = entityJson;
			this.entityLogicalName = entityLogicalName;
			this.primaryIdAttribute = primaryIdAttribute;
		}

        /// <summary>
        /// Represents a single record in CRM.
        /// </summary>
        /// <param name="entityJson">JSON to form entity.</param>
        /// <param name="entityMetadata">Metadata for the record.</param>
		public Entity(JToken entityJson, IEntityMetadata entityMetadata) : this(entityJson, entityMetadata.LogicalName, entityMetadata.PrimaryIdAttribute)
		{
			//
		}

        /// <summary>
        /// Get list of fields in the entity.
        /// </summary>
        /// <returns>Collection of field names</returns>
		public IEnumerable<string> GetFieldNames()
		{
			return this.EntityJson.GetChildNames();
		}

        /// <summary>
        /// Get the value for <paramref name="fieldName"/> if exists, otherwise return <paramref name="defaultValue"/>
        /// </summary>
        /// <typeparam name="ResultType">Type of the value.</typeparam>
        /// <param name="fieldName">Name of the field present in the entity.</param>
        /// <param name="defaultValue">Default value to return if <paramref name="fieldName"/> is not present.</param>
        /// <returns>Value for specified field/returns>
		public ResultType GetFieldValue<ResultType>(string fieldName, ResultType defaultValue = default(ResultType))
		{
			return this.EntityJson.ReadChildAs(fieldName, defaultValue);
		}
	}
}
