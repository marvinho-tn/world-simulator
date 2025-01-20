using Core.Api.Entities.Domain;
using Core.Api.Entities.Endpoints;
using MongoDB.Driver;

namespace Core.Api.Tests.Entities;

public class CreateEntityTests : IClassFixture<MongoDbFixture>
{
    private readonly CreateEntity.Endpoint _enpdoint;
    private readonly IMongoDatabase _database;
    private readonly CreateEntity.Validator _validator;

    public CreateEntityTests(MongoDbFixture mongoDbFixture)
    {
        _database = mongoDbFixture.Database;
        _enpdoint = new CreateEntity.Endpoint(_database);
        _validator = new CreateEntity.Validator();
    }

    [Fact]
    public async Task CreateEntity_ShouldCreateEntitySuccessfully()
    {
        // Arrange
        var request = CreateNewRequest();

        // Act
        await _enpdoint.HandleAsync(request, CancellationToken.None);

        // Assert
        var createdEntity = await _database.GetCollection<Entity>("Entities")
            .Find(e => e.Id.ToString() == _enpdoint.Response.Id)
            .FirstOrDefaultAsync();

        Assert.NotNull(createdEntity);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task RequestValidator_ShouldHaveError_WhenDescriptionIsEmpty(string? description)
    {
        // Arrange
        var request = CreateNewRequest()
            with { Description = description };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.Collection(
            result.Errors,
            error => Assert.Equal("'Description' must not be empty.", error.ErrorMessage));
    }

    [Fact]
    public async Task RequestValidator_ShouldHaveError_WhenPropertiessEmpty()
    {
        // Arrange
        var request = CreateNewRequest()
            with { Properties = [] };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.Collection(
            result.Errors,
            error => Assert.Equal(ErrorCodes.EntityPropertiesNotEmpty, error.ErrorCode));
    }

    [Fact]
    public async Task RequestValidator_ShouldHaveError_WhenPropertiesIsNull()
    {
        // Arrange
        var request = CreateNewRequest()
            with { Properties = null };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.Collection(
            result.Errors,
            error => Assert.Equal(ErrorCodes.EntityPropertiesNotEmpty, error.ErrorCode));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task RequestValidator_ShouldHaveError_WhenPropertyNameEmpty(string? propertyName)
    {
        // Arrange
        var request = CreateNewRequest()
            with { Properties = [
                CreateNewRequestProperty()
                with { Name = propertyName },
            ] };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.Collection(
            result.Errors,
            error => Assert.Equal(ErrorCodes.EntityPropertyNameNotEmpty, error.ErrorCode));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task RequestValidator_ShouldHaveError_WhenPropertyValueEmpty(string? propertyValue)
    {
        // Arrange
        var request = CreateNewRequest()
            with { Properties = [
                CreateNewRequestProperty()
                    with { Value = propertyValue },
            ] };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.Collection(
            result.Errors,
            error => Assert.Equal(ErrorCodes.EntityPropertyValueNotEmpty, error.ErrorCode));
    }

    private static CreateEntity.Request CreateNewRequest()
    {
        return new CreateEntity.Request("DefaultDescription", [
            CreateNewRequestProperty(),
        ]);
    }

    private static CreateEntity.Request.Property CreateNewRequestProperty()
    {
        return new CreateEntity.Request.Property("DefaultName", "DefaultValue", Entity.Property.PropertyType.String);
    }
}