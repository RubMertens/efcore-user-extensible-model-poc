using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Poc.CRM.EfExtensible.Web.Application;
using Poc.CRM.EfExtensible.Web.Infrastructure;
using Poc.CRM.EfExtensible.Web.Infrastructure.Meta;
using Poc.CRM.EfExtensible.Web.Infrastructure.Meta.Models;
using Poc.CRM.EfExtensible.Web.Infrastructure.Models;

namespace Poc.CRM.EfExtensible.Web.Features.Companies;

/// <summary>
/// Extends the company with a new field of extra data.
/// </summary>
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

public class AddFieldToCompanyHandler(CrmDbContext modelContext, MetaDbContext metaDbContext) : IAddFieldToCompany
{
    /// <summary>
    /// Map the field typs to their corresponding .NET types. This is used to generate the SQL for the new field based on EF conventions.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
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
        // Begin a transaction to ensure that the model and the database are in sync.
        await using var transaction = await metaDbContext.Database.BeginTransactionAsync();

        //find the company entity type in the dbContexts meta model
        var companyEntityType = modelContext.Model.FindEntityType(typeof(CompanyMetaModel)) ??
                                throw new InvalidOperationException();

        var additionalField = new AdditionalField
        {
            EntityName = companyEntityType.Name,
            PropertyName = command.FieldName,
            PropertyType = MapFieldType(command.FieldType),
            IsRequired = command.IsRequired,
            MaxLength = command.MaxLength
        };
        //add the new field to the in-mem metamodel used in the modelbuilder
        modelContext.MetaModel.Fields.Add(additionalField);
        //add the new field to the database using the metaContext (which is not impacted by the metamodel changes)
        metaDbContext.Fields.Add(new AdditionalFieldDto
        {
            EntityName = additionalField.EntityName,
            PropertyName = additionalField.PropertyName,
            PropertyFullyQualifiedType = additionalField.PropertyType.AssemblyQualifiedName,
            IsRequired = additionalField.IsRequired,
            MaxLength = additionalField.MaxLength
        });
        //Get the version of the meta model
        var info = await metaDbContext.Info.SingleOrDefaultAsync();
        if (info == null)
        {
            info = new MetaModelInfo();
            metaDbContext.Info.Add(info);
        }

        info.Version++;

        //Get the table name for the entity type in order to generate the SQL
        var companyTableName = companyEntityType.GetTableName();

        //Get the mapped primitive type for the field type using the EF Core type mapping source
        var typeMappingSource = modelContext.GetInfrastructure().GetRequiredService<IRelationalTypeMappingSource>();
        RelationalTypeMapping? mapping = typeMappingSource.FindMapping(additionalField.PropertyType);

        var columnSqlPart = command.FieldType switch
        {
            IAddFieldToCompany.Command.FieldTypes.Number => SqlForNumber(command, mapping),
            IAddFieldToCompany.Command.FieldTypes.Text => SqlForText(command, mapping),
            IAddFieldToCompany.Command.FieldTypes.Date => SqlForDate(command, mapping),
            _ => throw new ArgumentOutOfRangeException()
        };

        var sql = $"""
                   alter table {companyTableName}
                   {columnSqlPart};
                   """;

        //Execute the SQL to add the new column to the table
        await metaDbContext.Database.ExecuteSqlRawAsync(sql);
        //Save the changes to the metaDbContext
        await metaDbContext.SaveChangesAsync();
        //Update the version of the meta model triggering an invalidation of the model cache
        modelContext.MetaModel.Version = info.Version;
        //Commit the transaction
        await transaction.CommitAsync();

        return Result<int>.Succeed(modelContext.MetaModel.Version);
    }

    /// <summary>
    /// Generates sql for number column
    /// </summary>
    /// <param name="command"></param>
    /// <param name="mapping"></param>
    /// <returns></returns>
    private string SqlForNumber(IAddFieldToCompany.Command command, RelationalTypeMapping mapping)
    {
        var storeType = mapping.StoreType;
        var optionality = command.IsRequired ? "not null" : "";
        return $"add [{command.FieldName}] {storeType} {optionality};";
    }


    /// <summary>
    /// generates sql for date column
    /// </summary>
    /// <param name="command"></param>
    /// <param name="mapping"></param>
    /// <returns></returns>
    private string SqlForDate(IAddFieldToCompany.Command command, RelationalTypeMapping mapping)
    {
        var storeType = mapping.StoreType;
        var optionality = command.IsRequired ? "not null" : "";
        return $"add [{command.FieldName}] {storeType} {optionality};";
    }

    /// <summary>
    /// generates sql for text column
    /// </summary>
    /// <param name="command"></param>
    /// <param name="mapping"></param>
    /// <returns></returns>
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