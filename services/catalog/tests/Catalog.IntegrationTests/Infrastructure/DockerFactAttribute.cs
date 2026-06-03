namespace Catalog.IntegrationTests.Infrastructure;

public sealed class DockerFactAttribute : FactAttribute
{
    public DockerFactAttribute()
    {
        if (!string.Equals(
            Environment.GetEnvironmentVariable("RUN_DOCKER_INTEGRATION_TESTS"),
            "true",
            StringComparison.OrdinalIgnoreCase))
        {
            Skip = "Set RUN_DOCKER_INTEGRATION_TESTS=true to run Docker-backed integration tests.";
        }
    }
}
