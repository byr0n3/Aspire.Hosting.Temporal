using System.Diagnostics.CodeAnalysis;
using Aspire.Hosting.ApplicationModel;

namespace Byrone.Aspire.Hosting.Temporal
{
	/// <summary>
	/// A struct that configures the creation of the required Temporal resources.
	/// </summary>
	public readonly struct TemporalContainerOptions
	{
		/// <summary>
		/// The name of the Temporal container resource.
		/// </summary>
		public required string Name { get; init; } = "Temporal";

		/// <summary>
		/// The image to use for the Temporal container.
		/// </summary>
		/// <remarks>Defaults to <c>byr0n3/temporal</c></remarks>
		public required string Image { get; init; } = "byr0n3/temporal";

		/// <summary>
		/// The tag of the <see cref="Image"/> to use.
		/// </summary>
		/// <remarks>Defaults to <c>latest</c>.</remarks>
		public required string Tag { get; init; } = "latest";

		/// <summary>
		/// The namespace that should be created for the Temporal instance.
		/// </summary>
		public required string Namespace { get; init; } = "default";

		/// <summary>
		/// The name of the <see cref="PostgresServerResource"/> that will store all of Temporal's data.
		/// </summary>
		/// <remarks>Both the <see cref="PostgresName"/> and <see cref="DatabaseName"/> properties need to be set to create a Postgres resource.</remarks>
		public required string PostgresName { get; init; } = "TemporalPostgres";

		/// <summary>
		/// The name of the <see cref="PostgresDatabaseResource"/> that will store all of Temporal's data.
		/// </summary>
		/// <remarks>Both the <see cref="PostgresName"/> and <see cref="DatabaseName"/> properties need to be set to create a Postgres resource.</remarks>
		public required string DatabaseName { get; init; } = "TemporalPostgresDatabase";

		/// <summary>
		/// The optional volume name for the Postgres resource.
		/// If this property is <see langword="null"/>, Postgres data will <b>not</b> be persisted between restarts.
		/// </summary>
		/// <remarks>This property won't have any effect if the <see cref="PostgresName"/> or <see cref="DatabaseName"/> property isn't set.</remarks>
		public string? DatabaseVolumeName { get; init; }

		[SetsRequiredMembers]
		public TemporalContainerOptions()
		{
		}

		[SetsRequiredMembers]
		public TemporalContainerOptions(string name, string postgresName, string databaseName, string @namespace) :
			this(name, "latest", postgresName, databaseName, @namespace)
		{
		}

		[SetsRequiredMembers]
		public TemporalContainerOptions(string name, string tag, string postgresName, string databaseName, string @namespace)
		{
			this.Name = name;
			this.Tag = tag;
			this.PostgresName = postgresName;
			this.DatabaseName = databaseName;
			this.Namespace = @namespace;
		}
	}
}
