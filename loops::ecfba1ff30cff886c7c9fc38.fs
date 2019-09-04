FeatureScript 1135;
import(path : "onshape/std/geometry.fs", version : "1135.0");

annotation { "Feature Type Name" : "Loops" }
export const loops = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "Type of Loop" }
        definition.loopType is LoopType;
        
        annotation { "Name" : "Features to loop" }
        definition.instanceFunction is FeatureList;
        
        if (definition.loopType == LoopType.FOR)
        {
            annotation { "Name" : "Number of Iterations" }
            isInteger(definition.nIterations, POSITIVE_COUNT_BOUNDS);
        } else if (definition.loopType == LoopType.WHILE)
        {
            annotation { "Name" : "While Loop Guard" }
            isAnything(definition.continueExpression);
        }
    }
    {
        //make it an array of functions
        definition.instanceFunction = valuesSortedById(context, definition.instanceFunction);

        // var featureSuccessCount = 0;
        for (var i = 0; i < size(definition.transforms); i += 1)
        {
 
        }
    });
    
export enum LoopType
{
    WHILE, FOR
}

function performFeatures(context is Context, id is Id, definition is map)
{
    linearPattern(context, id + "linearPattern1", {
            "patternType" : PatternType.FEATURE,
            "instanceFunction" : definition.instanceFunction,
            "directionOne" : qCreatedBy(newId() + "Right", EntityType.FACE),
            "distance" : 1.0 * inch,
            "instanceCount" : 2
    });
}


