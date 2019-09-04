FeatureScript 1135;
import(path : "onshape/std/geometry.fs", version : "1135.0");

annotation { "Feature Type Name" : "Tester" }
export const tester = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "Test Suite Name" }
        definition.testSuiteName is string;
        
        annotation { "Name" : "Test Assertions", "Item name" : "Assertion"}
        definition.assertions is array;
        for (var assertion in definition.assertions)
        {
            annotation { "Name" : "My String" }
            assertion.assertionName is string;
            
            annotation { "Name" : "Expression to test" }
            isAnything(assertion.toTest);
            
            annotation { "Name" : "Should Equal" }
            isAnything(assertion.expected);
        }
        
    }
    {
        for (var assertion in definition.assertions)
        {
            if (assertion.toTest != assertion.expected)
            {
                throw regenError(definition.testSuiteName ~ "-->" ~ assertion.assertionName ~ " Failed! Got: " ~ assertion.toTest ~ " but expected: " ~ assertion.expected);
            }
        }
    });
