FeatureScript 1120;

import(path : "onshape/std/feature.fs", version : "1120.0");
import(path : "onshape/std/math.fs", version : "1120.0");
import(path : "onshape/std/units.fs", version : "1120.0");
export import(path : "onshape/std/variable.fs", version : "1120.0");
import(path : "onshape/std/valueBounds.fs", version : "1120.0");

export import(path : "07df3dc395e84ce7297d5b27", version : "6f458454d1f9bec78d2655bf");
icon::import(path : "1d8f208207193c8b15c1c797", version : "844418160c954dcb3b6d41bd");


annotation {"Feature Type Name" : "Headloss", "Feature Name Template": "###name = #value", "UIHint" : "NO_PREVIEW_PROVIDED", "Editing Logic Function" : "variableEditLogic", "Icon" : icon::BLOB_DATA}
export const headlossFeature = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        headlossUiPredicate(definition);
        
        annotation { "Name" : "Variable Name" }
        definition.name is string;
        
    }
    {
        dimensionChecker(definition.flowRate, {"meter":3, "second" : -1} as UnitSpec, "flowRate", FLOW_BOUNDS);
        assignVariable(context, id, {
                "variableType" : VariableType.ANY,
                "name" : definition.name,
                "anyValue" : headloss(definition.flowRate, definition.diam, definition.length, definition.nu*10^-6*meter^2/second, definition.pipeRough, definition.kMinor)
        });
    });

export predicate headlossUiPredicate(definition)
{
    annotation { "Name" : "Flow Rate", "Default" : "0.001*meter^3/second"}
    isAnything(definition.flowRate);
    
    annotation { "Name" : "Diameter" }
    isLength(definition.diam, LENGTH_BOUNDS);
    
    annotation { "Name" : "Length" }
    isLength(definition.length, LENGTH_BOUNDS);
    
    annotation { "Name" : "Kinematic Viscosity" }
    isReal(definition.nu, POSITIVE_REAL_BOUNDS);
    
    annotation { "Name" : "Pipe Roughness" }
    isLength(definition.pipeRough, LENGTH_BOUNDS);
    
    annotation { "Name" : "K Minor" }
    isReal(definition.kMinor, POSITIVE_REAL_BOUNDS);
}

export const RE_TRANSITION_PIPE = 2100;

export function rePipe(FlowRate, Diam, Nu)
{
    // Return the Reynolds Number for a pipe.
    return (4 * FlowRate) / (PI * Diam * Nu);
}

export function fric(flowRate, Diam, Nu, PipeRough)
{
    var f;
    const re = rePipe(flowRate, Diam, Nu);
    if (rePipe(flowRate, Diam, Nu) >= RE_TRANSITION_PIPE)
    {
        // Swamee-Jain friction factor for turbulent flow; best for
        // #Re>3000 and ε/Diam < 0.02
        f = (0.25 / (log10(PipeRough / (3.7 * Diam) + 5.74 / re ^ 0.9)) ^ 2);
    }
    else
    {
        f = 64 / re;
    }
    return f;
}

export function headlossFric(flowRate, diam, length, nu, pipeRough)
{
    // Return the major head loss (due to wall shear) in a pipe.
    // This equation applies to both laminar and turbulent flows.
    return (fric(flowRate, diam, nu, pipeRough)
            * 8 / (gravity * PI^2)
            * (length * flowRate^2) / diam^5
            );
}

export function headlossExp(flowRate, diam, kMinor)
{
    // Return the minor head loss (due to expansions) in a pipe.
    // This equation applies to both laminar and turbulent flows.
    return kMinor * 8 / (gravity * PI ^ 2) * flowRate ^ 2 / diam ^ 4;
}

export function headloss(flowRate, diam, length, nu, pipeRough, kMinor)
{
    // Return the total head loss from major and minor losses in a pipe.
    // This equation applies to both laminar and turbulent flows.
    return (headlossFric(flowRate, diam, length, nu, pipeRough)
            + headlossExp(flowRate, diam, kMinor));
}

