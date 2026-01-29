using Aspire.Hosting.ApplicationModel;

namespace Byrone.Aspire.Hosting.Temporal
{
	public sealed class TemporalContainerResource : ContainerResource, IResourceWithConnectionString
	{
		public ReferenceExpression ConnectionStringExpression =>
			ReferenceExpression.Create($"{this.GetEndpoint("server").Property(EndpointProperty.HostAndPort)}");

		internal TemporalContainerResource(string name) : base(name)
		{
		}
	}
}
