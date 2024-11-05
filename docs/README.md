# Extensible data types using EF core

## Introduction

This project contains a Proof of Concept (POC) to extend EF core's model and the underlying
tables in the database with additional fields. The goal is to provide a way to add fields to
the model without changing the model itself.

## Project layout

Project is build using vertical slices.

```
- Features
    - Domain
        - FeatureOne
        - FeatureTwo
        - ...
- Infrastructure
```

## Points of interest

### AddFieldToCompany

This handler creates a new field in the `Company` table.

### MetaModel

Configures ef core to understand the metamodel extensiosn using the modelbuilder

## Inspiration & Credit

This implementation heavily borows from
this [blog post](https://www.thinktecture.com/en/entity-framework-core/ef-core-user-defined-fields-and-tables/)

