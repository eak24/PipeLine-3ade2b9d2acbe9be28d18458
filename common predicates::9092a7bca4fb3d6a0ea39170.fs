FeatureScript 1135;
import(path : "onshape/std/geometry.fs", version : "1135.0");
import(path : "07df3dc395e84ce7297d5b27", version : "6f458454d1f9bec78d2655bf");

export predicate flowParametersPredicate(definition is map)
{
    annotation { "Group Name" : "Flow Parameters", "Collapsed By Default" : true }
    {
        annotation { "Name" : "Flow Rate", "Default" : "0.020*meter^3/second" }
        isAnything(definition.q);
        
        annotation { "Name" : "Temperature in Celsius"}
        isReal(definition.temp, {(unitless) : [0,20,100]} as RealBoundSpec);
        
        annotation { "Name" : "Headloss" }
        isLength(definition.hL, {(centimeter) : [0,40,1000]} as LengthBoundSpec);
    }
}

export const flowParameterDefaults = {
    q : 20*liter/second,
    temp : 20
};

export predicate variableNamePredicate(definition is map)
{
    annotation { "Name" : "Variable Name In Which To Store The Result" }
    definition.name is string;
}