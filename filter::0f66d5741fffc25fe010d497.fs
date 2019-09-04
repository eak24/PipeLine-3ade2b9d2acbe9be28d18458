FeatureScript 1135;
import(path : "onshape/std/geometry.fs", version : "1135.0");
export import(path : "onshape/std/variable.fs", version : "1135.0");

import(path : "9092a7bca4fb3d6a0ea39170", version : "269b4fa33957ad7e42f8a506");
import(path : "bee595512c86d688fcb12946", version : "aec52e4eeefe49a438fbbaf8");
import(path : "ef9a0209c0aa76f001749d5a", version : "354a414930020956ba6cf7e3");
export import(path : "07df3dc395e84ce7297d5b27", version : "6f458454d1f9bec78d2655bf");
import(path : "ce024f31e19b8aabf789b4c8", version : "7af2751a7071d578bef7e9ca");
import(path : "c1f5bd76824416da18879419", version : "cfbc3b7b245fefdf4d3557ca");
export import(path : "72b8225f8da139f77cf79750", version : "3910a37dfc0f4f98194bc007");

export const filterDesignDefaults = mergeMaps(flowParameterDefaults, {});

annotation { "Feature Type Name" : "Filter Design", "Feature Name Template": "###name = #value", "UIHint" : "NO_PREVIEW_PROVIDED", "Editing Logic Function" : "variableEditLogic"}
export const filterDesignFeature = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        variableNamePredicate(definition);
        
        flowParametersPredicate(definition);
        
    }
    {
        // Any definition verification needed
        definition.temp = (definition.temp+273.15)*kelvin;
        if (definition.drillBits == undefined)
        {
            definition.drillBits = DRILL_BITS;
        }
        
        
        // Call the design functions
        definition.viscosityKinematic = viscosityKinematic(definition.temp);
        // definition.rowN = rowN(definition);
        // definition.rowB = rowB(definition);
        // definition.velocityCritical = velocityCritical(definition);
        // definition.pipeAMin = pipeAMin(definition);
        // definition.pipeNd = pipeNd(definition);
        // definition.topRowOrificeA = topRowOrificeA(definition);
        // definition.orificeDMax = orificeDMax(definition);
        // definition.orificeD = orificeD(definition);
        // definition.orificeA = orificeA(definition);
        // definition.orificeNMaxPerRow = orificeNMaxPerRow(definition);
        // definition.qPerRow = qPerRow(definition);
        // definition.orificeHPerRow = orificeHPerRow(definition);
        // definition.orificeNPerRow = orificeNPerRow(definition);
        
        debugPrint(context, definition);
    
        assignVariable(context, id, {
                "variableType" : VariableType.ANY,
                "name" : definition.name,
                "anyValue" : definition
        });        
    }, filterDesignDefaults
);

function backwashHL(hSand, porosity, temp, sandDensity)
{
    return hSand*(1-porosity)*(sandDensity/densityWater(temp)-1);
}

