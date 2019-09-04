FeatureScript 1096;
import(path : "onshape/std/geometry.fs", version : "1096.0");

annotation { "Feature Type Name" : "Print Attribute" }
export const myFeature = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        // Define the parameters of the feature type
        annotation { "Name" : "My Query", "Filter" : EntityType.FACE || EntityType.VERTEX || EntityType.EDGE || EntityType.BODY || BodyType.MATE_CONNECTOR}
        definition.myQuery is Query;
        
    }
    {
        println("Attributes: " ~ getAttributes(context, {
                "entities" : definition.myQuery
        }));
        println("ID: " ~ definition.myQuery);
    });
