using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Core.Api.Tests;

public class MongoDbFixture : IDisposable
{
    public IMongoDatabase Database { get; }

    public MongoDbFixture()
    {
        var client = new MongoClient("mongodb://root:root@localhost:27017");
        
        Database = client.GetDatabase("WorldSimulatorTests");

        var pack = new ConventionPack
        {
            new CamelCaseElementNameConvention(),
            new EnumRepresentationConvention(BsonType.String)
        };
        
        ConventionRegistry.Register("camelCase", pack, t => true);
    }

    public void Dispose()
    {
        Database.Client.DropDatabase(Database.DatabaseNamespace.DatabaseName);
    }
}
