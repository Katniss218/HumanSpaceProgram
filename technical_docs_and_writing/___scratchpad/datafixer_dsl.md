# DataFixer Domain-Specific Language

    This document is not strictly documentation. It hopefully should be close, but I'm not perfect and sometimes can't be bothered to update every doc.
#

## Introduction:

The "program" of the DataFixer DSL consists of a sequence of transformations.
A transformation can be thought of as a function invocation.

Each of these transformations has a scope. It specifies which parts of the overall SerializedData tree the transformation can access, and specifies what the access paths are relative to.

## Transformations:

To change this scope you can use:
- selectors
- filters

#### Selectors:

Selectors are used to narrow the scope to a subset of the original scope by sepecting elements that match the predicate.
They reset the scope for things that come after them.

```df

(FROM "Vessels".[*]."gameobjects")      // Runs the transformation on every matched object.
{
    // ...
}

(FROM any)                              // Flattens the hierarchy, running the transform for every element.
{
    // ...
}

```

There is a big difference between the two. Selectors change the root element, while filters don't.

#### Filters:

Filters are used to narrow the scope based on a boolean condition.
They don't reset the scope for things after them, only filter out parts of the scope.

```df

(FROM any WHERE $"$type" == "somevalue")    // Runs the transform on any object that contains an immediate child "$type" of value "somevalue"
{
    // ...
}

```

## Operators:

DataFixer supports the standard range of operators on primitive types:
- `+`, `-`, `*`, `/`, `%` arithmetic operators
- `!`, `&`, `|`, `^` boolean and bitwise operators
- `>`, `>=`, `==`, `!=`, `<`, `<=` comparison operators
- `&&`, `||` conditional operators



# TODO

FROM and WHERE are like functions.
they have a scope, that is returned by the `$` operator.


