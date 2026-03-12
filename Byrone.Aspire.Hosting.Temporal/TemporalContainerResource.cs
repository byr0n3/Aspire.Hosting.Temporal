using Aspire.Hosting.ApplicationModel;

namespace Byrone.Aspire.Hosting.Temporal
{
	/// <summary>
	/// A resource that represents a Temporal container.
	/// </summary>
	public sealed class TemporalContainerResource : ContainerResource, IResourceWithConnectionString
	{
		/// <inheritdoc />
		public ReferenceExpression ConnectionStringExpression =>
			ReferenceExpression.Create($"{this.GetEndpoint("server").Property(EndpointProperty.HostAndPort)}");

		internal TemporalContainerResource(string name) : base(name)
		{
		}
	}
}
