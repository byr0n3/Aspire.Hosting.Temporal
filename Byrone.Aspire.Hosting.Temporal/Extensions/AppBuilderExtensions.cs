using Aspire.Hosting.ApplicationModel;
using Byrone.Aspire.Hosting.Temporal;

// ReSharper disable once CheckNamespace
namespace Aspire.Hosting
{
	public static class AppBuilderExtensions
	{
		extension(IDistributedApplicationBuilder builder)
		{
			/// <summary>
			/// Adds a <c>temporalio/server</c> container with its needed dependencies.
			/// </summary>
			/// <param name="options">The resource options that influence the created resource(s).</param>
			/// <returns>The created resource builder.</returns>
			public IResourceBuilder<TemporalContainerResource> AddTemporalContainer(TemporalContainerOptions options)
			{
				var name = options.Name;

				// Username & password are fixed resources so they can be properly shared with the Temporal container instance.
				var postgresUsername = builder.AddParameter($"{options.PostgresName}Username",
															static () => "postgres");
				var postgresPassword = builder.AddParameter($"{options.PostgresName}Password",
															new GenerateParameterDefault { MinLength = 22 }, secret: true);

				// Creates a resource that starts a `postgres:16-alpine` container.
				// The official Temporal docker-compose.yml example uses 16, which means that I will as well.
				// Source: https://github.com/temporalio/samples-server/blob/main/compose/.env
				// @todo Configure Postgres resource to wait exiting until Temporal has exited (Temporal tries to save to db when exiting)
				var postgres = builder.AddPostgres(options.PostgresName)
									  .WithImage("postgres", "16-alpine")
									  .WithUserName(postgresUsername)
									  .WithPassword(postgresPassword);

				// Add a data volume if requested.
				if (options.DatabaseVolumeName is not null)
				{
					postgres.WithDataVolume(options.DatabaseVolumeName);
				}

				// Creates a `temporal` database in the Postgres container.
				postgres.AddDatabase(options.DatabaseName, "temporal");

				// Temporal doesn't accept connection strings, we need to manually get the container's host/IP and port.
				// These need to be references so they get resolved after the container starts.
				var postgresHost =
					ReferenceExpression.Create($"{postgres.GetEndpoint("tcp").Property(EndpointProperty.Host)}");
				var postgresPort =
					ReferenceExpression.Create($"{postgres.GetEndpoint("tcp").Property(EndpointProperty.Port)}");

				// @todo Health check
				// @todo Auto-restart on crash
				// Creates the Temporal orchestrator server.
				var temporal = builder.AddResource(new TemporalContainerResource(name))
									  .WithImage(options.Image, options.Tag)
									  .WithChildRelationship(postgres)
									  .WithIconName("Tag")
									  .WithEnvironment("DB", "postgres12")
									  .WithEnvironment("DB_PORT", postgresPort)
									  .WithEnvironment("POSTGRES_SEEDS", postgresHost)
									  .WithEnvironment("POSTGRES_USER", postgresUsername)
									  .WithEnvironment("POSTGRES_PWD", postgresPassword)
									  .WithEnvironment("NAMESPACE", options.Namespace)
									  // Temporal binds to `127.0.0.1` by default,
									  // which can cause issues when connecting to the Temporal issue from other containers.
									  .WithEnvironment("BIND_ON_IP", "0.0.0.0")
									  .WithEnvironment("TEMPORAL_BROADCAST_ADDRESS", "0.0.0.0")
									  .WithHttpsEndpoint(port: 7233, targetPort: 7233, name: "server").AsHttp2Service()
									  .WithHttpEndpoint(port: 7234, targetPort: 7234, name: "http")
									  .WithHttpEndpoint(port: 7235, targetPort: 7235, name: "metrics")
									  .PublishAsConnectionString();

				return temporal;
			}

			/// <summary>
			/// Adds a <c>temporalio/ui</c> container that connects to the given <paramref name="temporal"/> instance.
			/// </summary>
			/// <param name="name">The name of the resource to add.</param>
			/// <param name="imageTag">The tag of the <c>temporalio/ui</c> image to use.</param>
			/// <param name="defaultNamespace">The namespace that should be initially shown when opening the UI.</param>
			/// <param name="temporal">The <see cref="TemporalContainerResource"/> builder that contains the Temporal connection string.</param>
			/// <returns>The created resource builder.</returns>
			public IResourceBuilder<TemporalUiContainerResource> AddTemporalUi(string name,
																			   string imageTag,
																			   string defaultNamespace,
																			   IResourceBuilder<TemporalContainerResource> temporal) =>
				// @todo Health check
				builder.AddResource(new TemporalUiContainerResource(name, temporal))
					   .WithImage("temporalio/ui", imageTag)
					   .WithIconName("TagMultiple")
					   .WithEnvironment("TEMPORAL_ADDRESS", temporal)
					   .WithEnvironment("TEMPORAL_DEFAULT_NAMESPACE", defaultNamespace)
					   .WithEnvironment("TEMPORAL_CSRF_COOKIE_INSECURE", "true")
					   .WithReference(temporal)
					   .WithHttpEndpoint(port: 8080, targetPort: 8080, name: "ui");
		}
	}
}
