using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Poc.CRM.EfExtensible.Web.Application;
using Poc.CRM.EfExtensible.Web.Infrastructure;
using Poc.CRM.EfExtensible.Web.Infrastructure.Models;

namespace Poc.CRM.EfExtensible.Web.Features.Companies;

public interface IAddFieldToCompany : ICommand
{
    record Command
    {
        public string FieldName { get; init; }
        public FieldTypes FieldType { get; init; }
        public bool IsRequired { get; init; }
        public int? MaxLength { get; init; }
        public object? DefaultValue { get; init; }

        public enum FieldTypes
        {
            Number,
            Text,
            Date
        }
    };

    Task<Result<int>> AddField(Command command);
}

public class AddFieldToCompanyHandler(CrmDbContext context) : IAddFieldToCompany
{
    private Type MapFieldType(IAddFieldToCompany.Command.FieldTypes type)
    {
        return type switch
        {
            IAddFieldToCompany.Command.FieldTypes.Number => typeof(int),
            IAddFieldToCompany.Command.FieldTypes.Text => typeof(string),
            IAddFieldToCompany.Command.FieldTypes.Date => typeof(DateTime),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public async Task<Result<int>> AddField(IAddFieldToCompany.Command command)
    {
        var companyEntityType = context.Model.FindEntityType(typeof(Company)) ?? throw new InvalidOperationException();

        var additionalField = new AdditionalField
        {
            EntityName = companyEntityType.Name,
            PropertyName = command.FieldName,
            PropertyType = MapFieldType(command.FieldType),
            IsRequired = command.IsRequired,
            MaxLength = command.MaxLength
        };
        context.MetaModel.Fields.Add(additionalField);
        var companyTableName = companyEntityType.GetTableName();

        var typeMappingSource = context.GetInfrastructure().GetRequiredService<IRelationalTypeMappingSource>();
        RelationalTypeMapping? mapping = typeMappingSource.FindMapping(additionalField.PropertyType);


        var columnSqlPart = command.FieldType switch
        {
            IAddFieldToCompany.Command.FieldTypes.Number => SqlForNuber(command, mapping),
            IAddFieldToCompany.Command.FieldTypes.Text => SqlForText(command, mapping),
            IAddFieldToCompany.Command.FieldTypes.Date => SqlForDate(command, mapping),
            _ => throw new ArgumentOutOfRangeException()
        };
        var sql = $"""
                   alter table {companyTableName}
                   {columnSqlPart};
                   """;
        await context.Database.ExecuteSqlRawAsync(sql);

        context.MetaModel.Version++;

        return Result<int>.Succeed(context.MetaModel.Version);
    }

    private string SqlForNuber(IAddFieldToCompany.Command command, RelationalTypeMapping mapping)
    {
        var storeType = mapping.StoreType;
        var optionality = command.IsRequired ? "not null" : "";
        return $"add [{command.FieldName}] {storeType} {optionality};";
    }

    private string SqlForDate(IAddFieldToCompany.Command command, RelationalTypeMapping mapping)
    {
        var storeType = mapping.StoreType;
        var optionality = command.IsRequired ? "not null" : "";
        return $"add [{command.FieldName}] {storeType} {optionality};";
    }

    private string SqlForText(IAddFieldToCompany.Command command, RelationalTypeMapping mapping)
    {
        var storeType = mapping.StoreType;
        if (command.MaxLength.HasValue)
        {
            storeType = mapping.StoreTypeNameBase + $"({command.MaxLength.Value})";
        }

        var nullOrNotNull = command.IsRequired ? $"NOT NULL" : $"NULL";
        var defaultOrNot = command.DefaultValue != null ? $"DEFAULT('{command.DefaultValue}')" : "";
        
        return $"add [{command.FieldName}] {storeType} {nullOrNotNull} {defaultOrNot};";
    }
}