using Aspire.Hosting.ApplicationModel;

namespace Byrone.Aspire.Hosting.Temporal
{
	/// <summary>
	/// A resource that represents a Temporal UI container.
	/// </summary>
	public sealed class TemporalUiContainerResource : ContainerResource, IResourceWithParent<TemporalContainerResource>
	{
		/// <summary>
		/// Gets the Temporal resource.
		/// </summary>
		public TemporalContainerResource Parent { get; init; }

		internal TemporalUiContainerResource(string name, IResourceBuilder<TemporalContainerResource> temporal) : base(name) =>
			this.Parent = temporal.Resource;
	}
}
