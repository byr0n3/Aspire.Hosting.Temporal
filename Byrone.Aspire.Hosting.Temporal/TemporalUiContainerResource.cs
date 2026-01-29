using Aspire.Hosting.ApplicationModel;

namespace Byrone.Aspire.Hosting.Temporal
{
	public sealed class TemporalUiContainerResource : ContainerResource, IResourceWithParent<TemporalContainerResource>
	{
		public TemporalContainerResource Parent { get; init; }

		internal TemporalUiContainerResource(string name, IResourceBuilder<TemporalContainerResource> temporal) : base(name) =>
			this.Parent = temporal.Resource;
	}
}
