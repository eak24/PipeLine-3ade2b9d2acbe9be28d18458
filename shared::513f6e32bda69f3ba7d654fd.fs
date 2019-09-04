FeatureScript 1096;
import(path : "onshape/std/geometry.fs", version : "1096.0");

export function invertMap(m is map) {
    var result = {};
    for (var entry in m)
    {
        result[entry.value] = entry.key;
    }
    return result;
}

// Return the type specified from the entity's attribute list. Returns undefined if the type 
// does not exist on the entity.
export function getAttributeType(context is Context, query is Query, attributeType is string)
{
    var attributes = getAttributes(context, {
            "entities" : query
    });
    for (var attribute in attributes)
    {
        if (attribute['type'] == attributeType)
        {
            return attribute;
        }
    }
}

// Gets all entities with a specific type within their attributes
export function getEntitiesWithAttributeType(context is Context, subQuery is Query, attributeType is string) returns Query
{
    return qAttributeFilter(subQuery, {'type': attributeType});
}

export const pipeSizeLookupTable = {
        "name" : "standard",
        "displayName" : "Pipe Standard",
        "default" : "ANSI B36.10M",
        "entries" : {
            "ANSI B36.10M" : {
                "name" : "size",
                "displayName" : "Size",
                "default" : "1",
                "entries" : {
                    "0.75" : "_0_75",
                    "1" : "_1",
                    "1.25" :"_1_25",
                    "1.5" : "_1_5"
                }
            }
        }
    };