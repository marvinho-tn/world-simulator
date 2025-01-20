using Core.Api.Entities.Domain;
using FastEndpoints;
using FluentValidation;
using MongoDB.Driver;

namespace Core.Api.Entities.Endpoints;

public static class CreateEntity
{
    public record Request(
        string Description,
        IEnumerable<Request.Property> Properties)
    {
        public record Property(string Name, object Value, Entity.Property.PropertyType Type);
    }

    public class Validator : Validator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Description)
                .NotEmpty()
                .WithErrorCode(ErrorCodes.EntityDescriptionNotEmpty);
            
            RuleFor(x => x.Properties)
                .NotEmpty()
                .WithErrorCode(ErrorCodes.EntityPropertiesNotEmpty);
            
            RuleForEach(x => x.Properties).SetValidator(new PropertyValidator());
        }
    }
    
    public class PropertyValidator : Validator<Request.Property>
    {
        public PropertyValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithErrorCode(ErrorCodes.EntityPropertyNameNotEmpty);
            
            RuleFor(x => x.Value)
                .NotEmpty()
                .WithErrorCode(ErrorCodes.EntityPropertyValueNotEmpty);
        }
    }
    
    public record Response(string Id);

    public class Endpoint(IMongoDatabase database) : Endpoint<Request, Response>
    {
        public override void Configure()
        {
            Post("/entities");
            AllowAnonymous();
        }
        
        public override async Task<Response> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var entity = new Entity
            {
                Description = request.Description,
                Properties = request.Properties
                    .Select(p => new Entity.Property(p.Name, p.Value, p.Type))
                    .ToList(),
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };
            
            var collection = database.GetCollection<Entity>("Entities");
            
            await collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
            
            return new Response(entity.Id.ToString()!);
        }
    }
}