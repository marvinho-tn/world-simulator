using MongoDB.Bson;

namespace Core.Api.Entities.Domain;

public class Entity
{
    public ObjectId? Id { get; set; }
    public required string Description { get; set; }
    public required List<Property> Properties { get; set; }
    public List<Rule>? Rules { get; set; }
    public required DateTime Created { get; set; }
    public required DateTime Updated { get; set; }

    public record Property(string Name, object Value, Property.PropertyType Type)
    {
        public enum PropertyType
        {
            None,
            String,
            Number,
            Boolean,
            DateTime
        }
    }
    
    public record Rule(
        string Description,
        TimeStep TimeStep,
        List<Rule.Condition> Conditions,
        List<Rule.Action> Actions)
    {
        public record Condition(string Description, string Property, ComparisonType Comparison, object Value);

        public record Action(string Property, OperationType Operation, object Value);
    }
}