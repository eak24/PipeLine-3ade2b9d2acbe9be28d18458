FeatureScript 1096;
import(path : "onshape/std/geometry.fs", version : "1096.0");

export const inserterTypeKey = 'simpleInserter';

/**
 * Insert a part centered at the mateConnector specified.
 **/
export const insert = function(context is Context, id is Id, definition is map) returns Query
{
    var location = evMateConnector(context, { "mateConnector" : definition.mateConnectors[0] });
    const addedPart = addInstance(definition.instantiator, definition.partStudio, {
        'transform': toWorld(location),
        'configurationOverride': definition.partInstantiationInfo.configuration,
        'configuration': definition.partInstantiationInfo.configuration,
        'identity' : definition.mateConnector
    });
    return addedPart;
};

export const postInstantiation = function(context is Context, id is Id, definition is map)
{
};
